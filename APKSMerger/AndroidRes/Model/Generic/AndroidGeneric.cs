using System.Xml.Serialization;

namespace APKSMerger.AndroidRes.Model.Generic
{
    public class AndroidGeneric : AndroidResource
    {
        [XmlText]
        public string Value { get; set; }
    }
}
