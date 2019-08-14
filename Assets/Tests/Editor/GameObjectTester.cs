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
    public class GameObjectTester : SerializerTesterBase
    {
        #region GameObject Hierarchy tests
        /// <summary>
        /// Tests if a single GameObject is correctly Ser/Desed.
        /// </summary>
        [Test]
        public void TestBasicGameObject()
        {
            //create GO and give it some interesting values
            var go1 = GimmeSomething();
            MakeInteresting(go1);
            var go2 = CloneWithDeserializer(go1);
            
            //is new object valid?
            Assert.NotNull(go2);

            //is new object NOT old object?
            Assert.AreNotEqual(go1, go2);

            //new object has same values as old?
            Assert.AreEqual(go1.name, go2.name);
            Assert.AreEqual(go1.tag, go2.tag);
            Assert.AreEqual(go1.layer, go2.layer);
            Assert.AreEqual(go1.isStatic, go2.isStatic);
            Assert.AreEqual(go1.activeSelf, go2.activeSelf);

            //confirm a wacky value that was supposedly given earlier is actually present
            Assert.AreEqual(go1.tag, "Player");
            Assert.AreEqual(go2.tag, "Player");
            
        }

        /// <summary>
        /// Ensures a that a GameObject's Transform is properly Ser/Desed.
        /// It also indirectly tests the TransformSurrogate.
        /// </summary>
        [Test]
        public void TestGameObjectTransform()
        {
            //create GO and give it some interesting values
            var go1 = GimmeSomething();
            MakeInteresting(go1);
            go1.transform.position = new Vector3(3.0f, 2.1f, 1.1f);
            go1.transform.localEulerAngles = new Vector3(25.0f, 35.1f, 90.0f);
            go1.transform.localScale = new Vector3(2.3f, 1.4f, 3.0f);

            var go2 = CloneWithDeserializer(go1);
            
            //is new object NOT old object?
            Assert.AreNotEqual(go1, go2);

            //check deserialized values
            var trans2 = go2.GetComponent<Transform>();
            Assert.NotNull(trans2);
            Assert.AreEqual(trans2.position, new Vector3(3.0f, 2.1f, 1.1f));
            Assert.AreEqual(trans2.localEulerAngles, new Vector3(25.0f, 35.1f, 90.0f));
            Assert.AreEqual(trans2.localScale, new Vector3(2.3f, 1.4f, 3.0f));

            Assert.False(ReferenceEquals(go2.transform.position, go1.transform.position));
        }

        /// <summary>
        /// Ensures that a GameObject hierarchy maintains it's proper
        /// child/parent relations after deserialization.
        /// </summary>
        [Test]
        public void TestGameObjectHierarchy()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            var go_2_2 = GimmeSomething();
            go_1_1.name = "Level 1";
            go_2_1.name = "Level 2-1";
            go_2_2.name = "Level 2-2";
            go_2_2.SetActive(false);
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);
            go_2_2.transform.SetParent(go_1_1.transform);

            var root2 = CloneWithDeserializer(root);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            var de_2_2 = de_1_1.transform.GetChild(1).gameObject;
            Assert.AreNotSame(de_1_1, go_1_1);
            Assert.AreNotSame(de_2_1, go_2_1);
            Assert.AreNotSame(de_2_2, go_2_2);

            Assert.AreEqual(de_1_1.name, go_1_1.name);
            Assert.AreEqual(de_2_1.name, "Level 2-1");
            Assert.AreEqual(de_2_2.activeSelf, false);

            GameObject go3 = root;
            Assert.AreSame(go3, root);
            

        }
        
        /// <summary>
        /// Ensures that serializer ignores references to GameObjects and components
        /// outside of the hierarchy being acted on.
        /// </summary>
        [Test]
        public void TestReferencesOutsideHierarchy()
        {
            var root = GimmeSomething();
            var comp = root.AddComponent<RefClassTestComponent>();
            

            var ignoredGo = GimmeSomething();
            comp.BaseClass = new RefBaseClass();
            comp.BaseClass.Go1 = ignoredGo;
            comp.BaseClass.Comp1 = ignoredGo.transform;

            Assert.AreSame(ignoredGo, comp.BaseClass.Go1);
            Assert.AreSame(ignoredGo.transform, comp.BaseClass.Comp1);

            GameObject go2 = CloneWithDeserializer(root);
            var comp2 = go2.GetComponent<RefClassTestComponent>();

            Assert.NotNull(comp2);
            Assert.NotNull(comp2.BaseClass);
            
            Assert.AreNotSame(go2, root);
            Assert.AreNotSame(comp, comp2);
            Assert.AreNotSame(comp2.BaseClass, comp.BaseClass);

            Assert.NotNull(comp.BaseClass.Go1);
            Assert.NotNull(comp.BaseClass.Comp1);
            Assert.Null(comp2.BaseClass.Go1);
            Assert.Null(comp2.BaseClass.Comp1);
        }

        /// <summary>
        /// Ensures that references to GameObjects and components within the
        /// hierarchy are properly Ser/Desed.
        /// </summary>
        [Test]
        public void TestReferenceWithinHierarchy()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            var go_2_2 = GimmeSomething();
            go_1_1.name = "Level 1";
            go_2_1.name = "Level 2-1";
            go_2_2.name = "Level 2-2";
            go_2_2.SetActive(false);
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);
            go_2_2.transform.SetParent(go_1_1.transform);

            //add components to the data that will be serialized
            var compRoot = root.AddComponent<RefClassTestComponent>();
            var comp_1_1 = go_1_1.AddComponent<RefClassTestComponent>();
            var comp_2_1 = go_2_1.AddComponent<RefClassTestComponent>();
            var comp_2_2 = go_2_2.AddComponent<RefClassTestComponent>();
            compRoot.BaseClass = new RefBaseClass();
            comp_1_1.BaseClass = new RefBaseClass();
            comp_2_1.BaseClass = new RefBaseClass();
            comp_2_2.BaseClass = new RefBaseClass();

            //give the components some interesting values
            compRoot.BaseClass.Go1      = go_2_2; //in-place serialization
            compRoot.BaseClass.Comp1    = comp_2_2; //example of defered component serialization

            comp_1_1.BaseClass.Go1      = root; //id-based shared reference
            comp_1_1.BaseClass.Comp1    = compRoot; //will be refering to a component that is currently being serialized

            comp_2_1.BaseClass.Go1      = go_2_1;
            comp_2_1.BaseClass.Comp1    = comp_2_2;

            comp_2_2.BaseClass.Go1      = go_2_2;
            comp_2_2.BaseClass.Comp1    = comp_2_2;


            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            var de_2_2 = de_1_1.transform.GetChild(1).gameObject;
            Assert.NotNull(de_2_1);
            Assert.NotNull(de_2_2);

            var c_root = root2.GetComponent<RefClassTestComponent>();
            var c_1_1 = de_1_1.GetComponent<RefClassTestComponent>();
            var c_2_1 = de_2_1.GetComponent<RefClassTestComponent>();
            var c_2_2 = de_2_2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_root);
            Assert.NotNull(c_1_1);
            Assert.NotNull(c_2_1);
            Assert.NotNull(c_2_2);

            //make sure all of our BaseClass instances are unique
            Assert.AreNotSame(c_root.BaseClass, c_1_1.BaseClass);
            Assert.AreNotSame(c_root.BaseClass, c_2_1.BaseClass);
            Assert.AreNotSame(c_root.BaseClass, c_2_2.BaseClass);

            Assert.AreNotSame(c_1_1.BaseClass, c_root.BaseClass);
            Assert.AreNotSame(c_1_1.BaseClass, c_2_1.BaseClass);
            Assert.AreNotSame(c_1_1.BaseClass, c_2_2.BaseClass);

            Assert.AreNotSame(c_2_1.BaseClass, c_1_1.BaseClass);
            Assert.AreNotSame(c_2_1.BaseClass, c_root.BaseClass);
            Assert.AreNotSame(c_2_1.BaseClass, c_2_2.BaseClass);

            Assert.AreNotSame(c_2_2.BaseClass, c_1_1.BaseClass);
            Assert.AreNotSame(c_2_2.BaseClass, c_2_1.BaseClass);
            Assert.AreNotSame(c_2_2.BaseClass, c_root.BaseClass);

            //ensure our references to other GameObjects and components in the hierarchy are intact
            Assert.AreSame(c_root.BaseClass.Go1, de_2_2);
            Assert.AreSame(c_root.BaseClass.Comp1, c_2_2);

            Assert.AreSame(c_1_1.BaseClass.Go1, root2);
            Assert.AreSame(c_1_1.BaseClass.Comp1, c_root);

            Assert.AreSame(c_2_1.BaseClass.Go1, de_2_1);
            Assert.AreSame(c_2_1.BaseClass.Comp1, c_2_2);

            Assert.AreSame(c_2_2.BaseClass.Go1, de_2_2);
            Assert.AreSame(c_2_2.BaseClass.Comp1, c_2_2);

        }

        /// <summary>
        /// Ensures that circular GameObject references throughout the hierachy's
        /// list of components are properly preserved after deserialization.
        /// </summary>
        [Test]
        public void TestCircularGameObjectReferences()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            go_1_1.name = "Level 1";
            go_2_1.name = "Level 2-1";
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);

            //add components to the data that will be serialized
            var compRoot = root.AddComponent<RefClassTestComponent>();
            var comp_1_1 = go_1_1.AddComponent<RefClassTestComponent>();
            var comp_2_1 = go_2_1.AddComponent<RefClassTestComponent>();
            compRoot.BaseClass = new RefBaseClass();
            comp_1_1.BaseClass = new RefBaseClass();
            comp_2_1.BaseClass = new RefBaseClass();

            //give the components some interesting values
            compRoot.BaseClass.Go1 = go_1_1; //in-place serialization
            comp_1_1.BaseClass.Go1 = go_2_1; //id-based shared reference
            comp_2_1.BaseClass.Go1 = root;
            

            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            Assert.NotNull(de_2_1);

            var c_root = root2.GetComponent<RefClassTestComponent>();
            var c_1_1 = de_1_1.GetComponent<RefClassTestComponent>();
            var c_2_1 = de_2_1.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_root);
            Assert.NotNull(c_1_1);
            Assert.NotNull(c_2_1);

            //make sure all of our BaseClass instances are unique
            Assert.AreNotSame(c_root.BaseClass, c_1_1.BaseClass);
            Assert.AreNotSame(c_root.BaseClass, c_2_1.BaseClass);

            Assert.AreNotSame(c_1_1.BaseClass, c_root.BaseClass);
            Assert.AreNotSame(c_1_1.BaseClass, c_2_1.BaseClass);

            Assert.AreNotSame(c_2_1.BaseClass, c_1_1.BaseClass);
            Assert.AreNotSame(c_2_1.BaseClass, c_root.BaseClass);
            

            //ensure our references to other GameObjects in the hierarchy are intact
            Assert.AreSame(c_root.BaseClass.Go1, de_1_1);
            Assert.AreSame(c_1_1.BaseClass.Go1, de_2_1);
            Assert.AreSame(c_2_1.BaseClass.Go1, root2);
        }

        /// <summary>
        /// Ensures that circular Component references throughout the hierachy's
        /// list of components are properly preserved after deserialization.
        /// </summary>
        [Test]
        public void TestCircularComponentReferences()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            go_1_1.name = "Level 1";
            go_2_1.name = "Level 2-1";
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);

            //add components to the data that will be serialized
            var compRoot = root.AddComponent<RefClassTestComponent>();
            var comp_1_1 = go_1_1.AddComponent<RefClassTestComponent>();
            var comp_2_1 = go_2_1.AddComponent<RefClassTestComponent>();
            compRoot.BaseClass = new RefBaseClass();
            comp_1_1.BaseClass = new RefBaseClass();
            comp_2_1.BaseClass = new RefBaseClass();

            //give the components some interesting values
            compRoot.BaseClass.Comp1 = comp_1_1; //example of defered component serialization
            comp_1_1.BaseClass.Comp1 = comp_2_1; //will be refering to a component that is currently being serialized
            comp_2_1.BaseClass.Comp1 = compRoot;


            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            Assert.NotNull(de_2_1);

            var c_root = root2.GetComponent<RefClassTestComponent>();
            var c_1_1 = de_1_1.GetComponent<RefClassTestComponent>();
            var c_2_1 = de_2_1.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_root);
            Assert.NotNull(c_1_1);
            Assert.NotNull(c_2_1);

            //make sure all of our BaseClass instances are unique
            Assert.AreNotSame(c_root.BaseClass, c_1_1.BaseClass);
            Assert.AreNotSame(c_root.BaseClass, c_2_1.BaseClass);

            Assert.AreNotSame(c_1_1.BaseClass, c_root.BaseClass);
            Assert.AreNotSame(c_1_1.BaseClass, c_2_1.BaseClass);

            Assert.AreNotSame(c_2_1.BaseClass, c_1_1.BaseClass);
            Assert.AreNotSame(c_2_1.BaseClass, c_root.BaseClass);


            //ensure our references to other Components in the hierarchy are intact
            Assert.AreSame(c_root.BaseClass.Comp1, c_1_1);
            Assert.AreSame(c_1_1.BaseClass.Comp1, c_2_1);
            Assert.AreSame(c_2_1.BaseClass.Comp1, c_root);

            Assert.AreSame(de_1_1, c_1_1.gameObject);
            Assert.AreSame(de_2_1, c_2_1.gameObject);
            Assert.AreSame(root2, c_root.gameObject);
        }

        /// <summary>
        /// Ensures that circular GameObject/Component references throughout the hierachy's
        /// list of components are properly preserved after deserialization.
        /// </summary>
        [Test]
        public void TestCircularGameObjectAndComponentReferences()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            go_1_1.name = "Level 1";
            go_2_1.name = "Level 2-1";
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);

            //add components to the data that will be serialized
            var compRoot = root.AddComponent<RefClassTestComponent>();
            var comp_1_1 = go_1_1.AddComponent<RefClassTestComponent>();
            var comp_2_1 = go_2_1.AddComponent<RefClassTestComponent>();
            compRoot.BaseClass = new RefBaseClass();
            comp_1_1.BaseClass = new RefBaseClass();
            comp_2_1.BaseClass = new RefBaseClass();

            //give the components some interesting values
            compRoot.BaseClass.Go1 = go_1_1; //in-place serialization
            compRoot.BaseClass.Comp1 = comp_1_1; //example of defered component serialization

            comp_1_1.BaseClass.Go1 = go_2_1; //id-based shared reference
            comp_1_1.BaseClass.Comp1 = comp_2_1; //will be refering to a component that is currently being serialized

            comp_2_1.BaseClass.Go1 = root;
            comp_2_1.BaseClass.Comp1 = compRoot;


            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            Assert.NotNull(de_2_1);

            var c_root = root2.GetComponent<RefClassTestComponent>();
            var c_1_1 = de_1_1.GetComponent<RefClassTestComponent>();
            var c_2_1 = de_2_1.GetComponent<RefClassTestComponent>();
            Assert.NotNull(c_root);
            Assert.NotNull(c_1_1);
            Assert.NotNull(c_2_1);

            //make sure all of our BaseClass instances are unique
            Assert.AreNotSame(c_root.BaseClass, c_1_1.BaseClass);
            Assert.AreNotSame(c_root.BaseClass, c_2_1.BaseClass);

            Assert.AreNotSame(c_1_1.BaseClass, c_root.BaseClass);
            Assert.AreNotSame(c_1_1.BaseClass, c_2_1.BaseClass);

            Assert.AreNotSame(c_2_1.BaseClass, c_1_1.BaseClass);
            Assert.AreNotSame(c_2_1.BaseClass, c_root.BaseClass);


            //ensure our references to other GameObjects and components in the hierarchy are intact
            Assert.AreSame(c_root.BaseClass.Go1, de_1_1);
            Assert.AreSame(c_root.BaseClass.Comp1, c_1_1);

            Assert.AreSame(c_1_1.BaseClass.Go1, de_2_1);
            Assert.AreSame(c_1_1.BaseClass.Comp1, c_2_1);

            Assert.AreSame(c_2_1.BaseClass.Go1, root2);
            Assert.AreSame(c_2_1.BaseClass.Comp1, c_root);
        }

        #endregion


        #region Lists and Dictionaries
        /// <summary>
        /// Ensures GameObject[] serializes properly.
        /// </summary>
        [Test]
        public void TestArrayOfGameObjectRefs()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);

            //add test component and start storing arrays of GameObjects!
            var comp1 = root.AddComponent<RefClassTestComponent>();
            comp1.Go_Array = new GameObject[] { go_2_1, root, go_1_1};


            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            Assert.NotNull(de_2_1);

            var comp2 = root2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(comp2);
            Assert.AreNotSame(comp1, comp2);

            //ensure our array of GOs is well and good
            Assert.NotNull(comp2.Go_Array);
            Assert.AreEqual(comp2.Go_Array.Length, 3);
            Assert.AreNotSame(comp2.Go_Array, comp1.Go_Array);
            Assert.AreSame(comp2.Go_Array[0], de_2_1);
            Assert.AreNotSame(de_2_1, go_2_1);
            Assert.AreSame(comp2.Go_Array[1], root2);
            Assert.AreSame(comp2.Go_Array[2], de_1_1);
            Assert.AreNotSame(de_1_1, go_1_1);
        }

        /// <summary>
        /// Ensures List<GameObject> and IList<GameObject> serialize properly.
        /// </summary>
        [Test]
        public void TestListOfGameObjecttRefs()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);

            //add test component and start storing arrays of GameObjects!
            var comp1 = root.AddComponent<RefClassTestComponent>();
            comp1.Go_List = new List<GameObject>(new GameObject[] { go_2_1, root, go_1_1 });


            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            Assert.NotNull(de_2_1);

            var comp2 = root2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(comp2);
            Assert.AreNotSame(comp1, comp2);

            //ensure our array of GOs is well and good
            Assert.NotNull(comp2.Go_List);
            Assert.AreEqual(comp2.Go_List.Count, 3);
            Assert.AreNotSame(comp2.Go_List, comp1.Go_List);
            Assert.AreSame(comp2.Go_List[0], de_2_1);
            Assert.AreNotSame(de_2_1, go_2_1);
            Assert.AreSame(comp2.Go_List[1], root2);
            Assert.AreSame(comp2.Go_List[2], de_1_1);
            Assert.AreNotSame(de_1_1, go_1_1);
        }

        /// <summary>
        /// Ensures Dictionary<GameObject,string> and IDictionary<GameObject,string> serialize properly.
        /// </summary>
        [Test]
        public void TestDictionaryOfGameObjectStrRefs()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);

            //add test component and start storing arrays of GameObjects!
            var comp1 = root.AddComponent<RefClassTestComponent>();
            comp1.Go_Dic2 = new Dictionary<GameObject, string>();
            comp1.Go_Dic2.Add(go_2_1, "hank");
            comp1.Go_Dic2.Add(root, "rusty");
            comp1.Go_Dic2.Add(go_1_1, "dean");


            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            Assert.NotNull(de_2_1);

            var comp2 = root2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(comp2);
            Assert.AreNotSame(comp1, comp2);

            //ensure our array of GOs is well and good
            Assert.NotNull(comp2.Go_Dic2);
            Assert.AreEqual(comp2.Go_Dic2.Count, 3);
            Assert.AreNotSame(comp2.Go_Dic2, comp1.Go_Dic2);
            Assert.AreEqual(comp2.Go_Dic2[de_2_1], "hank");
            Assert.AreNotSame(comp2.Go_Dic2[de_2_1], comp1.Go_Dic2[go_2_1]);
            Assert.AreEqual(comp2.Go_Dic2[root2], "rusty");
            Assert.AreNotSame(comp2.Go_Dic2[root2], comp1.Go_Dic2[root]);
            Assert.AreEqual(comp2.Go_Dic2[de_1_1], "dean");
            Assert.AreNotSame(comp2.Go_Dic2[de_1_1], comp1.Go_Dic2[go_1_1]);
        }

        /// <summary>
        /// Ensures Dictionary<string,GameObject> and IDictionaryt<string,GameObject> serialize properly.
        /// </summary>
        [Test]
        public void TestDictionaryOfStrGameObjectRefs()
        {
            //create GO and give it some interesting values
            var root = GimmeSomething();
            var go_1_1 = GimmeSomething();
            var go_2_1 = GimmeSomething();
            go_1_1.transform.SetParent(root.transform);
            go_2_1.transform.SetParent(go_1_1.transform);

            //add test component and start storing arrays of GameObjects!
            var comp1 = root.AddComponent<RefClassTestComponent>();
            comp1.Go_Dic1 = new Dictionary<string, GameObject>();
            comp1.Go_Dic1.Add("hank", go_2_1);
            comp1.Go_Dic1.Add("rusty", root);
            comp1.Go_Dic1.Add("dean", go_1_1);


            //get a deserialized copy of all the crap above
            var root2 = CloneWithDeserializer(root);
            Assert.NotNull(root2);
            Assert.AreNotSame(root2, root);

            var de_1_1 = root2.transform.GetChild(0).gameObject;
            Assert.NotNull(de_1_1);
            var de_2_1 = de_1_1.transform.GetChild(0).gameObject;
            Assert.NotNull(de_2_1);

            var comp2 = root2.GetComponent<RefClassTestComponent>();
            Assert.NotNull(comp2);
            Assert.AreNotSame(comp1, comp2);

            //ensure our array of GOs is well and good
            Assert.NotNull(comp2.Go_Dic1);
            Assert.AreEqual(comp2.Go_Dic1.Count, 3);
            Assert.AreNotSame(comp2.Go_Dic1, comp1.Go_Dic1);
            Assert.AreSame(comp2.Go_Dic1["hank"], de_2_1);
            Assert.AreNotSame(comp2.Go_Dic1["hank"], comp1.Go_Dic1["hank"]);
            Assert.AreSame(comp2.Go_Dic1["rusty"], root2);
            Assert.AreNotSame(comp2.Go_Dic1["rusty"], comp1.Go_Dic1["rusty"]);
            Assert.AreSame(comp2.Go_Dic1["dean"], de_1_1);
            Assert.AreNotSame(comp2.Go_Dic1["dean"], comp1.Go_Dic1["dean"]);
        }
        #endregion

    }
}
