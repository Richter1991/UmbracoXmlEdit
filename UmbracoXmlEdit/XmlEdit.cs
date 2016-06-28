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
        IContent _content;

        public XmlEdit(ILogger logger, IDataTypeService dataTypeService)
        {
            _logger = logger;
            _dataTypeService = dataTypeService;

        }

        public IContent UpdateXml(IContent content, string xml)
        {
            _content = content;

            var rootElement = XElement.Parse(xml);
            var propertyElements = rootElement.Elements().ToList();
            var properties = _content.Properties;

            // UpdateContentPropertyValues
            UpdateContentPropertyValues(rootElement);

            // UpdateExistingPropertyValues
            UpdateExistingPropertyValues(propertyElements, properties);

            // Remove properties existing on IContent but not in saved XML
            _content.Properties = RemoveMissingProperties(propertyElements, properties);

            return _content;
        }

        private void UpdateContentPropertyValues(XElement rootElement)
        {
            // TODO: Update all possible properties on IContent from node-attributes

            //SetContentPropertyValue<int>(rootElement, nameof(IContent.Id), "id");
            //SetContentPropertyValue<Guid>(rootElement, nameof(IContent.Key), "key");
            SetContentPropertyValue<int>(rootElement, nameof(IContent.ParentId), "parentID");
            //SetContentPropertyValue<int>(rootElement, nameof(IContent.Level), "level");
            //SetContentPropertyValue<int>(rootElement, nameof(IContent.CreatorId), "creatorID");
            SetContentPropertyValue<int>(rootElement, nameof(IContent.SortOrder), "sortOrder");
            SetContentPropertyValue<DateTime>(rootElement, nameof(IContent.CreateDate), "createDate");
            //SetContentPropertyValue<DateTime>(rootElement, nameof(IContent.UpdateDate), "updateDate"); // Can't be updated since it'll be updated when object is saved after this
            //SetContentPropertyValue<string>(rootElement, nameof(IContent.), "nodeName");
            //SetContentPropertyValue<>(rootElement, nameof(IContent.), "urlName");
            //SetContentPropertyValue<string>(rootElement, nameof(IContent.Path), "path");
            //SetContentPropertyValue<>(rootElement, nameof(IContent.), "nodeType");
            //SetContentPropertyValue<>(rootElement, nameof(IContent.), "creatorName");
            //SetContentPropertyValue<int>(rootElement, nameof(IContent.), "writerName");
            //SetContentPropertyValue<int>(rootElement, nameof(IContent.WriterId), "writerID");
            //SetContentPropertyValue<ITemplate>(rootElement, nameof(IContent.Template), "template");
            //SetContentPropertyValue<>(rootElement, nameof(IContent.), "nodeTypeAlias");
        }

        private void SetContentPropertyValue<T>(XElement rootElement, string propertyName, string attribute)
        {
            object objectValue = null;

            string value = rootElement.Attribute(attribute).Value;
            if (typeof(T) == typeof(int))
            {
                int intValue;
                if(int.TryParse(value, out intValue))
                {
                    objectValue = intValue;
                }
                else
                {
                    throw new InvalidCastException(""); // TODO
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
        /// Update value for each existing property
        /// </summary>
        private void UpdateExistingPropertyValues(List<XElement> propertyElements, PropertyCollection properties)
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