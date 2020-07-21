using APKSMerger.AndroidRes.Model;
using APKSMerger.AndroidRes.Model.Generic;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace APKSMerger.AndroidRes
{
    [XmlRoot("resources")]
    public sealed class AndroidResources
    {
        //basic
        [XmlElement("bool", Type = typeof(AndroidBool))]
        [XmlElement("integer", Type = typeof(AndroidInteger))]
        [XmlElement("dimen", Type = typeof(AndroidDimension))]
        [XmlElement("drawable", Type = typeof(AndroidDrawable))]
        [XmlElement("color", Type = typeof(AndroidColor))]
        [XmlElement("fraction", Type = typeof(AndroidFraction))]

        //extended
        [XmlElement("attr", Type = typeof(AndroidAttribute))]
        [XmlElement("string", Type = typeof(AndroidString))]
        [XmlElement("item", Type = typeof(AndroidTypedItem))]

        //complex
        [XmlElement("style", Type = typeof(AndroidStyle))]
        [XmlElement("plurals", Type = typeof(AndroidPlural))]
        [XmlElement("string-array", Type = typeof(AndroidStringArray))]
        [XmlElement("integer-array", Type = typeof(AndroidIntegerArray))]
        [XmlElement("array", Type = typeof(AndroidGenericArray))]
        [XmlElement("declare-styleable", Type = typeof(AndroidStyleable))]
        public List<AndroidResource> Values { get; set; } = new List<AndroidResource>();
    }
}
