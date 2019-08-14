using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;

using Pantagruel;

namespace Pantagruel.Editor
{
    /// <summary>
    /// Interface used by all controls.
    /// </summary>
    public class Control
    {
        #region Fields and Properties
        public static GUISkin Skin;

        protected string Title;

        /// <summary>
        /// The datasource of the bound property this control will render.
        /// </summary>
        public IBindableDataSource BoundProperty
        {
            set
            {
                if (_BoundProperty != null)
                {
                    _BoundProperty.OnPropertyChanged -= HandlePropertyChanged;
                    _BoundProperty.OnPropertyDataChanged -= HandlePropertyDataChanged;
                }
                _BoundProperty = value;
                if (_BoundProperty != null)
                {
                    _BoundProperty.OnPropertyChanged += HandlePropertyChanged;
                    _BoundProperty.OnPropertyDataChanged += HandlePropertyDataChanged;
                }

            }
        }
        private IBindableDataSource _BoundProperty;

        #endregion


        #region Helpers
        /// <summary>
        /// Helper method for drawing the background of a control with contents inside of it.
        /// </summary>
        /// <param name="action">Action.</param>
        public static void ControlBackground(DatabaseDefinition def, Color headerColor, Action action)
        {
            Rect rect = EditorGUILayout.BeginVertical();
            {
                GUILayout.Space(3);
                EditorGUI.DrawRect(rect, headerColor);
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5);
                    if (action != null) action();
                    GUILayout.Space(5);
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            EditorGUILayout.EndVertical();
        }

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
        public static void DrawHeaderBar<T>(int top, int bottom, Color color, T actionParam, Action<T> action)
        {
            Rect r = EditorGUILayout.BeginVertical();
            {
                r.width += 5;
                GUI.Box(r, "");
                if (color != Color.clear) Pantagruel.Editor.GUIDraw.Rect(r, color);
                GUILayout.Space(top);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5);
                    if (action != null) action(actionParam);
                    GUILayout.Space(5);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(bottom);
            }
            EditorGUILayout.EndVertical();

        }
        #endregion


        #region Public Methods
        /// <summary>
        /// Constructs a control.
        /// </summary>
        public Control(string title)
        {
            if (title == null) title = "";
            Title = title;
        }

        /// <summary>
        /// Constructs a control with the given datasource.
        /// </summary>
        public Control(IBindableDataSource dataSource)
        {
            Title = "";
            BoundProperty = dataSource;
        }

        /// <summary>
        /// Constructs a control with the given datasource.
        /// </summary>
        public Control(string title, IBindableDataSource dataSource)
        {
            if (title == null) title = "";
            Title = title;
            BoundProperty = dataSource;
        }

        /// <summary>
        /// Destructs this object and removes references to events.
        /// </summary>
        ~Control()
        {
            BoundProperty = null;
        }

        /// <summary>
        /// Provides this control with a data source with wich to bind.
        /// </summary>
        /// <param name="prop"></param>
        public void BindPrimaryData(IBindableDataSource dataSource)
        {
            BoundProperty = dataSource;
        }

        /// <summary>
        /// Renders this control. The actual GUI element used is determined by the bound data source.
        /// </summary>
        public virtual void Draw()
        {
            DrawBinding();
        }

        #endregion


        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propData"></param>
        protected virtual void HandlePropertyDataChanged(object propData)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prop"></param>
        protected virtual void HandlePropertyChanged(INPC prop)
        {

        }

        /// <summary>
        /// Internal method that is called to draw a control based on bound data.
        /// </summary>
        protected void DrawBinding()
        {
            BindingDrawer.DrawControl(_BoundProperty);
        }
        #endregion
    }

    /// <summary>
    /// Utility class that determines what kind of GUI
    /// control to draw based on the datatype given to it.
    /// </summary>
    public static class BindingDrawer
    {
        public static GUISkin Skin;
        static Dictionary<ControlDisplayType, Action<IBindableDataSource>> DrawActions;

        static BindingDrawer()
        {
            DrawActions = new Dictionary<ControlDisplayType, Action<IBindableDataSource>>();
            DrawActions.Add(ControlDisplayType.Bool, DrawBool);
            DrawActions.Add(ControlDisplayType.Integer, DrawInteger);
            DrawActions.Add(ControlDisplayType.Float, DrawFloat);
            DrawActions.Add(ControlDisplayType.Guid, DrawGuid);
            DrawActions.Add(ControlDisplayType.Color, DrawColor);
            DrawActions.Add(ControlDisplayType.UnityObject, DrawUnityObject);
            DrawActions.Add(ControlDisplayType.StaticImage, DrawImage);

        }

        /// <summary>
        /// Draws the control using the type of data as a context for the kind of control to be used.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="skin"></param>
        public static void DrawControl(IBindableDataSource dataSource)
        {
            if (dataSource == null) return;

            if(DrawActions.ContainsKey(dataSource.ControlType))
            {
                var temp = GUI.skin;
                if (Skin != null) GUI.skin = Skin;
                DrawActions[dataSource.ControlType].Invoke(dataSource);
                GUI.skin = temp;
            }
        }

        static public void DrawImage(IBindableDataSource source)
        {
            var rect = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(75));
            EditorGUI.DrawRect(rect, Color.white);
            EditorGUI.DrawTextureAlpha(rect, (Texture)source.Data, ScaleMode.ScaleToFit);
            EditorGUILayout.EndHorizontal();
        }

        static public void DrawBool(IBindableDataSource source)
        {
            source.Data = EditorGUILayout.Toggle((bool)source.Data, Skin.toggle);
        }

        static public void DrawInteger(IBindableDataSource source)
        {
            source.Data = EditorGUILayout.IntField((int)source.Data, Skin.textField);
        }

        static public void DrawFloat(IBindableDataSource source)
        {
            source.Data = EditorGUILayout.FloatField((int)source.Data, Skin.textField);
        }

        static public void DrawUnityObject(IBindableDataSource source)
        {
            if (source.EstablishedType.IsSubclassOf(typeof(Texture)) || source.EstablishedType == typeof(Sprite))
            {
                var rect = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(100));
                rect.width = 100;
                rect.height = 100;
                rect.x += 15;
                EditorGUI.DrawRect(rect, Color.white);

                Sprite s = source.Data as Sprite;
                if(s != null)
                {
                    Texture t = s.texture;
                    EditorGUI.DrawTextureAlpha(rect, t, ScaleMode.ScaleToFit);

                }
                GUILayout.Space(125);
                var fieldRect = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(fieldRect, Color.white);
                source.Data = EditorGUILayout.ObjectField((UnityEngine.Object)source.Data, source.EstablishedType, false);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                source.Data = EditorGUILayout.ObjectField((UnityEngine.Object)source.Data, source.EstablishedType, false);
            }
        }

        static public void DrawString(IBindableDataSource source)
        {
            source.Data = EditorGUILayout.TextField((string)source.Data, Skin.textField);
        }

        static public void DrawGuid(IBindableDataSource source)
        {
            string s = EditorGUILayout.TextField(source.Data.ToString(), Skin.textField);
            source.Data = new Guid(s);
        }

        static public void DrawColor(IBindableDataSource source)
        {
            source.Data = EditorGUILayout.ColorField((Color)source.Data);
        }
    }

    /*
    /// <summary>
    /// Defines a panel that has a non-scrolling upper section
    /// used for header and global controls and an expandable scrolling
    /// list region below it.
    /// </summary>
    public class DatabaseContentsPanel : ScrollingControlList
    {
        int TitlebarHeight;
        int HeightOffset;
        int MaxWidth;
        int MinWidth;
        
        public delegate ItemDefinition NameEvent(string name);
        public event NameEvent OnAddDefinition;
        public event NameEvent OnChangedDatabaseName;

        /// <summary>
        /// Default contrstructor.
        /// </summary>
        /// <param name="heightOffset"></param>
        /// <param name="minWidth"></param>
        /// <param name="maxWidth"></param>
        public DatabaseContentsPanel(string title, int minWidth, int maxWidth, int heightOffset, int titleBarHeight)
        {
            Title = title;
            TitlebarHeight = titleBarHeight;
            HeightOffset = heightOffset;
            MaxWidth = maxWidth;
            MinWidth = minWidth;

            OnAddDefinition += AddDefControl;
        }

        ItemDefinition AddDefControl(string name)
        {
            ItemDefinitionControl con = new ItemDefinitionControl(name);
            AddControl(con);
            return null;
        }
        
        /// <summary>
        /// Draws this control and all sub-controls within it.
        /// </summary>
        public override void Draw()
        {
            var groupRect = EditorGUILayout.BeginVertical(GUILayout.Height(HeightOffset),
                GUILayout.MaxWidth(200), GUILayout.MinWidth(200));
            {
                GUI.Box(groupRect, "");

                //draw database name text field
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                {
                    DrawHeaderBar(3, 3, Color.blue, this,
                        (x) => { GUILayout.Label("Item Definitions"); }
                    );
                    GUILayout.Space(10);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(25);
                    //draw database name editor
                    string dbName = EditorGUILayout.DelayedTextField(Title, Skin.textArea);
                    if (dbName != Title && OnChangedDatabaseName != null)
                    {
                        Title = dbName;
                        OnChangedDatabaseName(Title);
                    }
                    GUILayout.Space(5);

                    //Draw 'add item' button
                    GUIStyle s = new GUIStyle(Skin.GetStyle("DefinitionItem"));
                    s.alignment = TextAnchor.MiddleCenter;
                    if (GUILayout.Button(" + ", s, GUILayout.Height(TitlebarHeight)) &&
                        OnAddDefinition != null)
                    {
                        OnAddDefinition("New Definition");
                        GUI.FocusControl("");
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(9);

                //draws the scrolling list section
                base.Draw();
            }
            EditorGUILayout.EndVertical();
        }
    }
    */

        /*
    /// <summary>
    /// Defines a container for other controls that scrolls
    /// when its content is larger than its defined region.
    /// </summary>
    public class ScrollingControlList : Control
    {
        bool IsDrawingList;
        List<Control> Controls;
        List<Control> RemovalList;

        /// <summary>
        /// Default contructor.
        /// </summary>
        public ScrollingControlList()
        {
            Controls = new List<Control>();
            RemovalList = new List<Control>();

        }

        /// <summary>
        /// Adds a control to the list of rendered controls.
        /// </summary>
        /// <param name="control"></param>
        public void AddControl(Control control)
        {
            if(!Controls.Contains(control))
                Controls.Add(control);
        }

        /// <summary>
        /// Removes a control from the list. If the list is currently being drawn,
        /// //the actual removal will be delayed until afterward.
        /// </summary>
        /// <param name="control"></param>
        public void RemoveControl(Control control)
        {
            if (IsDrawingList) RemovalList.Add(control);
            else Controls.Remove(control);
        }

        /// <summary>
        /// Internal helper. Used when removing controls while drawing them,
        /// this will be invoked after the drawing to remove everything.
        /// </summary>
        /// <param name="control"></param>
        void RemoveControlsInList()
        {
            if (IsDrawingList) return;
            if (RemovalList == null) return;

            foreach(Control con in RemovalList)
            {
                Controls.Remove(con);
            }
            RemovalList.Clear();
        }

        /// <summary>
        /// Draws this control and all controls within it.
        /// </summary>
        public override void Draw()
        {
            IsDrawingList = true;
            foreach(var control in Controls)
            {
                control.Draw();
            }
            IsDrawingList = false;

            RemoveControlsInList();
        }

    }

    /*
    /// <summary>
    /// Defines a control that displays various information for an ItemDefinition overview.
    /// </summary>
    public class ItemDefinitionControl : Control
    {
        static GUIStyle DarkButton;
        static GUIStyle Background;
        static GUIStyle TextBox;
        static GUIStyle UpArrow;
        static GUIStyle DownArrow;
        static bool AltState = false;
        static int ElementHeight;

        public delegate void ItemDefinitionEvent(ItemDefinition def);
        public delegate void ItemRenameEvent(ItemDefinition def, string newName);
        public delegate void ItemDeleteEvent(ItemDefinition def);
        public event ItemRenameEvent OnRenameItem;
        public event ItemDeleteEvent OnDeleteItem;
        public event ItemDefinitionEvent OnMoveItemUp;
        public event ItemDefinitionEvent OnMoveItemDown;
        public event ItemDefinitionEvent OnSelectItem;


        /// <summary>
        /// Default contructor.
        /// </summary>
        /// <param name="def"></param>
        public ItemDefinitionControl(string name)
        {
            Title = name;
        }

        /// <summary>
        /// Used to apply global styling to the sub-ekements of this control.
        /// </summary>
        /// <param name="elementHieght"></param>
        /// <param name="background"></param>
        /// <param name="selector"></param>
        /// <param name="textBox"></param>
        /// <param name="upArrow"></param>
        /// <param name="downArrow"></param>
        public static void ApplyStyles(int elementHieght, GUIStyle background, GUIStyle selector, GUIStyle textBox, GUIStyle upArrow, GUIStyle downArrow)
        {
            DarkButton = selector;
            Background = background;
            TextBox = textBox;
            UpArrow = upArrow;
            DownArrow = downArrow;
            ElementHeight = elementHieght;
        }

        /// <summary>
        /// Attach this listener to a global event that checks the state of the left control button on the keyboard.
        /// </summary>
        /// <param name="state"></param>
        void OnAltState(bool state)
        {
            AltState = state;
        }

        /// <summary>
        /// Renders all sub-controls and process event triggers associated with them.
        /// </summary>
        public override void Draw()
        {
            GUILayout.BeginHorizontal();// Background);


            //remove item def button)
            if (AltState)
            {
                if (GUILayout.Button("-", GUILayout.Height(21), GUILayout.MaxWidth(28), GUILayout.MinWidth(28)) &&
                    OnDeleteItem != null)
                {
                    //OnDeleteItem(Definition);
                    GUI.FocusControl("");
                }
             
            }
            else
            {
                if (GUILayout.Button("", UpArrow, GUILayout.MaxWidth(12), GUILayout.MinWidth(12), GUILayout.Height(12)) &&
                    OnMoveItemUp != null)
                {
                    //OnMoveItemUp(Definition);
                }
                if (GUILayout.Button("", DownArrow, GUILayout.MaxWidth(12), GUILayout.MinWidth(12), GUILayout.Height(12)) &&
                    OnMoveItemDown != null)
                {
                    //OnMoveItemDown(Definition);
                }
            }

            if (GUILayout.Button(Title, DarkButton, GUILayout.Height(ElementHeight)) &&
                OnSelectItem != null)
            {
                //OnSelectItem(Definition);
                GUI.FocusControl("");
            }

            GUILayout.EndHorizontal();
        }
    }
    */

}