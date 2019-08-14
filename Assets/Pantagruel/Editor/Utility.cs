/**********************************************
* Pantagruel
* Copyright 2015-2016 James Clark
**********************************************/
using System;
using UnityEngine;
using System.Collections;
using System.Text;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace Pantagruel.Editor
{
    /// <summary>
    /// Editor utility class for all edit-time Pantagruel systems.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Ensures the database asset Directory exists and creates it if it doesn't.
        /// </summary>
        /// <param name="directories">A full directory path that is split into a list of strings where each string represents a the next folder in the path.</param>
        public static void ConfirmAssetDirectory(params string[] directories)
        {
            if (directories == null || directories.Length < 1) throw new NullReferenceException();
            StringBuilder sb = new StringBuilder("Assets");
            StringBuilder sb2 = new StringBuilder();
            foreach (string dir in directories)
            {
                sb.Append("/");
                sb.Append(dir);
                sb2.Append("/");
                sb2.Append(dir);

                if (!System.IO.Directory.Exists(Application.dataPath + sb2.ToString()))
                {
                    string d = sb.ToString().Remove(sb.ToString().LastIndexOf('/'));
                    AssetDatabase.CreateFolder(d, dir);

                }

            }

        }

        /// <summary>
        /// Ensures the database asset Directory exists and creates it if it doesn't.
        /// </summary>
        /// <param name="directories">A full directory path that is split into a list of strings where each string represents a the next folder in the path.</param>
        public static void ConfirmAssetDirectory(string path)
        {
            List<string> dirs = new List<string>();

            foreach(var s in path.Split('/'))
            {
                if (!string.IsNullOrEmpty(s)) dirs.Add(s);
            }

            ConfirmAssetDirectory(dirs.ToArray());
        }

        /// <summary>
        /// Returns true if a file exists at the given resource directory.
        /// </summary>
        /// <param name="path">A path to a file that is relative to the project's Assets directory.</param>
        public static bool ConfirmResourceExists(string path)
        {
            string fullPath = Application.dataPath + "/" + path;
            if (System.IO.File.Exists(fullPath))
                return true;

            return false;
        }
    }


    /// <summary>
    /// Helper class for previewing audio clips in an editor window/inspector
    /// </summary>
    public static class PublicAudioUtil
    {
        /// <summary>
        /// Plays the audio clip in-editor.
        /// 
        /// NOTE: This is using reflection to gain access to internal, undocumented
        /// classes that allow us to play sounds in-editor and as a result is 
        /// likely to break in future updates.
        /// </summary>
        /// <param name="clip"></param>
        public static void PlayClip(AudioClip clip)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] {
                    typeof(AudioClip)
                },
                null
            );
            method.Invoke(
                null,
                new object[] {
                    clip
                }
            );
        }
    }

/*
    /// <summary>
    /// Helper class from drawing shapes in the at runtime and the editor.
    /// </summary>
    public class GUIDraw
    {
        private static Texture2D _staticRectTexture;
        private static GUIStyle _staticRectStyle;

        /// <summary>
        /// Draws a filled rectangular shape. Only meant for use in OnGUI methods.
        /// </summary>
        /// <param name="position">Defines the size, shape, and position to draw the shape.</param>
        /// <param name="color">The color to fill the shape with.</param>
        public static void Rect(Rect position, Color fillColor)
        {
            if (_staticRectTexture == null)
            {
                _staticRectTexture = new Texture2D(1, 1);
            }

            if (_staticRectStyle == null)
            {
                _staticRectStyle = new GUIStyle();
            }

            if (fillColor != Color.clear)
            {
                _staticRectTexture.SetPixel(0, 0, fillColor);
                _staticRectTexture.Apply();
                _staticRectStyle.normal.background = _staticRectTexture;
            }

            UnityEngine.GUI.Box(position, GUIContent.none, _staticRectStyle);
        }
    }
    */

}
