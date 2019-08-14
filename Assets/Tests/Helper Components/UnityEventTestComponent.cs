using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

namespace Pantagruel.Serializer.Test
{
    /// <summary>
    /// A component used for unit testing the serializer on
    /// UnityEvents.
    /// </summary>
    public class UnityEventTestComponent : MonoBehaviour
    {
        #region Inner Classes
        public class SimpleClass
        {
            public int Value;
        }

        public class SimpleGenericClass<T>
        {
            public T Value;
        }


        public class CustomEventClass_Empty : UnityEvent { }
        public class CustomEventClass_Str : UnityEvent<string> { }
        public class CustomEventClass_Class : UnityEvent<SimpleClass> { }
        public class CustomEventClass_Generic_Int : UnityEvent<SimpleGenericClass<int>> { }
        #endregion


        #region Fields
        public UnityEvent Event_Empty;
        public UnityEvent<string> Event_String;
        public UnityEvent<SimpleClass> Event_Class;
        public UnityEvent<SimpleGenericClass<int>> Event_Generic_Str;

        public CustomEventClass_Empty Custom_Empty;
        public CustomEventClass_Str Custom_Str;
        public CustomEventClass_Class Custom_Class;
        public CustomEventClass_Generic_Int Custom_Generic_Int;
        #endregion

        public string GlobalDebugField = "Has not been changed.";

        #region Methods
        public void Empty_Func()
        {
            GlobalDebugField = "empty func called by: " + gameObject.name;
        }
        #endregion

    }
}
