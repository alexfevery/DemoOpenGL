using OpenTK;
using OpenTK.Input;
using System;
using System.Drawing;

namespace ProjectCobalt
{
    static class Controls
    {
        public static Vector2 lastMousePos = new Vector2();
        static Vector2 LastMouseDelta = new Vector2();
        public static bool iskeydown = false;
        public static void GetControls()
        {
            MouseState m1 = Mouse.GetState();
            KeyboardState k1 = Keyboard.GetState();
            LastMouseDelta = new Vector2(m1.X, m1.Y)-lastMousePos;
            if (LastMouseDelta.X != 0) { Camera.AddRotationX(LastMouseDelta.X); }
            if (LastMouseDelta.Y != 0) { Camera.AddRotationY(LastMouseDelta.Y); }
            ResetCursor();
            if (k1.IsKeyDown(Key.Escape)) { Program.gamewindow.Exit(); }
            if (k1.IsKeyDown(Key.W)) { Camera.Move(0, 0.1f, 0); }
            if (k1.IsKeyDown(Key.A)) { Camera.Move(0.1f, 0, 0); }
            if (k1.IsKeyDown(Key.S)) { Camera.Move(0, -0.1f, 0); }
            if (k1.IsKeyDown(Key.D)) { Camera.Move(-0.1f, 0, 0); }
            if (k1.IsKeyDown(Key.ControlLeft)) { Camera.Move(0, 0, +.1f); }
            if (k1.IsKeyDown(Key.ShiftLeft)) { Camera.Move(0, 0, -.1f); }
            if (k1.IsKeyDown(Key.Q)) { Camera.AddRotationZ(-.05f); }
            if (k1.IsKeyDown(Key.E)) { Camera.AddRotationZ(+.05f); }
            if (k1.IsKeyDown(Key.Left)) { Camera.AddRotationX(-3); }
            if (k1.IsKeyDown(Key.Right)) { Camera.AddRotationX(3); }
            if (k1.IsKeyDown(Key.Up)) { Camera.AddRotationY(-3); }
            if (k1.IsKeyDown(Key.Down)) { Camera.AddRotationY(3); }
            if (k1.IsKeyDown(Key.R))
            {
                iskeydown = true;
                Camera.Orientation = Quaternion.Identity;
                Camera.OrientationE = new Vector3();
            }
            else { iskeydown = false; }
        }

        public static void ResetCursor()
        {
            if (Program.gamewindow.Focused)
            {
                Mouse.SetPosition(Program.WindowCenter.X, Program.WindowCenter.Y);
                lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }
        }
    }
}
