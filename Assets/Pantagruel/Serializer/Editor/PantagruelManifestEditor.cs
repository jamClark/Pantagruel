/**********************************************
* Pantagruel
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using Pantagruel.Serializer;

namespace Pantagruel.Serializer.Editor
{
    
    /// <summary>
    /// Editor for generating and manipulating the resource manifest prefab
    /// that Pantagruel requires for serializing resource references correctly.
    /// </summary>
    public class PantagruelManifestEditor : EditorWindow
    {
        double LastBuildTime = 0.0;

        #region Unity Events
        [MenuItem("Window/Pantagruel/Resource Manifest")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<PantagruelManifestEditor>("Resource Manifest");
        }

        void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }

        void OnGUI()
        {
            GUILayout.Space(15);
            //The debug logs are actually useless since it will be delayed until
            //the entire OnGUI event is done. We should probably move the Building
            //to another thread.
            
            if (GUILayout.Button("Build Resource Manifest Library\n(Fast)", GUILayout.Height(45)))
            {
                Debug.Log("<color=blue>Building manifest library. Please wait...</color>");

                double start = EditorApplication.timeSinceStartup;
                //TODO: Delete all old manifest files
                Pantagruel.Editor.Utility.ConfirmAssetDirectory(Constants.ManifestPath);
                BuildManifestLibrary(Constants.ManifestPath, false, Constants.ResourceTypes);
                LastBuildTime = EditorApplication.timeSinceStartup - start;

                Debug.Log("<color=blue>Manifest library complete. All manifest assets can be found in</color> Assets/" + Constants.ManifestPath + "");

            }
            GUILayout.Space(25);
            if(GUILayout.Button("Build ResourceManifest Library\n(Slow but Safe)", GUILayout.Height(45)))
            {
                Debug.Log("<color=blue>Building manifest library. Please wait...</color>");

                double start = EditorApplication.timeSinceStartup;
                //TODO: Delete all old manifest files
                Pantagruel.Editor.Utility.ConfirmAssetDirectory(Constants.ManifestPath);
                BuildManifestLibrary(Constants.ManifestPath, true, Constants.ResourceTypes);
                LastBuildTime = EditorApplication.timeSinceStartup - start;

                Debug.Log("<color=blue>Manifest library complete. All manifest assets can be found in</color> Assets/" + Constants.ManifestPath + "");

            }

            
            GUILayout.Space(25);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Build Time:");
            GUILayout.Label(LastBuildTime.ToString());
            GUILayout.EndHorizontal();
        }
        #endregion


        #region Static Methods
        /// <summary>
        /// This forces the asset database to save any asset files associated with
        /// //these manifest using the latest state. Effectively it is 'saving the files'.
        /// </summary>
        /// <param name="manifests"></param>
        static void PushObjectsToAssets(List<ResourceManifest> manifests)
        {
            if (manifests == null || manifests.Count < 1) return;

            //TODO: ensure manifest files exist

            foreach (var manifest in manifests)
            {
                EditorUtility.SetDirty(manifest);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        static ResourceManifest CreateManifestAsset(string path)
        {
            ResourceManifest manifest = ScriptableObject.CreateInstance<ResourceManifest>();
            AssetDatabase.CreateAsset(manifest, "Assets/" + path);
            return manifest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static List<ResourceManifest> BuildManifestLibrary(string manifestPath, bool safetyChecks, params Type[] supportedTypes)
        { 
            List<ResourceManifest> list = new List<ResourceManifest>(10);
            Dictionary<string, ResourceManifest> map = new Dictionary<string, ResourceManifest>(10);

            //-remove all old manifests
            //-get all resources
            //-group by name
            //-assign each group to a resource manifest that is newly created
            foreach (var type in supportedTypes)
            {
                string[] guids = AssetDatabase.FindAssets("t:"+type.Name);
                for(int gi = 0; gi < guids.Length; gi++)
                {
                    string guid = guids[gi];
                    var obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), type);
                    string assetPath = AssetDatabase.GetAssetPath(obj);

                    //check to see if it is a Unity built-in resource, also make sure it is in a 'Resources' folder.
                    if (!assetPath.StartsWith("Assets/") || assetPath.IndexOf("Resources/") == -1)
                    {
                        continue;
                    }
                    else if(type == typeof(GameObject))
                    {
                        //only support GameObject prefabs for now
                        //TODO: Add support for model prefabs
                        if (PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab)
                            continue;
                    }
                    
                    string path = AssetPathToResourcePath(assetPath);
                    string file = AssetPathToResourceName(assetPath);
                    //We use this for the manifest rather than 'file' because
                    //we can't determine the other one at runtime due to the use
                    //of AssetDatabase. This only requires the object's name.
                    string manifestName = ResourceManifest.CleanResourceObjectName(obj.name);
                    
                    //obtain the manifest that stores all resources with this name
                    ResourceManifest manifest = null;
                    map.TryGetValue(manifestName, out manifest);
                    if (manifest == null)
                    {
                        manifest = CreateManifestAsset(manifestPath + manifestName + ".asset");
                        map.Add(manifestName, manifest);
                        list.Add(manifest);
                    }

                    //are we going to look for repeat paths to
                    //different resources of the same type? (slow but safe)
                    string fullPath = path + file;
                    if (safetyChecks)
                    {
                        if (SafetyCheck(manifest, fullPath, type))
                            manifest.AddResource(obj, fullPath);
                        else Debug.Log("<color=red>WARNING:</color>There are multiple resources of the type '"+type.Name+"' that are being compiled to the path 'Resources/" + fullPath + "'. The manifest cannot determine which object this path should point to so only the first occurance has been stored. Please ensure all resources of the same type and at the same directory level relative to 'Resources/' have unique names.");
                    }
                    else
                    {
                        manifest.AddResource(obj, fullPath);
                    }
                }
            }

            PushObjectsToAssets(list);
            return list;
        }

        /// <summary>
        /// Helper method to perform a safety check that ensures us we won't accidentally
        /// create two resources of the same type with the same name that result in the same
        /// path when the Resources folders are compiled for a runtime player.
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="path"></param>
        /// <returns><c>false if the 'Resources' relative path already exists within the manifest</c></returns>
        static bool SafetyCheck(ResourceManifest manifest, string path, Type type)
        {
            foreach(string val in manifest.AllPaths())
            {
                if (path == val)
                {
                    //TODO: We need a faster way of doing this. Large projects
                    //will suffer a lot in this section, I suspect.
                    //THIS CAN BE SUPER SLOW
                    if(manifest.GetResourceFromPath(path, type) != null)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Helper method that takes a full asset path to a
        /// resource (relative to the project folder) and strips
        /// away everything but subdirectories within 'Resources' folders.
        /// In this way we can convert the edit-time asset path to a path that
        /// is usable by Resources.Load() at runtime.
        /// </summary>
        /// <remarks>
        /// This will strip away leading paths up-to-and-including 
        /// 'Resources/' well as the filename, leaving either a
        /// subdirectory of 'Resources/'or an empty string.
        /// </remarks>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static string AssetPathToResourcePath(string assetPath)
        {
            int i = -1;
            if (assetPath.StartsWith("Assets/"))
            {
                //this will strip away the filename
                i = assetPath.LastIndexOf("/");
                assetPath = assetPath.Remove(i) + "/";
            }

            //This will remove all directories up-to-and-including 'Resources',
            //leaving only subdirectories of 'Resources' or an empty string.
            i = -1;
            i = assetPath.IndexOf("Resources/");
            if (i >= 0)
            {
                assetPath = assetPath.Substring(i + 10); //offset by 10 to account for 'Resources/' string
            }
            
            return assetPath;
        }

        /// <summary>
        /// Converts a full path to an edit-time asset to a filename (with no path)
        /// useable by Resources.Load().
        /// </summary>
        /// <remarks>
        /// Essentially, all this does is strip away the path (and possibly any extension)
        /// //and leave only the filename.
        /// </remarks>
        /// <param name="path"></param>
        /// <returns></returns>
        static string AssetPathToResourceName(string path)
        {
            //remove directories
            int i = path.LastIndexOf("/");
            if (i >= 0) path = path.Remove(0, i + 1);

            //remove extensions
            var split = path.Split('.');
            if (split != null && split.Length > 0) path = split[0];

            return path;
        }

        /// <summary>
        /// 
        /// </summary>
        static void DisplayManifest(List<ResourceManifest> manifests)
        {
            GUILayout.Label("TODO: Display manifest contents here.");
        }
        #endregion
    }


}