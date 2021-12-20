using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket.Basic;

namespace WebSocket
{
    /// <summary>
    /// 
    /// </summary>
    public class WebSocketMessageHandler : SocketHandler
    {


        private readonly IChatSessionService _ichatSessionService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="_chatSessionService"></param>
        /// <param name="userService"></param>
        public WebSocketMessageHandler(ConnectionManager connection, IChatSessionService _chatSessionService) : base(connection)
        {
            _ichatSessionService = _chatSessionService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="classRoomId"></param>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public override async Task OnConnected(System.Net.WebSockets.WebSocket socket, string classRoomId, string userId)
        {

            await base.OnConnected(socket, classRoomId, userId);
            int messageType = Convert.ToInt32(ChatEnum.AllRef);
            await SendMessageToAll($"{{\"type\":{messageType}}}", classRoomId);
        }
        /// <summary>
        /// 介绍到信息
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="classRoomId"></param>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override async Task Receive(System.Net.WebSockets.WebSocket socket, string classRoomId, string userId, WebSocketReceiveResult result, byte[] buffer)
        {
            var redisMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            if (redisMessage == "ping")
            {
                //心跳不保存数据
                int messageType = Convert.ToInt32(ChatEnum.ToSelf);
                string message = $"{{\"type\":{messageType}}}";
                byte[] bufferMessgae = Encoding.UTF8.GetBytes(message);
                //await SendMessageToSelf($"{{\"type\":{messageType}}}", classRoomId, userId);
                await socket.SendAsync(new ArraySegment<byte>(bufferMessgae, 0, bufferMessgae.Length), WebSocketMessageType.Text, true, CancellationToken.None);

            }
            else
            {
                int messageType = Convert.ToInt32(ChatEnum.SingleRef);
                var user = await _ichatSessionService.Getone<OS_User>(Convert.ToInt64(userId));
                if (user == null)
                    return;
                double socre = await _ichatSessionService.SaveMessageAsync(classRoomId, new RedisMessage { Id = Guid.NewGuid().ToString(), UserId = Convert.ToInt64(userId), Image = user.Image, Message = redisMessage, WebSocketId = $"{classRoomId}_{userId}", Name = user.Name });
                await SendMessageToAll($"{{\"type\":{messageType},\"score\":{socre}}} ", classRoomId);
            }

        }




    }
}

