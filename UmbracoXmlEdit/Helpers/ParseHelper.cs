using System;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace UmbracoXmlEdit.Helpers
{
    public class ParseHelper
    {
        readonly IPackagingService _packagingService;
        public ParseHelper(IPackagingService packagingService)
        {
            _packagingService = packagingService;
        }

        public static object GetTypedValue<T>(string value)
        {
            var currentType = typeof(T);
            object typedValue = null;

            bool validParsed = true;
            if (currentType == typeof(int))
            {
                int intValue;
                if (int.TryParse(value, out intValue))
                {
                    typedValue = intValue;
                }
                else
                {
                    validParsed = false;
                }
            }
            else if (currentType == typeof(Guid))
            {
                Guid guidValue;
                if (Guid.TryParse(value, out guidValue))
                {
                    typedValue = guidValue;
                }
                else
                {
                    validParsed = false;
                }
            }
            else if (currentType == typeof(DateTime))
            {
                DateTime dateTimeValue;
                if (DateTime.TryParse(value, out dateTimeValue))
                {
                    typedValue = dateTimeValue;
                }
                else
                {
                    validParsed = false;
                }
            }
            else if (currentType == typeof(string))
            {
                // Set value as string
                typedValue = value;
            }
            else
            {
                throw new NotImplementedException(string.Format("Type '{0}' is not implemented", currentType.Name));
            }

            if (!validParsed)
            {
                throw new InvalidCastException(string.Format("Couldn't parse value '{0}' to type '{1}'", value, currentType.Name));
            }

            return typedValue;
        }

        public XElement ToXml(IContent page)
        {
            // Convert to XML
            var xml = page.ToXml(_packagingService);

            // Remove attributes that can't be updated
            xml.Attributes().Where(a => IsInvalidAttibute(a)).Remove();

            return xml;
        }

        bool IsInvalidAttibute(XAttribute xmlAttribute)
        {
            return DefaultContentProperties.InvalidAttributes.Any(a => a == xmlAttribute.Name.LocalName);
        }
    }
}