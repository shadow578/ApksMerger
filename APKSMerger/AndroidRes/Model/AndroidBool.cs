using APKSMerger.AndroidRes.Model.Generic;
using System.Xml.Serialization;

namespace APKSMerger.AndroidRes.Model
{
    public sealed class AndroidBool : AndroidResource
    {
        [XmlText]
        public bool Value { get; set; }
    }
}
