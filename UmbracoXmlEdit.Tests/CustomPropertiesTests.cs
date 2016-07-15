using NUnit.Framework;
using System.Xml.Linq;
using Umbraco.Core.Models;
using UmbracoXmlEdit.Tests.TestHelpers;

namespace UmbracoXmlEdit.Tests
{
    [TestFixture]
    public class CustomPropertiesTests : TestsBase
    {
        [Test]
        public void GetValue_can_get_integer_value()
        {
            var dataTypeInteger = TestModels.MockDataTypeDefinition(DataTypeDatabaseType.Integer);
            var propertyElement = XElement.Parse("<MyProperty>123</MyProperty>");

            var xmlEdit = new CustomProperties(_logger, TestServices.MockDataTypeService(dataTypeInteger), null, null, propertyElement);

            var propertyType = TestModels.CreatePropertyType("propertyAlias", dataTypeInteger);

            var result = xmlEdit.GetValue(propertyElement, propertyType);
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(123, (int)result);
        }

        [Test]
        public void GetValue_can_handle_invalid_integer_value()
        {
            var dataTypeInteger = TestModels.MockDataTypeDefinition(DataTypeDatabaseType.Integer);
            var propertyElement = XElement.Parse("<MyProperty>Try passing text</MyProperty>");

            var xmlEdit = new CustomProperties(_logger, TestServices.MockDataTypeService(dataTypeInteger), null, null, propertyElement);

            var propertyType = TestModels.CreatePropertyType("propertyAlias", dataTypeInteger);

            var value = xmlEdit.GetValue(propertyElement, propertyType);
            Assert.IsNull(value);
        }

        [Test]
        public void GetValue_can_get_string_value()
        {
            var propertyElement = XElement.Parse("<MyProperty>Lorem ipsum</MyProperty>");
            var xmlEdit = new CustomProperties(_logger, TestServices.MockDataTypeService(), null, null, propertyElement);

            var propertyType = TestModels.CreatePropertyType("propertyAlias");

            var value = xmlEdit.GetValue(propertyElement, propertyType);
            Assert.AreEqual(typeof(string), value.GetType());
            Assert.AreEqual("Lorem ipsum", value.ToString());
        }
    }
}
