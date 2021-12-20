using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket
{
    /// <summary>
    /// 
    /// </summary>
    public interface IJsonHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string ToJson(object obj);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object ToObject(string json, Type type);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        TObject ToObject<TObject>(string json);
    }
}
