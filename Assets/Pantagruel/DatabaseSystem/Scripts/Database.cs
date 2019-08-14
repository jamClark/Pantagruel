using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;


namespace Pantagruel
{
    //HOOK TO THIS!!!
    /////////////////////// UnityEditor.AssetModificationProcessor

    /// <summary>
    /// Provides an interface for managing databases and accessing item definitions stored within them.
    /// </summary>
    [Serializable]
    public class Database : ScriptableObject
    {
        #region Members
        [SerializeField]
        private string _Name;
        public string Name
        {
            get { return _Name; }
            private set
            {
                _Name = value;
                //TODO: Maybe make a callback event here to let us know
                //we need to rename the asset if this changes.
            }
        }

        /// <summary>
        /// This is the complete path to the asset, relative to the project root directory.
        /// </summary>
        public string FullAssetName
        {
            get { return "Assets/"+Constants.DatabasePath + _Name + ".asset"; }
        }

        [SerializeField]
        List<DatabaseDefinition> _ItemDefinitions;
        public DatabaseDefinition[] ItemDefinitions
        {
            get { return _ItemDefinitions.ToArray(); }
        }

        [SerializeField]
        Guid DatabaseId;
        #endregion


        #region Methods
        /// <summary>
        /// Adds a new definition to the database.
        /// </summary>
        public DatabaseDefinition AddItem(string defName)
        {
            var def = new DatabaseDefinition(defName);
            _ItemDefinitions.Add(def);
            return def;
        }

        /// <summary>
        /// Removes the given definition from the database.
        /// </summary>
        /// <param name="def"></param>
        public bool RemoveItem(DatabaseDefinition def)
        {
            return _ItemDefinitions.Remove(def);
        }
        #endregion


        #region Static
        /// <summary>
        /// Helper method used to generate a unique name if the
        /// provided one already exists in the given list. 
        /// </summary>
        /// <param name="rootName"></param>
        /// <returns></returns>
        public static string GenerateUniqueName(string rootName, IEnumerable<string> usedNames, int index = 0)
        {
            foreach (string name in usedNames)
            {
                if (index == 0)
                {
                    if (name == rootName)
                    {
                        index++;
                        return GenerateUniqueName(rootName, usedNames, index);
                    }
                }
                else
                {
                    if (name == rootName + " " + index)
                    {
                        index++;
                        return GenerateUniqueName(rootName, usedNames, index);
                    }
                }
            }

            if (index == 0) return rootName;
            else return rootName + " " + index;
        }

        /// <summary>
        /// Creates a new database asset.
        /// </summary>
        /// <param name="name">The name of the new database. If there is already a database with this name this will append an indexed number to it.</param>
        /// <returns>A reference to the new database.</returns>
        public static Database CreateDatabase(string name)
        {
            Database newDB = ScriptableObject.CreateInstance<Database>();
            newDB._Name = name;
            newDB._ItemDefinitions = new List<DatabaseDefinition>();
            return newDB;
        }

        /// <summary>
        /// Creates a new database asset.
        /// </summary>
        /// <param name="name">The name of the new database. If there is already a database with this name this will append an indexed number to it.</param>
        /// <param name="copyFrom">Another database whose items should be copied to this database.</param>
        /// <returns>A reference to the new database.</returns>
        public static Database CreateDatabase(string name, Database copyFrom)
        {
            return null;
        }

        /// <summary>
        /// Merges all item definitions in a single database by copying
        /// all of them from 'toRemove' and placing them in 'dest'. Afterward
        /// 'toRemove' is deleted.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="toRemove"></param>
        public static void MergeDatabase(Database dest, Database toRemove)
        {
            throw new Exception("Not yet implemented");
        }

        /// <summary>
        /// Obtains a reference to the named database asset.
        /// </summary>
        /// <param name="name">The name of the database to obtain a reference to.</param>
        /// <returns>The database being sought or null if there is no database with the given name.</returns>
        public static Database FindDatabase(string name)
        {
            return null;
        }        
        #endregion


    }
}
