﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGL.SceneGraph.Effects;

namespace SharpGL.SceneComponent
{
    /// <summary>
    /// show the binded element in a specifed viewport.
    /// Don't use this class as it shows black background.
    /// </summary>
    public class ViewportEffect : Effect
    {
        public System.Drawing.Rectangle viewport;
        public System.Drawing.Rectangle fullViewport;

        public ViewportEffect(System.Drawing.Rectangle viewport, System.Drawing.Rectangle fullViewport)
        {
            this.viewport = viewport;
            this.fullViewport = fullViewport;
        }

        public override void Push(SharpGL.OpenGL gl, SharpGL.SceneGraph.Core.SceneElement parentElement)
        {
            gl.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
        }

        public override void Pop(SharpGL.OpenGL gl, SharpGL.SceneGraph.Core.SceneElement parentElement)
        {
            gl.Viewport(fullViewport.X, fullViewport.Y, fullViewport.Width, fullViewport.Height);
        }
    }
}
