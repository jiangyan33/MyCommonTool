namespace MyCommonTool.Models
{
    /// <summary>
    /// 接口请求最外层返回值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public ResultCode Code { get; set; }

        /// <summary>
        /// 错误信息提示
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 根据状态码返回数据
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public Result(ResultCode code, string message)
        {
            Code = code;
            Message = message;
        }

        /// <summary>
        /// 请求成功，且有数据返回
        /// </summary>
        /// <param name="data"></param>
        public Result(T data)
        {
            Code = ResultCode.Success;
            Data = data;
        }
    }

    /// <summary>
    /// 状态码
    /// </summary>
    public enum ResultCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 自定义错误
        /// </summary>
        CommonError = 10000,

        /// <summary>
        /// 请求参数错误
        /// </summary>
        ArgumentError = 10001,

        /// <summary>
        /// 内部接口逻辑错误
        /// </summary>
        LogicError = 10002,

        /// <summary>
        /// 权限错误
        /// </summary>
        AuthError = 10003,
    }
}
