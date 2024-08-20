using System;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using jackz_builder.Client.ExtensionMethods;
using static CitizenFX.Core.Native.API;

namespace jackz_builder.Client
{
    public class Color
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }
    
        public static readonly Color White = new Color(255, 255, 255, 255);
        public static readonly Color Black = new Color(0, 0, 0, 255);
        public static readonly Color Red = new Color(255, 0, 0, 255);
        public static readonly Color Green = new Color(0, 255, 0, 255);
        public static readonly Color Blue = new Color(0, 0, 255, 255);
        public static readonly Color Aqua = new Color(5, 195, 221, 255);
        public static readonly Color Orange = new Color(255, 163, 0, 255);
    
        public Color(int r, int g, int b, int a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    namespace ExtensionMethods
    {
        public static class Vector3Ext
        {
            public static Vector3 Clone(this Vector3 vec)
            {
                return new Vector3(vec.X, vec.Y, vec.Z);
            }
            public static Vector3 Offset(this Vector3 vec, Vector3 rotation, float leftRight, float forwardBackward, float upDown)
            {
                // Convert the rotation angles from degrees to radians
                var radX = Util.Deg2Rad * rotation.X;
                var radY = Util.Deg2Rad * rotation.Y;
                var radZ = Util.Deg2Rad * rotation.Z;

                // Calculate the sine and cosine of the rotation angles
                var sinX = Math.Sin(radX);
                var cosX = Math.Cos(radX);
                var sinY = Math.Sin(radY);
                var cosY = Math.Cos(radY);
                var sinZ = Math.Sin(radZ);
                var cosZ = Math.Cos(radZ);

                // Apply the rotations to the offset values
                var newX = cosY * (sinZ * upDown + cosZ * forwardBackward) - sinY * leftRight;
                var newY = sinX * (cosY * leftRight + sinY * (sinZ * upDown + cosZ * forwardBackward)) + cosX * (cosZ * upDown - sinZ * forwardBackward);
                var newZ = cosX * (cosY * leftRight + sinY * (sinZ * upDown + cosZ * forwardBackward)) - sinX * (cosZ * upDown - sinZ * forwardBackward);

                // Create a new vector with the computed x, y, and z values
                return new Vector3((float)newX, (float)newY, (float)newZ) + vec;
            }
        }
    }

    public class Util
    {
        public static Vector3 Midpoint(Vector3 a, Vector3 b)
        {
            return new Vector3(
                (a.X + b.X) / 2f,
                (a.Y + b.Y) / 2f,
                (a.Z + b.Z) / 2f
            );
        }
        
        public static float DistanceFrom(Entity entity, Vector3 position, bool squareResult = true)
        {
            if (squareResult)
            {
                return API.Vdist(position.X,
                    position.Y, position.Z, entity.Position.X, entity.Position.Y, entity.Position.Z);
            }
            else
            {
                return Vdist2(position.X,
                    position.Y, position.Z, entity.Position.X, entity.Position.Y, entity.Position.Z);
            }
        }
        
        public static float GameSpeedToMPH(float speed)
        {
            return speed * 2.237f;
        }

        public static float GameSpeedToKPH(float speed)
        {
            return speed * 3.6f;
        }

        public static float MPHToGameSpeed(float speed)
        {
            return speed / 2.237f;
        }

        public static float KPHToGameSpeed(float speed)
        {
            return speed / 3.6f;
        }
        
        public static float Vdist(Vector3 from, Vector3 to, bool squareRoot = false)
        {
            return squareRoot
                ? API.Vdist(from.X, from.Y, from.Z, to.X, to.Y, to.Z)
                : API.Vdist2(from.X, from.Y, from.Z, to.X, to.Y, to.Z);
        }
        
        public static void DrawLine(Vector3 from, Vector3 to, Color color)
        {
            API.DrawLine(from.X, from.Y, from.Z, to.X, to.Y,
                to.Z, color.R, color.G, color.B, color.A); 
        }
        

        public static Vector2 GetScreenCoords(Vector3 worldPos)
        {
            Vector2 outVec = Vector2.Zero;
            API.GetScreenCoordFromWorldCoord(worldPos.X, worldPos.Y, worldPos.Z, ref outVec.X, ref outVec.Y);
            return outVec;
        }

        public static void DrawSphere(Vector3 pos, float radius, Color color)
        {
            API.DrawMarker((int) MarkerType.DebugSphere, pos.X, pos.Y, pos.Z, 0f, 0,0f, 0f, 0f, 0f, radius, radius, radius, color.R, color.G, color.B, 255, false, false, 2, false, null, null, false);
            // API.DrawSphere(pos.X, pos.Y, pos.Z, radius, color.R, color.G, color.B, color.A / 255f);
        }

        public static void DrawSphere(Vector3 pos, float radius = 0.1f)
        {
            DrawSphere(pos, radius, Color.White);
        }

        public static void DrawBox(Vector3 cornerA, Vector3 cornerB, Color color)
        {
            API.DrawBox(cornerA.X, cornerA.Y, cornerA.Z, cornerB.X, cornerB.Y, cornerB.Z, color.R, color.G, color.B, color.A);
        }

        public const double Deg2Rad = Math.PI / 180d;

        public static void DrawBoundingBoxFromCenter(Vector3 center, Vector3 size, Vector3 rotation, Color color)
        {
            double[,] points =
            {
                { -size.X, -size.Y, size.Z },
                { size.X, -size.Y, size.Z },
                { size.X, size.Y, size.Z },
                { -size.X, size.Y, size.Z },
                
                { -size.X, size.Y, -size.Z },
                { size.X, size.Y, -size.Z },
                { size.X, -size.Y, -size.Z },
                { -size.X, -size.Y, -size.Z },
            };

            List<Vector3> vectors = new List<Vector3>(8);

            Vector3? prevVec = null;
            // DrawText(new Vector2(0.0f, 0.1f), $"{center.X:F2} {center.Y:F2} {center.Z:F2} - Center", 0.4f, Color.Orange);
            // DrawText(new Vector2(0.0f, 0.12f), $"{rotation.X:F2} {rotation.Y:F2} {rotation.Z:F2} - Rot", 0.4f, Color.Orange);
            var rotX = rotation.X * Deg2Rad;
            var rotY = rotation.Y * Deg2Rad;
            var rotZ = rotation.Z * Deg2Rad;
            for (var i = 0; i < 8; i++)
            {
                var x = points[i, 0];
                var y = points[i, 1];
                var z = points[i, 2];
                points[i, 0] = x * Math.Cos(rotY) * Math.Cos(rotZ)
                               + y * (Math.Cos(rotZ) * Math.Sin(rotX) * Math.Sin(rotY) - Math.Cos(rotX) * Math.Sin(rotZ))
                               + z * (Math.Cos(rotX) * Math.Cos(rotZ) * Math.Sin(rotY) + Math.Sin(rotX) * Math.Sin(rotZ));

                points[i, 1] = x * Math.Cos(rotY) * Math.Sin(rotZ)
                               + z * (-Math.Cos(rotZ) * Math.Sin(rotX) + Math.Cos(rotX) * Math.Sin(rotY) * Math.Sin(rotZ))
                               + y * (Math.Cos(rotX) * Math.Cos(rotZ) + Math.Sin(rotX) * Math.Sin(rotY) * Math.Sin(rotZ));

                points[i, 2] = z * Math.Cos(rotX) * Math.Cos(rotY)
                               + y * Math.Cos(rotY) * Math.Sin(rotX)
                               - x * Math.Sin(rotY);
                // Align with center pos
                points[i, 0] += center.X;
                points[i, 1] += center.Y;
                points[i, 2] += center.Z;
                var vec = new Vector3((float)points[i, 0], (float)points[i, 1], (float)points[i, 2]);
                vectors.Add(vec);
                
                // DrawText(new Vector2(0.0f, 0.14f + 0.02f * i), $"{vec.X:F2} {vec.Y:F2} {vec.Z:F2}", 0.4f, Color.White);
            }

            for (int i = 0; i < 4; i++)
            {
                // Form line for upper
                DrawLine(vectors[i], vectors[i + 1], color);
                // Connect upper to lower
                DrawLine(vectors[i], vectors[7 - i], color);
                DrawLine(vectors[i+3], vectors[i+4], color);
            }
            DrawLine(vectors[0], vectors[3], color);
            DrawLine(vectors[7], vectors[4], color);
        }
        
        public static void DrawBoxFromCenter(Vector3 center, Vector3 size, Color color)
        {
            Vector3 cornerA = new Vector3(center.X - size.X, center.Y - size.Y, center.Z - size.Z);
            Vector3 cornerB = new Vector3(center.X + size.X, center.Y + size.Y, center.Z + size.Z);
            API.DrawBox(cornerA.X, cornerA.Y, cornerA.Z, cornerB.X, cornerB.Y, cornerB.Z, color.R, color.G, color.B, color.A);
        }

        public static void DrawRect(Vector2 coords, Vector2 size, Color color)
        {
            API.DrawRect(coords.X, coords.Y, size.X, size.Y, color.R, color.B, color.G, color.A);
        }

        /// <summary>
        /// Draws text on the UI
        /// </summary>
        /// <param name="pos">The X and Y position to render at. 0, 0 is top left</param>
        /// <param name="content">The text content</param>
        /// <param name="scale">The font scale of the text</param>
        /// <param name="color">The color of the text</param>
        public static void DrawText(Vector2 pos, string content, float scale, Color color)
        {
            // var screenPos = GetScreenCoords(worldPos);
            ClearDrawOrigin();
            AddText(pos, content, scale, color);
        }

        /// <summary>
        /// Draws text on screen, ontop of the current draw origin
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="content"></param>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        public static void AddText(Vector2 pos, string content, float scale, Color color)
        {
            SetTextScale(0.0f, scale);
            SetTextDropshadow(1, 0, 0, 0,0);
            if (color == null) color = Color.White;
            SetTextColour(color.R, color.G, color.B, color.A);
            SetTextEntry("STRING");
            AddTextComponentString(content);
            API.DrawText(pos.X, pos.Y);
        }

        /// <summary>
        /// Draws text using SetDrawOrigin() to be 3d at a world coordinate. Only 32 are allowed per frame, any more and some elements may flicker
        /// </summary>
        /// <param name="worldPos">The position in the world</param>
        /// <param name="content">The text content</param>
        /// <param name="scale">The font scale of the text</param>
        /// <param name="color">The text color</param>
        /// <param name="offset">Offset of the UI position when drawing</param>
        public static void DrawText3D(Vector3 worldPos, string content, float scale, Color color, Vector2 offset)
        {
            SetDrawOrigin(worldPos.X, worldPos.Y, worldPos.Z, 0);
            AddText(offset, content, scale, color);
            API.ClearDrawOrigin();
        }
        
        public static void DrawText3D(Vector3 worldPos, IEnumerable<string> contents, float scale, Color color, Vector2 offset)
        {
            SetDrawOrigin(worldPos.X, worldPos.Y, worldPos.Z, 0);
            foreach (var line in contents)
            {
                AddText(offset, line, scale, color);
                offset.Y += scale;
            }
            API.ClearDrawOrigin();
        }

        public static void DrawText3D(Vector3 worldPos, string content, float scale, Color color = null)
        {
            DrawText3D(worldPos, content, scale, color, Vector2.Zero);
        }

        public static void DrawText3D(Entity entity, string content, float scale, Color color, Vector2 offset)
        {
            DrawText3D(entity.Position, content, scale, color, offset);
        }

        public static void DrawText3D(Entity entity, string content, float scale, Color color = null)
        {
            DrawText3D(entity.Position, content, scale, color, Vector2.Zero);
        }

        /// <summary>
        /// Draws 3D text similar to <see cref="DrawText3D" /> but without the limitation of the amount of origins. Will shift around as it's slower than SetDrawOrigin
        /// </summary>
        /// <param name="worldPos"></param>
        /// <param name="content"></param>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        /// <param name="offset"></param>
        public static void DrawText3D2(Vector3 worldPos, string content, float scale, Color color, Vector2 offset)
        {
            var screenPos = GetScreenCoords(worldPos) + offset;
            ClearDrawOrigin();
            AddText(screenPos, content, scale, color);
        }

        public static void DrawText3D2(Vector3 worldPos, string content, float scale, Color color = null)
        {
            DrawText3D2(worldPos, content, scale, color, Vector2.Zero);
        }

        public static void DrawText3D2(Entity entity, string content, float scale, Color color, Vector2 offset)
        {
            DrawText3D2(entity.Position, content, scale, color, offset);
        }

        public static void DrawText3D2(Entity entity, string content, float scale, Color color = null)
        {
            DrawText3D2(entity.Position, content, scale, color, Vector2.Zero);
        }

        /// <summary>
        /// Draws a marker, note that the Y position will be offset by +2f 
        /// </summary>
        public static void DrawMarker(int type, Vector3 pos, Vector3 dir, Vector3 rot, Vector3 scale, Color color, bool bobUpAndDown, bool faceCamera, bool rotate, bool drawOnEnts, string textureDict, string textureName)
        {
            API.DrawMarker(type, pos.X, pos.Y,
                pos.Z + 2f, dir.X, dir.Y, dir.Z, rot.X, rot.Y, rot.Z, scale.X, scale.Y, scale.Z, color.R, color.G, color.B, color.A, bobUpAndDown, faceCamera, 2, rotate, textureDict,
                textureName,
                drawOnEnts);
        }

        public static void DrawMarker(int type, Vector3 pos, Color color, bool bobUpAndDown = false, bool faceCamera = false, bool rotate = false, bool drawOnEnts = false)
        {
            DrawMarker(type, pos, Vector3.Zero, Vector3.Zero, Vector3.One, color, bobUpAndDown, faceCamera, rotate, drawOnEnts, null, null);
        }

        public static void DrawConeMarker(Entity entity, Color color)
        {
            Vector3 dimensions = entity.Model.GetDimensions();
            Vector3 pos = new Vector3(entity.Position.X, entity.Position.Y, entity.Position.Z + (dimensions.Z/2));
            DrawMarker(0, pos, Vector3.Zero, Vector3.Zero, Vector3.One, color, false, true, true, true, null, null);
        }

        public static void DrawEntityBox(Entity entity, Color color)
        {
            Vector3 dim = entity.Model.GetDimensions();
            Vector3 pos = entity.Position.Clone();
            pos.Z += dim.Z / 2;
            DrawBoundingBoxFromCenter(pos, new Vector3(dim.X/2, dim.Y/2, dim.Z/2), entity.Rotation, color);
        }
    }
}