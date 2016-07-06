using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace UmbracoXmlEdit
{
    public class XmlEdit
    {
        readonly ILogger _logger;
        readonly IDataTypeService _dataTypeService;
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

        public XmlEdit(ILogger logger, IDataTypeService dataTypeService, IContent content)
        {
            _logger = logger;
            _dataTypeService = dataTypeService;
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

            //SetContentPropertyValue<int>(nameof(IContent.Id), "id");
            //SetContentPropertyValue<Guid>(nameof(IContent.Key), "key");
            SetContentPropertyValue<int>(nameof(IContent.ParentId), "parentID");
            //SetContentPropertyValue<int>(nameof(IContent.Level), "level");
            //SetContentPropertyValue<int>(nameof(IContent.CreatorId), "creatorID");
            SetContentPropertyValue<int>(nameof(IContent.SortOrder), "sortOrder");
            SetContentPropertyValue<DateTime>(nameof(IContent.CreateDate), "createDate");
            //SetContentPropertyValue<DateTime>(nameof(IContent.UpdateDate), "updateDate"); // Can't be updated since it'll be updated when object is saved after this
            //SetContentPropertyValue<string>(nameof(IContent.), "nodeName");
            //SetContentPropertyValue<>(nameof(IContent.), "urlName");
            //SetContentPropertyValue<string>(nameof(IContent.Path), "path");
            //SetContentPropertyValue<>(nameof(IContent.), "nodeType");
            //SetContentPropertyValue<>(nameof(IContent.), "creatorName");
            //SetContentPropertyValue<int>(nameof(IContent.), "writerName");
            //SetContentPropertyValue<int>(nameof(IContent.WriterId), "writerID");
            //SetContentPropertyValue<ITemplate>(nameof(IContent.Template), "template");
            //SetContentPropertyValue<>(nameof(IContent.), "nodeTypeAlias");
        }

        private void SetContentPropertyValue<T>(string propertyName, string attributeName)
        {
            object objectValue = null;

            var attribute = _xml.Attribute(attributeName);
            if(attribute == null)
            {
                // If attribute doesn't exist in passed XML we don't do anything (not trying to set value and don't remove it)
                return;
            }

            string value = attribute.Value;
            if (typeof(T) == typeof(int))
            {
                int intValue;
                if(int.TryParse(value, out intValue))
                {
                    objectValue = intValue;
                }
                else
                {
                    throw new InvalidCastException(""); // TODO: Message + unit test
                }
            }
            else if (typeof(T) == typeof(Guid))
            {
                objectValue = Guid.Parse(value); // TODO: Try/parse
            }
            else if (typeof(T) == typeof(DateTime))
            {
                objectValue = DateTime.Parse(value); // TODO: Try/parse
            }
            else
            {
                // Set value as string
                objectValue = value;
            }

            if(objectValue != null)
            {
                _content.GetType().GetProperty(propertyName).SetValue(_content, objectValue);
            }
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
            for (int i = properties.Count - 1; i >= 0; i--)
            {
                var property = properties[i];
                bool propertyExistsInXml = propertyElements.Any(e => e.Name.LocalName == property.Alias);
                if (!propertyExistsInXml)
                {
                    properties.RemoveAt(i);
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