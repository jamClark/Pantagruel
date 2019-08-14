using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Pantagruel
{ 

    /// <summary>
    /// Defines a single data point in a <see cref="DatabaseDefinition"/>.
    /// If a DatabaseDefinition is an object or item, these are the stats
    /// that compose it.
    /// </summary>
    [Serializable]
    public class DatabaseProperty : INPC, IBindableDataSource
    {
        #region Fields and Properties
        /// <summary>
        /// The type of property.
        /// </summary>
        /// <remarks>
        /// This is used both to establish the type of data for the backing object as well as tell the
        /// editor what kind of control should be used to visualize and edit this property.
        /// </remarks>
        [XmlIgnore]
        public ControlDisplayType ControlType
        {
            get { return _PropType; }
        }
        [SerializeField]
        private ControlDisplayType _PropType;

        /// <summary>
        /// Returns the type of data that this DatabaseProperty is meant to store.
        /// </summary>
        /// <remarks>
        /// This is needed because the backing field is a System.Object and thus could potentially
        /// be overwritten by a different type that expected or set to null for data that is expected
        /// to be a value type.
        /// </remarks>
        [XmlIgnore]
        public Type EstablishedType
        {
            get { return _EstablishedType; }
        }
        [SerializeField]
        private Type _EstablishedType;

        /// <summary>
        /// The data stored by this property.
        /// </summary>
        /// <remarks>
        /// The data is always cast as a type of <see cref="System.Object"/> but can be detertmined
        /// at rntime using <see cref="DatabaseProperty.EstablishedType"/>. In some cases, if the internal object
        /// is null and the type this property represents is a value type, a new default value will be constructed
        /// and returned in place of the data.
        /// </remarks>
        [XmlIgnore]
        public object Data
        {
            get
            {
                _Data = SanitizeData(_Data);
                return _Data;
            }
            set
            {
                //make sure the value being given is a valid type for this DatabaseProperty's established type.
                if (value == null)
                {
                    if (EstablishedType.IsValueType)
                        throw new ArgumentNullException("Value type properties cannot be given null values.");
                }
                else
                {
                    if (value.GetType() != EstablishedType)
                        throw new ArgumentException("Cannot convert a property established as type '" + EstablishedType.Name + "' to the type '" + value.GetType() + ".");
                }
                var temp = SanitizeData(value);
                if (_Data != temp)
                {
                    _Data = value;
                    TriggerDataChangedEvent();
                }
            }
        }
        [SerializeField]
        private object _Data;

        /// <summary>
        /// The name of this property. This is largely for user-friendliness, however,
        /// properties can be sought within a <see cref="Pantagruel.DatabaseDefinition"/>
        /// by name so it does hold some bearing beyond that.
        /// </summary>
        [XmlIgnore]
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                TriggerPropertyChangedEvent();
            }
        }
        [SerializeField]
        private string _Name;


        public event NotifyPropertyDataChangedEvent OnPropertyDataChanged;
        public event NotifyPropertyChangedEvent OnPropertyChanged;
        #endregion


        #region Methods
        /// <summary>
        /// Constructs a new property with the given name and property type.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="propType"></param>
        public DatabaseProperty(string name, ControlDisplayType propType, Type establishedType)
        {
            var inferred = BindingUtility.InferTypeFromPropType(propType);
            if (establishedType != inferred && !establishedType.IsSubclassOf(inferred))
                throw new ArgumentException("The established type '" + establishedType + "' is not compatible with the property type '" + propType.ToString() + "'.");
            _Name = name;
            _PropType = propType;
            _EstablishedType = establishedType;
            }

        /// <summary>
        /// Ensures that data is not null when representing a value-type.
        /// This requires the backing datatype to actually have a default constructor!
        /// </summary>
        object SanitizeData(object input)
        {
            if (input == null && (EstablishedType.IsValueType || EstablishedType.IsPrimitive))
            {
                return Activator.CreateInstance(EstablishedType);
            }
            return input;
        }

        /// <summary>
        /// Helper used to trigger events when this property's data changes.
        /// </summary>
        void TriggerDataChangedEvent()
        {
            if (OnPropertyDataChanged != null) OnPropertyDataChanged(Data);
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