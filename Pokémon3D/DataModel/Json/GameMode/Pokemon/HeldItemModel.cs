﻿using System.Runtime.Serialization;

// Disable Code Analysis for warning CS0649: Field is never assigned to, and will always have its default value.
#pragma warning disable 0649

namespace Pokémon3D.DataModel.Json.GameMode.Pokemon
{
    /// <summary>
    /// A data model for an item held by a wild Pokémon.
    /// </summary>
    [DataContract]
    class HeldItemModel : JsonDataModel
    {
        /// <summary>
        /// The Id of the item.
        /// </summary>
        [DataMember(Order = 0)]
        public int ItemId;

        /// <summary>
        /// The chance of this item appearing.
        /// </summary>
        /// <remarks>This is not a percentage, but rather relative to all other objects in the same chance list.</remarks>
        [DataMember(Order = 1)]
        public int Chance;
    }
}
