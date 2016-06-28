using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Logging;
using System.Xml.Linq;

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

        Mock<IDataTypeService> SetupDataTypeService(IDataTypeDefinition dataTypeDefinition)
        {
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

            var xmlEdit = new XmlEdit(_logger.Object, dataTypeService.Object);
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

            var xmlEdit = new XmlEdit(_logger.Object, dataTypeService.Object);
            var propertyElement = XElement.Parse("<MyProperty>Try passing text</MyProperty>");
            var propertyType = new PropertyType(dataTypeInteger);

            var result = xmlEdit.GetValue(propertyElement, new PropertyType(dataTypeInteger));
            Assert.IsNull(result);
        }

        [Test]
        public void GetValue_can_get_string_value()
        {
            var dataType = new DataTypeDefinition("");
            var dataTypeService = SetupDataTypeService(dataType);

            var xmlEdit = new XmlEdit(_logger.Object, dataTypeService.Object);
            var propertyElement = XElement.Parse("<MyProperty>Lorem ipsum</MyProperty>");
            var propertyType = new PropertyType(dataType);

            var result = xmlEdit.GetValue(propertyElement, propertyType);

            Assert.AreEqual(typeof(string), result.GetType());
            Assert.AreEqual("Lorem ipsum", result.ToString());
        }
    }
}
