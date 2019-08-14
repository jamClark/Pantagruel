using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Pantagruel.Collections;

namespace Pantagruel
{
    /// <summary>
    /// Idetifies this item with a given database and item definition.
    /// </summary>
    [Serializable]
    public class BaseItem : MonoBehaviour
    {
        private Guid DatabaseId;
        private Guid ItemDefinitionId;

        //[ItemPropView]
        /// <summary>
        /// Properties that are tied to this item directly rather than to its database definition.
        /// These properties cannot be saved by Unity's built-in serialization system.
        /// </summary>
        private Dictionary<string, object> TransientProperties;
    }
}
