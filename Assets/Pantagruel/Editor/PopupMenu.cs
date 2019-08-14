/**********************************************
* Pantagruel
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Pantagruel.Editor
{
    /// <summary>
    /// Helper class to display popup menus.
    /// I have to use this instead of GUI.Window() becuase
    /// Unity's Immediate mode GUI is completely broken.
    /// </summary>
    public class PopupMenu : PopupWindowContent
    {
        public delegate void PopupGuiHandler(PopupMenu container);
        PopupGuiHandler Handler;
        Vector2 Size;

        /// <summary>
        /// The position and size of the popup widow.
        /// </summary>
        public Rect Position
        {
            get { return editorWindow.position; }
        }

        /// <summary>
        /// Closes this popup window.
        /// </summary>
        public void Close()
        {
            editorWindow.Close();
        }

        /// <summary>
        /// Creates a new popup window content instance. Popup windows
        /// close if any action is taken outside the bounds of the popup.
        /// </summary>
        /// <param name="width">The desired width of the popup window.</param>
        /// <param name="height">The desired height of the popup window.</param>
        /// <param name="func">An event handler that allows user-supplied OnGUI event handling.</param>
        public PopupMenu(int width, int height, PopupGuiHandler func)
        {
            Handler = func;
            Size = new Vector2(width, height);
        }

        /// <summary>
        /// Returns the size of the window.
        /// </summary>
        /// <returns>A Vector2 describing the width and height of the window.</returns>
        public override Vector2 GetWindowSize()
        {
            return Size;
        }

        /// <summary>
        /// Passes control to the user-supplied OnGUI handler if any.
        /// </summary>
        /// <param name="rect"></param>
        public override void OnGUI(Rect rect)
        {
            //editorWindow.wantsMouseMove = true;
            if (Handler != null) Handler(this);
        }

        /// <summary>
        /// Used to repaint the window everytime the inspector updates.
        /// This helps with responsiveness for detecting keypress events.
        /// </summary>
        void OnInspectorUpdate()
        {
            // This will only get called 10 times per second.
            this.editorWindow.Repaint(); //increase response rate for keypresses (useful when checking for left control state)

        }
    }
}
