/**********************************************
* Pantagruel
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using System.Collections;

namespace Pantagruel.Serializer
{
    /// <summary>
    /// Simple dummy component used by the serializer to identify
    /// GameObjects as prefab references and not simply normal scene objects.
    /// </summary>
    [ExecuteInEditMode]
    public class PrefabId : MonoBehaviour
    {
        public string ManifestId;

        void Reset()
        {
            ManifestId = gameObject.name;
        }

        void Awake()
        {
            //We never want runtime instance to have this component.
            //Only prefabs should be allowed to have this.
            Destroy(this);
        }
        
    }
}
