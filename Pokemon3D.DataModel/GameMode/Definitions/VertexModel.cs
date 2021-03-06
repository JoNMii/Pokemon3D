﻿using System.Runtime.Serialization;

// Disable Code Analysis for warning CS0649: Field is never assigned to, and will always have its default value.
#pragma warning disable 0649

namespace Pokemon3D.DataModel.GameMode.Definitions
{
    /// <summary>
    /// The data model for a vertex declaration with Position, Normal and Texture Coordinate within a primitive model.
    /// </summary>
    [DataContract(Namespace = "")]
    public class VertexModel : DataModel<VertexModel>
    {
        [DataMember(Order = 0)]
        public Vector3Model Position;

        [DataMember(Order = 1)]
        public Vector3Model Normal;

        [DataMember(Order = 2)]
        public Vector2Model TexCoord;

        public override object Clone()
        {
            var clone = (VertexModel)MemberwiseClone();
            clone.Position = Position.CloneModel();
            clone.Normal = Normal.CloneModel();
            clone.TexCoord = TexCoord.CloneModel();
            return clone;
        }
    }
}
