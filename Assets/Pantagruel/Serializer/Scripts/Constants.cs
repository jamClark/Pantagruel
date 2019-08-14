/**********************************************
* Pantagruel
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using System.Collections;
using System;

namespace Pantagruel.Serializer
{
    /// <summary>
    /// Public shared constants of Pantagruel.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// This is the root path used by all edit-time asset builders.
        /// If you re-locate or rename the 'Pantagruel' project folder then you
        /// must change this to reflect it otherwise unexpected behaviors might arise.
        /// </summary>
        public static readonly string RootPath = "Pantagruel/";

        /// <summary>
        /// Path to store the resource manifests when the library is built at edit time.
        /// </summary>
        public static readonly string ManifestPath = RootPath + "Serializer/Resources/Manifests/";
        
        /// <summary>
        /// This is a list of UnityEngine.Object sub-types that should be treated
        /// as 'Resource reference' types. That is, the system will try to serialize
        /// a string that can be used by Resources.Load() at runtime to deserialize
        /// the reference. This is also the list of types that are supported by the
        /// Resource Manifest Library builder.
        /// </summary>
        public static readonly Type[] ResourceTypes = new Type[] 
        {
            typeof(AnimationClip),
            typeof(RuntimeAnimatorController),
            typeof(AudioClip),
            typeof(Font),
            typeof(GameObject),
            typeof(Material),
            typeof(Mesh),
            typeof(PhysicMaterial),
            typeof(PhysicsMaterial2D),
            typeof(Shader),
            typeof(Sprite),
            typeof(Texture2D),
            typeof(Texture3D),
        };

    }
}
