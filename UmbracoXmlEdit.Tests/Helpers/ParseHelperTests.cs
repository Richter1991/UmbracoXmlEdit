using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using UmbracoXmlEdit.Helpers;
using UmbracoXmlEdit.Tests.TestHelpers;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace UmbracoXmlEdit.Tests.Helpers
{
    [TestFixture]
    public class ParseHelperTests : TestsBase
    {
        [Test]
        public void ToXml_removes_IContent_attributes_that_cant_be_updated()
        {
            var validXmlAttributes = new List<XAttribute>
            {
                new XAttribute(XName.Get("id"), 1078),
                new XAttribute(XName.Get("parentID"), 1077),
                new XAttribute(XName.Get("sortOrder"), 7),
                new XAttribute(XName.Get("createDate"), "2016-04-23T13:37:59"),
                new XAttribute(XName.Get("nodeName"), "My firstpage"),
                new XAttribute(XName.Get("creatorID"), 45),
                new XAttribute(XName.Get("path"), "-1,1077,1078"),
                new XAttribute(XName.Get("level"), 3),
                new XAttribute(XName.Get("key"), "6d28d0a4-6874-42d2-8c36-0bff8f00d686"),
                new XAttribute(XName.Get("template"), 1234)
            };

            var invalidXmlAttributes = new List<XAttribute>
            {
                new XAttribute(XName.Get("updateDate"), "2016-06-13T22:06:22"),
                new XAttribute(XName.Get("urlName"), "root"),
                new XAttribute(XName.Get("isDoc"), ""),
                new XAttribute(XName.Get("nodeType"), 1060),
                new XAttribute(XName.Get("creatorName"), "Lorem Ipsum"),
                new XAttribute(XName.Get("writerName"), "Lorem Ipsum"),
                new XAttribute(XName.Get("writerID"), 0),
                new XAttribute(XName.Get("nodeTypeAlias"), "Site"),
            };

            var xmlDocument = XElement.Parse("<Site></Site>");
            xmlDocument.Add(validXmlAttributes);
            xmlDocument.Add(invalidXmlAttributes);

            var content = TestModels.CreateContent();
            var service = new Mock<IPackagingService>();
            service.Setup(p => p.Export(content, false, false)).Returns(() => 
            {
                return xmlDocument;
            });

            var helper = new ParseHelper(service.Object);
            var xmlResult = helper.ToXml(content);

            Assert.AreEqual(10, xmlResult.Attributes().Count());
        }
    }
}
