/**********************************************
* Pantagruel
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Pantagruel.Editor
{
    /// <summary>
    /// Provides utility methods for skinning a custom editor.
    /// </summary>
    public class SkinnedEditorWindow : EditorWindow
    {
        Dictionary<int, SubWindow> SubWindows;
        List<SubWindow> SubwindowRemovalList;
        int WindowId;
        protected const string EditorColorSource = "ColorDefines";

        public GUISkin Skin;
        public Texture2D Icon;
        public GUIStyle PopupWindowStyle;
        public GUIStyle MenuItemStyle;
        public GUIStyle EditorColors;
        protected Color PrimaryColor = new Color(0.38f, 0.0f, 0.25f);
        protected Color SecondaryColor = new Color(0.30f, 0.0f, 0.17f);
        protected Color TirchiaryColor = new Color(0.28f, 0.2f, 0.1f);
        protected Color TrimColor = Color.white;
        protected Color TextColor = Color.black;
        protected Color TextColor2 = Color.black;
        protected Color TextHover = Color.cyan;
        protected Color TextActive = Color.grey;
        

        /// <summary>
        /// Helper class for defining regions that represent popup windows within this editor.
        /// </summary>
        public class SubWindow
        {
            public enum WindowType { Normal, Fixed, Popup,}
            public int WindowId;
            public Rect Position;
            public GUIStyle Style;
            public GUI.WindowFunction HandleUpdate;
            public WindowType Type;
        }
        


        #region Unity Events
        /// <summary>
        /// Sets up easy-access skin info.
        /// This *must* be called by derived classes OnEnable() method!
        /// </summary>
        protected virtual void OnEnable()
        {
            //set editor window properties
            wantsMouseMove = false;//true; //we'll need this for hilighting buttons
            if (Icon != null)
                titleContent = new GUIContent(titleContent.text, Icon);
            else titleContent = new GUIContent(titleContent.text);

            //setup some state info
            WindowId = 0;
            SubWindows = new Dictionary<int, SubWindow>();
            SubwindowRemovalList = new List<SubWindow>(4);

            SetSkin(Skin);
            
        }

        /// <summary>
        /// Helper method for setting up custom colors using a skin.
        /// </summary>
        /// <param name="skin"></param>
        protected void SetSkin(GUISkin skin)
        {
            Skin = skin;
            //setup easy-access colors
            if (Skin == null)
            {
                Skin = GUISkin.CreateInstance<GUISkin>();
            }
            else
            {
                //we are using the state textcolor entries to define our editor's colors.
                //Background colors are normal states (Normal, Hover, Active, ...)
                //text colors are state events (onNormal, onHover, onActive, ...)
                EditorColors = Skin.GetStyle(EditorColorSource);
                if (EditorColors != null)
                {
                    PrimaryColor = EditorColors.normal.textColor;
                    SecondaryColor =  EditorColors.hover.textColor;
                    TirchiaryColor = EditorColors.focused.textColor;
                    TrimColor = EditorColors.active.textColor;
                    TextColor = EditorColors.onNormal.textColor;
                    TextColor2 = EditorColors.onFocused.textColor;
                    TextHover = EditorColors.onHover.textColor;
                    TextActive = EditorColors.onActive.textColor;
                }
                MenuItemStyle = Skin.GetStyle("MenuItem");
                PopupWindowStyle = Skin.GetStyle("Popup Window");
            }
        }
        
        /// <summary>
        /// Used to repaint the window everytime the inspector updates.
        /// This helps with responsiveness for detecting keypress events.
        /// </summary>
        void OnInspectorUpdate()
        {
            // This will only get called 10 times per second.
            Repaint(); //increase response rate for keypresses (useful when checking for left control state)
        }

        /// <summary>
        /// Be sure to call this class from the derived OnGUI method.
        /// You should not override this ever. Override 'OnGUIContent' instead.
        /// </summary>
        protected virtual void OnGUI()
        {
            //fallback to the current skin if user didn't supply one.
            if (Skin == null) Skin = GUI.skin;
            else GUI.skin = Skin;
            
            BeginWindows();

            GUIDraw.Rect(new Rect(0, 0, position.width, position.height), PrimaryColor);
            OnGUIContent();

            //process subwindows
            foreach (var window in SubWindows.Values)
            {
                //An ivisible button that covers the entire editor. This will consume
                //our clicks outside of the subwindow.
                if (GUI.Button(new Rect(0,
                                        0,
                                        position.width,
                                        position.height), "", GUIStyle.none))
                {
                    if (window.Type == SubWindow.WindowType.Popup) CloseSubwindow(window);
                }
                if (window.Type == SubWindow.WindowType.Fixed || window.Type == SubWindow.WindowType.Popup)
                {
                    GUI.Window(window.WindowId, window.Position, DoSubwindow, "");
                }
                else if(window.Type == SubWindow.WindowType.Normal)
                {
                    window.Position = GUI.Window(window.WindowId, window.Position, DoSubwindow, "");
                }
            }

            //remove any subwindows that were closed this update
            foreach (var window in SubwindowRemovalList)
                SubWindows.Remove(window.WindowId);
            SubwindowRemovalList.Clear();
            
            EndWindows();

        }

        /// <summary>
        /// Performs some housekeeping then passes control to the user-supplied  handler.
        /// </summary>
        /// <param name="windowId"></param>
        void DoSubwindow(int windowId)
        {
            SubWindow window = GetSubwindowFromId(windowId);

            if (window.HandleUpdate != null) window.HandleUpdate(windowId);

            //for some insane reason we *have* to call this or all clicks
            //will leak onto controls behind this window. Fixed and Popup windows
            //will ignore the repositioning though.
            GUI.DragWindow();
        }
        
        /// <summary>
        /// This should be overriden in derived classes and used for handling
        /// OnGUI events. The object's actual OnGUI method wraps this method
        /// with some setup and cleanup code.
        /// </summary>
        protected virtual void OnGUIContent()
        {
            throw new UnityException("You should derive a class from SkinneEditorwindow and override OnGUIContent in order to display your editor's contents.");
        }
        #endregion


        #region GUIHelpers
        /// <summary>
        /// Creates a new 'Sub Window' that acts like a window within this editor.
        /// Useful for quick-n-dirty solutions to things like movable controls,
        /// popup windows, dropdown lists, etc...
        /// </summary>
        /// <param name="position">The editor-relative position of the window.</param>
        /// <param name="updateHandler">A user-supplied method that is called each frame
        /// that the SubWindow is active.</param>
        /// <returns></returns>
        public SubWindow ShowSubwindow(int id, Rect position, GUI.WindowFunction updateHandler, SubWindow.WindowType type, GUIStyle style = null)
        {
            if (SubWindows.ContainsKey(id)) return SubWindows[id];
            SubWindow sub = new SubWindow();

            sub.Position = GUI.Window(id, position, updateHandler, "", style);
            if (style != null) sub.Style = style;
            else sub.Style = Skin.window;
            sub.WindowId = id;
            sub.HandleUpdate = updateHandler;
            sub.Type = type;
            SubWindows[id] = sub;

            //make sure to refocus controls so that 
            //any delayed text boxes resets to current text
            GUI.FocusControl("");
            GUI.FocusWindow(id);

            return sub;
        }

        /// <summary>
        /// Creates a new 'Sub Window' that acts like a window within this editor.
        /// Useful for quick-n-dirty solutions to things like movable controls,
        /// popup windows, dropdown lists, etc...
        /// </summary>
        /// <param name="position">The editor-relative position of the window.</param>
        /// <param name="updateHandler">A user-supplied method that is called each frame
        /// that the SubWindow is active.</param>
        /// <returns></returns>
        public SubWindow ShowSubwindow(Rect position, GUI.WindowFunction updateHandler, SubWindow.WindowType type, GUIStyle style = null)
        {
            var win = ShowSubwindow(WindowId, position, updateHandler, type, style);
            while (SubWindows.ContainsKey(WindowId)) WindowId++;
            return win;
        }

        /// <summary>
        /// Provides an unused window id number for use with ShowSubwindow().
        /// </summary>
        /// <returns></returns>
        public int GenerateUniqueId()
        {
            while (SubWindows.ContainsKey(WindowId)) WindowId++;
            return WindowId;
        }

        /// <summary>
        /// Deactivates and destroys a previously activated SubWindow.
        /// </summary>
        /// <param name="window"></param>
        public void CloseSubwindow(SubWindow window)
        {
            if (window == null) return;
            if (SubWindows.ContainsKey(window.WindowId))
            {
                //since we might issue this command while iterating over
                //the list, we are going to add this window to a removal list
                ///that will be processed later rather than simply call List.Remove()
                SubwindowRemovalList.Add(window);

                //make sure to refocus controls so that 
                //any delayed text boxes resets to current text
                GUI.FocusControl("");

            }
        }

        /// <summary>
        /// Deactivates and destroys a previously activated SubWindow.
        /// </summary>
        /// <param name="window"></param>
        public void CloseSubwindow(int windowId)
        {
            if (SubWindows.ContainsKey(windowId))
            {
                //since we might issue this command while iterating over
                //the list, we are going to add this window to a removal list
                ///that will be processed later rather than simply call List.Remove()
                SubwindowRemovalList.Add(GetSubwindowFromId(windowId));

                //make sure to refocus controls so that 
                //any delayed text boxes resets to current text
                GUI.FocusControl("");
            }
        }

        /// <summary>
        /// Internal handler for subwindow processes. Hands control
        /// off to the user-supplied method after wrapping everything
        /// in a layout region.
        /// </summary>
        /// <param name="windowId"></param>
        void HandleSubwindow(int windowId)
        {
            SubWindow sub = GetSubwindowFromId(windowId);
            if (sub != null)
            {
                GUILayout.BeginArea(sub.Position, sub.Style);
                sub.HandleUpdate.Invoke(windowId);
                GUILayout.EndArea();
            }
        }
        
        /// <summary>
        /// Giving a Unity-supplied window id this will return any active subwindow
        /// of this editor that is using that id.
        /// </summary>
        /// <param name="windowId">The Window id supplied from something like </param>
        /// <returns></returns>
        public SubWindow GetSubwindowFromId(int windowId)
        {
            SubWindow sub = null;
            SubWindows.TryGetValue(windowId, out sub);
            return sub;
        }

        #endregion


        #region GUI Drawing helpers
        /// <summary>
        /// Draws a colored bar and executes an action that is intended for GUI controls.
        /// The action is wrapped inside a horizontal layout so that content will appear
        /// on top of the colored bar region.
        /// </summary>
        /// <typeparam name="T">The the type of data to pass to the actionParam.</typeparam>
        /// <param name="top">Padding space above the content.</param>
        /// <param name="bottom">Padding space below the content.</param>
        /// <param name="color">The color of the header bar.</param>
        /// <param name="actionParam">If a content action is supplied this data will be passed to it when invoked.</param>
        /// <param name="action">An optional Action that can be executed to process and draw the content within this header.</param>
        protected void DrawHeaderBar<T>(int top, int bottom, Color color, T actionParam, Action<T> action)
        {
            Rect r = EditorGUILayout.BeginVertical();
            {
                r.width += 5;
                GUI.Box(r, "");
                if(color != Color.clear) Pantagruel.Editor.GUIDraw.Rect(r, color);
                GUILayout.Space(top);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5);
                    if (action != null) action(actionParam);
                    GUILayout.Space(5);
                }GUILayout.EndHorizontal();
                GUILayout.Space(bottom);
            }
            EditorGUILayout.EndVertical();

        }

        /// <summary>
        /// Helper method that draws a region in the primary color with a thin border in the secondary color.
        /// </summary>
        /// <param name="region"></param>
        protected void DrawFramedRegion(Rect region)
        {
            EditorGUI.DrawRect(region, SecondaryColor);
            EditorGUI.DrawRect(new Rect(region.x + 1, region.y + 1, region.width - 2, region.height - 2), PrimaryColor);

        }
        #endregion
    }
}