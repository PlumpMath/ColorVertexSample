﻿using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimLab.GridSource
{

         /// <summary>
     /// map to opengl buffer
     /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Hexahedron
    {
        /// <summary>
        ///  front left top p0
        /// </summary>
        Vertex FLT;

        /// <summary>
        /// front right top p1
        /// </summary>
        Vertex FRT;

        /// <summary>
        /// back right top p2
        /// </summary>
        Vertex BRT;

        /// <summary>
        /// back left top p4
        /// </summary>
        Vertex BLT;

        /// <summary>
        /// 
        /// </summary>
        Vertex FLB;
        Vertex FRB;
        Vertex BRB;
        Vertex BLB;

    }
}
