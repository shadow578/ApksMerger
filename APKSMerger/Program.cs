using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace APKSMerger
{
    class Program
    {
        static void Main(string[] args)
        {
            TestDeserialize();
        }

        static void TestSerialize()
        {
            //complex
            AndroidStyle tStyle = new AndroidStyle() { Name = "theme", Parent = "AppTheme.Light" };
            tStyle.Items.Add(new AndroidGeneric() { Name = "itemBackground", Value = "bla" });

            AndroidPlural tPlural = new AndroidPlural() { Name = "plural" };
            tPlural.Values.AddRange(new AndroidPluralItem[]{
                new AndroidPluralItem() {Quantity = "few", Value = "bla"},
                new AndroidPluralItem() { Quantity = "many", Value = "blub"}
             });

            AndroidStringArray tsArray = new AndroidStringArray() { Name = "strarr" };
            tsArray.Values.AddRange(new AndroidArrayItem[] {
                new AndroidArrayItem(){ Value = "one"},
                new AndroidArrayItem(){ Value = "two"}
            });

            AndroidIntegerArray tiArray = new AndroidIntegerArray() { Name = "intarr" };
            tiArray.Values.AddRange(new AndroidArrayItem[] {
                new AndroidArrayItem(){ Value = "1"},
                new AndroidArrayItem(){ Value = "2"}
            });

            AndroidGenericArray tgArray = new AndroidGenericArray() { Name = "arr" };
            tgArray.Values.AddRange(new AndroidArrayItem[] {
                new AndroidArrayItem(){ Value = "@drawable/abc_test"},
                new AndroidArrayItem(){ Value = "@color/app_background"}
            });

            AndroidStyleable tStyleable = new AndroidStyleable() { Name = "dec-style" };
            tStyleable.Values.AddRange(new AndroidAttribute[] {
                new AndroidAttribute(){ Name = "itemBackground", Format = "boolean", Value = "false"}
            });

            AndroidResources res = new AndroidResources();
            res.Values.AddRange(new AndroidResource[] {
                //basic
                new AndroidBool(){Name = "bool", Value = false},
                new AndroidInteger(){Name = "inte", Value = 100},
                new AndroidDimension(){Name = "dimen", Value = "10dp"},
                new AndroidDrawable(){Name = "draw", Value = "@drawable/abc_test"},
                new AndroidColor(){Name = "col", Value = "@color/black"},
                new AndroidFraction(){Name = "frac", Value = "10.0"},

                //extended
                new AndroidAttribute(){Name = "attr", Format = "string", Value = "test"},
                new AndroidString(){Name = "str", Formatted = false, Translateable = false, Value = "bonjour"},
                new AndroidTypedItem(){Name = "itm", Type = "string", Value = "valhard"},

                //complex
                tStyle,
                tPlural,
                tsArray,
                tiArray,
                tgArray,
                tStyleable
            });

            XmlSerializer serializer = new XmlSerializer(typeof(AndroidResources));
            using (TextWriter textWriter = new StreamWriter("./test.xml"))
            {
                serializer.Serialize(textWriter, res, new XmlSerializerNamespaces());
            }
        }

        static void TestDeserialize()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AndroidResources));
            using (TextReader textReader = new StreamReader("./test.xml"))
            {
                AndroidResources res = serializer.Deserialize(textReader) as AndroidResources;
            }
        }
    }

    [XmlRoot("resources")]
    public class AndroidResources
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

    public class AndroidResource
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    public class AndroidGeneric : AndroidResource
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class AndroidGenericArray : AndroidResource
    {
        [XmlElement("item", Type = typeof(AndroidArrayItem))]
        public List<AndroidArrayItem> Values { get; set; } = new List<AndroidArrayItem>();
    }

    public sealed class AndroidBool : AndroidResource
    {
        [XmlText]
        public bool Value { get; set; }
    }

    public sealed class AndroidInteger : AndroidResource
    {
        [XmlText]
        public int Value { get; set; }
    }

    public sealed class AndroidString : AndroidResource
    {
        [XmlAttribute("formatted")]
        public bool Formatted { get; set; }

        [XmlAttribute("translatable")]
        public bool Translateable { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public sealed class AndroidAttribute : AndroidResource
    {
        [XmlAttribute("format")]
        public string Format { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public sealed class AndroidTypedItem : AndroidResource
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public sealed class AndroidStyle : AndroidResource
    {
        [XmlAttribute("parent")]
        public string Parent { get; set; }

        [XmlElement("item", Type = typeof(AndroidGeneric))]
        public List<AndroidGeneric> Items { get; set; } = new List<AndroidGeneric>();
    }

    public sealed class AndroidPluralItem
    {
        [XmlAttribute("quantitiy")]
        public string Quantity { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public sealed class AndroidPlural : AndroidResource
    {
        [XmlElement("item", Type = typeof(AndroidPluralItem))]
        public List<AndroidPluralItem> Values { get; set; } = new List<AndroidPluralItem>();
    }

    public sealed class AndroidArrayItem
    {
        [XmlText]
        public string Value { get; set; }
    }

    public sealed class AndroidStyleable : AndroidResource
    {
        [XmlElement("attr", Type = typeof(AndroidAttribute))]
        public List<AndroidAttribute> Values { get; set; } = new List<AndroidAttribute>();
    }

    public sealed class AndroidDimension : AndroidGeneric { }

    public sealed class AndroidDrawable : AndroidGeneric { }

    public sealed class AndroidColor : AndroidGeneric { }

    public sealed class AndroidFraction : AndroidGeneric { }

    public sealed class AndroidStringArray : AndroidGenericArray { }

    public sealed class AndroidIntegerArray : AndroidGenericArray { }
}
