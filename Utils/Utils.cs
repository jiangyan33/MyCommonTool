using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyCommonTool.Utils
{
    public static class Utils
    {
        /// <summary>
        /// 文本文件读写公共方法
        /// </summary>
        /// <param name="path"></param>
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
        /// </summary>
        /// <typeparam name="T">返回值的数据类型</typeparam>
        /// <param name="instance">实体类实例</param>
        /// <param name="AttrName">属性名称</param>
        /// <returns></returns>
        public static T GetAttrValue<T>(this object instance, string AttrName)
        {
            var prop = instance.GetType().GetProperties().ToList().Find(it => it.Name.ToLower() == AttrName.ToLower());
            try
            {
                return (T)prop.GetValue(instance, null);
            }
            catch
            {
                return default;
            };
        }

        /// <summary>
        /// 将DataTable序列化成对象，如果DataTable中和对象中存在相同的字段名，就会赋值，否则不会赋值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable">           </param>
        /// <param name="ignoreTableUnderline">是否忽略数据表中的下划线</param>
        /// <returns></returns>
        public static List<T> SerializeToObject<T>(this System.Data.DataTable dataTable, bool ignoreTableUnderline = true) where T : new()
        {
            if (dataTable == null)
                return null;

            var type = typeof(T);
            var props = type.GetProperties();
            // 如果数据表字段有下滑线会去掉
            var columns = dataTable.Columns.Cast<System.Data.DataColumn>().Select(c =>
                new
                {
                    ProptyName = ignoreTableUnderline ?
                        c.ColumnName.Replace("_", "").ToLower() :
                        c.ColumnName.ToLower(),
                    c.ColumnName
                }
            );
            if (columns.Count() == 0)
                return new List<T>();

            if (dataTable == null || dataTable.Rows.Count == 0)
                return new List<T>();

            var getDefaultValue = new Func<Type, object>(propType =>
            {
                // 如果是可空类型，直接返回null
                if (Nullable.GetUnderlyingType(propType) != null)
                    return null;
                // 如果时非可空类型 直接获取默认值
                else
                {
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

            var ret = new List<T>();
            foreach (System.Data.DataRow row in dataTable.Rows)
            {
                var obj = new T();
                foreach (var prop in props)
                {
                    var c = columns.Where(x => x.ProptyName.ToLower() == prop.Name.ToLower()).FirstOrDefault();
                    // 如果对象属性在DataTable中存在相应的列，就赋值
                    if (c != null)
                    {
                        try
                        {
                            // 类型相同直接赋值
                            if (row[c.ColumnName].GetType().FullName == prop.PropertyType.FullName)
                                prop.SetValue(obj, row[c.ColumnName], null);
                            // 类型不同，将table的类型转换为属性的类型，转换失败时赋值为当前类型的默认值
                            else
                                prop.SetValue(obj, convertTypeToProperty(prop.PropertyType, row[c.ColumnName]), null);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
                ret.Add(obj);
            }

            return ret;
        }
    }
}