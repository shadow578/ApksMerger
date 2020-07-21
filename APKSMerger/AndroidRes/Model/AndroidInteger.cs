using APKSMerger.AndroidRes.Model.Generic;
using System.Xml.Serialization;

namespace APKSMerger.AndroidRes.Model
{
    public sealed class AndroidInteger : AndroidResource
    {
        [XmlText]
        public int Value { get; set; }
    }
}
