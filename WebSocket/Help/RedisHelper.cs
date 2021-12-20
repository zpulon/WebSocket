using StackExchange.Redis;
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
    public class RedisHelper : IDisposable
    {
        //连接字符串
        private static string _connectionString;
        //实例名称
        private static string _instanceName;
        //默认数据库
        private static string _passWord;
        private int _defaultDB;
        private ConcurrentDictionary<string, ConnectionMultiplexer> _connections;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="passWord"></param>
        /// <param name="instanceName"></param>
        /// <param name="defaultDB"></param>
        public RedisHelper(string connectionString,string passWord, string instanceName, int defaultDB = 0)
        {
            _connectionString = connectionString;
            _passWord = passWord;
            _instanceName = instanceName;
            _defaultDB = defaultDB;
            _connections = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        }

        /// <summary>
        /// 获取ConnectionMultiplexer
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer GetConnect()
        {
            var config = new ConfigurationOptions
            {
                AbortOnConnectFail = false,//如果是true，Connect方法在链接不到有效的服务器的时候不会创建一个链接实例
                ConnectRetry =3,//在初始化 Connect 失败的时候重新尝试进行链接的次数
                AllowAdmin =false,// 当为true时 ，可以使用一些被认为危险的命令
                ConnectTimeout =15000,//设置建立连接到Redis服务器的超时时间为15000毫秒
                SyncTimeout =10000,//设置对Redis服务器进行同步操作的超时时间为10000毫秒
                AsyncTimeout=10000,//设置对Redis服务器进行异步操作的超时时间为10000毫秒
                Password = _passWord,
                EndPoints = { _connectionString }
            };
            return _connections.GetOrAdd(_instanceName, p => ConnectionMultiplexer.Connect(config));
        }

        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <param name="db">数据库序号</param>
        /// <returns></returns>
        public IDatabase GetDatabase(int db = -1)
        {
            return GetConnect().GetDatabase(db >= 0 ? db : _defaultDB);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="endPointsIndex"></param>
        /// <returns></returns>
        public IServer GetServer(string configName = null, int endPointsIndex = 0)
        {
            var confOption = ConfigurationOptions.Parse(_connectionString);
            return GetConnect().GetServer(confOption.EndPoints[endPointsIndex]);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public ISubscriber GetSubscriber(string configName = null)
        {
            return GetConnect().GetSubscriber();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_connections != null && _connections.Count > 0)
            {
                foreach (var item in _connections.Values)
                {
                    item.Close();
                }
            }
        }
    }
}
