using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace TRWMS_BillApi.Models.Response
{
    public class xml2str
    {
        //对象转成XML
        public static string ObjToXml(object obj)
        {
            using (MemoryStream Stream = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(obj.GetType());
                xml.Serialize(Stream, obj);
                Stream.Position = 0;
                StreamReader sr = new StreamReader(Stream);
                string str = sr.ReadToEnd();
                return str;
            }
        }

        //XML字符串转成对象对象
        //public static T XmlToObj(string xml)
        //{
        //    XmlSerializer xserializer = new XmlSerializer(typeof(T));
        //    using (StringReader sr = new StringReader(xml))
        //    {
        //        return (T)xserializer.Deserialize(sr);
        //    }
        //}
    }
}