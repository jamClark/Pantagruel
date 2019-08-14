using UnityEngine;
using System.Collections;

namespace Pantagruel.Serializer.Test
{
    /// <summary>
    /// Simple class for testing delegate serialization.
    /// </summary>
    public class DelegateTestClass
    {
        public delegate void DelegateTest(string param);
        public DelegateTest Del;
        public DelegateTargetTestClass Target;
    }

    /// <summary>
    /// Dummy target class for delegate.
    /// </summary>
    public class DelegateTargetTestClass
    {
        public void MyHandler(string param)
        {
            Value = param;
        }

        public void MyHandler1(string nada)
        {
            Value += "_X";
        }

        public void MyHandler2(string param)
        {
            Value += "_O";
        }

        public string Value = "Not Changed.";
    }
}
