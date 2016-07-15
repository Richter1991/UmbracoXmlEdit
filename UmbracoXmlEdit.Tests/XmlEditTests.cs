using NUnit.Framework;
using System.Xml.Linq;
using UmbracoXmlEdit.Tests.TestHelpers;
using System.Collections.Generic;
using System;

namespace UmbracoXmlEdit.Tests
{
    [TestFixture]
    public class XmlEditTests : TestsBase
    {
        [Test]
        public void UpdateContentFromXml_can_change_default_property_values()
        {
            var key = Guid.NewGuid();

            // Example XML
            // <home parentID=\"1077\" sortOrder=\"7\" createDate=\"2016-04-23T13:37:59\" nodeName=\"My firstpage\"></home>

            var xmlAttributes = new List<XAttribute>
            {
                new XAttribute(XName.Get("id"), 1078),
                new XAttribute(XName.Get("parentID"), 1077),
                new XAttribute(XName.Get("sortOrder"), 7),
                new XAttribute(XName.Get("createDate"), "2016-04-23T13:37:59"),
                new XAttribute(XName.Get("nodeName"), "My firstpage"),
                new XAttribute(XName.Get("creatorID"), 123),
                new XAttribute(XName.Get("path"), "-1,1077,1078"),
                new XAttribute(XName.Get("level"), 3),
                new XAttribute(XName.Get("key"), key.ToString()),
            };
            var xmlDocument = new XDocument(new XElement(XName.Get("home"), xmlAttributes));

            string xml = xmlDocument.ToString();
            var xmlEdit = new XmlEdit(_logger, TestServices.MockDataTypeService(), null, TestModels.CreateContent(), xml);

            xmlEdit.UpdateContentFromXml();
            Assert.IsTrue(xmlEdit.IsValidXml);

            var updatedContent = xmlEdit.Content;

            Assert.AreEqual(1078, updatedContent.Id);
            Assert.AreEqual(1077, updatedContent.ParentId);
            Assert.AreEqual(7, updatedContent.SortOrder);
            Assert.AreEqual("My firstpage", updatedContent.Name);
            Assert.AreEqual(123, updatedContent.CreatorId);
            Assert.AreEqual("-1,1077,1078", updatedContent.Path);
            Assert.AreEqual(3, updatedContent.Level);
            Assert.AreEqual(key, updatedContent.Key);

            // Create date
            Assert.AreEqual(2016, updatedContent.CreateDate.Year);
            Assert.AreEqual(4, updatedContent.CreateDate.Month);
            Assert.AreEqual(23, updatedContent.CreateDate.Day);
            Assert.AreEqual(13, updatedContent.CreateDate.Hour);
            Assert.AreEqual(37, updatedContent.CreateDate.Minute);
            Assert.AreEqual(59, updatedContent.CreateDate.Second);
        }

        [Test]
        public void UpdateContentFromXml_can_change_template()
        {
            var template = TestModels.CreateTemplate(9893, "Blog post", "blogPost");
            var fileService = TestServices.MockFileService(template);

            string xml = string.Format("<blogPost template=\"{0}\"></blogPost>", template.Id);
            var content = TestModels.CreateContent();
            var xmlEdit = new XmlEdit(_logger, TestServices.MockDataTypeService(), fileService, content, xml);
            xmlEdit.UpdateContentFromXml();

            Assert.IsNotNull(xmlEdit.Content.Template);
            Assert.AreEqual(template.Id, xmlEdit.Content.Template.Id);
            Assert.AreEqual(template.Alias, xmlEdit.Content.Template.Alias);
            Assert.AreEqual(template.Name, xmlEdit.Content.Template.Name);
        }

        [Test]
        public void UpdateContentFromXml_can_handle_invalid_value_in_attributes()
        {
            string xml = "<blogPost id=\"should-have-been-int\"></blogPost>";
            var content = TestModels.CreateContent();
            var xmlEdit = new XmlEdit(_logger, TestServices.MockDataTypeService(), null, content, xml);

            Assert.Throws<InvalidCastException>(() => xmlEdit.UpdateContentFromXml());
        }

        [Test]
        public void UpdateContentFromXml_handle_invalid_xml()
        {
            string xml = "<home parentID=\"22\">"; // No closing tag
            var xmlEdit = new XmlEdit(_logger, TestServices.MockDataTypeService(), null, TestModels.CreateContent(), xml);
            xmlEdit.UpdateContentFromXml();

            Assert.IsFalse(xmlEdit.IsValidXml);
            Assert.AreEqual(1, xmlEdit.Content.ParentId, "ParentId shouldn't be updated");
        }

        [Test]
        public void UpdateContentFromXml_update_value_for_custom_property()
        {
            string propertyAlias = "contentMiddle";
            var propertyType = TestModels.CreatePropertyType(propertyAlias);

            var content = TestModels.CreateContent(propertyType: propertyType);
            Assert.IsNull(content.GetValue<string>(propertyAlias));

            string xml = "<home><contentMiddle>Lorem ipsum!</contentMiddle></home>"; // XML for current IContent as a string
            var xmlEdit = new XmlEdit(_logger, TestServices.MockDataTypeService(), null, content, xml);
            xmlEdit.UpdateContentFromXml();

            Assert.IsTrue(xmlEdit.IsValidXml);
            Assert.AreEqual("Lorem ipsum!", xmlEdit.Content.GetValue<string>(propertyAlias));
        }

        [Test]
        public void UpdateContentFromXml_remove_properties_that_is_removed_from_XML_string()
        {
            string propertyAlias = "contentMiddle";
            var propertyType = TestModels.CreatePropertyType(propertyAlias);

            var content = TestModels.CreateContent("Home", propertyType);
            content.SetValue(propertyAlias, "Lorem ipsum!");
            Assert.AreEqual(1, content.Properties.Count);
            Assert.AreEqual("Lorem ipsum!", content.Properties[0].Value);
            
            string xml = "<home></home>"; // XML for current IContent as a string
            var xmlEdit = new XmlEdit(_logger, TestServices.MockDataTypeService(), null, content, xml);
            xmlEdit.UpdateContentFromXml();

            Assert.IsTrue(xmlEdit.IsValidXml);
            Assert.AreEqual(0, xmlEdit.Content.Properties.Count);
        }
    }
}
