using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyCommonTool.Utils
{
    public static class Utils
    {
        #region 数据库序列化相关

        /// <summary>
        /// 将DbDataReader序列化成对象，如果DataTable中和对象中存在相同的字段名，就会赋值，否则不会赋值 包含分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">游标</param>
        /// <param name="ignoreTableUnderline">是否忽略数据库表字段中的下划线</param>
        /// <returns></returns>
        public async static Task<(List<T>, long rows)> SerializeToPageObject<T>(this DbDataReader reader, bool ignoreTableUnderline = true) where T : new()
        {
            var posts = new List<T>();
            var rows = 0L;
            if (reader == null) return (posts, rows);
            using (reader)
            {
                if (!reader.HasRows) return (posts, rows);
                var columnList = reader.GetColumnSchema();

                var type = typeof(T);
                var props = type.GetProperties();
                var columns = columnList.Select(c =>
                new
                {
                    ProptyName = ignoreTableUnderline ?
                        c.ColumnName.Replace("_", "").ToLower() :
                        c.ColumnName.ToLower(),
                    c.ColumnName
                }
            );
                if (columns.Count() == 0)
                    return (posts, rows);
                var getDefaultValue = new Func<Type, object>(propType =>
                {
                    // 如果是可空类型，直接返回null
                    if (Nullable.GetUnderlyingType(propType) != null)
                        return null;
                    else
                    {
                        // 如果时非可空类型 直接获取默认值
                        // 如果不是值类型，直接返回null
                        return propType.IsValueType ? Activator.CreateInstance(propType) : null;
                    }
                });
                var convertTypeToProperty = new Func<Type, object, object>((prop, value) =>
                {
                    try
                    {
                        // 当datatable中字段的值为空时，需要根据实体类的属性类型来初始化默认值
                        if (value == null || value == DBNull.Value)
                        {
                            return getDefaultValue(prop);
                        }
                        // 当datatable中字段的值不为空时，将其转换为属性类型，如果失败，则抛出异常。
                        else
                        {
                            var t = Nullable.GetUnderlyingType(prop) ?? prop;
                            return Convert.ChangeType(value, t);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                });
                while (await reader.ReadAsync())
                {
                    var obj = new T();
                    foreach (var prop in props)
                    {
                        var c = columns.Where(x => x.ProptyName.ToLower() == prop.Name.ToLower()).FirstOrDefault();
                        if (c != null)
                        {
                            try
                            {
                                // 类型相同直接赋值
                                if (reader[c.ColumnName].GetType().FullName == prop.PropertyType.FullName)
                                    prop.SetValue(obj, reader[c.ColumnName], null);
                                // 类型不同，将table的类型转换为属性的类型，转换失败时赋值为当前类型的默认值
                                else
                                    prop.SetValue(obj, convertTypeToProperty(prop.PropertyType, reader[c.ColumnName]), null);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                    posts.Add(obj);
                }
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        rows = Convert.ToInt64(reader["totalRecords"]);
                    }
                }
            }
            return (posts, rows);
        }

        /// <summary>
        /// 将DbDataReader序列化成对象，如果DataTable中和对象中存在相同的字段名，就会赋值，否则不会赋值，没有分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">游标</param>
        /// <param name="ignoreTableUnderline">是否忽略数据库表字段中的下划线</param>
        /// <returns></returns>
        public async static Task<List<T>> SerializeToObject<T>(this DbDataReader reader, bool ignoreTableUnderline = true) where T : new()
        {
            var posts = new List<T>();
            if (reader == null) return posts;
            using (reader)
            {
                if (!reader.HasRows) return posts;
                var columnList = reader.GetColumnSchema();

                var type = typeof(T);
                var props = type.GetProperties();
                var columns = columnList.Select(c =>
                new
                {
                    ProptyName = ignoreTableUnderline ?
                        c.ColumnName.Replace("_", "").ToLower() :
                        c.ColumnName.ToLower(),
                    c.ColumnName
                }
            );
                if (columns.Count() == 0)
                    return posts;
                var getDefaultValue = new Func<Type, object>(propType =>
                {
                    // 如果是可空类型，直接返回null
                    if (Nullable.GetUnderlyingType(propType) != null)
                        return null;
                    else
                    {
                        // 如果时非可空类型 直接获取默认值
                        // 如果不是值类型，直接返回null
                        return propType.IsValueType ? Activator.CreateInstance(propType) : null;
                    }
                });
                var convertTypeToProperty = new Func<Type, object, object>((prop, value) =>
                {
                    try
                    {
                        // 当datatable中字段的值为空时，需要根据实体类的属性类型来初始化默认值
                        if (value == null || value == DBNull.Value)
                        {
                            return getDefaultValue(prop);
                        }
                        // 当datatable中字段的值不为空时，将其转换为属性类型，如果失败，则抛出异常。
                        else
                        {
                            var t = Nullable.GetUnderlyingType(prop) ?? prop;
                            return Convert.ChangeType(value, t);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                });
                while (await reader.ReadAsync())
                {
                    var obj = new T();
                    foreach (var prop in props)
                    {
                        var c = columns.Where(x => x.ProptyName.ToLower() == prop.Name.ToLower()).FirstOrDefault();
                        if (c != null)
                        {
                            try
                            {
                                // 类型相同直接赋值
                                if (reader[c.ColumnName].GetType().FullName == prop.PropertyType.FullName)
                                    prop.SetValue(obj, reader[c.ColumnName], null);
                                // 类型不同，将table的类型转换为属性的类型，转换失败时赋值为当前类型的默认值
                                else
                                    prop.SetValue(obj, convertTypeToProperty(prop.PropertyType, reader[c.ColumnName]), null);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                    posts.Add(obj);
                }
            }
            return posts;
        }

        /// <summary>
        /// 判断是否查下到数据
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static bool IsExist(this DbDataReader reader)
        {
            bool result = false;
            if (reader == null) return result;
            using (reader)
            {
                result = reader.HasRows;
            }
            return result;
        }

        #endregion 数据库序列化相关

        #region 类型转换

        public static KeyValuePair<string, object>[] ConvertObjToKeyPair(object obj)
        {
            var type = obj.GetType();
            var list = new List<KeyValuePair<string, object>>();
            foreach (PropertyInfo item in type.GetProperties())
            {
                list.Add(new KeyValuePair<string, object>(item.Name, item.GetValue(obj, null)));
            }
            return list.ToArray();
        }

        public static object[] ConvertObjToKeyPairObject(object obj)
        {
            return ConvertObjToKeyPair(obj).Cast<object>().ToArray();
        }

        #endregion 类型转换

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

            using (SymmetricAlgorithm algorithm = Aes.Create())
            using (ICryptoTransform encryptor = algorithm.CreateEncryptor(keyByte, vi))
            {
                var resultByte = encryptor.TransformFinalBlock(strByte, 0, strByte.Length);
                return Convert.ToBase64String(resultByte, 0, resultByte.Length);
            }
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
            using (SymmetricAlgorithm algorithm = Aes.Create())
            using (ICryptoTransform encryptor = algorithm.CreateDecryptor(keyByte, vi))
            {
                var resByte = encryptor.TransformFinalBlock(strByte, 0, strByte.Length);
                return Encoding.UTF8.GetString(resByte);
            }
        }

        #endregion 加密解密

        #region 字符串操作

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
        /// Sha1加密，采用UTF-8编码
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Sha1(string content)
        {
            using var sha1 = SHA1.Create();
            return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(content))).Replace("-", "").ToLower();
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

        /// <summary>
        /// 随机生成长度11位的随机字符串
        /// </summary>
        /// <returns></returns>
        public static string NonceStr()
        {
            return Guid.NewGuid().ToString("N").Substring(2, 11);
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
        /// url中的query参数序列化位字典类型
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
            return "?" + (result.Length > 2 ? result.Substring(2) : result);
        }

        /// <summary>
        /// 将驼峰格式调整为下划线
        /// </summary>
        /// <param name="attrName"></param>
        /// <returns></returns>
        public static string ChangeAttrToLineName(this string attrName)
        {
            if (attrName.Length <= 1) return attrName;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(attrName.ToLower()[0]);
            for (int i = 1; i < attrName.Length; i++)
            {
                if ('A' <= attrName[i] && attrName[i] <= 'Z')
                {
                    stringBuilder.Append($"_{attrName[i]}".ToLower());
                }
                else
                {
                    stringBuilder.Append(attrName[i]);
                }
            }
            return stringBuilder.ToString();
        }

        #endregion 字符串操作
    }
}