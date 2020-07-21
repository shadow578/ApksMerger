using APKSMerger.AndroidRes;
using APKSMerger.AndroidRes.Model;
using APKSMerger.AndroidRes.Model.Generic;
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
            tPlural.Values.AddRange(new AndroidPlural.Item[]{
                new AndroidPlural.Item() {Quantity = "few", Value = "bla"},
                new AndroidPlural.Item() { Quantity = "many", Value = "blub"}
             });

            AndroidStringArray tsArray = new AndroidStringArray() { Name = "strarr" };
            tsArray.Values.AddRange(new AndroidGenericArray.Item[] {
                new AndroidGenericArray.Item(){ Value = "one"},
                new AndroidGenericArray.Item(){ Value = "two"}
            });

            AndroidIntegerArray tiArray = new AndroidIntegerArray() { Name = "intarr" };
            tiArray.Values.AddRange(new AndroidGenericArray.Item[] {
                new AndroidGenericArray.Item(){ Value = "1"},
                new AndroidGenericArray.Item(){ Value = "2"}
            });

            AndroidGenericArray tgArray = new AndroidGenericArray() { Name = "arr" };
            tgArray.Values.AddRange(new AndroidGenericArray.Item[] {
                new AndroidGenericArray.Item(){ Value = "@drawable/abc_test"},
                new AndroidGenericArray.Item(){ Value = "@color/app_background"}
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
}
