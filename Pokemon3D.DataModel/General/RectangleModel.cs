﻿using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

// Disable Code Analysis for warning CS0649: Field is never assigned to, and will always have its default value.
#pragma warning disable 0649

namespace Pokemon3D.DataModel
{
    /// <summary>
    /// The data model for a rectangle definition.
    /// </summary>
    [DataContract(Namespace = "")]
    public class RectangleModel : DataModel<RectangleModel>
    {
        /// <summary>
        /// The x position of this rectangle model.
        /// </summary>        
        [DataMember(Order = 0)]
        public int X;

        /// <summary>
        /// The y position of this rectangle model.
        /// </summary>        
        [DataMember(Order = 1)]
        public int Y;

        /// <summary>
        /// The width of this rectangle model.
        /// </summary>        
        [DataMember(Order = 2)]
        public int Width;

        /// <summary>
        /// The height of this rectangle model.
        /// </summary>        
        [DataMember(Order = 3)]
        public int Height;

        /// <summary>
        /// Returns the corresponding <see cref="Rectangle"/> to this model.
        /// </summary>
        public Rectangle GetRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}
