using System;
using System.Reflection;
using System.Xml;

namespace ExportDAE
{
    internal static class WriteRuntime
    {
        public static int InsertionPoint
        {
            get;
            set;
        }

        public static decimal SkipSmallerThan
        {
            get;
            set;
        }

        public static bool SkipInteriorDetails
        {
            get;
            set;
        }

        public static bool CollectTextures
        {
            get;
            set;
        }

        public static bool UnicodeSupport
        {
            get;
            set;
        }

        public static bool GeometryOptimization
        {
            get;
            set;
        }

        public static int levelOfDetail
        {
            get;
            set;
        }



        private static string DEFAULT_FILENAME = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
        public static void Save()
        {
            XmlDocument config = new XmlDocument();
            XmlDeclaration xmlDeclaration = config.CreateXmlDeclaration("1.0", "utf-8", "yes");
            config.AppendChild(xmlDeclaration);
            XmlElement rootElement = config.CreateElement("Configure");
            config.AppendChild(rootElement);
            XmlElement xml_Runtime_Element = config.CreateElement("Runtime");
            rootElement.AppendChild(xml_Runtime_Element);


            config.Save(DEFAULT_FILENAME);
            
        }


        public static void Load()
        {
            
        }
    }

}
