using System;

namespace MyCommonTool.Models
{
    /// <summary>
    /// 自定义公共异常类
    /// </summary>
    public class CommonException : Exception
    {

        /// <summary>
        /// 状态码
        /// </summary>
        public ResultCode Code { get; set; }

        /// <summary>
        /// 错误信息提示
        /// </summary>
        public override string Message { get; }

        /// <summary>
        /// 保存状态码的构造方法
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">错误内容</param>
        public CommonException(ResultCode code, string message = null)
        {
            Code = code;
            if (string.IsNullOrEmpty(message))
            {
                switch (code)
                {
                    case ResultCode.ArgumentError:
                        Message = "请求参数错误"; break;

                    case ResultCode.AuthError:
                        Message = "Authorization验证失败"; break;
                }
            }
            else Message = message;
        }
    }
}
