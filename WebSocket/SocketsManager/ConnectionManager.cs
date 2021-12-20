
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionManager
    {


        #region 

        /// <summary>
        /// 通过classRoomId_userId 组合为key 
        /// </summary>
        private static readonly ConcurrentDictionary<Tuple<string, string>, System.Net.WebSockets.WebSocket> _connections = new ConcurrentDictionary<Tuple<string, string>, System.Net.WebSockets.WebSocket>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="classRoomId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public System.Net.WebSockets.WebSocket GetSocketById(string classRoomId, string userId)
        {

            Tuple<string, string> tuple = new Tuple<string, string>(classRoomId, userId);
            var result = _connections.FirstOrDefault(x => x.Key == tuple).Value;
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="classRoomId">教室标识</param>
        /// <returns></returns>
        public ConcurrentDictionary<Tuple<string, string>, System.Net.WebSockets.WebSocket> GetAllConnectionByClassRoomId(string classRoomId)
        {
            var dictionary = _connections.Where(x => x.Key.Item1 == classRoomId).ToDictionary(z => z.Key, z => z.Value);
            ConcurrentDictionary<Tuple<string, string>, System.Net.WebSockets.WebSocket> concurrentDictionary =
            new ConcurrentDictionary<Tuple<string, string>, System.Net.WebSockets.WebSocket>(dictionary);
            return concurrentDictionary;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="classRoomId">教室标识</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ConcurrentDictionary<Tuple<string, string>, System.Net.WebSockets.WebSocket> GetSelfWebsocket(string classRoomId, string userId)
        {
            var dictionary = _connections.Where(x => x.Key.Item1 == classRoomId && x.Key.Item2 == userId).ToDictionary(z => z.Key, z => z.Value);
            ConcurrentDictionary<Tuple<string, string>, System.Net.WebSockets.WebSocket> concurrentDictionary =
            new ConcurrentDictionary<Tuple<string, string>, System.Net.WebSockets.WebSocket>(dictionary);
            return concurrentDictionary;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <returns>元组 第一个 教室标识 第二个用户标识</returns>
        public Tuple<string, string> GetId(System.Net.WebSockets.WebSocket socket)
        {
            var key = _connections.FirstOrDefault(x => x.Value == socket).Key;
            if (key != null)
            {
                Tuple<string, string> tuple = new Tuple<string, string>(key.Item1, key.Item2);
                return tuple;
            }
            else
            {
                return new Tuple<string, string>(null, null);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="classRoomId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task RemoveSocketAsync(string classRoomId, string userId)
        {
            Tuple<string, string> tuple = new Tuple<string, string>(classRoomId, userId);
            _connections.TryRemove(tuple, out var socket);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "socket connection closed", CancellationToken.None);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="classRoomId"></param>
        /// <param name="userId"></param>

        public async Task AddsSocketAsync(System.Net.WebSockets.WebSocket socket, string classRoomId, string userId)
        {
            await Task.Run(() => {
                Tuple<string, string> tuple = new Tuple<string, string>(classRoomId, userId);
                if (_connections.ContainsKey(tuple))
                {
                    _connections[tuple] = socket;
                }
                else
                {
                    _connections.TryAdd(tuple, socket);
                }
            });

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="classRoomId"></param>
        /// <returns></returns>
        public async Task<long> GetClassRoomByIdAsync(string classRoomId)
        {
            long number = 0;
            await Task.Run(() => {
                number = _connections.Where(z => z.Key.Item1 == classRoomId).Count();
            });
            return number;
        }
        #endregion

    }

}
