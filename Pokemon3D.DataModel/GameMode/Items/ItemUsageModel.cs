﻿using System.Runtime.Serialization;

// Disable Code Analysis for warning CS0649: Field is never assigned to, and will always have its default value.
#pragma warning disable 0649

namespace Pokemon3D.DataModel.GameMode.Items
{
    [DataContract(Namespace = "")]
    public class ItemUsageModel : DataModel<ItemUsageModel>
    {
        [DataMember(Order = 0)]
        public bool Overworld;

        [DataMember(Order = 1)]
        public bool Trade;

        [DataMember(Order = 2)]
        public bool Held;

        [DataMember(Order = 3)]
        public bool InBattle;

        [DataMember(Order = 4)]
        public bool Toss;

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}
