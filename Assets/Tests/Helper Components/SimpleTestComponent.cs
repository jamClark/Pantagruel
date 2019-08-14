using UnityEngine;
using System.Collections;
using System;

namespace Pantagruel.Serializer.Test
{
    /// <summary>
    /// Simple component with no references. For unit testing purposes.
    /// </summary>
    public class SimpleTestComponent : MonoBehaviour
    {
#pragma warning disable 0414 // assigned but never used
        public string PublicString = "String Value 1.";
        public int PublicInteger = 11;
        [SerializeField]
        private float SerializedPrivateFloat = 2.0f;

        [NonSerialized]
        public int NonSerializedPublicInt = 250;
        private int NonSerializedPrivateInt = 200;
#pragma warning restore 0414 // assigned but never used

        public void SetPrivateValues(float serializedFloat, int nonserializedInt)
        {
            SerializedPrivateFloat = serializedFloat;
            NonSerializedPrivateInt = nonserializedInt;
        }
        
    }
}
