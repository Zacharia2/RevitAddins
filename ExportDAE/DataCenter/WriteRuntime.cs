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


        //TODO 我希望它能够在插件的目录下放置。
        private static string DEFAULT_FILENAME = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".xml";
        public static void Save()
        {
            XmlDocument config = new XmlDocument();
            XmlDeclaration xmlDeclaration = config.CreateXmlDeclaration("1.0", "utf-8", "yes");

            XmlElement rootElement = config.CreateElement("Configure");



            XmlElement xml_Element_InsertionPoint = config.CreateElement("InsertionPoint");
            XmlElement xml_Element_SkipSmallerThan = config.CreateElement("SkipSmallerThan");
            XmlElement xml_Element_SkipInteriorDetails = config.CreateElement("SkipInteriorDetails");
            XmlElement xml_Element_CollectTextures = config.CreateElement("CollectTextures");
            XmlElement xml_Element_UnicodeSupport = config.CreateElement("UnicodeSupport");
            XmlElement xml_Element_GeometryOptimization = config.CreateElement("GeometryOptimization");
            XmlElement xml_Element_levelOfDetail = config.CreateElement("levelOfDetail");



            xml_Element_InsertionPoint.InnerText = InsertionPoint.ToString();
            xml_Element_SkipSmallerThan.InnerText = SkipSmallerThan.ToString();
            xml_Element_SkipInteriorDetails.InnerText = SkipInteriorDetails.ToString();
            xml_Element_CollectTextures.InnerText = CollectTextures.ToString();
            xml_Element_UnicodeSupport.InnerText = UnicodeSupport.ToString();
            xml_Element_GeometryOptimization.InnerText = GeometryOptimization.ToString();
            xml_Element_levelOfDetail.InnerText = levelOfDetail.ToString();


            config.AppendChild(xmlDeclaration);
            config.AppendChild(rootElement);
            rootElement.AppendChild(xml_Element_InsertionPoint);
            rootElement.AppendChild(xml_Element_SkipSmallerThan);
            rootElement.AppendChild(xml_Element_SkipInteriorDetails);
            rootElement.AppendChild(xml_Element_CollectTextures);
            rootElement.AppendChild(xml_Element_UnicodeSupport);
            rootElement.AppendChild(xml_Element_GeometryOptimization);
            rootElement.AppendChild(xml_Element_levelOfDetail);

            config.Save(DEFAULT_FILENAME);
 
        }


        public static void Load()
        {
            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(DEFAULT_FILENAME);

                XmlElement rootElement = document.DocumentElement;
                XmlNodeList childNodeList = rootElement.ChildNodes;
                foreach (XmlNode current in childNodeList)
                {

                    switch (current.Name)
                    {
                        case "InsertionPoint":
                            InsertionPoint = int.Parse(current.InnerText);
                            break;

                        case "SkipSmallerThan":
                            SkipSmallerThan = Decimal.Parse(current.InnerText);
                            break;

                        case "SkipInteriorDetails":
                            SkipInteriorDetails = Boolean.Parse(current.InnerText);
                            break;

                        case "CollectTextures":
                            CollectTextures = Boolean.Parse(current.InnerText);
                            break;

                        case "UnicodeSupport":
                            UnicodeSupport = Boolean.Parse(current.InnerText);
                            break;

                        case "GeometryOptimization":
                            GeometryOptimization = Boolean.Parse(current.InnerText);
                            break;

                        case "levelOfDetail":
                            levelOfDetail = int.Parse(current.InnerText);
                            break;
                    }

                   // Console.WriteLine("名字::{0}，，值::{1}", current.Name, current.InnerText);


                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }

}
