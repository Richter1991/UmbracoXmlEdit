using System;
using System.Xml.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using UmbracoXmlEdit.Helpers;

namespace UmbracoXmlEdit
{
    class DefaultContentProperties
    {
        readonly ILogger _logger;
        readonly IFileService _fileService;

        IContent _content;
        XElement _xml;

        public DefaultContentProperties(ILogger logger, IFileService fileService, IContent content, XElement xml)
        {
            _logger = logger;
            _fileService = fileService;
            _content = content;
            _xml = xml;
        }

        /// <summary>
        /// Update value of IContent default-properties from XML-attributes
        /// </summary>
        public IContent SetProperties()
        {
            // TODO: Only set if values is different than stored value
            SetPropertyValueFromAttribute<int>(nameof(IContent.Id), "id");
            SetPropertyValueFromAttribute<Guid>(nameof(IContent.Key), "key");
            SetPropertyValueFromAttribute<int>(nameof(IContent.ParentId), "parentID");
            SetPropertyValueFromAttribute<int>(nameof(IContent.Level), "level");
            SetPropertyValueFromAttribute<int>(nameof(IContent.CreatorId), "creatorID");
            SetPropertyValueFromAttribute<int>(nameof(IContent.SortOrder), "sortOrder");
            SetPropertyValueFromAttribute<DateTime>(nameof(IContent.CreateDate), "createDate");
            SetPropertyValueFromAttribute<string>(nameof(IContent.Name), "nodeName");
            SetPropertyValueFromAttribute<string>(nameof(IContent.Path), "path");
            SetPropertyValueFromAttribute<int>(nameof(IContent.ContentTypeId), "nodeType");
            //SetContentPropertyValue<string>(nameof(IContent.), "urlName"); // TODO

            // Template
            int templateId;
            if (int.TryParse(GetAttributeValue("template"), out templateId))
            {
                var template = _fileService.GetTemplate(templateId);
                SetPropertyValue(nameof(IContent.Template), template);
            }

            return _content;
        }

        /// <summary>
        /// Set value for a specific property
        /// </summary>
        /// <param name="propertyName">Name of property on IContent</param>
        /// <param name="value">Typed value to set</param>
        void SetPropertyValue(string propertyName, object value)
        {
            if (value != null)
            {
                _content.GetType().GetProperty(propertyName).SetValue(_content, value);
            }
        }

        /// <summary>
        /// Set value from XML attribute
        /// </summary>
        /// <typeparam name="T">Set value as this type</typeparam>
        /// <param name="propertyName">Name of property on IContent</param>
        /// <param name="attributeName">Name of attribute in XML</param>
        void SetPropertyValueFromAttribute<T>(string propertyName, string attributeName)
        {
            string attributeValue = GetAttributeValue(attributeName);
            if (attributeValue == null)
            {
                // If attribute doesn't exist or value is null in passed XML we don't do anything (not trying to set value and don't remove it)
                return;
            }

            var value = ParseHelper.GetTypedValue<T>(attributeValue);
            SetPropertyValue(propertyName, value);
        }

        string GetAttributeValue(string attributeName)
        {
            var attribute = _xml.Attribute(attributeName);
            if (attribute != null)
            {
                return attribute.Value;

            }
            return null;
        }
    }
}