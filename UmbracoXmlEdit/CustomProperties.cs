using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace UmbracoXmlEdit
{
    public class CustomProperties
    {
        readonly ILogger _logger;
        readonly IDataTypeService _dataTypeService;
        readonly IFileService _fileService;

        IContent _content;
        List<XElement> _propertyElements;

        public CustomProperties(ILogger logger, IDataTypeService dataTypeService, IFileService fileService, IContent content, XElement xml)
        {
            _logger = logger;
            _dataTypeService = dataTypeService;
            _fileService = fileService;
            _content = content;
            _propertyElements = xml.Elements().ToList();
        }

        /// <summary>
        /// Update value for each custom property in PropertyCollection and remove properties that no longer exists in XML
        /// </summary>
        public IContent Process()
        {
            var properties = _content.Properties;

            foreach (var propertyElement in _propertyElements)
            {
                string propertyTypeAlias = propertyElement.Name.LocalName;
                if (_content.HasProperty(propertyTypeAlias))
                {
                    var propertyType = properties[propertyTypeAlias].PropertyType;
                    object value = GetValue(propertyElement, propertyType);
                    _content.SetValue(propertyTypeAlias, value);
                }
                else
                {
                    // TODO: Show in UI that property couldn't be added
                    _logger.Info(GetType(), string.Format("Couldn't add property since it's not a valid PropertyType. PropertyType.Alias: '{0}'", propertyTypeAlias));
                }
            }

            // Remove properties existing on IContent but not in saved XML
            RemoveProperties();

            return _content;
        }

        /// <summary>
        /// Remove properties that is removed from XML but exist in property-list for IContent
        /// </summary>
        PropertyCollection RemoveProperties()
        {
            var properties = _content.Properties;
            if (properties != null)
            {
                for (int i = properties.Count - 1; i >= 0; i--)
                {
                    var property = properties[i];
                    bool propertyExistsInXml = _propertyElements.Any(e => e.Name.LocalName == property.Alias);
                    if (!propertyExistsInXml)
                    {
                        properties.RemoveAt(i);
                    }
                }
            }

            return properties;
        }

        public object GetValue(XElement propertyElement, PropertyType propertyType)
        {
            string propertyElementValue = propertyElement.Value;

            object value = null;
            var dataTypeDefinition = _dataTypeService.GetDataTypeDefinitionById(propertyType.DataTypeDefinitionId).DatabaseType;
            switch (dataTypeDefinition)
            {
                case DataTypeDatabaseType.Integer:
                    int typedValue;
                    if (int.TryParse(propertyElementValue, out typedValue))
                    {
                        value = typedValue;
                    }
                    break;
                default:
                    value = propertyElementValue;
                    break;
            }

            return value;
        }
    }
}