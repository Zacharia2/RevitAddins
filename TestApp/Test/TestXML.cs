using System;
using System.Xml;

namespace ExportDAE.Test
{
    class TestXML
    {
        static void Main(string[] args)
        {
            //实现xml的写入
            //1、在内存中构建Dom对象
            XmlDocument xmlDoc = new XmlDocument();
            //增加文档说明
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            xmlDoc.AppendChild(xmlDeclaration);
            //增加根元素
            //  创建根元素
            XmlElement rootElement = xmlDoc.CreateElement("school");
            xmlDoc.AppendChild(rootElement);
            //3、增加子元素，接下来添加的子元素增加到rootElement节点下
            XmlElement xmlClassElement = xmlDoc.CreateElement("class");
            // 为class元素添加id属性
            XmlAttribute attr = xmlDoc.CreateAttribute("id");
            attr.Value = "x01";
            xmlClassElement.Attributes.Append(attr);
            rootElement.AppendChild(xmlClassElement);
            //4、为class创建student节点。
            XmlElement xmlStudentElement = xmlDoc.CreateElement("student");
            // 为student元素添加sid 属性.
            XmlAttribute studentAttr = xmlDoc.CreateAttribute("sid");
            studentAttr.Value = "s011";
            xmlStudentElement.Attributes.Append(studentAttr);
            xmlClassElement.AppendChild(xmlStudentElement);
            //student中增加name节点。
            XmlElement xmlNameElement = xmlDoc.CreateElement("name");
            xmlNameElement.InnerText = "天";
            xmlStudentElement.AppendChild(xmlNameElement);


            //2、将该Dom对象写入xml文件中
            xmlDoc.Save("school.xml");
            Console.WriteLine("ok");
        }
    }
}
