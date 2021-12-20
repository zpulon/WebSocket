
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebSocket.Basic;

namespace WebSocket
{
    /// <summary>
    /// 
    [Route("api/comment")]
    [ApiController]
    public class CommentController : Controller
    {
        private readonly IChatSessionService _ichatSessionService;
        /// <summary>
        /// 
        /// </summary>
        public ConnectionManager Connections { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ichatSessionService"></param>
        /// <param name="connections"></param>
        public CommentController(IChatSessionService ichatSessionService, ConnectionManager connections)
        {
            _ichatSessionService = ichatSessionService;
            Connections = connections;
        }


        /// <summary>
        /// 进入直播间到redis 获取信息
        /// </summary>
        /// <param name="request">用户信息</param>
        /// <returns></returns>
        [HttpPost("getmessagelist")]
        public async Task<PageResult<RedisMessage>> GetMessageList([FromBody] ChatRequest request)
        {

            try
            {
                PageResult<RedisMessage> result = new PageResult<RedisMessage>()
                {
                    Total = (int)await _ichatSessionService.GetMessageCount(request.ClassRoomId),
                    Items = await _ichatSessionService.GetMessageList(request.ClassRoomId, request.PageIndex, request.PageSize, request.desc)
                };
                return result;
            }
            catch (Exception)
            {

                throw;
            }

        }
        /// <summary>
        /// 进入直播间到redis 获取单条信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("message")]
        public async Task<RedisMessage> GetMessage([FromBody] MessageRequest request)
        {
            RedisMessage response = new RedisMessage();
            try
            {
                response = await _ichatSessionService.GetMessageAsync<RedisMessage>(request.ClassRoomId, request.Score);
            }
            catch (Exception)
            {

                throw;
            }
            return response;

        }
        /// <summary>
        /// 获取在线人数   
        /// </summary>
        /// <param name="classRoomId">教室标识</param>
        /// <returns></returns>
        [HttpGet("classroonum")]
        public async Task<long> GetClassRoomNumber([FromQuery] string classRoomId)
        {
            try
            {
                long number = 0;
                //return  Connections.GetClassRoomById(classRoomId);
                await Task.Run(async () => { number = await Connections.GetClassRoomByIdAsync(classRoomId); });
                return number;
            }
            catch (Exception)
            {

                throw;
            }

        }

    }
}
