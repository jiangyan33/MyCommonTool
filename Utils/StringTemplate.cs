using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyCommonTool.Utils
{
    public class StringTemplate
    {
        private string _tempString { get; set; }
        private string DateFormat { get; set; }

        public StringTemplate(string tempString)
        {
            _tempString = tempString;
        }

        public void SetDateFormat(string dateFormat)
        {
            DateFormat = dateFormat;
        }

        /// <summary>
        /// 暂时实现3种数据结构，具体请参考测试种的html文件
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public string Compile(object source)
        {
            string[] pattern = { "{{/each}}", "{{#each", "{{", "}}" };

            // 方法的参数一定要全
            var stringArr = _tempString.Split(pattern, StringSplitOptions.None);
            if (stringArr.Length == 0) return "";
            var stringList = stringArr.ToList();

            var props = source.GetType().GetProperties().ToList();

            var stringBuilder = new StringBuilder();
            for (int i = 0; i < stringList.Count; i++)
            {
                if (stringList[i].Contains("["))
                {
                    // 数组下标格式
                    var attr = stringList[i].Split('[')[0].Trim();
                    var prop = props.Find(prop => prop.Name == attr);
                    // 简单数组暂时只支持这些基本数据类型
                    if (prop.PropertyType == typeof(List<int>) || prop.PropertyType == typeof(List<double>) || prop.PropertyType == typeof(List<decimal>) || prop.PropertyType == typeof(List<string>) || prop.PropertyType == typeof(List<DateTime>))
                    {
                        //提取数字下标
                        char[] charPattern = { '[', ']' };
                        var resArr = stringList[i].Trim().Split(charPattern, StringSplitOptions.RemoveEmptyEntries);
                        int index = Convert.ToInt32(resArr[1]);
                        var list = prop.GetValue(source, null);
                        if ((list as List<string>) != null)
                        {
                            stringBuilder.Append((list as List<string>)[index]);
                        }
                        else if ((list as List<int>) != null)
                        {
                            stringBuilder.Append((list as List<int>)[index]);
                        }
                        else if ((list as List<double>) != null)
                        {
                            stringBuilder.Append((list as List<double>)[index]);
                        }
                        else if ((list as List<decimal>) != null)
                        {
                            stringBuilder.Append((list as List<decimal>)[index]);
                        }
                        else if ((list as List<DateTime>) != null)
                        {
                            stringBuilder.Append((list as List<DateTime>)[index]);
                        }
                    }
                }
                else
                {
                    stringBuilder.Append(stringList[i]);
                }
            }

            //string[] stringSeparators = { "{{/each}}", "{{#each", "{{", "}}" };

            //_tempString.Split(stringSeparators);
            //if (value.GetType() == typeof(string))
            //{
            //    // 字符串执行简单的替换
            //    _tempString.Replace($"${key}$", value.ToString());
            //}
            //else
            //{
            //    // 简单的对象处理;过滤出非空的数据
            //    var props = value.GetType().GetProperties().ToList().FindAll(it => it.GetValue(value, null) != null && !string.IsNullOrEmpty(it.GetValue(value, null).ToString()));
            //    // 替换实体类中所有的字符串属性
            //    foreach (var it in props)
            //    {
            //        if (it.PropertyType == typeof(string))
            //        {
            //            // 字符串属性直接替换
            //            _tempString = _tempString.Replace($"${key}.{it.Name}$", it.GetValue(value, null).ToString());
            //        }
            //        else if (it.PropertyType == typeof(List<string>))
            //        {
            //            // 这里只处理List<string>集合这一种情况，如有需要可以自行扩展
            //            var list = it.GetValue(value, null) as List<string>;
            //            for (int i = 0; i < list.Count; i++)
            //            {
            //                _tempString = _tempString.Replace($"${key}.{it.Name}[{i}]$", list[i]);
            //            }
            //        }
            //    }
            //}
            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return _tempString;
        }
    }
}