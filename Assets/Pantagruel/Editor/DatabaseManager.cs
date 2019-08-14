/**********************************************
* Pantagruel
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


using Pantagruel.Editor;
using UnityEditor;
using System.Reflection;
using System;

namespace Pantagruel
{
    /// <summary>
    /// Used to handle creation, removal, and maintenance of database files.
    /// </summary>
    public class DatabaseManager
    {
        //database stuff
        public DatabaseDefinition CurrentItem;

        /// <summary>
        /// Don't access this. Use the property
        /// <see cref="DatabaseEditor.CurrentDatabase"/>
        /// instead to ensure proper background processing.
        /// </summary>
        Database _CurrentDatabase;

        /// <summary>
        /// Gets or sets the current database to be displayed and edited. Whent set
        /// it also internally resets the CurrentItem value to the first item of
        /// the new database.
        /// </summary>
        public Database CurrentDatabase
        {
            get { return _CurrentDatabase; }
            set
            {
                _CurrentDatabase = value;
                if (_CurrentDatabase.ItemDefinitions != null && _CurrentDatabase.ItemDefinitions.Length > 0)
                    CurrentItem = _CurrentDatabase.ItemDefinitions[0];
                else CurrentItem = null;
            }
        }

        public List<Database> AllDatabases;

        public string[] AllDatabaseNames
        {
            get
            {
                List<string> names = new List<string>(4);
                foreach (var db in AllDatabases)
                {
                    names.Add(db.Name);
                }
                return names.ToArray();
            }
        }


        #region Instance Methods
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DatabaseManager()
        {
            BuildDatabaseList();
        }

        /// <summary>
        /// Initializes the list of available databases displayed and manipulated by the editor.
        /// </summary>
        protected void BuildDatabaseList()
        {
            if (AllDatabases == null) AllDatabases = new List<Database>();

            //TODO: We need to look through all asset files in the databases folder
            //and load any of the type 'Pantagruel.Database'. This way, if users
            //copy files in the editor we can include them in the list.
            string[] folders = { "Assets/" + Constants.DatabasePath.TrimEnd('/') };
            foreach (var guid in AssetDatabase.FindAssets("t:Database", folders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Database db = AssetDatabase.LoadAssetAtPath<Database>(path);
                if (db == null)
                {
                    Debug.LogWarning("Failed to load Database asset: " + path);
                }
                else
                {
                    if (!AllDatabases.Contains(db)) AllDatabases.Add(db);
                }
            }
            if (CurrentDatabase == null && AllDatabases != null && AllDatabases.Count > 0)
                CurrentDatabase = AllDatabases[0];
        }

        /// <summary>
        /// Saves the asset.
        /// </summary>
        public void SaveAllDatabases()
        {
            foreach (var db in AllDatabases)
            {
                if (db != null) SaveDatabase(db);
            }
        }

        /// <summary>
        /// This should be called when the manager's reference will no longer be in use.
        /// It ensures all active databased are pushed to their associated asset files and
        /// cleans up any leftover data.
        /// </summary>
        public void CleanupManager()
        {
            if (AllDatabases != null)
            {
                foreach (var db in AllDatabases)
                {
                    if (db != null) EditorUtility.SetDirty(db);
                }
            }
        }

        /// <summary>
        /// Creates a new database asset and names it accordingly.
        /// </summary>
        /// <returns></returns>
        public Database CreateNewDatabase()
        {
            Database db = Database.CreateDatabase("Database");
            DatabaseManager.CreateDatabaseAsset(db);
            if (!AllDatabases.Contains(db)) AllDatabases.Add(db);
            CurrentDatabase = db;
            return db;
        }
        
        /// <summary>
        /// Sets the currently selected database.
        /// </summary>
        /// <param name="name">The name of the databa.se to set as teh currently active one</param>
        public void SelectCurrentDatabase(string name)
        {
            foreach (var db in this.AllDatabases)
            {
                if (db.Name == name)
                {
                    CurrentDatabase = db;
                    return;
                }
            }
        }
        #endregion


        #region Static Methods
        /// <summary>
        /// Creates an asset file for the given database. If there is already
        /// an asset with the database's name a unique one is generated and the database
        /// is renamed to match.
        /// </summary>
        public static void CreateDatabaseAsset(Database db)
        {
            Utility.ConfirmAssetDirectory(Constants.DatabasePath);

            //confirm
            var name = AssetDatabase.GenerateUniqueAssetPath(db.FullAssetName);
            AssetDatabase.CreateAsset(db, name);

            SyncDatabaseToAsset(db, name);
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Updates the associated asset file with the latest state of the Database.
        /// 
        /// If there is no asset associated with this database, one is created.
        /// This method may change the Database's name and Guid to match
        /// those of the asset created.
        /// </summary>
        /// <returns>The filename of the asset saved.</returns>
        public static void SaveDatabase(Database db)
        {
            Utility.ConfirmAssetDirectory(Constants.DatabasePath);
            string assetName = db.FullAssetName;

            //confirm
            if (AssetDatabase.LoadAssetAtPath(db.FullAssetName, typeof(Database)) == null)
            {
                assetName = AssetDatabase.GenerateUniqueAssetPath(db.FullAssetName);
                AssetDatabase.CreateAsset(db, assetName);
            }

            SyncDatabaseToAsset(db, assetName);
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Helper method for making sure a Databases's name and Guid
        /// matches those of the asset it is saved as.
        /// </summary>
        /// <param name="db"></param>
        public static void SyncDatabaseToAsset(Database db, string assetPath)
        {
            //The database name and Guid are private so we'll need
            //to use reflection to change them.
            
            //Make sure the Database's name matches the assets's too. Normally
            //the asset would be the same but if we tried to create one with a name
            //already in use then we'll need to make this match the unique name
            //that was generated for the asset.
            var file = assetPath.Split('/');
            var name = file[file.Length - 1].Split('.');

            FieldInfo info = db.GetType().GetField("_Name", BindingFlags.Instance | BindingFlags.NonPublic);
            info.SetValue(db, name[0]);


            //Make sure the Database's Guid matches the asset's.
            info = db.GetType().GetField("DatabaseId", BindingFlags.Instance | BindingFlags.NonPublic);
            info.SetValue(db, new Guid(AssetDatabase.AssetPathToGUID(db.FullAssetName)));

        }

        /// <summary>
        /// Renames a database and its asset file.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="newName"></param>
        public static void RenameDatabase(Database db, string newName)
        {
            //FieldInfo info = db.GetType().GetField("_Name", BindingFlags.Instance | BindingFlags.NonPublic);
            //info.SetValue(db, name[0]);
            string msg = AssetDatabase.RenameAsset(db.FullAssetName, newName);
            if (!string.IsNullOrEmpty(msg))
            {
                Debug.LogError(msg);
            }
            else
            {
                SyncDatabaseToAsset(db, Constants.DatabasePath + newName + ".asset");
                SaveDatabase(db);
            }

        }

        /// <summary>
        /// Removes the given database asset.
        /// </summary>
        /// <param name="db"></param>
        public static void RemoveDatabase(Database db)
        {
            //TODO: Add 'Are you sure' message here

        }
        #endregion
    }
}
