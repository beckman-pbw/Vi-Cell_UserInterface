using ScoutLanguageResources;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ScoutUtilities.Helper
{
    public class XmlHelper
    {
        public static void CreateFileXml(FileType fileType, string filePath, int listType)
        {
            var xmlPath = AppDomain.CurrentDomain.BaseDirectory + "XMLDB\\FilesXml.xml";
            if (!File.Exists(xmlPath))
            {
                using (var file = File.Create(xmlPath))
                {
                    file.Close();
                    var docs = new XmlDocument();
                    var rootNode = docs.CreateElement("FileList");
                    docs.AppendChild(rootNode);
                    docs.Save(xmlPath);
                }
            }

            var doc = XDocument.Load(xmlPath);
            var type = listType == 1 ? "Carousel" : "Wellplate";
            var mainRoot = doc.Root;
            if (!doc.Descendants("Files").Any())
            {
                var xElement = new XElement("Files");
                mainRoot?.Add(xElement);
            }

            if (mainRoot != null)
            {
                var fileElement = mainRoot.Element("Files");
                if (fileElement == null)
                    return;
                if (!fileElement.Elements(type).Any())
                {
                    var xElement = new XElement(type);
                    xElement.Add(SetValue(fileType, filePath, listType));
                    fileElement?.Add(xElement);
                }
                else
                {
                    var files = mainRoot.Element("Files");
                    if (files != null)
                    {
                        var element = files.Element(type);
                        switch (fileType)
                        {
                            case FileType.Csv:
                                if (!mainRoot.Descendants("Files").Descendants(type).Elements("CSV").Any())
                                {
                                    element?.Add(SetValue(fileType, filePath, listType));
                                }
                                else
                                {
                                    if (mainRoot.Elements("Files").Descendants(type).Elements("CSV").All(a => a.Value != filePath))
                                    {
                                        element?.Add(SetValue(fileType, filePath, listType));
                                    }
                                }

                                break;
                            case FileType.Pdf:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
                        }
                    }
                }
            }

            doc.Save(xmlPath);
        }

        public static XElement SetValue(FileType type, string filePath, int listType)
        {
            XElement childElement = null;
            switch (type)
            {
                case FileType.Csv:
                    childElement = new XElement("CSV");
                    break;
            }

            if (childElement == null)
                return null;
            childElement.Value = filePath;
            return childElement;
        }

        public static List<string> GetXmlByRoot(string type, string root)
        {
            var collections = new List<string>();
            var xmlPath = AppDomain.CurrentDomain.BaseDirectory + "XMLDB\\FilesXml.xml";
            if (!File.Exists(xmlPath))
            {
                return null;
            }

            var doc = XDocument.Load(xmlPath);
            var mainRoot = doc.Root;
            if (mainRoot == null)
                return null;
            if (!mainRoot.Descendants("Files").Elements(type).Any())
                return null;
            var lists = mainRoot.Descendants("Files").Elements(type).ToList();
            lists.ForEach(element =>
            {
                collections.AddRange(element.Elements().Select(itm => itm.Value));
            });
            return collections;
        }

        public static string GetSelectedLanguageForCulture()
        {
            var selectedLanguage = LanguageType.eEnglishUS.ToString();
            var xmlPath = AppDomain.CurrentDomain.BaseDirectory + "XMLDB\\ConfigurationSettings.xml";
            var doc = XDocument.Load(xmlPath);
            var mainRoot = doc.Root;
            if (mainRoot == null)
                return selectedLanguage;
            if (!mainRoot.Descendants("User").Elements("GeneralSettings").Elements("SelectedLanguage").Any())
                return null;
            var lists = mainRoot.Descendants("User").Elements("GeneralSettings").Elements("LanguageID").ToList();
            if (lists.Count.Equals(0))
                lists = mainRoot.Descendants("User").Elements("GeneralSettings").Elements("SelectedLanguage").ToList();
            var element = lists.Select(a => a).FirstOrDefault();

            if (element != null)
            {
                selectedLanguage = element.Value;
            }

            return selectedLanguage;
        }
    }
}
