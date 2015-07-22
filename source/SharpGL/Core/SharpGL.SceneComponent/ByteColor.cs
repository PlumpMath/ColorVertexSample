﻿using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpGL.SceneComponent
{
    public struct ByteColor
    {
        public byte red;
        public byte green;
        public byte blue;
        //public byte alpha;
    }

    [StructLayout(LayoutKind.Sequential,Pack=1)]
    public struct ColorF
    {
        float r;  //red
        float g;  //green
        float b;  //blue
        float a;  //alpha

        public float R
        {
            get
            {
                return r;
            }
        }

        public float G
        {
            get
            {
                return g;
            }
        }

        public float B
        {
            get
            {
                return b;
            }
        }

        public float A
        {
            get
            {
                return a;
            }
        }

        public static explicit operator ColorF(GLColor color)
        {
            ColorF c;
            c.r = color.R;
            c.g = color.G;
            c.b = color.B;
            c.a = 1.0f;
            return c;
        }


    }
    
}
