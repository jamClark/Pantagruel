
using System;
using UnityEngine;

namespace Pantagruel
{
    public delegate void NotifyPropertyDataChangedEvent(object propData);
    public delegate void NotifyPropertyChangedEvent(INPC prop);

    public enum ControlDisplayType
    {
        Char,
        String,
        Paragraph,
        Bool,
        Integer,
        Float,
        Guid,
        Color,
        Curve,
        LayerMask,
        Vector2,
        Vector3,
        Vector4,
        Quaternion,
        Bounds,
        Rect,
        Gradient,

        //display-only controls
        StaticImage,
        StaticText,

        //these are for drop-down lists. The contents of the lists are based on 'established type'
        DropDownList,
        DropDownEnum,

        //spacial case handler for controls embedded in other controls
        EmbeddedControls,

        //the actual data is restricted to the 'established type'
        UnityObject,
        
    }


    /// <summary>
    /// Interface used to notify others that this property has changed its value.
    /// </summary>
    public interface INPC
    {
        event NotifyPropertyDataChangedEvent OnPropertyDataChanged;
        event NotifyPropertyChangedEvent OnPropertyChanged;
    }

    /// <summary>
    /// An iterface for describing a datasource that can be bound to a control.
    /// </summary>
    public interface IBindableDataSource : INPC
    {
        object Data { get; set; }
        ControlDisplayType ControlType { get; }
        Type EstablishedType { get; }
    }


    /// <summary>
    /// Utility class for IBindableDataSources.
    /// </summary>
    public static class BindingUtility
    {
        public static Type InferTypeFromPropType(ControlDisplayType propType)
        {
            switch (propType)
            {
                case ControlDisplayType.Bool: { return typeof(bool); }
                case ControlDisplayType.Integer: { return typeof(int); }
                case ControlDisplayType.Float: { return typeof(float); }
                case ControlDisplayType.String: { return typeof(string); }
                case ControlDisplayType.UnityObject: { return typeof(UnityEngine.Object); }
                case ControlDisplayType.Guid: { return typeof(Guid); }
                case ControlDisplayType.Color: { return typeof(Color); }
                case ControlDisplayType.DropDownList: { return typeof(string); }
                case ControlDisplayType.DropDownEnum: { return typeof(int); }
                case ControlDisplayType.Curve: { return typeof(AnimationCurve); }
                default: return typeof(object);
            }
        }
    }
}
