using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using Pantagruel.Serializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine.Events;

namespace Pantagruel.Serializer.Test
{
    
    /// <summary>
    /// Unit tests for checking serialization of UnityEvents in components.
    /// </summary>
    public class UnityEventTester : SerializerTesterBase
    {
        #region UnityEvents
        /// <summary>
        /// REMOVED AS TEST: It appears that there is no particulaly 
        /// easy way to get access to the non-persistent targets 
        /// and methods attached to these components so for now I 
        /// will follow Unity convention and simply not serializ/deserialize 
        /// the non-persistent listeners (the ones added dynamically in code).
        /// </summary>
        //[Test]
        public void TestUnityEvent_Empty()
        {
            //setup Go, component, and event
            var go = GimmeSomething();
            go.name = "Empty Event GameObject";
            var comp = go.AddComponent<UnityEventTestComponent>();
            comp.Event_Empty = new UnityEvent();
            comp.Event_Empty.AddListener(new UnityAction(comp.Empty_Func));

            //test event to make sure it is working
            Assert.AreEqual("Has not been changed.", comp.GlobalDebugField);
            //Assert.AreEqual(1, comp.Event_Empty.GetPersistentEventCount());
            comp.Event_Empty.Invoke();
            Assert.AreEqual("empty func called by: " + go.name, comp.GlobalDebugField);
            
            //Ser/Des
            var go2 = CloneWithDeserializer(go);
            Assert.NotNull(go2);
            Assert.AreNotSame(go, go2);

            //test deserielized event to make sure it is still working
            var comp2 = go2.GetComponent<UnityEventTestComponent>();
            Assert.NotNull(comp2);
            Assert.NotNull(comp2.Event_Empty);
            Assert.AreNotSame(comp.Event_Empty, comp2.Event_Empty);

            Assert.AreEqual("Has not been changed.", comp2.GlobalDebugField);
            //Assert.AreEqual(1, comp2.Event_Empty.GetPersistentEventCount());
            comp2.Event_Empty.Invoke();
            Assert.AreEqual("empty func called by: " + go2.name, comp2.GlobalDebugField);
        }
        
        #endregion
    }
}
