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
            if (prop == null) return default;
            return (T)prop.GetValue(instance, null);
        }
    }
}