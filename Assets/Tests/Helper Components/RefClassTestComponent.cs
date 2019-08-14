using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pantagruel.Serializer.Test
{
    /// <summary>
    /// Used by the unit test system for testing various
    /// complex reference serialization/deserialization senarios.
    /// </summary>
    public class RefClassTestComponent : MonoBehaviour
    {
        public RefBaseClass BaseClass;
        public RefDerivedClass DerivedClass;
        public RefBaseClass DerivedClassCastToBase;

        public GameObject[] Go_Array;
        public List<GameObject> Go_List;
        public Dictionary<string, GameObject> Go_Dic1;
        public Dictionary<GameObject, string> Go_Dic2;

        public Component[] Comp_Array;
        public List<Component> Comp_List;
        public Dictionary<string, Component> Comp_Dic1;
        public Dictionary<Component, string> Comp_Dic2;

    }
}
