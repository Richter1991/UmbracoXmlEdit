using Moq;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace UmbracoXmlEdit.Tests.TestHelpers
{
    internal class ModelsBase
    {
        readonly IContentType _contentType = new Mock<IContentType>().Object;

        internal IDataTypeDefinition MockDataTypeDefinition(DataTypeDatabaseType dataTypeDatabaseType = DataTypeDatabaseType.Nvarchar)
        {
            var dataTypeDefinition = new Mock<IDataTypeDefinition>();
            dataTypeDefinition.Setup(d => d.DatabaseType).Returns(dataTypeDatabaseType);
            return dataTypeDefinition.Object;
        }

        internal ITemplate CreateTemplate(int id, string name, string alias)
        {
            //var templateMock = new Mock<ITemplate>();
            //templateMock.Setup(t => t.Alias).Returns("blogPost");
            //template.Setup(t => t.Name).Returns("Blog post");
            //return templateMock.Object;

            var template = new Template(name, alias);
            template.Id = id;
            return template;
        }

        internal IContent CreateContent(string name = null, PropertyType propertyType = null)
        {
            var contentMock = new Content(name, 1, _contentType);//new Mock<IContent>();
            //contentMock.Setup(t => t.Name).Returns(name);
            //contentMock.Setup(t => t.ParentId).Returns(1);
            //contentMock.Setup(t => t.ContentType).Returns(_contentType);

            if (propertyType != null)
            {
                //contentMock.Setup(t => t.Properties).Returns(new PropertyCollection(new List<Property>
                //{
                //    new Property(propertyType)
                //}));
                contentMock.Properties = new PropertyCollection(new List<Property>
                {
                    new Property(propertyType)
                });
            }

            //return contentMock.Object;
            return contentMock;
        }

        internal PropertyType CreatePropertyType(string propertyTypeAlias, IDataTypeDefinition dataTypeDefinition = null)
        {
            return new PropertyType(dataTypeDefinition ?? MockDataTypeDefinition(), propertyTypeAlias);
        }
    }
}
