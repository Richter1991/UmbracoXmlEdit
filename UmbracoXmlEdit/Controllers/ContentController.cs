using System;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using UmbracoXmlEdit.Helpers;
using UmbracoXmlEdit.Models;

namespace UmbracoXmlEdit.Controllers
{
    [PluginController("XmlEdit")]
    public class ContentController : UmbracoAuthorizedApiController 
    {
        readonly IContentService _contentService;
        readonly IUserService _userService;
        readonly ILogger _logger;
        readonly IDataTypeService _dataTypeService;
        readonly IFileService _fileService;
        readonly IPackagingService _packagingService;

        ParseHelper _parseHelper;
        public ContentController()
        {
            _contentService = ApplicationContext.Current.Services.ContentService;
            _userService = ApplicationContext.Current.Services.UserService;
            _logger = ApplicationContext.Current.ProfilingLogger.Logger;
            _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
            _fileService = ApplicationContext.Current.Services.FileService;
            _packagingService = ApplicationContext.Current.Services.PackagingService;
            _parseHelper = new ParseHelper(_packagingService);
        }

        [HttpGet]
        public string GetXml(string nodeId)
        {
            var page = _contentService.GetById(int.Parse(nodeId));
            return _parseHelper.ToXml(page).ToString();
        }

        [HttpPost]
        public string SaveXml(SaveXml model)
        {
            int contentType = int.Parse(model.ContentId);
            var content = _contentService.GetById(contentType);
            if (content == null)
            {
                throw new NullReferenceException("IContent object not found");
            }

            var currentUser = _userService.GetByUsername(HttpContext.Current.User.Identity.Name);

            // Update XML
            var xmlEdit = new XmlEdit(_logger, _dataTypeService, _fileService, content, model.Xml);
            xmlEdit.UpdateContentFromXml();

            if(xmlEdit.IsValidXml)
            {
                if(model.Publish)
                {
                    // Save and publish page
                    _contentService.SaveAndPublishWithStatus(xmlEdit.Content, currentUser.Id);
                }
                else
                {
                    // Save page
                    _contentService.Save(xmlEdit.Content, currentUser.Id);
                }

            }
            else
            {
                // TODO: Show an error
            }

            // Get the page that we just saved
            content = _contentService.GetById(content.Id);
            
            return _parseHelper.ToXml(content).ToString();
        }
    }
}