using Moq;
using Umbraco.Core.Logging;

namespace UmbracoXmlEdit.Tests.TestHelpers
{
    public class TestsBase
    {
        internal readonly ILogger _logger = new Mock<ILogger>().Object;

        ServicesBase _testServices;
        internal ServicesBase TestServices
        {
            get
            {
                if (_testServices == null)
                {
                    _testServices = new ServicesBase();
                }
                return _testServices;
            }
        }

        ModelsBase _testModels;
        internal ModelsBase TestModels
        {
            get
            {
                if(_testModels == null)
                {
                    _testModels = new ModelsBase();
                }
                return _testModels;
            }
        }
    }
}
