using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using Pantagruel.Serializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Pantagruel.Serializer.Test
{
    /// <summary>
    /// Unit tests for checking serialization of general data,
    /// GameObjects hierarchies, and single components.
    /// </summary>
    public class ComponentTester : SerializerTesterBase
    {

        #region No Shared/Circular References
        /// <summary>
        /// Ensures that a simple component using primities values only is Ser/Desed properly.
        /// </summary>
        [Test]
        public void TestSimpleComponents()
        {
            //create GO and give it some interesting values
            var go1 = GimmeSomething();
            MakeInteresting(go1);

            //add a simple component that only has primitive variables
            var c1 = go1.AddComponent<SimpleTestComponent>();
            c1.PublicString = "Test String 5000";
            c1.SetPrivateValues(45.77f, 802);
            c1.NonSerializedPublicInt = 10001;
            c1.PublicInteger = 20002;

            var go2 = CloneWithDeserializer(go1);

            //is new object NOT old object?
            Assert.AreNotEqual(go1, go2);

            //check deserialized values
            var c2 = go2.GetComponent<SimpleTestComponent>();
            Assert.NotNull(c2);
            Assert.False(ReferenceEquals(c2, c1));

            Assert.AreEqual(c2.PublicString, "Test String 5000");
            Assert.AreEqual(c2.PublicInteger, 20002);
            Assert.AreEqual((float)GetValue("SerializedPrivateFloat", c2), 45.77f, 0.0001f);

            //make sure stuff that shouldn't be serialized, isn't
            Assert.AreNotEqual((int)GetValue("NonSerializedPrivateInt", c2), 802);
            Assert.AreNotEqual(c2.NonSerializedPublicInt, 10001);
        }
        #endregion 


        #region GameObject user-made class reference tests
        /// <summary>
        /// Ensures that a component with references to user-made (non-component)
        /// classes is Ser/Desed properly. It also ensures that circular references and
        /// shared references work properly when deserialized.
        /// </summary>
        [Test]
        public void TestComponentWithUserClassRefs_CastRefs()
        {
            //create GO and give it some interesting values
            var go1 = GimmeSomething();
            var comp1 = go1.AddComponent<RefClassTestComponent>();
            comp1.BaseClass = new RefBaseClass();
            comp1.DerivedClass = new RefDerivedClass();
            comp1.DerivedClassCastToBase = new RefDerivedClass();

            var go2 = CloneWithDeserializer(go1);
            var comp2 = go2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(comp2);

        }

        /// <summary>
        /// Ensures that a component with references to user-made (non-component)
        /// classes is Ser/Desed properly. It also ensures that circular references and
        /// shared references work properly when deserialized.
        /// </summary>
        [Test]
        public void TestComponentWithUserClassRefs_Primitives()
        {
            //create GO and give it some interesting values
            var go1 = GimmeSomething();
            var comp1 = go1.AddComponent<RefClassTestComponent>();
            comp1.BaseClass = new RefBaseClass();
            comp1.BaseClass.IdString = "superman";
            comp1.BaseClass.Number = 231;

            var go2 = CloneWithDeserializer(go1);
            var comp2 = go2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(comp2);

            Assert.AreNotEqual(comp1, comp2);
            Assert.NotNull(comp2.BaseClass);
            Assert.AreNotEqual(comp1.BaseClass, comp2.BaseClass);
            Assert.AreEqual(comp2.BaseClass.IdString, "superman");
            Assert.AreEqual(comp2.BaseClass.Number, 231);

        }

        /// <summary>
        /// Ensures that a component with references to user-made (non-component)
        /// classes is Ser/Desed properly. It also ensures that circular references and
        /// shared references work properly when deserialized.
        /// </summary>
        [Test]
        public void TestComponentWithUserClassRefs_Arrays()
        {
            //create GO and give it some interesting values
            var go1 = GimmeSomething();
            var comp1 = go1.AddComponent<RefClassTestComponent>();

            comp1.BaseClass = new RefBaseClass();
            comp1.BaseClass.Array_Base = new RefBaseClass[4];
            comp1.BaseClass.Array_Base[0] = new RefBaseClass();
            comp1.BaseClass.Array_Base[1] = new RefBaseClass();
            comp1.BaseClass.Array_Base[2] = new RefBaseClass();
            comp1.BaseClass.Array_Base[3] = new RefBaseClass();

            comp1.BaseClass.Array_Base[0].Number = 5;
            comp1.BaseClass.Array_Base[1].IdString = "array1";
            comp1.BaseClass.Array_Base[2].Number = 45;
            comp1.BaseClass.Array_Base[3].IdString = "hello";

            var go2 = CloneWithDeserializer(go1);
            var comp2 = go2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(comp2);

            Assert.AreNotEqual(comp1, comp2);
            Assert.False(ReferenceEquals(comp1, comp2));
            Assert.NotNull(comp2.BaseClass.Array_Base);
            Assert.False(ReferenceEquals(comp1.BaseClass.Array_Base, comp2.BaseClass.Array_Base));

            Assert.AreEqual(comp2.BaseClass.Array_Base[0].Number, 5);
            Assert.AreEqual(comp2.BaseClass.Array_Base[1].IdString, "array1");
            Assert.AreEqual(comp2.BaseClass.Array_Base[2].Number, 45);
            Assert.AreEqual(comp2.BaseClass.Array_Base[3].IdString, "hello");

        }

        /// <summary>
        /// Ensures that a component with references to user-made (non-component)
        /// classes is Ser/Desed properly. It also ensures that circular references and
        /// shared references work properly when deserialized.
        /// </summary>
        [Test]
        public void TestComponentWithUserClassRefs_IList()
        {
            //create GO and give it some interesting values
            var go1 = GimmeSomething();
            var comp1 = go1.AddComponent<RefClassTestComponent>();

            comp1.BaseClass = new RefBaseClass();
            comp1.BaseClass.List_Base1 = new List<RefBaseClass>();
            comp1.BaseClass.List_Base1.Add(new RefBaseClass());
            comp1.BaseClass.List_Base1.Add(new RefBaseClass());
            comp1.BaseClass.List_Base1.Add(new RefBaseClass());
            comp1.BaseClass.List_Base1.Add(new RefBaseClass());

            comp1.BaseClass.List_Base1[0].Number = 1;
            comp1.BaseClass.List_Base1[1].IdString = "batman";
            comp1.BaseClass.List_Base1[2].Number = 99;
            comp1.BaseClass.List_Base1[3].IdString = "robin";

            var go2 = CloneWithDeserializer(go1);
            var comp2 = go2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(comp2);

            Assert.AreNotEqual(comp1, comp2);
            Assert.False(ReferenceEquals(comp1, comp2));
            Assert.NotNull(comp2.BaseClass.List_Base1);
            Assert.False(ReferenceEquals(comp1.BaseClass.List_Base1, comp2.BaseClass.List_Base1));

            Assert.AreEqual(comp2.BaseClass.List_Base1[0].Number, 1);
            Assert.AreEqual(comp2.BaseClass.List_Base1[1].IdString, "batman");
            Assert.AreEqual(comp2.BaseClass.List_Base1[2].Number, 99);
            Assert.AreEqual(comp2.BaseClass.List_Base1[3].IdString, "robin");

        }

        /// <summary>
        /// Ensures that a component with references to user-made (non-component)
        /// classes is Ser/Desed properly. It also ensures that circular references and
        /// shared references work properly when deserialized.
        /// </summary>
        [Test]
        public void TestComponentWithUserClassRefs_List()
        {
            //create GO and give it some interesting values
            var go1 = GimmeSomething();
            var comp1 = go1.AddComponent<RefClassTestComponent>();

            comp1.BaseClass = new RefBaseClass();
            comp1.BaseClass.List_Base2 = new List<RefBaseClass>();
            comp1.BaseClass.List_Base2.Add(new RefBaseClass());
            comp1.BaseClass.List_Base2.Add(new RefBaseClass());
            comp1.BaseClass.List_Base2.Add(new RefBaseClass());
            comp1.BaseClass.List_Base2.Add(new RefBaseClass());

            comp1.BaseClass.List_Base2[0].Number = 1;
            comp1.BaseClass.List_Base2[1].IdString = "batman";
            comp1.BaseClass.List_Base2[2].Number = 99;
            comp1.BaseClass.List_Base2[3].IdString = "robin";

            var go2 = CloneWithDeserializer(go1);
            var comp2 = go2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(comp2);

            Assert.AreNotEqual(comp1, comp2);
            Assert.False(ReferenceEquals(comp1, comp2));
            Assert.NotNull(comp2.BaseClass.List_Base2);
            Assert.False(ReferenceEquals(comp1.BaseClass.List_Base2, comp2.BaseClass.List_Base2));

            Assert.AreEqual(comp2.BaseClass.List_Base2[0].Number, 1);
            Assert.AreEqual(comp2.BaseClass.List_Base2[1].IdString, "batman");
            Assert.AreEqual(comp2.BaseClass.List_Base2[2].Number, 99);
            Assert.AreEqual(comp2.BaseClass.List_Base2[3].IdString, "robin");

        }

        /// <summary>
        /// Ensures that a component with references to user-made (non-component)
        /// classes is Ser/Desed properly. It also ensures that circular references and
        /// shared references work properly when deserialized.
        /// </summary>
        [Test]
        public void TestComponentWithUserClassRefs_Dic_StrInt()
        {
            //create GO and give it some interesting values
            var go1 = GimmeSomething();
            var comp1 = go1.AddComponent<RefClassTestComponent>();

            comp1.BaseClass = new RefBaseClass();
            comp1.BaseClass.Dic_StrInt = new Dictionary<string, int>();
            comp1.BaseClass.Dic_StrInt.Add("corwin", 1);
            comp1.BaseClass.Dic_StrInt.Add("random", 2);
            comp1.BaseClass.Dic_StrInt.Add("bleys", 6);
            comp1.BaseClass.Dic_StrInt.Add("gerard", 4);

            var go2 = CloneWithDeserializer(go1);
            var comp2 = go2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(comp2);

            Assert.AreNotEqual(comp1, comp2);
            Assert.False(ReferenceEquals(comp1, comp2));
            Assert.NotNull(comp2.BaseClass.Dic_StrInt);
            Assert.False(ReferenceEquals(comp1.BaseClass.Dic_StrInt, comp2.BaseClass.Dic_StrInt));

            Assert.True(comp2.BaseClass.Dic_StrInt.ContainsKey("corwin"));
            Assert.AreEqual(comp2.BaseClass.Dic_StrInt["corwin"], 1);
            Assert.AreEqual(comp2.BaseClass.Dic_StrInt["random"], 2);
            Assert.AreEqual(comp2.BaseClass.Dic_StrInt["gerard"], 4);
            Assert.AreEqual(comp2.BaseClass.Dic_StrInt["bleys"], 6);

        }
        #endregion


        #region Lists and Dictionaries
        /// <summary>
        /// Ensures Component[] serializes properly.
        /// </summary>
        [Test]
        public void TestArrayOfComponentRefs()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);

            //add test component and start storing arrays of GameObjects!
            var compRoot = root.AddComponent<RefClassTestComponent>();
            var comp_1_1 = go_1_1.AddComponent<RefClassTestComponent>();
            var comp_2_1 = go_2_1.AddComponent<RefClassTestComponent>();
            compRoot.Comp_Array = new Component[] { comp_2_1, compRoot, comp_1_1 };


            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            Assert.NotNull(de_2_1);

            var compRoot2 = root2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(compRoot2);
            Assert.AreNotSame(compRoot, compRoot2);
            var c_1_1 = de_1_1.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_1_1);
            Assert.AreNotSame(comp_1_1, c_1_1);
            var c_2_1 = de_2_1.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_2_1);
            Assert.AreNotSame(comp_2_1, c_2_1);

            //ensure our array of GOs is well and good
            Assert.NotNull(compRoot2.Comp_Array);
            Assert.AreEqual(compRoot2.Comp_Array.Length, 3);
            Assert.AreNotSame(compRoot2.Comp_Array, compRoot.Comp_Array);
            Assert.AreSame(compRoot2.Comp_Array[0], c_2_1);
            Assert.AreNotSame(c_2_1, comp_2_1);
            Assert.AreSame(compRoot2.Comp_Array[1], compRoot2);
            Assert.AreSame(compRoot2.Comp_Array[2], c_1_1);
            Assert.AreNotSame(comp_1_1, c_1_1);
        }

        /// <summary>
        /// Ensures List<Component> and IList<Component> serialize properly.
        /// </summary>
        [Test]
        public void TestListOfComponentRefs()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);

            //add test component and start storing arrays of GameObjects!
            var compRoot = root.AddComponent<RefClassTestComponent>();
            var comp_1_1 = go_1_1.AddComponent<RefClassTestComponent>();
            var comp_2_1 = go_2_1.AddComponent<RefClassTestComponent>();
            compRoot.Comp_List = new List<Component>(new Component[] { comp_2_1, compRoot, comp_1_1 });


            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            Assert.NotNull(de_2_1);

            var compRoot2 = root2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(compRoot2);
            Assert.AreNotSame(compRoot, compRoot2);
            var c_1_1 = de_1_1.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_1_1);
            Assert.AreNotSame(comp_1_1, c_1_1);
            var c_2_1 = de_2_1.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_2_1);
            Assert.AreNotSame(comp_2_1, c_2_1);

            //ensure our array of GOs is well and good
            Assert.NotNull(compRoot2.Comp_List);
            Assert.AreEqual(compRoot2.Comp_List.Count, 3);
            Assert.AreNotSame(compRoot2.Comp_List, compRoot.Comp_List);
            Assert.AreSame(compRoot2.Comp_List[0], c_2_1);
            Assert.AreNotSame(c_2_1, comp_2_1);
            Assert.AreSame(compRoot2.Comp_List[1], compRoot2);
            Assert.AreSame(compRoot2.Comp_List[2], c_1_1);
            Assert.AreNotSame(comp_1_1, c_1_1);
        }

        /// <summary>
        /// Ensures Dictionary<Component,string> and IDictionary<Component,string> serialize properly.
        /// </summary>
        [Test]
        public void TestDictionaryOfComponentStrRefs()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);

            //add test component and start storing arrays of GameObjects!
            var compRoot = root.AddComponent<RefClassTestComponent>();
            var comp_1_1 = go_1_1.AddComponent<RefClassTestComponent>();
            var comp_2_1 = go_2_1.AddComponent<RefClassTestComponent>();
            compRoot.Comp_Dic2 = new Dictionary<Component,string>();
            compRoot.Comp_Dic2.Add(comp_2_1, "broc");
            compRoot.Comp_Dic2.Add(compRoot, "morpheous");
            compRoot.Comp_Dic2.Add(comp_1_1, "helper");


            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            Assert.NotNull(de_2_1);

            var compRoot2 = root2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(compRoot2);
            Assert.AreNotSame(compRoot, compRoot2);
            var c_1_1 = de_1_1.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_1_1);
            Assert.AreNotSame(comp_1_1, c_1_1);
            var c_2_1 = de_2_1.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_2_1);
            Assert.AreNotSame(comp_2_1, c_2_1);

            //ensure our array of GOs is well and good
            Assert.NotNull(compRoot2.Comp_Dic2);
            Assert.AreEqual(compRoot2.Comp_Dic2.Count, 3);
            Assert.AreNotSame(compRoot2.Comp_Dic2, compRoot.Comp_Dic2);
            Assert.AreEqual(compRoot2.Comp_Dic2[c_2_1], "broc");
            Assert.AreNotSame(c_2_1, comp_2_1);
            Assert.AreEqual(compRoot2.Comp_Dic2[compRoot2], "morpheous");
            Assert.AreEqual(compRoot2.Comp_Dic2[c_1_1], "helper");
            Assert.AreNotSame(comp_1_1, c_1_1);
        }

        /// <summary>
        /// Ensures Dictionary<string,Component> and IDictionary<string,Component> serialize properly.
        /// </summary>
        [Test]
        public void TestDictionaryOfStrComponentRefs()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);

            //add test component and start storing arrays of GameObjects!
            var compRoot = root.AddComponent<RefClassTestComponent>();
            var comp_1_1 = go_1_1.AddComponent<RefClassTestComponent>();
            var comp_2_1 = go_2_1.AddComponent<RefClassTestComponent>();
            compRoot.Comp_Dic1 = new Dictionary<string, Component>();
            compRoot.Comp_Dic1.Add("broc", comp_2_1);
            compRoot.Comp_Dic1.Add("morpheous", compRoot);
            compRoot.Comp_Dic1.Add("helper", comp_1_1);


            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            Assert.NotNull(de_2_1);

            var compRoot2 = root2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(compRoot2);
            Assert.AreNotSame(compRoot, compRoot2);
            var c_1_1 = de_1_1.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_1_1);
            Assert.AreNotSame(comp_1_1, c_1_1);
            var c_2_1 = de_2_1.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_2_1);
            Assert.AreNotSame(comp_2_1, c_2_1);

            //ensure our array of GOs is well and good
            Assert.NotNull(compRoot2.Comp_Dic1);
            Assert.AreEqual(compRoot2.Comp_Dic1.Count, 3);
            Assert.AreNotSame(compRoot2.Comp_Dic1, compRoot.Comp_Dic1);
            Assert.AreSame(compRoot2.Comp_Dic1["broc"], c_2_1);
            Assert.AreNotSame(c_2_1, comp_2_1);
            Assert.AreSame(compRoot2.Comp_Dic1["morpheous"], compRoot2);
            Assert.AreSame(compRoot2.Comp_Dic1["helper"], c_1_1);
            Assert.AreNotSame(comp_1_1, c_1_1);
        }
        #endregion


        [Test]
        public void TestComponentDeserializer()
        {
            //create GO and give it some interesting values
            var go1 = GimmeSomething();

            //add a simple component that only has primitive variables
            var c1 = go1.AddComponent<SimpleTestComponent>();
            c1.PublicString = "Hiya";

            SimpleTestComponent c2 = CloneComponentWithDeserializer(c1);
            
            //check deserialized values
            Assert.NotNull(c2);
            Assert.AreNotEqual(c2, c1);

            Assert.AreEqual("Hiya", c1.PublicString);
            Assert.AreEqual("Hiya", c2.PublicString);
        }

        #region Super-special stuff
        //[Test]
        public void TestResources()
        {
            Assert.Fail();
        }

        //[Test]
        public void TestPrefabRefs()
        {
            Assert.Fail();
        }

        //[Test]
        public void TestInSceneGameObjects()
        {
            Assert.Fail();
        }

        //[Test]
        public void TestSingleComponentSave()
        {
            Assert.Fail();
        }
        #endregion
    }
}
