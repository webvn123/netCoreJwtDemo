using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyDemo.Dto
{
    /// <summary>
    ///统一消息
    /// </summary>
    public class VimData
    {
        public VimData()
        {
            this.Success = true;
        }
        public string Msg { get; set; }
        public bool Success { get; set; }

        public int Code { get; set; }
    }

    public class VimData<T> : VimData where T : class
    {
        public VimData(T data) : base()
        {
            this.Data = data;
        }
        public T Data { get; set; }
    }
    public class VimDataList<T> : VimData<T> where T : class
    {
        public VimDataList(T data, int count) : base(data)
        {
            this.Count = count;
        }
        public int Count { get; set; }
    }
}
