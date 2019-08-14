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
    public abstract class SerializerTesterBase
    {
        protected string SavePath = Application.dataPath + System.IO.Path.DirectorySeparatorChar + "unit_test_file.xml";


        #region Utilities
        /// <summary>
        /// Provides a GameObject that can be cleaned up at the end of the test and
        /// has the given components attached to it.
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        public GameObject GimmeSomething(params Type[] components)
        {
            var go = new GameObject("Test GameObject");
            if (components != null)
            {
                foreach (var t in components) go.AddComponent(t);
            }

            Undo.RegisterCreatedObjectUndo(go, "Created test GameObject");
            return go;
        }

        /// <summary>
        /// Serializes the source GameObject to an XmlDocument and
        /// then deserializes that document to create an (hopefully)
        /// exact clone of the source object.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public GameObject CloneWithDeserializer(GameObject source)
        {
            XmlDocument doc = null;
            try
            {
                doc = XmlSerializer.Serialize(source, 1);
            }
            catch (Exception e)
            {
                Assert.Fail("There was an exception in the serialization process.\n" + e.Message);
            }
            GameObject go2 = null;
            try
            {
                go2 = XmlDeserializer.Deserialize(doc.InnerXml, 1) as GameObject;
            }
            catch (Exception e)
            {
                Assert.Fail("There was an exception in the deserialization process.\n" +
                    e.Message +
                    "\n--------------------\n" + e.StackTrace);
            }
            if (go2 != null) Undo.RegisterCreatedObjectUndo(go2, "Deserialized test GameObject");
            else Assert.Fail("Failed to deserialize back into a GameObject.");
            return go2;
        }

        /// <summary>
        /// Serializes the source GameObject to an XmlDocument and
        /// then deserializes that document to create an (hopefully)
        /// exact clone of the source object.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public T CloneComponentWithDeserializer<T>(T source) where T : Component
        {
            T dest = null;
            XmlDocument doc = null;
            try
            {
                doc = XmlSerializer.Serialize(source, 1, "Component");
            }
            catch (Exception e)
            {
                Assert.Fail("There was an exception in the serialization process.\n'" + e.Message+"'");
            }
            GameObject go2 = new GameObject();
            try
            {
                dest = XmlDeserializer.DeserializeComponent<T>(doc.InnerXml, 1, go2);
            }
            catch (Exception e)
            {
                Assert.Fail("There was an exception in the deserialization process.\n" +
                    e.Message +
                    "\n--------------------\n" + e.StackTrace);
            }
            if (go2 != null) Undo.RegisterCreatedObjectUndo(go2, "Deserialized test GameObject");
            else Assert.Fail("Failed to deserialize back into a GameObject.");
            return dest;
        }

        /// <summary>
        /// Supplies non-default but valid values for various GameObject fields that are serialized.
        /// This is a helper method for quickly making GameObjects with values different from
        /// the defaults so that they can be tested upon deserialization.
        /// </summary>
        /// <param name="go"></param>
        public void MakeInteresting(GameObject go)
        {
            //go.name = "Tester GO 1";
            go.tag = "Player";
            go.layer = UnityEngine.Random.Range(0, 32);

            if (UnityEngine.Random.Range(0, 2) == 0) go.isStatic = false;
            else go.isStatic = true;

            if (UnityEngine.Random.Range(0, 2) == 0) go.SetActive(false);
            else go.SetActive(true);
        }

        /// <summary>
        /// Uses reflection to access public and privates fields of an object.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static object GetValue(string fieldName, object source)
        {
            Type t = source.GetType();
            FieldInfo field = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) return field.GetValue(source);
            return null;
        }
        #endregion

        //[Test]
        public void TestXmlFileSave()
        {
            Debug.ClearDeveloperConsole();//this does nothing apparently :/
            Dictionary<string, int> Dic_StrInt;
            Dic_StrInt = new Dictionary<string, int>();
            Dic_StrInt["superman"] = 1;
            Dic_StrInt["batman"] = 2;
            Dic_StrInt["wonder woman"] = 3;
            Dic_StrInt["martian manhunter"] = 4;
            Dic_StrInt["black canary"] = 10;
            Dic_StrInt["green arrow"] = 20;
            XmlDocument doc = XmlSerializer.Serialize(Dic_StrInt, 1, "league_ranks");

            doc.Save(SavePath);
        }

    }
}
