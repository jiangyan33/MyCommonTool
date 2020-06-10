using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyCommonTool.Utils
{
    public class StringTemplate
    {
        private string _tempString { get; set; }
        private string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        public StringTemplate(string tempString)
        {
            _tempString = tempString;
        }

        /// <summary>
        /// 设置日期格式
        /// </summary>
        /// <param name="dateFormat"></param>
        public void SetDateFormat(string dateFormat)
        {
            DateFormat = dateFormat;
        }

        /// <summary>
        /// 暂时实现3种数据结构，具体请参考测试中的html文件
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
                    if (prop != null)
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
                                stringBuilder.Append((list as List<DateTime>)[index].ToString(DateFormat));
                            }
                        }
                }
                else if (props.Find(it => it.Name == stringList[i].Trim()) != null)
                {
                    var prop = props.Find(it => it.Name == stringList[i].Trim());
                    // 如果是基本数据类型直接替换
                    if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(decimal))
                    {
                        stringBuilder.Append(prop.GetValue(source, null));
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        stringBuilder.Append(Convert.ToDateTime(prop.GetValue(source, null)).ToString(DateFormat));
                    }
                    else if (prop.PropertyType == typeof(List<int>) || prop.PropertyType == typeof(List<double>) || prop.PropertyType == typeof(List<decimal>) || prop.PropertyType == typeof(List<string>) || prop.PropertyType == typeof(List<DateTime>))
                    {
                        // 基本数据类型的集合 该次循环的次算，外层循环就不需要执行了
                        int count = 0;
                        var list = prop.GetValue(source, null) as IList;
                        for (int j = i + 1, k = 0; j < stringList.Count; j++, k++)
                        {
                            if (stringList[j].Contains("this"))
                            {
                                if (prop.PropertyType == typeof(List<DateTime>))
                                {
                                    stringBuilder.Append(Convert.ToDateTime(list[k]).ToString(DateFormat));
                                }
                                else
                                {
                                    stringBuilder.Append(list[k]);
                                }
                            }
                            else if (props.FindIndex(it => it.Name == stringList[j].Trim()) != -1)
                            {
                                // 碰到下一个特殊值的时候停止
                                i += count;
                                break;
                            }
                            else if (j == stringList.Count - 1)
                            {
                                stringBuilder.Append(stringList[j]);
                                // 已经遍历到最后一个了，直接结束,最后一个需要为非特殊元素
                                return stringBuilder.ToString();
                            }
                            else
                            {
                                stringBuilder.Append(stringList[j]);
                            }
                            count++;
                        }
                    }
                    else
                    {
                        // 对象数据类型的集合
                        int count = 0;
                        var list = prop.GetValue(source, null) as IList;
                        for (int j = i + 1, k = 0; j < stringList.Count; j++, k++)
                        {
                            if (stringList[j].Contains("this."))
                            {
                                var resArr = stringList[j].Split("this.");
                                var value = list[k].GetAttrValue<DateTime>(resArr[1].Trim());
                                if (value == default)
                                {
                                    // 非日期类型
                                    stringBuilder.Append(list[k].GetAttrValue<object>(resArr[1].Trim()));
                                }
                                else
                                {
                                    // 日期类型
                                    stringBuilder.Append(value.ToString(DateFormat));
                                }
                            }
                            else if (props.FindIndex(it => it.Name == stringList[j].Trim()) != -1)
                            {
                                // 碰到下一个特殊值的时候停止
                                i += count;
                                break;
                            }
                            else if (j == stringList.Count - 1)
                            {
                                stringBuilder.Append(stringList[j]);
                                // 已经遍历到最后一个了，直接结束,最后一个需要为非特殊元素
                                return stringBuilder.ToString();
                            }
                            else
                            {
                                stringBuilder.Append(stringList[j]);
                            }
                            count++;
                        }
                    }
                }
                else
                {
                    stringBuilder.Append(stringList[i]);
                }
            }
            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return _tempString;
        }
    }
}