using APKSMerger.AndroidRes.Model.Generic;
using System.Xml.Serialization;

namespace APKSMerger.AndroidRes.Model
{
    public sealed class AndroidTypedItem : AndroidResource
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
