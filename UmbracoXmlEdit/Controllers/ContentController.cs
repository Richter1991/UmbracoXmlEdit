using System;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using UmbracoXmlEdit.Models;

namespace UmbracoXmlEdit.Controllers
{
    [PluginController("XmlEdit")]
    public class ContentController : UmbracoAuthorizedApiController 
    {
        readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;
        readonly IUserService _userService = ApplicationContext.Current.Services.UserService;
        readonly ILogger _logger = ApplicationContext.Current.ProfilingLogger.Logger;
        readonly IDataTypeService _dataTypeService = ApplicationContext.Current.Services.DataTypeService;

        [HttpGet]
        public string GetXml(string nodeId)
        {
            var page = ApplicationContext.Services.ContentService.GetById(int.Parse(nodeId));
            return ToXml(page);
        }

        private string ToXml(IContent page)
        {
            var xml = page.ToXml(ApplicationContext.Services.PackagingService);
            return xml.ToString();
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
            var xmlEdit = new XmlEdit(_logger, _dataTypeService, content);
            xmlEdit.UpdateContentFromXml(model.Xml);

            if(xmlEdit.Successful)
            {
                // Save page
                _contentService.Save(xmlEdit.Content, currentUser.Id);
            }
            else
            {
                // TODO: Show an error
            }

            // Get the page that we just saved
            content = _contentService.GetById(content.Id);

            return ToXml(content);
        }
    }
}