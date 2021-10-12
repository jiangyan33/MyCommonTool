using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MyCommonTool.Utils
{
    /// <summary>
    /// 封装一些通用的方法
    /// </summary>
    public static class CommonUtils
    {

        #region 日期操作

        /// <summary>
        /// 解析时间戳为日期类型
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long timeStamp)
        {
            long longTime = 621355968000000000;
            int samllTime = 10000000;
            DateTime dateTime = new DateTime(longTime + timeStamp * samllTime, DateTimeKind.Utc).ToLocalTime();
            return dateTime;
        }

        /// <summary>
        /// 移除时间中的毫秒值信息
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime CutMillisecondFromDateTime(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }

        /// <summary>
        /// 当前的时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        #endregion 日期操作

        #region 加密解密

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="key">32位长度的字符串</param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Encrypt(string key, string str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(key)) return "";
            var keyByte = Encoding.UTF8.GetBytes(key);
            // 将要加密的字符串转换为字节存储
            var strByte = Encoding.UTF8.GetBytes(str);
            var vi = new byte[16];
            for (int i = 0; i < 16; i++) vi[i] = keyByte[i];

            using SymmetricAlgorithm algorithm = Aes.Create();
            using ICryptoTransform encryptor = algorithm.CreateEncryptor(keyByte, vi);
            var resultByte = encryptor.TransformFinalBlock(strByte, 0, strByte.Length);
            return Convert.ToBase64String(resultByte, 0, resultByte.Length);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="key">32位长度的字符串</param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Decode(string key, string str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(key)) return "";
            // 准备解密
            var strByte = Convert.FromBase64String(str);
            var keyByte = Encoding.UTF8.GetBytes(key);
            var vi = new byte[16];
            for (int i = 0; i < 16; i++) vi[i] = keyByte[i];
            using SymmetricAlgorithm algorithm = Aes.Create();
            using ICryptoTransform encryptor = algorithm.CreateDecryptor(keyByte, vi);
            var resByte = encryptor.TransformFinalBlock(strByte, 0, strByte.Length);
            return Encoding.UTF8.GetString(resByte);
        }

        /// <summary>
        /// Sha1加密，采用UTF-8编码
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Sha1(string content)
        {
            using var sha1 = SHA1.Create();
            return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(content))).Replace("-", "").ToLower();
        }

        #endregion 加密解密

        #region 字符串操作

        /// <summary>
        /// 字符串大驼峰和下划线相互转换
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string StrChange(this string src)
        {
            if (string.IsNullOrEmpty(src)) return "";

            var regex = new Regex("[A-Z]");
            var charStr = '_';
            if (regex.IsMatch(src[0].ToString()))
            {
                // 大驼峰
                var stringBuilder = new StringBuilder();
                foreach (var item in src)
                {
                    if (regex.IsMatch(item.ToString()))
                    {
                        stringBuilder.Append(charStr);
                        stringBuilder.Append(item.ToString().ToLower());
                    }
                    else
                    {
                        stringBuilder.Append(item);
                    }
                }
                return stringBuilder.ToString().TrimStart(charStr);
            }
            else
            {
                // 下划线
                var strArr = src.Split(charStr);
                var stringBuilder = new StringBuilder();
                foreach (var item in strArr)
                {
                    for (var i = 0; i < item.Length; i++)
                    {
                        stringBuilder.Append(i == 0 ? item[i].ToString().ToUpper() : item[i].ToString());
                    }
                }
                return stringBuilder.ToString().TrimStart(charStr);
            }
        }

        /// <summary>
        /// 类型转换，数据库字段类型转换为C#字段类型
        /// </summary>
        /// <param name="src"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        public static string MySqlTypeChange(this string src, bool isNull = false)
        {
            var result = src switch
            {
                "varchar" => "string",
                "datetime" => "DateTime",
                "decimal" => "decimal",
                "tinyint" => "int",
                "int" => "int",
                "bigint" => "long",
                _ => throw new Exception("类型匹配失败，未知类型，请联系管理员"),
            };
            if (isNull && result != "string") result += "?";
            return result;
        }

        /// <summary>
        /// 随机生成长度11位的随机字符串
        /// </summary>
        /// <returns></returns>
        public static string NonceStr()
        {
            return Guid.NewGuid().ToString("N").Substring(2, 11);
        }

        /// <summary>
        ///  全角转半角
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToHalfAngle(this string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == '，') continue;
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        /// <summary>
        /// 读取文本文件内容
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static string ReadFile(string path)
        {
            if (!File.Exists(path))
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(path))
            {
                string line;

                // 从文件读取并显示行，直到文件的末尾
                while ((line = sr.ReadLine()) != null)
                {
                    sb.Append(line).Append("\n");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 动态获取实体类中的属性值，忽略大小写
        /// 如果没有获取到返回指定泛型的默认值
        /// </summary>
        /// <typeparam name="T">返回值的数据类型</typeparam>
        /// <param name="instance">实体类实例</param>
        /// <param name="AttrName">属性名称</param>
        /// <returns></returns>
        public static T GetAttrValue<T>(this object instance, string AttrName)
        {
            var prop = instance.GetType().GetProperties().ToList().Find(it => it.Name.ToLower() == AttrName.ToLower());
            var res = prop.GetValue(instance, null);
            return (res is T) ? (T)prop.GetValue(instance, null) : default;
        }

        /// <summary>
        /// 将整数转为小写的中文数字,(一万以内的数据)
        /// </summary>
        /// <param name="ni_intInput"></param>
        /// <returns></returns>
        public static string ToCNLowerCase(this int ni_intInput)

        {
            string tstrRet = "";

            int tintInput;

            int tintRemainder, tintDigitPosIndex = 0;
            string[] tastrNumCNChar = new string[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };

            string[] tastrDigitPosCNChar = new string[] { "", "十", "百", "千", "万", "亿" };

            tintInput = ni_intInput;

            int tintLoopX = 0;
            while (tintInput / 10 > 0 || tintInput > 0)

            {
                tintRemainder = (tintInput % 10);

                if (tintLoopX == 5)//十万

                    tintDigitPosIndex = 1;
                else if (tintLoopX == 8)//亿

                    tintDigitPosIndex = 5;
                else if (tintLoopX == 9)//十亿

                    tintDigitPosIndex = 1;

                if (tintRemainder > 0)

                    tstrRet = tastrNumCNChar[tintRemainder] + tastrDigitPosCNChar[tintDigitPosIndex] + tstrRet;
                else

                    tstrRet = tastrNumCNChar[tintRemainder] + tstrRet; ;

                tintDigitPosIndex += 1;

                tintLoopX += 1;

                tintInput /= 10;
            }

            tstrRet = System.Text.RegularExpressions.Regex.Replace(tstrRet, "零零*零*", "零");
            return tstrRet.TrimEnd('零');
        }

        /// <summary>
        /// url中的query参数序列化为字典类型
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Dictionary<string, string> UriQueryToDictionary(this Uri uri)
        {
            var result = new Dictionary<string, string>();
            var param = uri.Query.Replace("?", "").Split('&');

            foreach (var it in param)
            {
                var temp = it.Split('=');
                result.Add(temp[0], temp[1]);
            }
            return result;
        }

        /// <summary>
        /// 移除字符串结尾处的换行
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string RemoveLastBreak(this string line)
        {
            if (line.EndsWith("\n"))
            {
                var index = line.LastIndexOf('\n');
                line = line.Substring(0, index);
            }
            return line;
        }

        /// <summary>
        /// 对象序列化为query参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string SerializeToQueryParams<T>(this T t) where T : new()
        {
            var result = "";
            if (t == null) return result;

            var props = t.GetType().GetProperties();

            foreach (var item in props)
            {
                var value = item.GetValue(t);
                var type = item.PropertyType;
                var defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
                if (defaultValue?.ToString() != value?.ToString())
                {
                    result += $"&&{item.Name}={value}";
                }
            }
            return "?" + (result.Length > 2 ? result[2..] : result);
        }

        #endregion 字符串操作
    }
}