﻿using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ColorVertexSample
{
    public class AxisAttachment
    {
        public int AxisWidth { get; set; }
        public int AxisHeight { get; set; }
        public float PenWidth
        {
            get { return _penWidth; }
            set
            {
                if (this.pens == null)
                {
                    this.pens = new Pen[] { new Pen(Color.Red, value), new Pen(Color.Green, value), new Pen(Color.Blue, value) };
                }
                else
                {
                    foreach (var item in this.pens)
                    {
                        item.Width = value;
                    }
                }
                this._penWidth = value;
            }
        }

        private float _penWidth = 2;
        private Scene axisScene = new Scene();
        private ArcBallEffect2 rotationEffect;
        private LookAtCamera parallelCamera;
        private Pen[] pens;
        private AxisSpy axisSpy;
        private SceneControl control;
        private MouseEventHandler mouseDownEventHandler;
        private MouseEventHandler mouseMoveEventHandler;
        private MouseEventHandler mouseUpEventHandler;
        private RenderEventHandler renderEventHandler;

        public AxisAttachment()
        {
            AxisWidth = 80;
            AxisHeight = 80;
            PenWidth = 2;
            this.mouseDownEventHandler = new MouseEventHandler(SceneControl_MouseDown);
            this.mouseMoveEventHandler = new MouseEventHandler(SceneControl_MouseMove);
            this.mouseUpEventHandler = new MouseEventHandler(SceneControl_MouseUp);
            this.renderEventHandler = new RenderEventHandler(SceneControl_GDIDraw);
        }

        public void AttachTo(SceneControl control)
        {
            if (control == null)
            { throw new ArgumentNullException("control"); }


            CreateOpenGL(axisScene, control);

            InitParallelCamera(axisScene, control);

            InitAxis(this.axisScene, control);

            control.MouseDown += this.mouseDownEventHandler;
            control.MouseMove += this.mouseMoveEventHandler;
            control.MouseUp += this.mouseUpEventHandler;
            control.GDIDraw += this.renderEventHandler;

            this.control = control;
        }

        public void Dettach()
        {
            var control = this.control;
            if (control == null) { return; }

            control.MouseDown -= this.mouseDownEventHandler;
            control.MouseMove -= this.mouseMoveEventHandler;
            control.MouseUp -= this.mouseUpEventHandler;
            control.GDIDraw -= this.renderEventHandler;

            this.control = null;
        }

        private void InitParallelCamera(Scene scene, SceneControl control)
        {
            parallelCamera = new LookAtCamera();
            parallelCamera.AspectRatio = (double)AxisWidth / (double)AxisHeight;
            parallelCamera.Near = 0.001f;
            parallelCamera.Far = float.MaxValue;

            var modelSceneCamera = control.Scene.CurrentCamera as LookAtCamera;
            if (modelSceneCamera != null)
            {
                var position = modelSceneCamera.Position - modelSceneCamera.Target;
                position.Normalize();
                parallelCamera.Position = position * 7;
                parallelCamera.UpVector = modelSceneCamera.UpVector;
                parallelCamera.FieldOfView = modelSceneCamera.FieldOfView;
            }
            else
            {
                parallelCamera.Position = new Vertex(0f, 0f, 7f);
                parallelCamera.UpVector = new Vertex(0f, 1f, 0f);
                parallelCamera.FieldOfView = 60;
            }

            scene.CurrentCamera = this.parallelCamera;
        }

        /// <summary>
        /// Draw axis at corner of view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void SceneControl_GDIDraw(object sender, RenderEventArgs args)
        {
            var control = this.control;
            if (control == null) { return; }

            var modelSceneCamera = control.Scene.CurrentCamera as LookAtCamera;
            if (modelSceneCamera != null)
            {
                UpdateParallelCamera(modelSceneCamera);
            }

            // update data of this.axisSpy.projectedAxisVertexes
            this.axisScene.OpenGL.MakeCurrent();
            this.axisScene.Draw();

            var targets = this.axisSpy.projectedAxisVertexes;
            for (int i = 1; i < targets.Length; i++)
            {
                args.Graphics.DrawLine(this.pens[i - 1],
                    targets[0].X, control.Height - targets[0].Y,
                    targets[i].X, control.Height - targets[i].Y);
            }
        }

        private void UpdateParallelCamera(LookAtCamera modelSceneCamera)
        {
            var position = modelSceneCamera.Position - modelSceneCamera.Target;
            position.Normalize();
            parallelCamera.Position = position * 7;
            parallelCamera.UpVector = modelSceneCamera.UpVector;
            parallelCamera.FieldOfView = modelSceneCamera.FieldOfView;
            this.rotationEffect.ArcBall.SetCamera(parallelCamera);
        }

        private void CreateOpenGL(Scene scene, SceneControl control)
        {
            OpenGL gl = new OpenGL();
            gl.Create(control.OpenGLVersion, SharpGL.RenderContextType.FBO,
                AxisWidth, AxisHeight, 32, null);
            //  Set the most basic OpenGL styles.
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            gl.ClearDepth(1.0f);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.DepthFunc(OpenGL.GL_LEQUAL);
            gl.Hint(OpenGL.GL_PERSPECTIVE_CORRECTION_HINT, OpenGL.GL_NICEST);
            gl.SetDimensions(AxisWidth, AxisHeight);
            gl.Viewport(0, 0, AxisWidth, AxisHeight);
            scene.OpenGL = gl;
        }

        private void InitAxis(SharpGL.SceneGraph.Scene scene, SceneControl control)
        {
            this.axisSpy = new AxisSpy();
            this.rotationEffect = new ArcBallEffect2(this.parallelCamera);
            scene.SceneContainer.AddChild(this.axisSpy);
            scene.SceneContainer.AddEffect(this.rotationEffect);
        }

        void SceneControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var control = this.control;
            if (control == null) { return; }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.rotationEffect.ArcBall.MouseUp(e.X, e.Y);
                control.Invalidate();
            }
        }

        void SceneControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var control = this.control;
            if (control == null) { return; }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.rotationEffect.ArcBall.MouseMove(e.X, e.Y);
                control.Invalidate();
            }
        }

        void SceneControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var control = this.control;
            if (control == null) { return; }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.rotationEffect.ArcBall.SetBounds(control.Width, control.Height);
                this.rotationEffect.ArcBall.MouseDown(e.X, e.Y);
                control.Invalidate();
            }
        }

        public void ResetAxisRotation()
        {
            this.rotationEffect.ArcBall.ResetRotation();
        }
    }
}
