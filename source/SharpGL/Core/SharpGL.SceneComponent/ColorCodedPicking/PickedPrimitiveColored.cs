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
    public class PickedPrimitiveColored : PickedPrimitiveBase 
    {
        /// <summary>
        /// Gets or sets colors of this primitive.
        /// </summary>
        public float[] colors { get; set; }

        public override string ToString()
        {
            var positions = this.positions;
            if (positions == null) { positions = new float[0]; }
            var colors = this.colors;
            if (colors == null) { colors = new float[0]; }

            string strPositions = positions.PrintVectors();
            string strColors = colors.PrintVectors();

            int stageVertexID = this.StageVertexID;
            IColorCodedPicking picking = this.Element;

            string lastVertexID = "?";
            if (picking != null)
            { lastVertexID = string.Format("{0}", picking.GetLastVertexIDOfPickedPrimitive(stageVertexID)); }

            string result = string.Format("{0}:{1}|{2}|ID:{3}/{4}|∈{5}",
                GeometryType, strPositions, strColors, lastVertexID, stageVertexID, Element);

            return result;
            //return base.ToString();
        }

    }
}