using System;
using System.Xml.Serialization;

namespace APKSMerger.AndroidRes.Model.Generic
{
    public class AndroidResource
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            //check other object is of correct type, otherwise not equal
            if (!(obj is AndroidResource other)) return false;

            //check if name is equal
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
