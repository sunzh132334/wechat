namespace Wechat.Api
{
    /// <summary>
    /// 返回参数
    /// </summary>
    public class ResponseBase
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public virtual bool Success { get; set; } = true;

        /// <summary>
        /// 返回码
        /// </summary>
        public virtual string Code { get; set; } = "0";

        /// <summary>
        /// 返回消息
        /// </summary>
        public virtual string Message { get; set; } = "成功";
    }

    public class ResponseBase<T> : ResponseBase
    {

        /// <summary>
        /// 数据
        /// </summary>
        public virtual T Data { get; set; }



    }


    public class ResponseBase<T1,T2> : ResponseBase
    {

        /// <summary>
        /// 数据
        /// </summary>
        public virtual T1 Data { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public virtual T2 Result { get; set; }

    }
}