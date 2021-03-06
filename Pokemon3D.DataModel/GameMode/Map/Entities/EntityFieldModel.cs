﻿using System.Runtime.Serialization;

// Disable Code Analysis for warning CS0649: Field is never assigned to, and will always have its default value.
#pragma warning disable 0649

namespace Pokemon3D.DataModel.GameMode.Map.Entities
{
    /// <summary>
    /// A field of entities, defined by a single entity definition.
    /// </summary>
    [DataContract(Namespace = "")]
    public class EntityFieldModel : DataModel<EntityFieldModel>
    {
        [DataMember(Order = 1)]
        public EntityFieldPositionModel[] Placing;

        [DataMember(Order = 2)]
        public EntityModel Entity;

        public override object Clone()
        {
            var clone = (EntityFieldModel)MemberwiseClone();
            clone.Placing = (EntityFieldPositionModel[])Placing.Clone();
            clone.Entity = Entity.CloneModel();
            return clone;
        }
    }
}
