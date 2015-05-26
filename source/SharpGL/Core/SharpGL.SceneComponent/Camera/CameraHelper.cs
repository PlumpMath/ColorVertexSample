﻿using GlmNet;
using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGL.SceneComponent
{
    public static class CameraHelper
    {
        /// <summary>
        /// Adjusts camera according to bounding box.
        /// <para>Use this when bounding box's size or positon is changed.</para>
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="boundingBox"></param>
        /// <param name="openGL"></param>
        public static void AdjustCamera(this ScientificCamera camera, IBoundingBox boundingBox, OpenGL openGL)
        {
            float sizeX, sizeY, sizeZ;
            boundingBox.GetBoundDimensions(out sizeX, out sizeY, out sizeZ);
            float size = Math.Max(Math.Max(sizeX, sizeY), sizeZ);

            float centerX, centerY, centerZ;
            boundingBox.GetCenter(out centerX, out centerY, out centerZ);
            Vertex target = new Vertex(centerX, centerY, centerZ);

            Vertex target2Position = camera.Position - camera.Target;
            target2Position.Normalize();

            Vertex position = target + target2Position * (size * 2 + 1);
            //new Vertex(0.0f, 0.0f, 1.0f) * (size * 2);

            int[] viewport = new int[4];
            openGL.GetInteger(SharpGL.Enumerations.GetTarget.Viewport, viewport);
            int width = viewport[2]; int height = viewport[3];

            IPerspectiveCamera perspectiveCamera = camera;
            perspectiveCamera.FieldOfView = 60;
            perspectiveCamera.AspectRatio = (double)width / (double)height;
            perspectiveCamera.Near = 0.01;
            perspectiveCamera.Far = size * 3 + 1;// double.MaxValue;

            IOrthoCamera orthoCamera = camera;
            if (width > height)
            {
                orthoCamera.Left = -size * width / height;
                orthoCamera.Right = size * width / height;
                orthoCamera.Bottom = -size;
                orthoCamera.Top = size;
            }
            else
            {
                orthoCamera.Left = -size;
                orthoCamera.Right = size;
                orthoCamera.Bottom = -size * height / width;
                orthoCamera.Top = size * height / width;
            }
            orthoCamera.Near = 0;// 0.001;
            //orthoCamera.Far = double.MaxValue;
            orthoCamera.Far = size * 3 + 1;// double.MaxValue;

            camera.Position = position;
            camera.Target = target;
            //camera.UpVector = new Vertex(0f, 1f, 0f);
        }

        /// <summary>
        /// Apply specifed viewType to camera according to bounding box's size and position.
        /// <para>    +-------+    </para>
        /// <para>   /|      /|    </para>
        /// <para>  +-------+ |    </para>
        /// <para>  | |     | |    </para>
        /// <para>  | O-----|-+---X</para>
        /// <para>  |/      |/     </para>
        /// <para>  +-------+      </para>
        /// <para> /  |            </para>
        /// <para>Y   Z            </para>
        /// <para>其边长为(2 * Math.Sqrt(3)), 所在的坐标系如下</para>
        /// <para>   O---X</para>
        /// <para>  /|    </para>
        /// <para> Y |    </para>
        /// <para>   Z    </para>
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="boundingBox"></param>
        /// <param name="openGL"></param>
        /// <param name="viewType"></param>
        public static void ApplyViewType(this ScientificCamera camera, IBoundingBox boundingBox,
            OpenGL openGL, EViewType viewType)
        {
            float sizeX, sizeY, sizeZ;
            boundingBox.GetBoundDimensions(out sizeX, out sizeY, out sizeZ);
            float size = Math.Max(Math.Max(sizeX, sizeY), sizeZ);

            float centerX, centerY, centerZ;
            boundingBox.GetCenter(out centerX, out centerY, out centerZ);
            Vertex target = new Vertex(centerX, centerY, centerZ);

            Vertex target2Position;
            Vertex upVector;
            switch (viewType)
            {
                case EViewType.UserView:
                    //UserView 定义为从顶视图开始，绕X 轴旋转30 度，在绕Z 轴45 度，并且能看到整个模型的虚拟模型空间。
                    target2Position = new Vertex((float)Math.Sqrt(3), (float)Math.Sqrt(3), -1);
                    target2Position.Normalize();
                    upVector = new Vertex(0, 0, -1);
                    break;
                case EViewType.Top:
                    target2Position = new Vertex(0, 0, -1);
                    upVector = new Vertex(0, -1, 0);
                    break;
                case EViewType.Bottom:
                    target2Position = new Vertex(0, 0, 1);
                    upVector = new Vertex(0, -1, 0);
                    break;
                case EViewType.Left:
                    target2Position = new Vertex(-1, 0, 0);
                    upVector = new Vertex(0, 0, -1);
                    break;
                case EViewType.Right:
                    target2Position = new Vertex(1, 0, 0);
                    upVector = new Vertex(0, 0, -1);
                    break;
                case EViewType.Front:
                    target2Position = new Vertex(0, 1, 0);
                    upVector = new Vertex(0, 0, -1);
                    break;
                case EViewType.Back:
                    target2Position = new Vertex(0, -1, 0);
                    upVector = new Vertex(0, 0, -1);
                    break;
                default:
                    throw new NotImplementedException(string.Format("new value({0}) of EViewType is not implemented!", viewType));
                //break;
            }

            Vertex position = target + target2Position * (size * 2 + 1);
            //new Vertex(0.0f, 0.0f, 1.0f) * (size * 2);

            int[] viewport = new int[4];
            openGL.GetInteger(SharpGL.Enumerations.GetTarget.Viewport, viewport);
            int width = viewport[2]; int height = viewport[3];

            IPerspectiveCamera perspectiveCamera = camera;
            perspectiveCamera.FieldOfView = 60;
            perspectiveCamera.AspectRatio = (double)width / (double)height;
            perspectiveCamera.Near = 0.01;
            perspectiveCamera.Far = size * 3 + 1;// double.MaxValue;

            IOrthoCamera orthoCamera = camera;
            if (width > height)
            {
                orthoCamera.Left = -size * width / height;
                orthoCamera.Right = size * width / height;
                orthoCamera.Bottom = -size;
                orthoCamera.Top = size;
            }
            else
            {
                orthoCamera.Left = -size;
                orthoCamera.Right = size;
                orthoCamera.Bottom = -size * height / width;
                orthoCamera.Top = size * height / width;
            }
            orthoCamera.Near = 0.001;
            orthoCamera.Far = size * 3 + 1;// double.MaxValue;

            camera.Position = position;
            camera.Target = target;
            camera.UpVector = upVector;
        }

        /// <summary>
        /// Extension method for <see cref="IPerspectiveCamera"/> to get projection matrix.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static mat4 GetProjectionMat4(this IPerspectiveCamera camera)
        {
            GlmNet.mat4 perspective = GlmNet.glm.perspective(
                (float)(camera.FieldOfView / 360.0 * Math.PI * 2),
                (float)camera.AspectRatio, (float)camera.Near, (float)camera.Far);
            return perspective;
        }

        /// <summary>
        /// Extension method for <see cref="IOrthoCamera"/> to get projection matrix.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static mat4 GetProjectionMat4(this IOrthoCamera camera)
        {
            GlmNet.mat4 ortho = GlmNet.glm.ortho((float)camera.Left, (float)camera.Right,
                (float)camera.Bottom, (float)camera.Top,
                (float)camera.Near, (float)camera.Far);
            return ortho;
        }

        /// <summary>
        /// Extension method for <see cref="IViewCamera"/> to get view matrix.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static GlmNet.mat4 GetViewMat4(this IViewCamera camera)
        {
            GlmNet.vec3 position = ToVec3(camera.Position);
            GlmNet.vec3 target = ToVec3(camera.Target);
            GlmNet.vec3 up = ToVec3(camera.UpVector);
            GlmNet.mat4 lookAt = GlmNet.glm.lookAt(position, target, up);
            return lookAt;
        }

        private static vec3 ToVec3(SharpGL.SceneGraph.Vertex vertex)
        {
            vec3 result = new vec3(vertex.X, vertex.Y, vertex.Z);
            return result;
        }
    }
}