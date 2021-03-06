using System.Runtime.Serialization;

// Disable Code Analysis for warning CS0649: Field is never assigned to, and will always have its default value.
#pragma warning disable 0649

namespace Pokemon3D.DataModel.GameMode.Definitions
{
    /// <summary>
    /// The data model for a primitive model.
    /// </summary>
    [DataContract(Namespace = "")]
    public class PrimitiveModel : DataModel<PrimitiveModel>
    {
        /// <summary>
        /// Referenced in: <see cref="Map.Entities.EntityRenderModeModel.PrimitiveModelId"/>.
        /// </summary>
        [DataMember(Order = 0)]
        public string Id;
        
        [DataMember(Order = 1)]
        public VertexModel[] Vertices;
        
        [DataMember(Order = 2)]
        public int[] Indices;

        public override object Clone()
        {
            var clone = (PrimitiveModel)MemberwiseClone();
            clone.Vertices = (VertexModel[])Vertices.Clone();
            clone.Indices = (int[])Indices.Clone();
            return clone;
        }
    }
}
