using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace Pantagruel.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class DatabaseMenuWindow : EditorWindow
    {
        public static DatabaseEditor Editor;

        public static DatabaseMenuWindow ShowWindow(DatabaseEditor databaseEditor, Rect where)
        {
            Editor = databaseEditor;
            var win = DatabaseMenuWindow.CreateInstance<DatabaseMenuWindow>();
            win.ShowAsDropDown(where, where.size);
            win.Focus();
            //win.O
            return win;
        }

        public void OnGUI()
        {
            EditorGUI.DrawRect(position, Color.blue);
            //this.ShowAsDropDown(this.position, this.position.size);
            /*if(GUILayout.Button("New Database", GUILayout.MaxWidth(300), GUILayout.MinHeight(50)))
            {
                Editor.AddDatabase();
            }
            GUILayout.Space(15);
            */
            //ListIndex = EditorGUILayout.Popup(ListIndex, Editor.AllDatabaseNames);
            Repaint();
        }
    }
}
