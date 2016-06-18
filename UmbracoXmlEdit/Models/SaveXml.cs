using Newtonsoft.Json;

namespace UmbracoXmlEdit.Models
{
    public class SaveXml
    {
        [JsonProperty("nodeId")]
        public string NodeId { get; set; }

        [JsonProperty("xml")]
        public string Xml { get; set; }
    }
}