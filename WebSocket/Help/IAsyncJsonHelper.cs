using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebSocket
{
    /// <summary>
    /// 
    /// </summary>
   public interface IAsyncJsonHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task ToJsonAsync(Stream stream, object obj);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        ValueTask<object> ToObjectAsync(Stream stream, Type type);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        ValueTask<TObject> ToObjectAsync<TObject>(Stream stream);
    }
}
