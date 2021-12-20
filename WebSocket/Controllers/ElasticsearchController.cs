using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocket.Manager;

namespace WebSocket.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/es")]
    [ApiController]
    public class ElasticsearchController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("defaultindex")]
        public async Task<ResponseMessage> DefaultIndexAsync()
        {
            ResponseMessage response = new();
            try
            {
                TestData testData = new TestData();
                await testData.IndexManyAsync();
            }
            catch (Exception)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                throw;
            }
            return response;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("defaultsearch")]
        public PagingResponseMessage<Person> DefaultSearch()
        {
            PagingResponseMessage<Person> response = new() {  Extension=new List<Person>()};
            try
            {
                TestData testData = new();
                response.Extension= testData.Search().ToList();
            }
            catch (Exception)
            {
                response.Code = ResponseCodeDefines.ServiceError;
                throw;
            }
            return response;
        }
    }
}
