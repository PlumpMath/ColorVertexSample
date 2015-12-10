﻿using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimLab2.SimGrid.Geometry
{


    /// <summary>
    /// 描述一条线段
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LinePosition
    {

        /// <summary>
        /// 起始点
        /// </summary>
       public  Vertex P1;

        /// <summary>
        /// 终点
        /// </summary>
       public  Vertex P2;
    }
}
