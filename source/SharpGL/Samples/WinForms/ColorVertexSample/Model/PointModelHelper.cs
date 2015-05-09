﻿using SharpGL.SceneComponent;
using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorVertexSample.Model
{
    class PointModelHelper
    {
        internal static void Build(PointModel model, int nx, int ny, int nz, float radius, float minValue, float maxValue)
        {
            Random positionRandom = new Random();
            Random colorRandom = new Random();

            Vertex min = new Vertex(), max = new Vertex();
            bool isInit = false;

            unsafe
            {
                for (long i = 0; i < model.PointCount; i++)
                {
                    float x = minValue + ((float)positionRandom.NextDouble()) * (maxValue - minValue);
                    float y = minValue + ((float)positionRandom.NextDouble()) * (maxValue - minValue);
                    float z = minValue + ((float)positionRandom.NextDouble()) * (maxValue - minValue);
                    if (!isInit)
                    {
                        min = new Vertex(x, y, z);
                        max = new Vertex(x, y, z);
                        isInit = true;
                    }
                    if (x < min.X) min.X = x;
                    if (x > max.X) max.X = x;
                    if (y < min.Y) min.Y = y;
                    if (y > max.Y) max.Y = y;
                    if (z < min.Z) min.Z = z;
                    if (z > max.Z) max.Z = z;

                    Vertex* positions = model.Positions;
                    positions[i].X = x;
                    positions[i].Y = y;
                    positions[i].Z = z;

                    ByteColor* colors = model.Colors;
                    colors[i].red = (byte)colorRandom.Next(0, 256);
                    colors[i].green = (byte)colorRandom.Next(0, 256);
                    colors[i].blue = (byte)colorRandom.Next(0, 256);

                }

                //model.Translate = (max + min) / 2;

                //for (long i = 0; i < model.PointCount; i++)
                //{
                //    Vertex* centers = model.Positions;
                //    centers[i].X -= model.Translate.X;
                //    centers[i].Y -= model.Translate.Y;
                //    centers[i].Z -= model.Translate.Z;
                //}

                //Vertex location = min - model.Translate;
                //Size3D size = max - min;
                //Rect3D rect = new Rect3D(location, size);

                //model.Bounds = rect;
                //model.BoundingBox = new BoundingBox() { MinPosition = min, MaxPosition = max };
                model.BoundingBox.MaxPosition = max;
                model.BoundingBox.MinPosition = min;
            }
        }
    }
}
