using Moq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace UmbracoXmlEdit.Tests.TestHelpers
{
    internal class ServicesBase
    {
        internal IFileService MockFileService(ITemplate template)
        {
            var fileService = new Mock<IFileService>();
            fileService.Setup(f => f.GetTemplate(template.Id)).Returns(template);
            return fileService.Object;
        }

        internal IDataTypeService MockDataTypeService(IDataTypeDefinition dataTypeDefinition = null)
        {
            var dataTypeService = new Mock<IDataTypeService>();
            dataTypeService.Setup(d => d.GetDataTypeDefinitionById(0)).Returns(dataTypeDefinition ?? new ModelsBase().MockDataTypeDefinition());
            return dataTypeService.Object;
        }
    }
}
