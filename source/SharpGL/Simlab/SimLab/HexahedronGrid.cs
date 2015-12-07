﻿using GlmNet;
using SharpGL;
using SharpGL.SceneComponent;
using SharpGL.SceneGraph.Core;
using SharpGL.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLab
{
    public class HexahedronGrid : SimLabGrid, IRenderable
    {
        private const string in_Position = "in_Position";
        private const string in_uv = "in_uv";
        uint ATTRIB_INDEX_POSITION = 0;
        uint ATTRIB_INDEX_COLOUR = 1;

        protected uint[] indexBuffer;
        protected int indexBufferLength;

        protected uint[] wireframeIndexBuffer;
        protected int wireframeIndexBufferLength;

        uint[] vertexArrayObject;

        private GlmNet.mat4 projectionMatrix;
        private GlmNet.mat4 viewMatrix;
        private mat4 modelMatrix;
        private ShaderProgram shaderProgram;

        public HexahedronGrid(OpenGL gl, IScientificCamera camera)
            : base(gl, camera)
        {

        }

        public void Init(HexahedronMeshGeometry3D geometry)
        {
            base.Init(geometry);

            indexBuffer = new uint[1];
            indexBuffer[0] = CreateVertexBufferObject(OpenGL.GL_ELEMENT_ARRAY_BUFFER, geometry.TriangleIndices, OpenGL.GL_STATIC_DRAW);

            int elementLength = sizeof(uint);// should be 4.
            this.indexBufferLength = geometry.TriangleIndices.SizeInBytes / (elementLength);
        }

        public override void SetWireframe(WireFrameBufferData lineIndexes)
        {
            if (lineIndexes != null)
            {
                if (wireframeIndexBuffer != null)
                {
                    gl.DeleteBuffers(wireframeIndexBuffer.Length, wireframeIndexBuffer);
                }
                ////TODO:如果用此方式，则必须先将此对象加入scene树，然后再调用Init
                //OpenGL gl = this.TraverseToRootElement().ParentScene.OpenGL;
                wireframeIndexBuffer = new uint[1];
                wireframeIndexBuffer[0] = CreateVertexBufferObject(OpenGL.GL_ARRAY_BUFFER, lineIndexes, OpenGL.GL_STATIC_DRAW);

                int elementLength = sizeof(uint);// should be 4.
                this.wireframeIndexBufferLength = lineIndexes.SizeInBytes / (elementLength);
            }
            else
            {
                if (wireframeIndexBuffer != null)
                {
                    gl.DeleteBuffers(wireframeIndexBuffer.Length, wireframeIndexBuffer);
                }
            }
        }

        #region IRenderable

        protected void BeforeRendering(OpenGL gl, RenderMode renderMode)
        {
            IScientificCamera camera = this.camera;
            if (camera != null)
            {
                if (camera.CameraType == CameraTypes.Perspecitive)
                {
                    IPerspectiveViewCamera perspective = camera;
                    this.projectionMatrix = perspective.GetProjectionMat4();
                    this.viewMatrix = perspective.GetViewMat4();
                }
                else if (camera.CameraType == CameraTypes.Ortho)
                {
                    IOrthoViewCamera ortho = camera;
                    this.projectionMatrix = ortho.GetProjectionMat4();
                    this.viewMatrix = ortho.GetViewMat4();
                }
                else
                { throw new NotImplementedException(); }
            }

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            this.texture.Bind(gl);

            modelMatrix = mat4.identity();
            ShaderProgram shaderProgram = this.shaderProgram;
            //  Bind the shader, set the matrices.
            shaderProgram.Bind(gl);
            shaderProgram.SetUniformMatrix4(gl, "projectionMatrix", projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "viewMatrix", viewMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "modelMatrix", modelMatrix.to_array());

            shaderProgram.SetUniform1(gl, "tex", this.texture.TextureName);


            gl.Enable(OpenGL.GL_POLYGON_SMOOTH);
            gl.Hint(OpenGL.GL_POLYGON_SMOOTH_HINT, OpenGL.GL_NICEST);
        }

        #region IRenderable 成员

        void IRenderable.Render(OpenGL gl, RenderMode renderMode)
        {
            if (positionBuffer == null || colorBuffer == null) { return; }

            if (this.shaderProgram == null)
            {
                this.shaderProgram = InitShader(gl, renderMode);
            }
            if (this.vertexArrayObject == null)
            {
                CreateVertexArrayObject(gl, renderMode);
            }

            BeforeRendering(gl, renderMode);

            if (this.RenderGridWireFrame)
            {
                if (wireframeIndexBuffer != null)
                {
                    shaderProgram.SetUniform1(gl, "renderingWireframe", 1.0f);

                    gl.Disable(OpenGL.GL_LINE_STIPPLE);
                    gl.Disable(OpenGL.GL_POLYGON_STIPPLE);
                    gl.Enable(OpenGL.GL_LINE_SMOOTH);
                    gl.Enable(OpenGL.GL_POLYGON_SMOOTH);
                    gl.ShadeModel(SharpGL.Enumerations.ShadeModel.Smooth);
                    gl.Hint(SharpGL.Enumerations.HintTarget.LineSmooth, SharpGL.Enumerations.HintMode.Nicest);
                    gl.Hint(SharpGL.Enumerations.HintTarget.PolygonSmooth, SharpGL.Enumerations.HintMode.Nicest);
                    gl.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Lines);

                    gl.BindVertexArray(this.vertexArrayObject[0]);
                    gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, wireframeIndexBuffer[0]);
                    gl.DrawElements(OpenGL.GL_LINES, this.wireframeIndexBufferLength, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    gl.BindVertexArray(0);

                    gl.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Filled);
                    gl.Disable(OpenGL.GL_POLYGON_SMOOTH);

                }
            }


            if (this.RenderGrid)
            {
                if (positionBuffer != null && colorBuffer != null && indexBuffer != null)
                {
                    shaderProgram.SetUniform1(gl, "renderingWireframe", 0.0f);
                    gl.BindVertexArray(this.vertexArrayObject[0]);
                    gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, indexBuffer[0]);
                    gl.DrawElements(OpenGL.GL_TRIANGLES, this.indexBufferLength, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    gl.BindVertexArray(0);
                }
            }

            AfterRendering(gl, renderMode);
        }


        private void CreateVertexArrayObject(OpenGL gl, RenderMode renderMode)
        {
            this.vertexArrayObject = new uint[1];
            gl.GenVertexArrays(1, this.vertexArrayObject);
            gl.BindVertexArray(this.vertexArrayObject[0]);

            // prepare positions
            {
                int location = shaderProgram.GetAttributeLocation(gl, in_Position);
                ATTRIB_INDEX_POSITION = (uint)location;
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, positionBuffer[0]);
                gl.VertexAttribPointer(ATTRIB_INDEX_POSITION, 3, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.EnableVertexAttribArray(ATTRIB_INDEX_POSITION);
            }
            // prepare colors
            {
                int location = shaderProgram.GetAttributeLocation(gl, in_uv);
                ATTRIB_INDEX_COLOUR = (uint)location;
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, colorBuffer[0]);
                gl.VertexAttribPointer(ATTRIB_INDEX_COLOUR, 1, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.EnableVertexAttribArray(ATTRIB_INDEX_COLOUR);
            }

            gl.BindVertexArray(0);
        }

        private ShaderProgram InitShader(OpenGL gl, RenderMode renderMode)
        {
            String vertexShaderSource = ManifestResourceLoader.LoadTextFile(@"HexahedronGrid.vert");
            String fragmentShaderSource = ManifestResourceLoader.LoadTextFile(@"HexahedronGrid.frag");
            ShaderProgram shaderProgram = new ShaderProgram();
            shaderProgram.Create(gl, vertexShaderSource, fragmentShaderSource, null);
            //shaderProgram.BindAttributeLocation(gl, ATTRIB_INDEX_POSITION, in_position);
            //shaderProgram.BindAttributeLocation(gl, ATTRIB_INDEX_COLOUR, in_uv);
            {
                int location = shaderProgram.GetAttributeLocation(gl, in_Position);
                if (location < 0) { throw new ArgumentException(); }
                this.ATTRIB_INDEX_POSITION = (uint)location;
            }
            {
                int location = shaderProgram.GetAttributeLocation(gl, in_uv);
                if (location < 0) { throw new ArgumentException(); }
                this.ATTRIB_INDEX_COLOUR = (uint)location;
            }
            shaderProgram.AssertValid(gl);
            return shaderProgram;
        }

        #endregion

        protected void AfterRendering(OpenGL gl, RenderMode renderMode)
        {
            gl.Disable(OpenGL.GL_POLYGON_SMOOTH);

            shaderProgram.Unbind(gl);

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);

            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }

        #endregion IRenderable

        protected override void DisposeUnmanagedResources()
        {
            base.DisposeUnmanagedResources();

            if (this.indexBuffer != null)
            {
                gl.DeleteBuffers(this.indexBuffer.Length, this.indexBuffer);
            }

            if (this.wireframeIndexBuffer != null)
            {
                gl.DeleteBuffers(this.wireframeIndexBuffer.Length, this.wireframeIndexBuffer);
            }

            if (this.vertexArrayObject != null)
            {
                gl.DeleteVertexArrays(this.vertexArrayObject.Length, this.vertexArrayObject);
            }
        }
    }
}
