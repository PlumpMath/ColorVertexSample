﻿using GlmNet;
using SharpGL;
using SharpGL.SceneComponent;
using SharpGL.SceneComponent.Utility;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Core;
using SharpGL.Shaders;
using SimLab.SimGrid.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLab
{
    public class DynamicUnstructureGrid : SimLabGrid, IRenderable
    {
        private bool renderFraction = true;

        public bool RenderFraction
        {
            get { return renderFraction; }
            set { renderFraction = value; }
        }

        private bool renderFractionWireframe = false;

        public bool RenderFractionWireframe
        {
            get { return renderFractionWireframe; }
            set { renderFractionWireframe = value; }
        }

        private FractionType fractionType;

        public FractionType FractionType
        {
            get { return fractionType; }
            set { fractionType = value; }
        }

        private int fractionLineWidth = 1;

        /// <summary>
        /// 当fraction是线段时，此值表示绘制线段的宽度。
        /// </summary>
        public int FractionLineWidth
        {
            get { return fractionLineWidth; }
            set { fractionLineWidth = value; }
        }

        private int fractionVertexCount;

        /// <summary>
        /// fraction有多少个顶点
        /// </summary>
        public int FractionVertexCount
        {
            get { return fractionVertexCount; }
            set { fractionVertexCount = value; }
        }

        private MatrixType matrixType;

        public MatrixType MatrixType
        {
            get { return matrixType; }
            set { matrixType = value; }
        }

        private int matrixVertexOrIndexCount;

        /// <summary>
        /// 当基质为三角形时，此值表示基质有多少个顶点。
        /// <para>当基质为四面体时，此值表示基质的索引数组(uint[])的元素数目。</para>
        /// </summary>
        public int MatrixVertexOrIndexCount
        {
            get { return matrixVertexOrIndexCount; }
            set { matrixVertexOrIndexCount = value; }
        }

        private const string in_Position = "in_Position";
        private const string in_uv = "in_uv";

        uint ATTRIB_INDEX_POSITION = 0;
        uint ATTRIB_INDEX_UV = 1;
        //uint ATTRIB_INDEX_FRACTION_POSITION = 2;
        //uint ATTRIB_INDEX_FRACTION_UV = 2;

        /// <summary>
        /// 如果基质是四面体，则不需要此indexbuffer
        /// </summary>
        private uint[] matrixIndexBuffer;

        private uint[] fractionPositionBuffer;
        private uint[] fractionUVBuffer;

        uint[] matrixVertexArrayObject;
        uint[] fractionVertexArrayObject;


        private GlmNet.mat4 projectionMatrix;
        private GlmNet.mat4 viewMatrix;
        private mat4 modelMatrix;
        private ShaderProgram shaderProgram;

        public DynamicUnstructureGrid(OpenGL gl, IScientificCamera camera)
            : base(gl, camera)
        {

        }

        //TODO: param type not defined yet
        public void Init(DynamicUnstructureGeometry geometry)
        {
            base.Init(geometry);

            this.FractionType =
                (geometry.FracturePositions.Shape == FracturePositionBufferData.SHAPE_LINE) ? FractionType.Lines : FractionType.Triangles;
            this.MatrixType =
                (geometry.MatrixPositions.Shape == MatrixPositionBufferData.SHAPE_TRIANGLE) ? MatrixType.Triangles : MatrixType.Tetrahedron;

            this.positionBuffer = new uint[1];
            this.positionBuffer[0] = CreateVertexBufferObject(OpenGL.GL_ARRAY_BUFFER, geometry.MatrixPositions, OpenGL.GL_STATIC_DRAW);
            if (geometry.MatrixIndices != null)
            {
                this.matrixIndexBuffer = new uint[1];
                this.matrixIndexBuffer[0] = CreateVertexBufferObject(OpenGL.GL_ARRAY_BUFFER, geometry.MatrixIndices, OpenGL.GL_STATIC_DRAW);

                this.MatrixVertexOrIndexCount = geometry.MatrixIndices.SizeInBytes / sizeof(uint);
            }
            else
            {
                unsafe
                {
                    //int elementSize = (geometry.MatrixPositions.Shape == MatrixPositionBufferData.SHAPE_TRIANGLE) ?
                    //    sizeof(TrianglePositions) : sizeof(TetrahedronPositions);
                    int elementSize = sizeof(Vertex);

                    this.MatrixVertexOrIndexCount = geometry.Positions.SizeInBytes / elementSize;
                }
            }

            this.fractionPositionBuffer = new uint[1];
            this.fractionPositionBuffer[0] = CreateVertexBufferObject(OpenGL.GL_ARRAY_BUFFER, geometry.FracturePositions, OpenGL.GL_STATIC_DRAW);

            unsafe
            {
                //int elementSize = (geometry.FracturePositions.Shape == FracturePositionBufferData.SHAPE_LINE) ?
                //    sizeof(LinePositions) : sizeof(TrianglePositions);
                int elementSize = sizeof(Vertex);

                this.FractionVertexCount = geometry.FracturePositions.SizeInBytes / elementSize;
            }
        }

        /// <summary>
        /// 设置基质的纹理坐标（用于上色）
        /// </summary>
        public void SetMatrixTextureCoords(BufferData matrixTextureCoordsBufferData)
        {
            base.SetTextureCoods(matrixTextureCoordsBufferData);
        }

        public void SetFractionTextureCoords(BufferData textureCoords)
        {
            ////TODO:如果用此方式，则必须先将此对象加入scene树，然后再调用Init
            //OpenGL gl = this.TraverseToRootElement().ParentScene.OpenGL;
            if (this.fractionUVBuffer == null)
            {
                this.fractionUVBuffer = new uint[1];
                this.fractionUVBuffer[0] = CreateVertexBufferObject(OpenGL.GL_ARRAY_BUFFER, textureCoords, OpenGL.GL_STREAM_DRAW);
            }
            else
            {
                UpdateFractionTextureCoords(textureCoords);
            }
        }

        protected void UpdateFractionTextureCoords(BufferData textureCoords)
        {
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, this.fractionUVBuffer[0]);
            IntPtr destVisibles = gl.MapBuffer(OpenGL.GL_ARRAY_BUFFER, OpenGL.GL_READ_WRITE);
            MemoryHelper.CopyMemory(destVisibles, textureCoords.Data, (uint)textureCoords.SizeInBytes);
            gl.UnmapBuffer(OpenGL.GL_ARRAY_BUFFER);
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

            modelMatrix = mat4.identity();
            ShaderProgram shaderProgram = this.shaderProgram;
            //  Bind the shader, set the matrices.
            shaderProgram.Bind(gl);
            shaderProgram.SetUniformMatrix4(gl, "projectionMatrix", projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "viewMatrix", viewMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "modelMatrix", modelMatrix.to_array());

            //gl.Enable(OpenGL.GL_POLYGON_SMOOTH);
            //gl.Hint(OpenGL.GL_POLYGON_SMOOTH_HINT, OpenGL.GL_NICEST);

            this.texture.Bind(gl);
            shaderProgram.SetUniform1(gl, "tex", this.texture.TextureName);
            shaderProgram.SetUniform1(gl, "brightness", this.Brightness);

        }

        #region IRenderable 成员

        void IRenderable.Render(OpenGL gl, RenderMode renderMode)
        {
            if (this.shaderProgram == null)
            {
                this.shaderProgram = InitShader(gl, renderMode);
            }
            if (this.matrixVertexArrayObject == null)
            {
                CreateVertexArrayObject(gl, renderMode);
            }
            if (this.fractionVertexArrayObject == null)
            {
                CreateFractionVertexArrayObject(gl, renderMode);
            }

            BeforeRendering(gl, renderMode);

            DoRenderMatrix(gl, renderMode);

            DoRenderFraction(gl, renderMode);

            AfterRendering(gl, renderMode);
        }

        private void DoRenderFraction(OpenGL gl, RenderMode renderMode)
        {
            if (this.fractionPositionBuffer == null || this.fractionUVBuffer == null) { return; }

            if (this.RenderFraction && this.fractionVertexArrayObject != null)
            {
                shaderProgram.SetUniform1(gl, "renderingWireframe", 0.0f);

                gl.Enable(OpenGL.GL_POLYGON_OFFSET_FILL);
                gl.PolygonOffset(1.0f, 1.0f);

                gl.BindVertexArray(fractionVertexArrayObject[0]);

                switch (this.FractionType)
                {
                    case FractionType.Lines:
                        float[] originalWidth = new float[1];
                        gl.GetFloat(SharpGL.Enumerations.GetTarget.LineWidth, originalWidth);

                        gl.LineWidth(this.FractionLineWidth);
                        gl.DrawArrays(OpenGL.GL_LINES, 0, this.FractionVertexCount);

                        gl.LineWidth(originalWidth[0]);
                        break;
                    case FractionType.Triangles:
                        gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, this.FractionVertexCount);
                        break;
                    default:
                        throw new NotImplementedException();
                    //break;
                }

                gl.BindVertexArray(0);

                gl.Disable(OpenGL.GL_POLYGON_OFFSET_FILL);
            }

            if (this.renderFractionWireframe && this.fractionVertexArrayObject != null)
            {
                shaderProgram.SetUniform1(gl, "renderingWireframe", 1.0f);

                gl.BindVertexArray(fractionVertexArrayObject[0]);
                {
                    gl.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Lines);

                    switch (this.FractionType)
                    {
                        case FractionType.Lines:
                            gl.DrawArrays(OpenGL.GL_LINES, 0, this.FractionVertexCount);
                            break;
                        case FractionType.Triangles:
                            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, this.FractionVertexCount);
                            break;
                        default:
                            break;
                    }

                    gl.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Filled);
                }
                gl.BindVertexArray(0);
            }
        }

        /// <summary>
        /// 渲染基质
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="renderMode"></param>
        private void DoRenderMatrix(OpenGL gl, RenderMode renderMode)
        {
            if (this.positionBuffer == null || this.colorBuffer == null) { return; }

            if (this.RenderGrid && this.matrixVertexArrayObject != null)
            {
                shaderProgram.SetUniform1(gl, "renderingWireframe", 0.0f);

                gl.Enable(OpenGL.GL_POLYGON_OFFSET_FILL);
                gl.PolygonOffset(1.0f, 1.0f);

                gl.BindVertexArray(matrixVertexArrayObject[0]);

                switch (this.MatrixType)
                {
                    case MatrixType.Triangles:
                        gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, this.MatrixVertexOrIndexCount);
                        break;
                    case MatrixType.Tetrahedron:
                        gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, this.matrixIndexBuffer[0]);
                        gl.DrawElements(OpenGL.GL_TRIANGLES, this.MatrixVertexOrIndexCount, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                        //    gl.Enable(OpenGL.GL_PRIMITIVE_RESTART);
                        //gl.PrimitiveRestartIndex(uint.MaxValue);
                        //gl.DrawElements(OpenGL.GL_TRIANGLE_STRIP, this.tetrasIndexBufferObjectCount, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                        ////gl.DrawElements(OpenGL.GL_TRIANGLE_STRIP, this.CurrentTetrasIndexCount, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                        ////gl.DrawArrays(OpenGL.GL_TRIANGLE_STRIP, 0, this.tetrasIndexBufferObjectCount);
                        //gl.Disable(OpenGL.GL_PRIMITIVE_RESTART);
                        break;
                    default:
                        break;
                }

                gl.BindVertexArray(0);

                gl.Disable(OpenGL.GL_POLYGON_OFFSET_FILL);
            }

            if (this.RenderGridWireframe && this.matrixVertexArrayObject != null)
            {
                shaderProgram.SetUniform1(gl, "renderingWireframe", 1.0f);

                gl.BindVertexArray(matrixVertexArrayObject[0]);
                {
                    gl.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Lines);

                    switch (this.MatrixType)
                    {
                        case MatrixType.Triangles:
                            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, this.MatrixVertexOrIndexCount);
                            break;
                        case MatrixType.Tetrahedron:
                            gl.DrawElements(OpenGL.GL_TRIANGLES, this.MatrixVertexOrIndexCount, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                            break;
                        default:
                            break;
                    }

                    gl.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Filled);
                }
                gl.BindVertexArray(0);
            }
        }

        private void CreateFractionVertexArrayObject(OpenGL gl, RenderMode renderMode)
        {
            if (this.fractionPositionBuffer == null || this.fractionUVBuffer == null) { return; }

            this.fractionVertexArrayObject = new uint[1];
            gl.GenVertexArrays(1, this.fractionVertexArrayObject);
            gl.BindVertexArray(this.fractionVertexArrayObject[0]);
            // prepare positions
            {
                int location = shaderProgram.GetAttributeLocation(gl, in_Position);
                ATTRIB_INDEX_POSITION = (uint)location;
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, this.fractionPositionBuffer[0]);
                gl.VertexAttribPointer(ATTRIB_INDEX_POSITION, 3, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.EnableVertexAttribArray(ATTRIB_INDEX_POSITION);
            }
            // prepare colors
            {
                int location = shaderProgram.GetAttributeLocation(gl, in_uv);
                ATTRIB_INDEX_UV = (uint)location;
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, this.fractionUVBuffer[0]);
                gl.VertexAttribPointer(ATTRIB_INDEX_UV, 1, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.EnableVertexAttribArray(ATTRIB_INDEX_UV);
            }

            gl.BindVertexArray(0);
        }

        private void CreateVertexArrayObject(OpenGL gl, RenderMode renderMode)
        {
            if (this.positionBuffer == null || this.colorBuffer == null) { return; }

            this.matrixVertexArrayObject = new uint[1];
            gl.GenVertexArrays(1, this.matrixVertexArrayObject);
            gl.BindVertexArray(this.matrixVertexArrayObject[0]);
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
                ATTRIB_INDEX_UV = (uint)location;
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, colorBuffer[0]);
                gl.VertexAttribPointer(ATTRIB_INDEX_UV, 1, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.EnableVertexAttribArray(ATTRIB_INDEX_UV);
            }

            gl.BindVertexArray(0);
        }

        private ShaderProgram InitShader(OpenGL gl, RenderMode renderMode)
        {
            String vertexShaderSource = ManifestResourceLoader.LoadTextFile(@"DynamicUnstructureGrid.vert");
            String fragmentShaderSource = ManifestResourceLoader.LoadTextFile(@"DynamicUnstructureGrid.frag");

            var shaderProgram = new ShaderProgram();
            shaderProgram.Create(gl, vertexShaderSource, fragmentShaderSource, null);

            {
                int location = shaderProgram.GetAttributeLocation(gl, in_Position);
                if (location < 0) { throw new ArgumentException(); }
                this.ATTRIB_INDEX_POSITION = (uint)location;
            }
            {
                int location = shaderProgram.GetAttributeLocation(gl, in_uv);
                if (location < 0) { throw new ArgumentException(); }
                this.ATTRIB_INDEX_POSITION = (uint)location;
            }

            shaderProgram.AssertValid(gl);

            return shaderProgram;
        }

        #endregion

        protected void AfterRendering(OpenGL gl, RenderMode renderMode)
        {
            //gl.Disable(OpenGL.GL_POLYGON_SMOOTH);

            shaderProgram.Unbind(gl);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
        }

        #endregion IRenderable
    }

    public enum FractionType
    {
        Lines,
        Triangles,
    }

    public enum MatrixType
    {
        Triangles,
        Tetrahedron,
    }
}
