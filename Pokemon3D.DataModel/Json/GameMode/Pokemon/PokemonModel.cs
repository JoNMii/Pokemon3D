﻿using System;
using System.Runtime.Serialization;
using Pokemon3D.DataModel.Json.Pokemon;

// Disable Code Analysis for warning CS0649: Field is never assigned to, and will always have its default value.
#pragma warning disable 0649

namespace Pokemon3D.DataModel.Json.GameMode.Pokemon
{
    /// <summary>
    /// The data model for a Pokémon definition.
    /// </summary>
    [DataContract]
    public class PokemonModel : JsonDataModel<PokemonModel>
    {
        [DataMember(Order = 0)]
        public string Name;
        
        [DataMember(Order = 1)]
        public string Id;

        [DataMember(Order = 2)]
        public PokedexEntryModel PokedexEntry;

        [DataMember(Order = 3)]
        public string InitScript;

        [DataMember(Order = 4)]
        public PokemonFormModel[] Forms;

        [DataMember(Order = 5)]
        public SpawnHeldItemModel[] HeldItems;

        [DataMember(Order = 6, Name = "ExperienceType")]
        private string _experienceType;
        
        public ExperienceType ExperienceType
        {
            get { return ConvertStringToEnum<ExperienceType>(_experienceType); }
        }
        
        [DataMember(Order = 7)]
        public EvolutionConditionModel[] EvolutionConditions;

        [DataMember(Order = 8)]
        public bool IsLegendary;

        [DataMember(Order = 9)]
        public int BaseFriendship;

        [DataMember(Order = 10)]
        public PokemonStatSetModel RewardEV;

        [DataMember(Order = 11)]
        public double IsMale;

        [DataMember(Order = 12)]
        public bool IsGenderless;

        [DataMember(Order = 13)]
        public bool CanBreed;

        [DataMember(Order = 14)]
        public int BaseEggSteps;

        [DataMember(Order = 15)]
        public int EggPokemon;

        [DataMember(Order = 16)]
        public int Devolution;

        [DataMember(Order = 17)]
        public string[] EggGroups;

        public override object Clone()
        {
            var clone = (PokemonModel)MemberwiseClone();
            clone.PokedexEntry = PokedexEntry.CloneModel();
            clone.Forms = (PokemonFormModel[])Forms.Clone();
            clone.HeldItems = (SpawnHeldItemModel[])HeldItems.Clone();
            clone.EvolutionConditions = (EvolutionConditionModel[])EvolutionConditions.Clone();
            clone.RewardEV = RewardEV.CloneModel();
            clone.EggGroups = (string[])EggGroups.Clone();
            return clone;
        }
    }
}
