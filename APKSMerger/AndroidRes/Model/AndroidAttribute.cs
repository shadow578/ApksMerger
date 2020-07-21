﻿using APKSMerger.AndroidRes.Model.Generic;
using System.Xml.Serialization;

namespace APKSMerger.AndroidRes.Model
{
    public sealed class AndroidAttribute : AndroidResource
    {
        [XmlAttribute("format")]
        public string Format { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
