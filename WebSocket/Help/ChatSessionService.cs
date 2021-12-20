﻿using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocket
{
	/// <summary>
	/// 
	/// </summary>
	public class ChatSessionService : IChatSessionService
	{
		private static IDatabase _redis;
		private static IJsonHelper _jsonHelper;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="redis"></param>
		/// <param name="jsonHelper"></param>
		public ChatSessionService(RedisHelper redis, IJsonHelper jsonHelper)
		{
			_redis = redis.GetDatabase();
			_jsonHelper = jsonHelper;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="classRoomId"></param>
		/// <param name="score"></param>
		/// <returns></returns>
		public async Task<T> GetMessageAsync<T>(string classRoomId, double score)
		{
			var result=await _redis.SortedSetRangeByScoreAsync($"{CommonConstant.CHAT_COMMON_PREFIX}{classRoomId}", score, score, Exclude.None,Order.Descending,0,1);
			return _jsonHelper.ToObject<T>(result[0]);

		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="classRoomId"></param>
		/// <param name="redisMessage"></param>
		/// <returns></returns>
		public async Task<double> SaveMessageAsync(string classRoomId, RedisMessage redisMessage)
		{
			double socre = -1;
			await Task.Run(() => { socre= SortedSetAdd($"{CommonConstant.CHAT_COMMON_PREFIX}{classRoomId}", redisMessage); });
			return socre;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="classRoomId"></param>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <param name="desc"></param>
		/// <returns></returns>
		public async Task<List<RedisMessage>> GetMessageList(string classRoomId, int pageIndex, int pageSize, bool desc = false)
		{
			List<RedisMessage> result = new List<RedisMessage>();
			await Task.Run(() => {
				result = SortedSetRangeByRank<RedisMessage>($"{CommonConstant.CHAT_COMMON_PREFIX}{classRoomId}", (pageIndex - 1) * pageSize, (pageIndex * pageSize) - 1, desc);

			});
			return result;
		}

		public async Task<long> GetMessageCount(string classRoomId)
		{
			long count = 0;
			await Task.Run(() => {
				count = SortedSetLength($"{CommonConstant.CHAT_COMMON_PREFIX}{classRoomId}");

			});
			return count;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key">classRoomId</param>
		/// <param name="dataKey">userId</param>
		/// <returns></returns>
		public async Task<T> GetSocketByIdAsync<T>(string key, string dataKey)
		{
			string value = await _redis.HashGetAsync(key, dataKey);
			return _jsonHelper.ToObject<T>(value);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public ConcurrentDictionary<string, T> HashGetAll<T>(string key)
		{
			var query = _redis.HashGetAll(key);
			ConcurrentDictionary<string, T> dic = new ConcurrentDictionary<string, T>();
			foreach (var item in query)
			{
				dic.TryAdd(item.Name, ConvertObj<T>(item.Value));
			}
			return dic;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="expiry"></param>
		/// <returns></returns>
		public async Task SaveClassRoomSndUserInfo<T>(T key, string value, TimeSpan? expiry = default)
		{
			string keyValue = ConvertJson(key);
			await _redis.StringSetAsync($"{CommonConstant.WEBSOCKET_PREFIX}{keyValue}", value, expiry);
		}

		public async Task<Tuple<string, string, string>> GetClassRoomSndUserInfo<T>(T key)
		{
			string keyValue = ConvertJson(key);
			var result = (await _redis.StringGetAsync($"{CommonConstant.WEBSOCKET_PREFIX}{keyValue}")).ToString();
			if (!string.IsNullOrWhiteSpace(result))
			{
				return new Tuple<string, string, string>(result.Split('_')[0], result.Split('_')[1], result.Split('_')[2]);
			}
			return new Tuple<string, string, string>(null, null, null); ;
		}
		/// <summary>
		/// websocket 存入redis
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="dataKey"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public async Task<bool> WebSocketHashSet<T>(string key, string dataKey, T t)
		{
			string json = ConvertJson(t);
			return await _redis.HashSetAsync(key, dataKey, json);
		}

		public async Task<bool> HashDeleteAsync(string key, string dataKey)
		{
			return await _redis.HashDeleteAsync(key, dataKey);
		}

		public async Task<bool> StringDeleteAsync<T>(T key)
		{
			string keyValue = ConvertJson(key);
			return await _redis.KeyDeleteAsync($"{CommonConstant.WEBSOCKET_PREFIX}{keyValue}");
		}

		public async Task Incr(string classRoomId)
		{
			await _redis.StringIncrementAsync($"{CommonConstant.CLASSROOM_NUMBER}{classRoomId}");
		}
		public async Task Decr(string classRoomId)
		{
			await _redis.StringDecrementAsync($"{CommonConstant.CLASSROOM_NUMBER}{classRoomId}");
		}
		public async Task<long> HashStringLength(string key)
		{
			var value = await _redis.StringGetAsync($"{CommonConstant.CLASSROOM_NUMBER}{key}");
			long.TryParse(value, out long number);
			return number;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public async Task<TEntity> Getone<TEntity>(long key) where TEntity:new()
		{
			TEntity entity = new TEntity();
			var datahash = await _redis.HashGetAsync($"Data:OS_User", key);
			if (datahash.HasValue)
			{
				entity = ConvertObj<TEntity>(datahash);
				
			}
			return entity;
		}
	
		#region 常规
		/// <summary>
		/// 将对象转换成string字符串
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		private string ConvertJson<T>(T value)
		{
			string result = value is string ? value.ToString() :
				_jsonHelper.ToJson(value);
			//JsonConvert.SerializeObject(value, Formatting.None);
			return result;
		}

		/// <summary>
		/// 将值集合转换成RedisValue集合
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="redisValues"></param>
		/// <returns></returns>
		private RedisValue[] ConvertRedisValue<T>(params T[] redisValues) => redisValues.Select(o => (RedisValue)ConvertJson<T>(o)).ToArray();

		/// <summary>
		/// 将值反系列化成对象集合
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="values"></param>
		/// <returns></returns>
		private List<T> ConvetList<T>(RedisValue[] values)
		{
			List<T> result = new List<T>();
			foreach (var item in values)
			{
				var model = ConvertObj<T>(item);
				result.Add(model);
			}
			return result;
		}
		/// <summary>
		/// 将值反系列化成对象
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		private T ConvertObj<T>(RedisValue value)
		{
			return value.IsNullOrEmpty ? default :
				_jsonHelper.ToObject<T>(value);
			//JsonConvert.DeserializeObject<T>(value);

		}

		#endregion

		#region Zset

		/// <summary>
		/// 添加一个值到Key
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="score">排序分数，为空将获取集合中最大score加1</param>
		/// <returns></returns>
		private double SortedSetAdd<T>(string key, T value, double? score = null)
		{

			double scoreNum = score ?? _GetScore(key);
			if (_redis.SortedSetAdd(key, ConvertJson<T>(value), scoreNum))
				return scoreNum;
			else return -1;
		}

		/// <summary>
		/// 获取集合中的数量
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private long SortedSetLength(string key)
		{

			return _redis.SortedSetLength(key);
		}

		/// <summary>
		/// 获取从 start 开始的 stop 条数据
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="start">起始数</param>
		/// <param name="stop">-1表示到结束，0为1条</param>
		/// <param name="desc">是否按降序排列</param>
		/// <returns></returns>
		private List<T> SortedSetRangeByRank<T>(string key, long start = 0, long stop = -1, bool desc = false)
		{

			Order orderBy = desc ? Order.Descending : Order.Ascending;
			var rValue = _redis.SortedSetRangeByRank(key, start, stop, orderBy);
			return ConvetList<T>(rValue);
		}

		#region 内部辅助方法
		/// <summary>
		/// 获取指定Key中最大Score值,
		/// </summary>
		/// <param name="key">key名称，注意要先添加上Key前缀</param>
		/// <returns></returns>
		private double _GetScore(string key)
		{
			double dValue = 0;
			var rValue = _redis.SortedSetRangeByRankWithScores(key, 0, 0, Order.Descending).FirstOrDefault();
			dValue = rValue != null ? rValue.Score : 0;
			return dValue + 1;
		}





		#endregion

		#endregion


	}
}
