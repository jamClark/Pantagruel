using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Pantagruel
{
    /// <summary>
    /// A container for a list of properties. This acts similar to an object
    /// or item that is composed of a series of <see cref="DatabaseProperty"/>s.
    /// </summary>
    [Serializable]
    public class DatabaseDefinition : INPC, IBindableDataSource
    {
        #region Fields and Propeties
        /// <summary>
        /// A user-friendly name to refer to this definition by. Other than that, it holds no bearing
        /// on how this definition is categorized, stored, or organized by the database system.
        /// </summary>
        /// <remarks>
        /// Note however, that if this name is changed, the version number will be icremented.
        /// </remarks>
        [XmlIgnore]
        public string Name
        {
            get { return _Name; }
        }
        [SerializeField]
        public string _Name;

        /// <summary>
        /// Tracks changes to this definition over time. Any time this definition's internal data
        /// is altered by using one of its methods, this number will be incremented.
        /// </summary>
        [XmlIgnore]
        public ulong Version
        {
            get { return _Version; }
        }
        [SerializeField]
        private ulong _Version;

        /// <summary>
        /// A unique identifier for this definition. Represented by a <see cref="Guid"/>.
        /// </summary>
        [XmlIgnore]
        public Guid Id
        {
            get { return _Id; }
        }
        [SerializeField]
        private Guid _Id;

        /// <summary>
        /// A list of all <see cref="DatabaseProperty"/> elements that compose this definition.
        /// </summary>
        [XmlIgnore]
        public List<DatabaseProperty> Properties
        {
            get
            {
                if (_PropertiesList == null) _PropertiesList = new List<DatabaseProperty>();
                return _PropertiesList;
            }
        }
        [SerializeField]
        private List<DatabaseProperty> _PropertiesList;

        /// <summary>
        /// Returns a list of the names of all properties in this definition.
        /// </summary>
        public string[] AllPropertyNames
        {
            get
            {
                string[] list = new string[_PropertiesList.Count];
                for (int i = 0; i < list.Length; i++)
                {
                    list[i] = _PropertiesList[i].Name;
                }
                return list;
            }
        }

        /// <summary>
        /// Provides an interfaced way of accessing this object's <see cref="DatabaseProperty"/> list.
        /// </summary>
        [XmlIgnore]
        public object Data
        {
            get
            {
                return Properties;
            }
            set
            {
                //make sure the value being given is a valid type for this DatabaseProperty's established type.
                if (value == null)
                {
                    throw new ArgumentNullException("The database definitions list can not be supplied with a null value.");
                }
                else
                {
                    if (value.GetType() != typeof(List<DatabaseProperty>))
                        throw new ArgumentException("Cannot convert a property established as type 'List<DatabaseProperty>' to the type '" + value.GetType() + ".");
                }
                //NOTE: we don't check for equality here because we don't know if the reference stayed the same
                //and the data is what changed. This wouldn't be a problem if we used immutable data but then... Copytown ahoy!
                _PropertiesList = value as List<DatabaseProperty>;
                TriggerDataChangedEvent();
            }
        }

        /// <summary>
        /// Always returns <see cref="ControlDisplayType.SubControlsList"/> because we want the context of <see cref="Data>"/>
        /// to be a list of all of this definitions properties as defined by controls.
        /// </summary>
        public ControlDisplayType ControlType
        {
            get { return ControlDisplayType.EmbeddedControls; }
        }

        /// <summary>
        /// Always returns the type <see cref="DatabaseDefinition"/>.
        /// </summary>
        public Type EstablishedType
        {
            get { return typeof(DatabaseDefinition); }
        }

        public event NotifyPropertyDataChangedEvent OnPropertyDataChanged;
        public event NotifyPropertyChangedEvent OnPropertyChanged;

        #endregion


        #region Methods
        /// <summary>
        /// Creates a new database item.
        /// </summary>
        /// <param name="name">The name of the new item.</param>
        /// <returns>A reference to the new database.</returns>
        public DatabaseDefinition(string name)
        {
            _Name = name;
            _Id = Guid.NewGuid();
            _Version = 1;
        }

        /// <summary>
        /// Checks to see if there is a property in this item definition with the given name.
        /// </summary>
        /// <param name="name">The name of the property being sought.</param>
        /// <returns><c>true</c> if this definition has a property with the given name.</returns>
        public bool PropertyExists(string name)
        {
            string[] names = AllPropertyNames;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == name) return true;
            }

            return false;
        }

        /// <summary>
        /// Retrives a property by name.
        /// </summary>
        /// <param name="name">The name of the property being sought.</param>
        /// <returns>The first property in this definition with the given name.</returns>
        public DatabaseProperty GetProperty(string name)
        {
            for (int i = 0; i < _PropertiesList.Count; i++)
            {
                if (_PropertiesList[i].Name == name) return _PropertiesList[i];
            }

            return null;
        }

        /// <summary>
        /// Adds a new property of the given type to this definition.
        /// </summary>
        /// <param name="controlType">The type of property to add.</param>
        /// <param name="propertyName">Optional name to give to the property that will be created.</param>
        /// <returns>A reference to the newly created property.</returns>
        public DatabaseProperty AddProperty(ControlDisplayType controlType, Type dataType, string propertyName = null)
        {
            _Version++;
            if (string.IsNullOrEmpty(propertyName)) propertyName = "New Property";
            var prop = new DatabaseProperty(Database.GenerateUniqueName(propertyName, AllPropertyNames), controlType, dataType);
            Properties.Add(prop);
            return prop;
        }

        /// <summary>
        /// Removes the given property from this definition.
        /// </summary>
        /// <param name="prop">The property to remove.</param>
        public void RemoveProperty(DatabaseProperty prop)
        {
            if (_PropertiesList.Remove(prop)) _Version++;
        }
        
        /// <summary>
        /// Creates an exact, deep-copy of this item and all of its properties.
        /// </summary>
        /// <remarks>
        /// References to GameObjects and Components are not supported by this method.
        /// This is due to the internal use of the serializer and its constraints on
        /// serializing GameObject hierarchy data.
        /// </remarks>
        /// <returns>A new <see cref="DatabaseDefinition"/> with properties identical to this one.</returns>
        public DatabaseDefinition Duplicate()
        {
            var doc = Pantagruel.Serializer.XmlSerializer.Serialize(this, 1, "duplicate");
            return Pantagruel.Serializer.XmlDeserializer.Deserialize(doc.InnerXml, 1) as DatabaseDefinition;
        }

        /// <summary>
        /// Returns a list of all properties of this item that store the given type of information.
        /// </summary>
        /// <typeparam name="T">The type of data to filter the list by.</typeparam>
        /// <returns>A list of item properties.</returns>
        public List<T> GetPropertiesOfType<T>()
        {
            throw new UnityException("TODO");
        }

        /// <summary>
        /// Returns a list of all properties of this item that store the given type of information.
        /// </summary>
        /// <param name="t">The type of data to filter the list by.</param>
        /// <returns>A list of item properties.</returns>
        public List<object> GetPropertiesOfType(Type t)
        {
            throw new UnityException("TODO");
        }

        /// <summary>
        /// Attempts to change the given property's name to a new one. This will
        /// fail if the new name is already taken or is invalid.
        /// </summary>
        /// <param currentName="name">The current name of the property that is to be renamed.</param>
        /// <param name="newName">The new name to be given to the property. If this name is already taken, the method will fail.</param>
        /// <returns><c>true</c> if the property was sucessfully renames, <c>false</c> otherwise.</returns>
        public bool TryChangePropertyName(string currentName, string newName)
        {
            if (string.IsNullOrEmpty(newName) || PropertyExists(newName)) return false;

            var prop = GetProperty(currentName);
            var otherProp = GetProperty(newName);
            if(prop != null && otherProp == null)
            {
                prop.Name = newName;
                _Version++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Helper used to trigger events when this property's data changes.
        /// </summary>
        void TriggerDataChangedEvent()
        {
            if (OnPropertyDataChanged != null) OnPropertyDataChanged(Properties);
            if (OnPropertyChanged != null) OnPropertyChanged(this);
        }

        /// <summary>
        /// Helper used to trigger events when this property changes.
        /// </summary>
        void TriggerPropertyChangedEvent()
        {
            if (OnPropertyChanged != null) OnPropertyChanged(this);
        }
        #endregion

    }


}
