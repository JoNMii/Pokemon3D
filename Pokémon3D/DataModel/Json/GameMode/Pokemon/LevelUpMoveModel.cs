﻿using System.Runtime.Serialization;

// Disable Code Analysis for warning CS0649: Field is never assigned to, and will always have its default value.
#pragma warning disable 0649

namespace Pokémon3D.DataModel.Json.GameMode.Pokemon
{
    /// <summary>
    /// The data model for a move a Pokémon learns at level up.
    /// </summary>
    [DataContract]
    class LevelUpMoveModel : JsonDataModel
    {
        /// <summary>
        /// The level the Pokémon learns the move at.
        /// </summary>
        [DataMember(Order = 0)]
        public int Level;

        /// <summary>
        /// The Id of the move.
        /// </summary>
        [DataMember(Order = 1)]
        public int Id;
    }
}
