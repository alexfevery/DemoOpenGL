using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using OpenTK.Input;

namespace ProjectCobalt
{
    public static class Camera
    {
        public static Matrix4 View;
        public static Matrix4 lookat;
        public static Vector3 Position = new Vector3(0,0, -.01f);
        public static Quaternion Orientation = Quaternion.Identity;
        public static Vector3 OrientationE = new Vector3();
        public static float MoveSpeed = 0.0001f;
        public static float MouseSensitivity = 0.1f;

        public static Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Vector3.Transform(Vector3.UnitZ, Orientation), Vector3.Transform(Vector3.UnitY,Orientation)) * View;
        }
       
        public static void Move(float x, float y, float z)
        {
            Vector3 offset = new Vector3();
            offset += x * Vector3.Transform(Vector3.UnitX, Orientation);
            offset += y * Vector3.Transform(Vector3.UnitZ, Orientation);
            offset += z * Vector3.Transform(-Vector3.UnitY, Orientation);
            offset.NormalizeFast();
            if (Mouse.GetState().IsButtonDown(MouseButton.Left)) {Position += Vector3.Multiply(offset, MoveSpeed*50); }
            else { Position += Vector3.Multiply(offset, MoveSpeed); }
        }

        public static void AddRotationX(float x)
        {
            x *= MouseSensitivity;
            OrientationE.X += -x;
            if(OrientationE.X >= 360) { OrientationE.X = 0; }
            if(OrientationE.X < 0) { OrientationE.X = 360; }
            Orientation *= Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(-x));
        }
        public static void AddRotationY(float y)
        {
            y *= MouseSensitivity;
            OrientationE.Y += y;
            if (OrientationE.Y >= 360) { OrientationE.Y = 0; }
            if (OrientationE.Y < 0) { OrientationE.Y = 360; }
            Orientation *= Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(y));
        }
        public static void AddRotationZ(float z)
        {
            z *= 20;
            OrientationE.Z += z;
            if (OrientationE.Z >= 360) { OrientationE.Z = 0; }
            if (OrientationE.Z < 0) { OrientationE.Z = 360; }
            Orientation *= Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(z));
        }


    }
}
