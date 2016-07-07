using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace UmbracoXmlEdit
{
    public class XmlEdit
    {
        readonly ILogger _logger;
        readonly IDataTypeService _dataTypeService;
        readonly IFileService _fileService;
        IContent _content;
        XElement _xml;

        /// <summary>
        /// Wheter update of XML was successfully or not
        /// </summary>
        public bool Successful { get; private set; }

        public IContent Content
        {
            get
            {
                return _content;
            }
        }

        public XmlEdit(ILogger logger, IDataTypeService dataTypeService, IFileService fileService, IContent content)
        {
            _logger = logger;
            _dataTypeService = dataTypeService;
            _fileService = fileService;
            _content = content;
        }

        /// <summary>
        /// Update content object from XML
        /// </summary>
        /// <param name="content">Content/page to update</param>
        /// <param name="xml">Updated XML</param>
        public void UpdateContentFromXml(string xml)
        {
            if (TryParse(xml))
            {
                var propertyElements = _xml.Elements().ToList();
                var properties = _content.Properties;

                // UpdateContentPropertyValues
                UpdateContentPropertyValues();

                // Update value for custom properties
                UpdateCustomPropertyValues(propertyElements, properties);

                // Remove properties existing on IContent but not in saved XML
                _content.Properties = RemoveMissingProperties(propertyElements, properties);

                // Update successful
                Successful = true;
            }
        }

        private bool TryParse(string xml)
        {
            try
            {
                _xml = XElement.Parse(xml);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateContentPropertyValues()
        {
            // TODO: Update all possible properties on IContent from node-attributes
            // TODO: Only set if values is different

            SetContentPropertyValue<int>(nameof(IContent.Id), "id");
            SetContentPropertyValue<Guid>(nameof(IContent.Key), "key");
            SetContentPropertyValue<int>(nameof(IContent.ParentId), "parentID");
            SetContentPropertyValue<int>(nameof(IContent.Level), "level");
            SetContentPropertyValue<int>(nameof(IContent.CreatorId), "creatorID");
            SetContentPropertyValue<int>(nameof(IContent.SortOrder), "sortOrder");
            SetContentPropertyValue<DateTime>(nameof(IContent.CreateDate), "createDate");
            //SetContentPropertyValue<DateTime>(nameof(IContent.UpdateDate), "updateDate"); // Can't be updated since it'll be updated when object is saved after this
            SetContentPropertyValue<string>(nameof(IContent.Name), "nodeName");
            //SetContentPropertyValue<string>(nameof(IContent.), "urlName");
            SetContentPropertyValue<string>(nameof(IContent.Path), "path");
            //SetContentPropertyValue<>(nameof(IContent.), "nodeType");
            //SetContentPropertyValue<>(nameof(IContent.), "creatorName");
            //SetContentPropertyValue<int>(nameof(IContent.), "writerName");
            SetContentPropertyValue<int>(nameof(IContent.WriterId), "writerID");
            //SetContentPropertyValue<>(nameof(IContent.), "nodeTypeAlias");

            // Template
            int templateId;
            if (int.TryParse(GetXmlAttributeValue("template"), out templateId))
            {
                var template = _fileService.GetTemplate(templateId);
                SetPropertyValue(nameof(IContent.Template), template);
            }
        }

        string GetXmlAttributeValue(string attributeName)
        {
            var attribute = _xml.Attribute(attributeName);
            if (attribute != null)
            {
                return attribute.Value;
                
            }
            return null;
        }

        object ParseValue<PropertyType>(string value)
        {
            bool validParsed = true;
            object objectValue = null;
            string valueStr = value.ToString();

            if (typeof(PropertyType) == typeof(int))
            {
                int intValue;
                if (int.TryParse(valueStr, out intValue))
                {
                    objectValue = intValue;
                }
                else
                {
                    validParsed = false;
                }
            }
            else if (typeof(PropertyType) == typeof(Guid))
            {
                objectValue = Guid.Parse(valueStr); // TODO: Try/parse 
            }
            else if (typeof(PropertyType) == typeof(DateTime))
            {
                objectValue = DateTime.Parse(valueStr); // TODO: Try/parse 
            }
            else
            {
                // Set value as string
                objectValue = valueStr;
            }

            if (!validParsed)
            {
                throw new InvalidCastException(""); // TODO: Message + unit test
            }

            return objectValue;
        }

        private void SetPropertyValue(string propertyName, object value)
        {
            if (value != null)
            {
                _content.GetType().GetProperty(propertyName).SetValue(_content, value);
            }
        }

        private void SetContentPropertyValue<PropertyType>(string propertyName, string attributeName)
        {
            string attributeValue = GetXmlAttributeValue(attributeName);
            if(attributeValue == null)
            {
                // If attribute doesn't exist or value is null in passed XML we don't do anything (not trying to set value and don't remove it)
                return;
            }

            var value = ParseValue<PropertyType>(attributeValue);
            SetPropertyValue(propertyName, value);
        }

        /// <summary>
        /// Update value for each custom property
        /// </summary>
        private void UpdateCustomPropertyValues(List<XElement> propertyElements, PropertyCollection properties)
        {
            foreach (var propertyElement in propertyElements)
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
        }

        /// <summary>
        /// Remove properties that is removed from XML but exist in property-list for IContent
        /// </summary>
        private PropertyCollection RemoveMissingProperties(List<XElement> propertyElements, PropertyCollection properties)
        {
            if (properties != null)
            {
                for (int i = properties.Count - 1; i >= 0; i--)
                {
                    var property = properties[i];
                    bool propertyExistsInXml = propertyElements.Any(e => e.Name.LocalName == property.Alias);
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
                    if(int.TryParse(propertyElementValue, out typedValue))
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