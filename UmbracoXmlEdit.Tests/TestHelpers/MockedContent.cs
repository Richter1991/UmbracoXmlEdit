using System.Collections.Generic;
using Umbraco.Core.Models;

namespace UmbracoXmlEdit.Tests.TestHelpers
{
    public class MockedContent
    {
        public static IContent CreateContent(string name, PropertyType propertyType = null)
        {
            var contentType = new ContentType(-1);
            var content = new Content(name, -1, contentType);

            if (propertyType != null)
            {
                content.Properties = new PropertyCollection(new List<Property>
                {
                    new Property(propertyType)
                });
            }
            return content;
        }
    }
}
