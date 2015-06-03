﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGL.SceneComponent
{
    /// <summary>
    /// The color-coded picking result.
    /// <para>Representing a primitive.</para>
    /// </summary>
    public abstract class PickedGeometryBase : IPickedGeometry
    {
        /// <summary>
        /// Gets or sets primitive's geometry type.
        /// </summary>
        public GeometryTypes GeometryType { get; set; }

        /// <summary>
        /// Gets or sets positions of this primitive's vertices.
        /// </summary>
        public float[] positions { get; set; }

        /// <summary>
        /// The last vertex's id that constructs the picked primitive.
        /// <para>This id is in scene's all <see cref="IColorCodedPicking"/>s' order.</para>
        /// </summary>
        public int StageVertexID { get; set; }

        /// <summary>
        /// The element that this picked primitive belongs to.
        /// </summary>
        public virtual IColorCodedPicking Element { get; set; }

        public override string ToString()
        {
            var positions = this.positions;
            if (positions == null) { positions = new float[0]; }

            string strPositions = positions.PrintVectors();

            int stageVertexID = this.StageVertexID;
            IColorCodedPicking picking = this.Element;

            string lastVertexID = "?";
            if (picking != null)
            { lastVertexID = string.Format("{0}", picking.GetLastVertexIDOfPickedPrimitive(stageVertexID)); }

            string result = string.Format("{0}: P: {1} ID:{2}/{3} ∈{4}",
                GeometryType, strPositions, lastVertexID, stageVertexID, Element);
            return result;
            //return base.ToString();
        }

    }
}