using log4net;
using ScoutDataAccessLayer.IDAL;
using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.UIConfiguration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ScoutDataAccessLayer.DAL
{
    public class XMLDataAccess : IDataAccess
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private XDocument masterDataXMLDoc;

        private XDocument configurationDataXMLDoc;

        private string configurationDataXmlFilePath;

        private string masterDataXmlFilePath;

        private string masterDataXmlFilePathDAL;

        private string configurationDataXmlFilePathDAL;

        #region Singleton

        private static object syncRoot = new Object();

        private static XMLDataAccess _instance;

        public static XMLDataAccess Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new XMLDataAccess();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Constructor

        // Loads master data and configuration data xml files based on the file location in config file.
        private XMLDataAccess()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var masterXmlFilePath = UISettings.MasterDataXmlFilePath;
            var configurationXmlFilePath = UISettings.XmlDbConfigurationXmlFilePath;
            masterDataXmlFilePath = basePath + masterXmlFilePath;
            masterDataXmlFilePathDAL = basePath + masterXmlFilePath;
            if (File.Exists(masterDataXmlFilePath))
            {
                masterDataXMLDoc = XDocument.Load(masterDataXmlFilePath);
            }
            else if (File.Exists(masterDataXmlFilePathDAL))
            {
                File.Copy(masterDataXmlFilePathDAL, masterDataXmlFilePath);
                masterDataXMLDoc = XDocument.Load(masterDataXmlFilePath);
            }
            else
            {
                masterDataXMLDoc = new XDocument(new XElement("MasterData"));
            }

            configurationDataXmlFilePath = basePath + configurationXmlFilePath;
            configurationDataXmlFilePathDAL = basePath + configurationXmlFilePath + ".template";

            if (File.Exists(configurationDataXmlFilePath))
            {
                configurationDataXMLDoc = XDocument.Load(configurationDataXmlFilePath);
            }
            else if (File.Exists(configurationDataXmlFilePathDAL))
            {
                File.Copy(configurationDataXmlFilePathDAL, configurationDataXmlFilePath);
                configurationDataXMLDoc = XDocument.Load(configurationDataXmlFilePath);
            }
            else
            {
                configurationDataXMLDoc = new XDocument(new XElement("Users"));
            }
        }

        #endregion

        #region Read the configuration xml file

        // Read the configuration file and parse xml into object of expected generic type.
        // if settingsName is null or empty, it returns all the settings object of specified user ID.
        public T ReadConfigurationData<T>(string userId, string settingsName, out bool userFound, bool removeDomainTextFromSettingsName = false)
        {
            userFound = false;
            var settingsNameMod = settingsName;
            if (removeDomainTextFromSettingsName && !string.IsNullOrEmpty(settingsNameMod))
            {
                settingsNameMod = settingsNameMod.Replace("Domain", string.Empty);
            }

            var outputObject = GetDefaultObject<T>();
            if (configurationDataXMLDoc.Root == null)
                return outputObject;

            var userElement = configurationDataXMLDoc.Root
                .Elements("User").SingleOrDefault(user => userId != null && ((string)user.Attribute("UserId")).Equals(userId));
            if(userElement == null)
            {
#if DEBUG
                Log.Warn(string.Format("User id {0} does not exist", userId));
#endif
                return outputObject;
            }

            userFound = true;
            var type = typeof(T);

            if (string.IsNullOrEmpty(settingsNameMod))
            {
                foreach (var element in userElement.Elements())
                {
                    var currentProperty = type.GetProperties().Single(property =>
                        property.Name.IndexOf(element.Name.ToString(), StringComparison.Ordinal) == 0);
                    if (currentProperty != null)
                    {
                        var propertyName = currentProperty.Name;
                        var propertyType = currentProperty.PropertyType;

                        Activator.CreateInstance(propertyType);

                        if (outputObject != null)
                            outputObject.GetType().GetProperty(propertyName)?.SetValue(outputObject,
                                CreateObjectFromXElementWrapper<dynamic>(element));
                    }
                }
            }
            else
            {
                if (userElement != null)
                    return CreateObjectFromXElement<T>(userElement.Elements(settingsNameMod).Single());
            }

            return outputObject;
        }

        #endregion

        #region Read the master xml file

        // Read the master xml file and parse xml into object of expected generic type.
        public T ReadMasterData<T>()
        {
            var elementToRead = default(XElement);
            var outputObject = GetDefaultObject<T>();
            if (outputObject != null)
            {
                var type = outputObject.GetType();
                string typeName = string.Empty;
                if (type.Name.IndexOf("List", StringComparison.Ordinal) == 0)
                {
                    IList objectInList = (IList) CastToDynamicAndToList(outputObject);

                    objectInList.Clear();

                    Type elementType = objectInList.GetType().GetGenericArguments()[0];

                    typeName = elementType.Name;
                }

                elementToRead = masterDataXMLDoc?.Root?.Descendants(typeName).FirstOrDefault()?.Parent ?? elementToRead;
            }

            return CreateObjectFromXElement<T>(elementToRead);
        }

        #endregion

        #region Write to configuration xml file

        public void WriteToConfigurationXML<T>(T inputObject, string userId, DateTime? dateTime)
            where T : class
        {
            if (configurationDataXMLDoc?.Root == null)
                return;

            // Check if new User fill the xml with default values and update.
            if (!configurationDataXMLDoc.Root.Descendants("User")
                .Where(user => ((string) user.Attribute("UserId")).Equals(userId)).Any())
            {
                XElement defaultSettings = new XElement(configurationDataXMLDoc.Root.Descendants("User")
                    .Where(user => ((string) user.Attribute("UserId")).Equals("DefaultSettings")).Single());
                defaultSettings.SetAttributeValue("UserId", userId);
                defaultSettings.SetAttributeValue("CreationDate", Misc.ConvertToCustomDateFormat((dateTime.HasValue ? dateTime.Value : DateTime.Now), DateTimeFormat));
                configurationDataXMLDoc.Root.Add(defaultSettings);
            }
            else
            {
                var existingUser = configurationDataXMLDoc.Root.Descendants("User")
                    .Where(user => ((string) user.Attribute("UserId")).Equals(userId)).Single();
                existingUser.SetAttributeValue("UserId", userId);
                if (dateTime.HasValue)
                    existingUser.SetAttributeValue("CreationDate", Misc.ConvertToCustomDateFormat(dateTime.Value, DateTimeFormat));
            }

            //Check if Settings already exist?
            if (inputObject != null)
            {
                var temp = inputObject.GetType().Name.Replace("Domain", "");

                if (configurationDataXMLDoc.Root
                        .Descendants(temp)
                        .Count(x => x.Parent != null && ((string) x.Parent.Attribute("UserId")).Equals(userId)) == 1)
                {
                    configurationDataXMLDoc.Root.Descendants(temp)
                        .Where(x => x.Parent != null && ((string) x.Parent.Attribute("UserId")).Equals(userId)).First()
                        .ReplaceWith(CreateXElement(inputObject));
                }
                else if (configurationDataXMLDoc.Root.Descendants(inputObject.GetType().Name).Where(x =>
                             x.Parent != null && ((string) x.Parent.Attribute("UserId")).Equals(userId)).Count() == 1)
                {
                    configurationDataXMLDoc.Root.Descendants(inputObject.GetType().Name)
                        .Where(x => x.Parent != null && ((string) x.Parent.Attribute("UserId")).Equals(userId)).First()
                        .ReplaceWith(CreateXElementWithDomain(inputObject));
                }
                else
                {
                    configurationDataXMLDoc.Root.Descendants(temp)
                        .Where(x => x.Parent != null && ((string) x.Parent.Attribute("UserId")).Equals(userId))
                        .SingleOrDefault()?.Add(CreateXElement(inputObject));
                }
            }

            configurationDataXMLDoc.Save(configurationDataXmlFilePath);
        }

        #endregion

        #region Private methods to read xml

        private T CreateObjectFromXElementWrapper<T>(XElement element)
        {
            return (T) CreateObjectFromXElement<T>(element);
        }

        private T CreateObjectFromXElement<T>(XElement xElement)
        {
            var outputObject = GetDefaultObject<T>();
            if (outputObject == null)
            {
                return default(T);
            }
            var type = outputObject.GetType();
            if (type.Name.IndexOf("List", StringComparison.Ordinal) == 0)
            {
                return RecursiveCreateObjectFromXElement(xElement, outputObject, type);
            }
            foreach (var property in type.GetProperties())
            {
                if (xElement.Element(property.Name) != null)
                {
                    var propertyValue = RecursiveCreateObjectFromXElement(xElement.Element(property.Name),
                        outputObject.GetType().GetProperty(property.Name)?.GetValue(outputObject),
                        property.PropertyType);
                    if (null == propertyValue)
                        continue;
                    outputObject.GetType().GetProperty(property.Name)?.SetValue(outputObject,
                        propertyValue);
                }
            }
            return outputObject;
        }

        private T RecursiveCreateObjectFromXElement<T>(XElement xElement, T outputObject, Type outputElementType)
        {
            if (outputObject == null)
            {
                outputObject = (T) GetDefaultValue(outputElementType);
                if (outputObject == null)
                {
                    outputObject = (T) Activator.CreateInstance(outputElementType);
                }
            }

            Type type = outputObject.GetType();

            if (IsPrimitiveType(type))
            {
                if (xElement == null)
                {
                    throw new NullReferenceException("xElement is null for " + type.FullName);
                }
                if (string.IsNullOrEmpty(xElement.Value))
                {
                    outputObject = (T) Convert.ChangeType(GetDefaultValue(type), type, CultureInfo.InvariantCulture);
                }
                else
                {
                    switch (type.Name)
                    {
                        case "LanguageType":
                            outputObject =
                                (T)Enum.Parse(typeof(LanguageType), xElement.Value);
                            break;
                        case "ResultParameter":
                            outputObject =
                                (T)Enum.Parse(typeof(ResultParameter), xElement.Value);
                            break;
                        case "DateTime":
                            var dt = DateTime.ParseExact(xElement.Value, DateTimeFormat, CultureInfo.InvariantCulture);
                            outputObject =
                                (T)Convert.ChangeType(dt, type, CultureInfo.InvariantCulture);
                            break;
                        case "SamplePostWash":
							// When upgrading from 1.3 to 1.4 (offline and on instrument) the "XMLDB/ConfigurationSettings.xml" file
							//	is not upgraded since this file is modified whenever the a user's RunOptions are modified.  This
							//	is important to know because in v1.4 the type "eSamplePostWash" was changed to "SamplePostWash".
							// This caused an exception to be thrown because the line noted below failed to correctly parse the wash type.

							// Check for the "eSamplePostWash" data type from v1.2/v1.3 and convert it to the v1.4 data type.
							if (xElement.Value.Substring(0,1) == "e")
							{
								xElement.Value = xElement.Value.Substring(1);
							}

							// This threw an exception here when upgrading from v1.3 to v1.4 until the above check was added.
							outputObject = (T)Enum.Parse(typeof(SamplePostWash), xElement.Value);
                            break;
                        default:
                            outputObject =
                                (T)Convert.ChangeType(xElement.Value, type, CultureInfo.InvariantCulture);
                            break;
                    }
                }
            }
            else if (type.Name.IndexOf("List", StringComparison.Ordinal) == 0)
            {
                IList objectInList = (IList) CastToDynamicAndToList(outputObject);
                objectInList.Clear();

                Type elementType = objectInList.GetType().GetGenericArguments()[0];
                if (xElement != null)
                {
                    foreach (var elements in xElement.Descendants(elementType.Name))
                    {
                        var objectOfElementType = Activator.CreateInstance(elementType);
                        objectInList.Add(RecursiveCreateObjectFromXElement(elements, objectOfElementType, elementType));
                    }

                    outputObject = (T) Convert.ChangeType(objectInList, type, CultureInfo.InvariantCulture);
                }
            }
            else
            {
                foreach (var property in type.GetProperties())
                {
                    if (xElement.Element(property.Name) == null)
                    {
                        throw new NullReferenceException("Property name: " + property.Name + " not found");
                    }
                    var value = RecursiveCreateObjectFromXElement(xElement.Element(property.Name),
                        outputObject.GetType().GetProperty(property.Name)?.GetValue(outputObject),
                        property.PropertyType);
                    outputObject.GetType().GetProperty(property.Name)?.SetValue(outputObject, value);
                }
            }

            return outputObject;
        }

        #endregion

        #region Private methods to write into xml

        private XElement CreateXElement<T>(T inputObject)
        {
            Type type = inputObject.GetType();

            XElement xElement = new XElement(type.Name.Replace("Domain", ""));

            foreach (var property in type.GetProperties())
            {
                xElement.Add(RecursiveGetXElement(
                    inputObject.GetType().GetProperty(property.Name)?.GetValue(inputObject),
                    new XElement(property.Name.Replace("Domain", ""))));
            }

            return xElement;
        }

        private XElement CreateXElementWithDomain<T>(T inputObject)
        {
            Type type = inputObject.GetType();

            XElement xElement = new XElement(type.Name);

            foreach (var property in type.GetProperties())
            {
                xElement.Add(RecursiveGetXElement(
                    inputObject.GetType().GetProperty(property.Name)?.GetValue(inputObject),
                    new XElement(property.Name.Replace("Domain", ""))));
            }

            return xElement;
        }

        private XElement RecursiveGetXElement<T>(T inputObject, XElement xElement) where T : class
        {
            if (inputObject != null)
            {
                Type type = inputObject.GetType();

                if (IsPrimitiveType(type))
                {
                    xElement.Value = inputObject.ToString();
                }
                else if (type.Name.IndexOf("List", StringComparison.Ordinal) == 0)
                {
                    IEnumerable objectInList = (IEnumerable) CastToDynamicAndToList(inputObject);
                    foreach (var item in objectInList)
                    {
                        xElement.Add(RecursiveGetXElement(item, new XElement(item.GetType().Name)));
                    }
                }
                else
                {
                    foreach (var property in type.GetProperties())
                    {
                        xElement.Add(RecursiveGetXElement(
                            inputObject.GetType().GetProperty(property.Name)?.GetValue(inputObject),
                            new XElement(property.Name.Replace("Domain", ""))));
                    }
                }
            }

            return xElement;
        }

        #endregion

        #region Helper methods

        private T GetDefaultObject<T>()
        {
            T outputObject = default(T);
            if (outputObject == null)
            {
                outputObject = (T) GetDefaultValue(typeof(T));

                if (outputObject == null)
                {
                    outputObject = (T) Activator.CreateInstance(typeof(T));
                }
            }

            return outputObject;
        }

        private static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            else if (type.ToString().Contains("String"))
            {
                return string.Empty;
            }
            else
                return null;
        }

        private static bool IsPrimitiveType(Type propertyType)
        {
            return (propertyType == typeof(object) || Type.GetTypeCode(propertyType) != TypeCode.Object ||
                    propertyType == typeof(Guid)) || Nullable.GetUnderlyingType(propertyType) != null;
        }

        private object CastToDynamicAndToList(dynamic input)
        {
            return CastToList(input);
        }

        private IList<T> CastToList<T>(IEnumerable<T> input)
        {
            return input.ToList();
        }

        private string DateTimeFormat
        {
            get { return "MM/dd/yyyy HH:mm:ss"; }
        }

        #endregion
    }
}
