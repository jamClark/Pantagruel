using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pantagruel.Serializer.Test
{
    /// <summary>
    /// Component used for testing serialization of Dictionaries.
    /// </summary>
    public class TestDictionaryComponent : MonoBehaviour
    {
        public Dictionary<string, int> Dic_StrInt;
        
        public void Init()
        {
            Dic_StrInt = new Dictionary<string, int>();
            Dic_StrInt["superman"] = 1;
            Dic_StrInt["batman"] = 2;
            Dic_StrInt["wonder woman"] = 3;
            Dic_StrInt["martian manhunter"] = 4;
            Dic_StrInt["black canary"] = 10;
            Dic_StrInt["green arrow"] = 20;
        } 
    }
}
