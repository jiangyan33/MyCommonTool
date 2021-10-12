namespace MyCommonTool.Models
{
    /// <summary>
    /// 数据库连接配置
    /// </summary>
    public class DBOption
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Uid { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 重新后的ToString方法
        /// </summary>
        /// <returns></returns>
        override
        public string ToString()
        {
            return $"Server={Server};Database={Database};Uid={Uid};Pwd={Pwd};Port={Port}";
        }
    }
}
