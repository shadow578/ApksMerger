using System.Xml.Serialization;

namespace APKSMerger.AndroidRes.Model.Generic
{
    public class AndroidResource
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }
}
