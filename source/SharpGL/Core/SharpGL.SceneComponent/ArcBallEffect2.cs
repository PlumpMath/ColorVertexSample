﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Effects;

namespace SharpGL.SceneComponent
{
    /// <summary>
    /// The ArcBall camera supports arcball projection, making it ideal for use with a mouse.
    /// <para>supports arcball rotation in a moving camera</para>
    /// </summary>
    public class ArcBallEffect2 : Effect
    {
        /// <summary>
        ///  if null, please set arcBall.Camera property later.
        /// </summary>
        /// <param name="camera">if null, please set arcBall.Camera property later.</param>
        public ArcBallEffect2(LookAtCamera camera = null)
        {
            this.arcBall.Camera = camera;
        }

        /// <summary>
        /// Pushes the effect onto the specified parent element.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        /// <param name="parentElement">The parent element.</param>
        public override void Push(OpenGL gl, SceneElement parentElement)
        {
            //  Push the stack.
            gl.PushMatrix();

            //  Perform the transformation.
            arcBall.TransformMatrix(gl);
        }

        /// <summary>
        /// Pops the specified parent element.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        /// <param name="parentElement">The parent element.</param>
        public override void Pop(OpenGL gl, SceneElement parentElement)
        {
            //  Pop the stack.
            gl.PopMatrix();
        }

        /// <summary>
        /// The arcball.
        /// </summary>
        private ArcBall2 arcBall = new ArcBall2();

        /// <summary>
        /// Gets or sets the linear transformation.
        /// </summary>
        /// <value>
        /// The linear transformation.
        /// </value>
        [Description("The ArcBall."), Category("Effect")]
        public ArcBall2 ArcBall
        {
            get { return arcBall; }
            set { arcBall = value; }
        }
    }
}