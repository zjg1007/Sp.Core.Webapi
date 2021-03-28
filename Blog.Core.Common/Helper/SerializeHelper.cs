using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Blog.Core.Common
{
    public class SerializeHelper
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static byte[] Serialize(object item)
        {
            var jsonString = JsonConvert.SerializeObject(item);

            return Encoding.UTF8.GetBytes(jsonString);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TEntity Deserialize<TEntity>(byte[] value)
        {
            if (value == null)
            {
                return default(TEntity);
            }
            var jsonString = Encoding.UTF8.GetString(value);
            return JsonConvert.DeserializeObject<TEntity>(jsonString);
        }
        /// <summary>
        /// 反序列化XML为实体类
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <summary>
        /// 将XML转换成实体对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="strXML">XML</param>
        public static TEntity DeserializeToXml<TEntity>(string value)
        {
            if (value == null)
            {
                return default(TEntity);
            }
            return JsonConvert.DeserializeObject<TEntity>(value);
        }
        public static TEntity DESerializerStringToEntity<TEntity>(string strXML) where TEntity : class
        {
            try
            {
                using (StringReader sr = new StringReader(strXML))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TEntity));
                    return serializer.Deserialize(sr) as TEntity;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
