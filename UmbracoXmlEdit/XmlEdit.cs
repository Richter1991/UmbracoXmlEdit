using System.Xml.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace UmbracoXmlEdit
{
    public class XmlEdit
    {
        readonly ILogger _logger;
        readonly IDataTypeService _dataTypeService;
        readonly IFileService _fileService;
        IContent _content;
        XElement _xml;

        /// <summary>
        /// Wheter XML could be parsed successfully or not
        /// </summary>
        public bool IsValidXml { get; private set; }

        public IContent Content
        {
            get
            {
                return _content;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="content">IContent/page to update</param>
        /// <param name="xml">XML</param>
        public XmlEdit(ILogger logger, IDataTypeService dataTypeService, IFileService fileService, IContent content, string xml)
        {
            _logger = logger;
            _dataTypeService = dataTypeService;
            _fileService = fileService;
            _content = content;

            TryParse(xml);
        }

        /// <summary>
        /// Update IContent-object from XML
        /// </summary>
        public void UpdateContentFromXml()
        {
            if (IsValidXml)
            {
                // Update value of IContent default-properties from XML-attributes
                _content = new DefaultContentProperties(_logger, _fileService, _content, _xml).SetProperties();

                // Update value for custom properties
                _content = new CustomProperties(_logger, _dataTypeService, _fileService, _content, _xml).Process();
            }
        }

        void TryParse(string xml)
        {
            try
            {
                _xml = XElement.Parse(xml);
                IsValidXml = true;
            }
            catch
            {
                IsValidXml = false;
            }
        }
    }
}