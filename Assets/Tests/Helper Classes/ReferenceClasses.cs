using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pantagruel.Serializer.Test
{
    /// <summary>
    /// Used by the unit test system for testing various
    /// complex reference serialization/deserialization senarios.
    /// </summary>
    public class RefBaseClass
    {
        //basic primitives
        public string IdString;
        public int Number;

        //potentially circular references
        public RefBaseClass Ref1;
        public RefBaseClass Ref2;

        //arrays and lists
        public RefBaseClass[] Array_Base;
        public IList<RefBaseClass> List_Base1;
        public List<RefBaseClass> List_Base2;

        //dictionaries - some of these get quite funky
        public Dictionary<string, int> Dic_StrInt;
        public IDictionary<int, string> IDic_IntStr;
        public Dictionary<string, RefBaseClass> Dic_StrBase;
        public Dictionary<RefBaseClass, string> Dic_BaseStr;
        public Dictionary<int, List<RefBaseClass>> Dic_StrList;

        //references to Unity objects
        public GameObject Go1;
        public Component Comp1;
        public MonoBehaviour Mono1;

        //lists and dictionaries of Unity objects
        public Dictionary<int, Component> Dic_IntComp;
        public List<Component> List_Comp;
    }

    /// <summary>
    /// Used by the unit test system for testing various
    /// complex reference serialization/deserialization senarios.
    /// </summary>
    public class RefDerivedClass : RefBaseClass
    {
        public int DerivedInt;
        public string DerivedString;
    }

}
