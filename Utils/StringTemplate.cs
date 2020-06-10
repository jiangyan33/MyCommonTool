using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyCommonTool.Utils
{
    /// <summary>
    /// 简单的字符串模板
    /// </summary>
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
            string[] pattern = { "{{", "}}" };

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
                            var list = prop.GetValue(source, null) as IList;
                            // 区分开来基本数据类型和日期类型
                            stringBuilder.Append(prop.PropertyType == typeof(List<DateTime>) ? Convert.ToDateTime(list[index]).ToString(DateFormat) : list[index]);
                        }
                }
                else if (props.Find(it => it.Name == stringList[i].Trim()) != null)
                {
                    var prop = props.Find(it => it.Name == stringList[i].Trim());
                    // 如果是基本数据类型直接替换
                    if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(string) || prop.PropertyType == typeof(decimal))
                    {
                        stringBuilder.Append(prop.GetValue(source, null));
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        stringBuilder.Append(Convert.ToDateTime(prop.GetValue(source, null)).ToString(DateFormat));
                    }
                }
                else if (stringList[i].Trim().IndexOf("#each") != -1)
                {
                    // 寻找与之对应的结束节点索引
                    var index = stringList.FindIndex(i, it => it == "/each");
                    var resArr = stringList[i].Trim().Split(" ");
                    var prop = props.Find(it => it.Name == resArr[1].Trim());
                    if (prop != null)
                    {
                        if (prop.PropertyType == typeof(List<int>) || prop.PropertyType == typeof(List<double>) || prop.PropertyType == typeof(List<decimal>) || prop.PropertyType == typeof(List<string>) || prop.PropertyType == typeof(List<DateTime>))
                        {
                            // 执行循环遍历
                            var list = prop.GetValue(source, null) as IList;
                            for (int j = 0; j < list.Count; j++)
                            {
                                // 需要循环的数据为从i到index之间的数据
                                for (int k = i + 1; k < index; k++)
                                {
                                    if (stringList[k].Contains("this"))
                                    {
                                        stringBuilder.Append(prop.PropertyType == typeof(List<DateTime>) ? Convert.ToDateTime(list[j]).ToString(DateFormat) : list[j]);
                                    }
                                    else
                                    {
                                        stringBuilder.Append(stringList[k]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 对象遍历 执行循环遍历
                            var list = prop.GetValue(source, null) as IList;
                            for (int j = 0; j < list.Count; j++)
                            {
                                // 需要循环的数据为从i到index之间的数据
                                for (int k = i + 1; k < index; k++)
                                {
                                    if (stringList[k].Contains("this."))
                                    {
                                        var resArrTemp = stringList[k].Split("this.");
                                        var value = list[j].GetAttrValue<DateTime>(resArrTemp[1]);
                                        stringBuilder.Append(value == default ? list[j].GetAttrValue<object>(resArrTemp[1]) : value.ToString(DateFormat));
                                    }
                                    else
                                    {
                                        stringBuilder.Append(stringList[k]);
                                    }
                                }
                            }
                        }
                        i = index;
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