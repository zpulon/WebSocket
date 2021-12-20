using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocket.Basic
{
    public class PageResult<TEntity>
    {
        public IEnumerable<TEntity> Items { get; set; }
        public int Total { get; set; }
    }

    public class Pager
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; } = 1;
        /// <summary>
        /// 分页大小
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
