using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Logging;
using System.Xml.Linq;
using UmbracoXmlEdit.Tests.TestHelpers;

namespace UmbracoXmlEdit.Tests
{
    [TestFixture]
    public class XmlEditTests
    {
        Mock<ILogger> _logger
        {
            get
            {
                return new Mock<ILogger>();
            }
        }

        readonly IDataTypeDefinition _defaultDataTypeDefinition = new DataTypeDefinition("");

        Mock<IDataTypeService> SetupDataTypeService(IDataTypeDefinition dataTypeDefinition = null)
        {
            if (dataTypeDefinition == null)
            {
                dataTypeDefinition = _defaultDataTypeDefinition;
            }

            var dataTypeService = new Mock<IDataTypeService>();
            dataTypeService.Setup(d => d.GetDataTypeDefinitionById(0)).Returns(dataTypeDefinition);
            return dataTypeService;
        }

        [Test]
        public void GetValue_can_get_integer_value()
        {
            var dataTypeInteger = new DataTypeDefinition("")
            {
                DatabaseType = DataTypeDatabaseType.Integer
            };
            var dataTypeService = SetupDataTypeService(dataTypeInteger);

            var xmlEdit = new XmlEdit(_logger.Object, dataTypeService.Object, null);
            var propertyElement = XElement.Parse("<MyProperty>123</MyProperty>");
            var propertyType = new PropertyType(dataTypeInteger);

            var result = xmlEdit.GetValue(propertyElement, propertyType);
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(123, (int)result);
        }

        [Test]
        public void GetValue_can_handle_invalid_integer_value()
        {
            var dataTypeInteger = new DataTypeDefinition("")
            {
                DatabaseType = DataTypeDatabaseType.Integer
            };
            var dataTypeService = SetupDataTypeService(dataTypeInteger);

            var xmlEdit = new XmlEdit(_logger.Object, dataTypeService.Object, null);
            var propertyElement = XElement.Parse("<MyProperty>Try passing text</MyProperty>");
            var propertyType = new PropertyType(dataTypeInteger);

            var value = xmlEdit.GetValue(propertyElement, new PropertyType(dataTypeInteger));
            Assert.IsNull(value);
        }

        [Test]
        public void GetValue_can_get_string_value()
        {
            var dataTypeService = SetupDataTypeService();

            var xmlEdit = new XmlEdit(_logger.Object, dataTypeService.Object, null);
            var propertyElement = XElement.Parse("<MyProperty>Lorem ipsum</MyProperty>");
            var propertyType = new PropertyType(_defaultDataTypeDefinition);

            var value = xmlEdit.GetValue(propertyElement, propertyType);
            Assert.AreEqual(typeof(string), value.GetType());
            Assert.AreEqual("Lorem ipsum", value.ToString());
        }

        [Test]
        public void UpdatePage_can_change_default_property_values()
        {
            var dataTypeService = SetupDataTypeService();

            var contentType = new ContentType(-1);
            var content = new Content("Home", 11, contentType);

            string xml = "<home parentID=\"22\" sortOrder=\"7\" createDate=\"2016-04-23T13:37:59\"></home>";
            var xmlEdit = new XmlEdit(_logger.Object, dataTypeService.Object, content);

            xmlEdit.UpdateContentFromXml(xml);
            Assert.IsTrue(xmlEdit.Successful);

            var updatedContent = xmlEdit.Content;

            // Parent ID
            Assert.AreEqual(22, updatedContent.ParentId);

            // Sort order
            Assert.AreEqual(7, updatedContent.SortOrder);

            // Create date
            Assert.AreEqual(2016, updatedContent.CreateDate.Year);
            Assert.AreEqual(4, updatedContent.CreateDate.Month);
            Assert.AreEqual(23, updatedContent.CreateDate.Day);
            Assert.AreEqual(13, updatedContent.CreateDate.Hour);
            Assert.AreEqual(37, updatedContent.CreateDate.Minute);
            Assert.AreEqual(59, updatedContent.CreateDate.Second);
        }

        [Test]
        public void UpdatePage_handle_invalid_xml()
        {
            var dataTypeService = SetupDataTypeService();

            var contentType = new ContentType(-1);
            var content = new Content("Home", 11, contentType);

            string xml = "<home parentID=\"22\">"; // No closing tag
            var xmlEdit = new XmlEdit(_logger.Object, dataTypeService.Object, content);
            xmlEdit.UpdateContentFromXml(xml);

            Assert.IsFalse(xmlEdit.Successful);
            Assert.AreEqual(11, xmlEdit.Content.ParentId, "ParentId shouldn't be updated");
        }

        [Test]
        public void UpdatePage_update_value_for_custom_property()
        {
            string propertyAlias = "contentMiddle";
            var dataTypeService = SetupDataTypeService();
            var propertyType = new PropertyType(_defaultDataTypeDefinition, propertyAlias);

            var content = MockedContent.CreateContent("Home", propertyType);
            Assert.IsNull(content.GetValue<string>(propertyAlias));

            string xml = "<home><contentMiddle>Lorem ipsum!</contentMiddle></home>"; // XML for current IContent as a string
            var xmlEdit = new XmlEdit(_logger.Object, dataTypeService.Object, content);
            xmlEdit.UpdateContentFromXml(xml);

            Assert.IsTrue(xmlEdit.Successful);
            Assert.AreEqual("Lorem ipsum!", xmlEdit.Content.GetValue<string>(propertyAlias));
        }

        [Test]
        public void UpdatePage_remove_properties_that_is_removed_from_XML_string()
        {
            string propertyAlias = "contentMiddle";
            var dataTypeService = SetupDataTypeService();
            var propertyType = new PropertyType(_defaultDataTypeDefinition, propertyAlias);

            var content = MockedContent.CreateContent("Home", propertyType);
            content.SetValue(propertyAlias, "Lorem ipsum!");
            Assert.AreEqual(1, content.Properties.Count);
            Assert.AreEqual("Lorem ipsum!", content.Properties[0].Value);
            
            string xml = "<home></home>"; // XML for current IContent as a string
            var xmlEdit = new XmlEdit(_logger.Object, dataTypeService.Object, content);
            xmlEdit.UpdateContentFromXml(xml);

            Assert.IsTrue(xmlEdit.Successful);
            Assert.AreEqual(0, xmlEdit.Content.Properties.Count);
        }
    }
}
