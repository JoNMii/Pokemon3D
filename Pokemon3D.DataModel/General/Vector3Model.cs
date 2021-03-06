﻿using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

// Disable Code Analysis for warning CS0649: Field is never assigned to, and will always have its default value.
#pragma warning disable 0649

namespace Pokemon3D.DataModel
{
    /// <summary>
    /// The data model for a <see cref="Vector3"/> definition.
    /// </summary>
    [DataContract(Namespace = "")]
    public class Vector3Model : DataModel<Vector3Model>
    {
        /// <summary>
        /// The X coordinate of this vector.
        /// </summary>
        [DataMember(Order = 0)]
        public float X;

        /// <summary>
        /// The Y coordinate of this vector.
        /// </summary>
        [DataMember(Order = 1)]
        public float Y;

        /// <summary>
        /// The Z coordinate of this vector.
        /// </summary>
        [DataMember(Order = 2)]
        public float Z;

        /// <summary>
        /// Returns the corresponding <see cref="Vector3"/> to this model.
        /// </summary>
        public Vector3 GetVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}
