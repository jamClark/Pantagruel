using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Xml;
using System;
using Pantagruel.Serializer.Surrogate;

namespace Pantagruel.Serializer.Test
{
    /// <summary>
    /// Used to test serialization surrogate classes.
    /// </summary>
    public class SurrogateTester
    {
        /// <summary>
        /// Tests the Ser/Des of the KeyValuePairSurrogate.
        /// </summary>
        [Test]
        public void TestKeyValuePair()
        {
            KeyValuePair<int, int> pairIn = new KeyValuePair<int, int>(5, 100);
            XmlDocument doc = XmlSerializer.Serialize(pairIn, 1, "pair");
            //doc.Save(Application.dataPath + "/" + "pair_test.xml");
            KeyValuePair<int, int> pairOut = (KeyValuePair<int, int>)XmlDeserializer.Deserialize(doc.InnerXml, 1);

            
            Assert.AreNotSame(pairIn, pairOut);
            Assert.False(ReferenceEquals(pairIn, pairOut));

            Assert.AreEqual(pairIn.Key, pairOut.Key);
            Assert.AreEqual(pairIn.Value, pairOut.Value);
            Assert.AreEqual(pairOut.Value, 100);
            
        }

        /// <summary>
        /// Indirectly tests the Ser/Des of the KeyValuePairSurrogate
        /// in the context of serializing a Dictionary object.
        /// </summary>
        [Test]
        public void TestDictionary()
        {
            Dictionary<string, int> dicIn;
            dicIn = new Dictionary<string, int>();
            dicIn["superman"] = 1;
            dicIn["batman"] = 2;
            dicIn["wonder woman"] = 3;
            dicIn["martian manhunter"] = 4;
            dicIn["black canary"] = 10;
            dicIn["green arrow"] = 20;
            XmlDocument doc = XmlSerializer.Serialize(dicIn, 1, "LeagueRanks");
            //doc.Save(Application.dataPath + "/" + "dic_test.xml");

            Dictionary<string,int> dicOut = XmlDeserializer.Deserialize(doc.InnerXml, 1) as Dictionary<string, int>;

            Assert.NotNull(dicOut);
            Assert.AreNotSame(dicOut, dicIn);
            Assert.AreEqual(dicOut.Count, 6);
            Assert.True(dicOut.ContainsKey("green arrow"));
            Assert.AreEqual(dicOut["superman"], 1);
            Assert.AreEqual(dicOut["batman"], 2);
            Assert.AreEqual(dicOut["wonder woman"], 3);
            Assert.AreEqual(dicOut["martian manhunter"], 4);
            Assert.AreEqual(dicOut["black canary"], 10);
            Assert.AreEqual(dicOut["green arrow"], 20);
        }

        /// <summary>
        /// Test the Ser/Des of C# delegates. This does not test multi-cast delegates,
        /// delegates instances linked to static methods, or any variety of anonymous
        /// //functions assigned to delegates.
        /// </summary>
        [Test]
        public void TestSingleDelegate()
        {
            var temp = new DelegateTestClass();
            temp.Target = new DelegateTargetTestClass();
            temp.Del = new DelegateTestClass.DelegateTest(temp.Target.MyHandler);
            Assert.NotNull(temp.Target);
            Assert.NotNull(temp.Del);

            temp.Target.Value = "Serialized.";

            var doc = XmlSerializer.Serialize(temp, 1, "delegate_test");
            //doc.Save(Application.dataPath + "/t.xml");

            var temp2 = XmlDeserializer.Deserialize(doc.InnerXml, 1) as DelegateTestClass;
            Assert.NotNull(temp2);
            Assert.NotNull(temp2.Target);
            Assert.NotNull(temp2.Del);

            Assert.AreNotSame(temp, temp2);
            Assert.AreNotSame(temp.Target, temp2.Target);
            Assert.AreNotSame(temp.Del, temp2.Del);

            Assert.AreEqual("Serialized.", temp2.Target.Value);

            temp2.Del("Invoked."); //already asserted for null value
            Assert.AreEqual("Invoked.", temp2.Target.Value);
        }

        /// <summary>
        /// Test the Ser/Des of C# delegates. This specifically tests a delegate with
        /// multiple handlers attached to it.
        /// </summary>
        [Test]
        public void TestMultiDelegate()
        {
            var temp = new DelegateTestClass();
            temp.Target = new DelegateTargetTestClass();
            temp.Del = temp.Target.MyHandler1;
            temp.Del += temp.Target.MyHandler2;
            Assert.NotNull(temp.Target);
            Assert.NotNull(temp.Del);
            Assert.AreEqual(2, temp.Del.GetInvocationList().Length);

            temp.Del("");
            Assert.AreEqual("Not Changed._X_O", temp.Target.Value);
            temp.Target.Value = "Serialized.";

            var doc = XmlSerializer.Serialize(temp, 1, "delegate_test");
            //doc.Save(Application.dataPath + "/t.xml");

            var temp2 = XmlDeserializer.Deserialize(doc.InnerXml, 1) as DelegateTestClass;
            Assert.NotNull(temp2);
            Assert.NotNull(temp2.Target);
            Assert.NotNull(temp2.Del);

            Assert.AreNotSame(temp, temp2);
            Assert.AreNotSame(temp.Target, temp2.Target);
            Assert.AreNotSame(temp.Del, temp2.Del);

            Assert.AreEqual("Serialized.", temp2.Target.Value);
            Assert.AreEqual(2, temp2.Del.GetInvocationList().Length);

            temp2.Del(""); //already asserted for null value
            Assert.AreEqual("Serialized._X_O", temp2.Target.Value);
        }
    }
}