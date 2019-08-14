using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using UnityEditor;
using Pantagruel.Editor;

namespace Pantagruel.Editor
{

    public class DatabaseEditor : SkinnedEditorWindow
    {
        IBindableDataSource state;
        Control Toggle;
        Control Toggle2;

        [MenuItem("Tools/Pantagruel/Test Window")]
        static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<DatabaseEditor>("Test Window", true);
        }

        void InitState()
        {
            if (state == null) state = new DatabaseProperty("Some State", ControlDisplayType.UnityObject, typeof(Sprite));
            if (Toggle == null) Toggle = new Control(state);
            if (Toggle2 == null) Toggle2 = new Control(state);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            hideFlags = HideFlags.HideAndDontSave;
            InitState();
            
        }

        int index = 0;
        /// <summary>
        /// Updates the editor window and all controls within it.
        /// </summary>
        protected override void OnGUIContent()
        {
            InitState();
            BindingDrawer.Skin = GUI.skin;
            Toggle.Draw();
            Toggle2.Draw();

            GUILayout.Space(15);

            string[] list = { "poop", "farts", "butts" };
            index = EditorGUILayout.Popup(index, list, this.Skin.GetStyle("DropDownBox"));
        }
    }
    /*
    /// <summary>
    /// Custom editor window for Pantagruel's item databases.
    /// 
    /// TODO:
    /// -Need a way to bind and sync asset filename and database name so that when
    /// one changes, the other changes as well.
    /// 
    /// -Not syncing asset name and database name on load
    ///     -A: because UnityEngine.Object.name is not the same as the filename?
    /// 
    /// -Deleting asset deletes database reference too?!?!
    /// 
    /// </summary>
    public class DatabaseEditor : SkinnedEditorWindow
    {
        DatabaseManager DbManager;

        //window state stuff
        int MainMenuId;
        int DatabaseSelectionId;
        bool ButtonState_lCntrl;

        const int HeaderBarHeight = 28;
        Vector2 LeftPanelScrollPos;
        Vector2 RightScrollPos;
        const int LeftPanelItemHeight = 20;
        const int LeftPanelTitleHeight = 15;

        GUIStyle ItemDefStyle;

        



        #region Unity Events
        /// <summary>
        /// Displays the editor window.
        /// </summary>
        //[MenuItem("Tools/Pantagruel/Item Database")]
        static void ShowWindow()
        {
           EditorWindow.GetWindow<DatabaseEditor>("Pantagruel", true);
           
        }

        /// <summary>
        /// Perform startup initialization for this editor.
        /// </summary>
        protected override void OnEnable()
        {
            //if (this.Skin == null) Skin = GUI.skin;
            //if (this.Skin == null) throw new UnityException("You must provide a properly formatted skin for this editor window.");
            
            //must do this!
            base.OnEnable();
            hideFlags = HideFlags.HideAndDontSave;
            Pantagruel.Editor.Utility.ConfirmAssetDirectory(Constants.DatabasePath);

            //these ids are used for the poup windows
            DatabaseSelectionId = GenerateUniqueId();
            MainMenuId = GenerateUniqueId();

            DbManager = new DatabaseManager();

            //setup style refs
            ItemDefStyle = Skin.GetStyle("DefinitionItem");

        }

        /// <summary>
        /// Raises the disable event.
        /// </summary>
        void OnDisable()
        {
            //added this null check so I could avoid errors when actively developing the editor.
            DbManager.CleanupManager();
            
        }

        /// <summary>
        /// Checks for left control.
        /// </summary>
        void CheckForLeftControl()
        {

            if (Event.current != null)
            {
                if (Event.current.control)
                    ButtonState_lCntrl = true;
                else ButtonState_lCntrl = false;
               
            }
        }
        #endregion


        #region Model Events
        
        #endregion

        
        #region GUI
        /// <summary>
        /// Updates the editor window and all controls within it.
        /// </summary>
        protected override void OnGUIContent()
        {
            CheckForLeftControl();

            //GUI
            DrawHeaderBar(8,8, PrimaryColor, this, (x) =>
                { x.DrawTopBar(); });
            GUILayout.Space(-2);
            //CONTENT
            GUILayout.BeginHorizontal();
            {
                DrawLeftPanel(DbManager.CurrentDatabase);
                DrawRightPanel(DbManager.CurrentDatabase, DbManager.CurrentItem);
            }GUILayout.EndHorizontal();
            


        }

        /// <summary>
        /// Helper to draw the very top of the editor window.
        /// </summary>
        void DrawTopBar()
        {
            //MAIN MENU BUTTON
            Rect mainButton = new Rect(15, 8, 16, 16);
            if (GUI.Button(mainButton, "", Skin.GetStyle("MainMenuButton")))
            {
                ShowSubwindow(MainMenuId, 
                                new Rect(15, 23, 200, 45), 
                                DoMainMenu, 
                                SubWindow.WindowType.Popup);
            }

            //HEADER BAR
            if (DbManager.CurrentDatabase != null)
            {
                //current database name field
                GUILayout.Space(72);

                if(GUILayout.Button(DbManager.CurrentDatabase.Name, Skin.textField, GUILayout.Width(200), GUILayout.Height(16)))
                {
                    {
                        //HACK ALERT: hard coding height calculation for dropdown list here
                        int height = 50 + (20 * DbManager.AllDatabaseNames.Length);
                        ShowSubwindow(DatabaseSelectionId,
                                        new Rect(75, 25, 200, height),
                                        DoDatabaseDropDown,
                                        SubWindow.WindowType.Popup);
                    }
                }


                //combo-like dropdown button
                GUI.Box(new Rect(277, 7, 16, 16), GUIContent.none, Skin.textField);

                if (GUI.Button(new Rect(279, 9, 12, 12), "", Skin.GetStyle("DropDownButton")))
                {
                    //HACK ALERT: hard coding height calculation for dropdown list here
                    int height = 50 + (20 * DbManager.AllDatabaseNames.Length);
                    ShowSubwindow(DatabaseSelectionId,
                                    new Rect(75, 25, 200, height),
                                    DoDatabaseDropDown,
                                    SubWindow.WindowType.Popup);
                }
            }
            else
            {
                GUILayout.Space(72);
                GUILayout.Label(" - No Databases - ", Skin.textField, GUILayout.Width(200));

            }
            
        }

        //DatabaseContentsPanel LeftPanel;
        /// <summary>
        /// Draws the Left-hand panel that contains an overview of
        /// a Database and lists all items in it.
        /// </summary>
        /// <param name="db"></param>
        void DrawLeftPanel(Database db)
        {
           
            var groupRect = EditorGUILayout.BeginVertical(GUILayout.Height(position.height - HeaderBarHeight), 
                GUILayout.MaxWidth(200), GUILayout.MinWidth(200));
            {
                GUI.Box(groupRect, "");

                if (db == null)
                {
                    GUILayout.Label("There is no database to display.");
                }
                else
                {
                    //draw database name text field
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    {
                        DrawHeaderBar(3, 3, this.TextHover, db,
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
                        string dbName = EditorGUILayout.DelayedTextField(db.Name, Skin.textArea);
                        if (dbName != db.Name)
                            DatabaseManager.RenameDatabase(db, dbName);
                        GUILayout.Space(5);

                        //Draw 'add item' button
                        GUIStyle s = new GUIStyle(ItemDefStyle);
                        s.alignment = TextAnchor.MiddleCenter;
                        if (GUILayout.Button(" + ", ItemDefStyle, GUILayout.Height(LeftPanelTitleHeight)))
                        {
                            DbManager.CurrentItem = db.AddItem("New Definition");
                            GUI.FocusControl("");
                        }
                    }GUILayout.EndHorizontal();
                    GUILayout.Space(9);



                    //draw list of items here
                    LeftPanelScrollPos = GUILayout.BeginScrollView(LeftPanelScrollPos);
                    //foreach (var item in db.ItemDefinitions)
                    for (int i = 0; i < db.ItemDefinitions.Length; i++)
                    {
                        if (db.ItemDefinitions[i] != null)
                        {
                            if(!DrawItemSelector(db, db.ItemDefinitions[i], i))
                            {
                            //    //if it rturn false we changed something
                            //    //so we need to stop iterating
                               break;
                            }
                            
                        }
                    }
                    GUILayout.EndScrollView();

                }

            } EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the right-hand panel that contains all item property controls.
        /// </summary>
        /// <param name="def"></param>
        void DrawRightPanel(Database db, DatabaseDefinition def)
        {
            /*
            var groupRect = EditorGUILayout.BeginVertical(GUILayout.Height(position.height - HeaderBarHeight),
            GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUI.Box(groupRect, "");
            GUILayout.Space(1);//NEED THIS. it makes the layout active even when empty

            //bulk of the work is here
            if (db != null && def != null)
            {
                //name and immutable stats of the currently selected item
                DrawHeaderBar(3, 3, this.TextHover, def,
                    (x) =>{ GUILayout.Label("Item Properties");}
                );
                def.Name = GUILayout.TextField(def.Name, Skin.textField, GUILayout.Width(150));
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Version", GUILayout.Width(50), GUILayout.ExpandWidth(false));
                    GUILayout.Label(def.Version.ToString(), Skin.textField, GUILayout.Width(50), GUILayout.ExpandWidth(false));
                }GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Id", GUILayout.Width(50), GUILayout.ExpandWidth(false));
                    GUILayout.Label(def.Id, Skin.textField, GUILayout.ExpandWidth(false));
                }GUILayout.EndHorizontal();

                
                RightScrollPos = GUILayout.BeginScrollView(RightScrollPos);
                {
                    GUILayout.Space(3);
                    ItemProperty prop;
                    for(int i = 0; i < def.Properties.Count; i++)
                    {
                        prop = def.Properties[i];
                        if (prop == null)
                        { 
                            def.Properties.Remove(def.Properties[i]);
                            break;
                        }
                        //if (!DrawItemProperty(prop)) break;
                    }
                    //foreach(var prop in def.Properties)
                    //{
                    //    if (!DrawItemProperty(prop)) break;
                    //}
                    GUILayout.Space(10);
                    //'add new prop' button
                    if (GUILayout.Button("+", ItemDefStyle, GUILayout.Height(LeftPanelTitleHeight)))
                    {
                        //TODO: popup window propmpting for the type to add
                        //Supported types should include:
                        //-bool
                        //-int 
                        //-float
                        //-string
                        //-guid
                        //-Color
                        //-UnityEngine.Object asset (i.e. a serializable resource, including prefabs)
                        //
                        def.AddProperty(24, typeof(int));
                        GUI.FocusControl("");
                    }
                    GUILayout.Space(30);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
          ///////////////////////////////////////////////////////////////
        }
        
        /// <summary>
        /// Draws a single 'item overview' control for the 
        /// left-hand panel of the editor window.
        /// </summary>
        /// <param name="def"></param>
        bool DrawItemSelector(Database db, DatabaseDefinition def, int itemIndex)
        {
            if (db == null || def == null) return true;
            GUIStyle s = (def != DbManager.CurrentItem) ? Skin.GetStyle("ItemSelectionBox") : Skin.GetStyle("ActiveItemSelectionBox");
            GUILayout.BeginHorizontal(s);


            //remove item def button)
            if (this.ButtonState_lCntrl)
            {
                if (GUILayout.Button("-", GUILayout.Height(21), GUILayout.MaxWidth(28), GUILayout.MinWidth(28)))
                {
                    
                    if (DbManager.CurrentItem == db.ItemDefinitions[itemIndex])
                    {
                        int len = db.ItemDefinitions.Length;
                        if (len > 1)
                        {
                            if (itemIndex > 0)
                                DbManager.CurrentItem = db.ItemDefinitions[itemIndex - 1];
                            else if (len > 1) DbManager.CurrentItem = db.ItemDefinitions[itemIndex + 1];
                            else DbManager.CurrentItem = db.ItemDefinitions[0];
                        }
                        else DbManager.CurrentItem = null;
                                
                    }
                    db.RemoveItem(def);
                    GUI.FocusControl("");
                    return false;
                }
            }
            else
            {
                if (GUILayout.Button("", Skin.GetStyle("Up Arrow"), GUILayout.MaxWidth(12), GUILayout.MinWidth(12), GUILayout.Height(12)))
                {
                    return false;
                }
                if (GUILayout.Button("", Skin.GetStyle("Down Arrow"), GUILayout.MaxWidth(12), GUILayout.MinWidth(12), GUILayout.Height(12)))
                {
                    return false;
                }
            }

            if (GUILayout.Button(def.Name, ItemDefStyle, GUILayout.Height(LeftPanelItemHeight)))
            {
                DbManager.CurrentItem = def;
                //return true;
                GUI.FocusControl("");
            }
            
            GUILayout.EndHorizontal();
            return true;
        }

        /*
        //Dictionary<Type, IItemPropDrawer<object>> PropertyDrawers;

        /// <summary>
        /// Draws a single ItemProperty entry for the right-hand
        /// panel that displays all properties of an item.
        /// </summary>
        /// <param name="prop"></param>
        bool DrawItemProperty(ItemProperty prop)
        {
            var PropertyDrawers = new Dictionary<Type, ITypeControlDrawer>();
            PropertyDrawers[typeof(int)] = new IntegerPropDrawer();

            GUILayout.Space(5);
            if (prop != null && PropertyDrawers != null)
            {
                Type t = prop.DataType;
                string newName = "";
                DrawHeaderBar(3, 1, this.TextHover, prop,
                    (x) =>
                    {
                        GUILayout.Space(10);
                        newName = EditorGUILayout.DelayedTextField(x.Name, EditorStyles.whiteBoldLabel, GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));
                        GUILayout.Label(t.Name, GUILayout.ExpandWidth(false));
                        GUILayout.Space(50);
                    }
                );
                //if we change the name, then we need to return false so that the loop
                //calling this knows to stop iterating on a list that has changed.
                if (DbManager.CurrentItem.TryChangePropertyName(prop.Name, newName)) return false; //return false to stop the loop that renders

                //PropertyDrawers[t].Invoke(name, prop, t);
                PropertyDrawers[t].Draw(prop, Skin);
                GUILayout.Space(20);
            }

            return true;
        }
///////////////////////////////////////////////////////////////////////////
        #endregion


        #region Sub Window handlers
        /// <summary>
        /// Handles sub-window for MainMenu.
        /// </summary>
        /// <param name="unusedWindowID"></param>
        void DoMainMenu(int windowId)
        {
            SubWindow win = GetSubwindowFromId(windowId);

            if (GUILayout.Button("New Database", MenuItemStyle, GUILayout.MaxWidth(win.Position.width-8)))
            {
                DbManager.CreateNewDatabase();
                CloseSubwindow(windowId);
            }
            
        }

        /// <summary>
        /// Handles the drop-down region for database selection.
        /// </summary>
        /// <param name="windowId"></param>
        void DoDatabaseDropDown(int windowId)
        {
            GUIStyle s = new GUIStyle(Skin.label);
            s.fontStyle = FontStyle.Bold;
            s.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Select Database", s);
            foreach (var dbName in DbManager.AllDatabaseNames)
            {
                if(GUILayout.Button("- " + dbName + " -", Skin.GetStyle("MenuItem")))
                {
                    DbManager.SelectCurrentDatabase(dbName);
                    CloseSubwindow(windowId);
                }
            }
            //add space so that we still show a box even if empty
            if(DbManager.AllDatabaseNames.Length < 1) GUILayout.Space(1);
            
        }
        #endregion
    }
*/

}
