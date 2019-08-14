using System;
using UnityEngine;

namespace Pantagruel.Editor
{
    /// <summary>
    /// Helper class from drawing shapes in the editor.
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

}