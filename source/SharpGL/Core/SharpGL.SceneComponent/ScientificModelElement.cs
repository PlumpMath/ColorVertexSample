﻿using SharpGL.SceneGraph.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGL.SceneComponent
{
    public class ScientificModelElement : SceneElement, IRenderable
    {
        public IScientificModel Model { get; set; }

        #region IRenderable 成员

        public void Render(OpenGL gl, RenderMode renderMode)
        {
            IScientificModel model = this.Model;
            if (model == null) { return; }

            model.Render(gl, renderMode);
        }

        #endregion
    }
}