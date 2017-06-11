using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Diagnostics;
using FileFormatWavefront.Model;
using FileFormatWavefront;
using OpenTK.Input;
using System.Threading;
using System.Configuration;

namespace ProjectCobalt
{
    class Program
    {
        public static GameWindow gamewindow = new GameWindow();
        public static Point WindowCenter;
        public static Stopwatch watch = new Stopwatch();
        public static Thread UIThread;
        public static int UntexturedShader;
        public static int TexturedShader;
        public static ErrorCode error;

        static void Main()
        {
            UIThread = Thread.CurrentThread;
            using (gamewindow)
            {
                gamewindow.VSync = VSyncMode.On;
                gamewindow.WindowState = WindowState.Maximized;
                //gamewindow.Height = 1000;
                //gamewindow.Width = 2000;
                gamewindow.Location = new Point((3840 - gamewindow.Width) / 2, (2160 - gamewindow.Height) / 2);
                gamewindow.Load += (sender, e) => { Load(); };
                gamewindow.Resize += (sender, e) => { Resize(); };
                gamewindow.UpdateFrame += (sender, e) => { Update(); };
                gamewindow.RenderFrame += (sender, e) => { Render(); };
                gamewindow.Run();
            }
        }

        public static void Resize()
        {

            GL.Viewport(gamewindow.ClientRectangle.X, gamewindow.ClientRectangle.Y, gamewindow.ClientRectangle.Width, gamewindow.ClientRectangle.Height);
            Camera.View = Matrix4.CreatePerspectiveFieldOfView(1f, (float)gamewindow.Width / (float)gamewindow.Height, 0.1f, 500000);
            WindowCenter = new Point(gamewindow.Bounds.Left + (int)(gamewindow.Bounds.Width / 2.0), gamewindow.Bounds.Top + (int)(gamewindow.Bounds.Height / 2.0));
        }

        public static Content.Mesh testobj;
        public static Content.Mesh testobj2;
        public static Content.Mesh testmap;
        public static void Load()
        {
            UntexturedShader = Shaders.LoadShaders(Shaders.VUntextured, Shaders.FUntextured);
            TexturedShader = Shaders.LoadShaders(Shaders.VTextured, Shaders.FTextured);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.DepthClamp);
            gamewindow.CursorVisible = false;
            GL.ClearColor(Color.Black);

            testobj = Content.Mesh.LoadObj(@"C:\Users\alexf\Google Drive\Computer\Desktop\cube.obj");
            testobj.ShaderProgram = TexturedShader;
            testobj.Texture = Content.Mesh.LoadTexture();
          //  testobj2 = Content.Mesh.LoadObj(@"C:\Users\alexf\Google Drive\Computer\Desktop\cube.obj");
          //  testobj2.ShaderProgram = TexturedShader;
           // testmap = Content.Mesh.LoadHeightMap(@"C:\Users\alexf\Google Drive\Computer\Desktop\Kauai terrain\Kauai Height Map (USGS NED 10m).png");
           // testmap.ShaderProgram = UntexturedShader;
        }

        public static void Update()
        {
            if (gamewindow.Focused) { Controls.GetControls(); }
            Camera.lookat = Camera.GetViewMatrix();
        }

        public static float angle = 0;
        public static Stopwatch w1 = new Stopwatch();
        public static int frames = 0;
        public static void Render()
        {
            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception(error.ToString());
            }
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            GL.Color3(Color.Blue);



            if (testobj != null){ testobj.Draw(Matrix4.CreateScale(.001f,.001f,.001f)*Matrix4.CreateTranslation(0,0.002f,0), PolygonMode.Fill);}
           // if (testmap != null){ testmap.Draw(Matrix4.CreateScale(.01f, .01f, .01f), PolygonMode.Line); }
            //if (testobj2 != null) { testobj2.Draw(Matrix4.CreateScale(.001f, .001f, .001f) * Matrix4.CreateTranslation(0, -0.02f, 0), PolygonMode.Fill); }
            


            Shaders.UseShader(UntexturedShader);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Yellow);
            GL.Vertex3(0, 0, -20);
            GL.Vertex3(0, 0, 0);
            GL.Color3(Color.Blue);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, +20);
            GL.Color3(Color.Red);
            GL.Vertex3(-20, 0, 0);
            GL.Vertex3(0, 0, 0);
            GL.Color3(Color.Orange);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(+20, 0, 0);
            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, +20, 0);
            GL.Color3(Color.Purple);
            GL.Vertex3(0, -20, 0);
            GL.Vertex3(0, 0, 0);
            GL.End();
            gamewindow.SwapBuffers();
            frames++;
            if (!w1.IsRunning || w1.ElapsedMilliseconds >= 1000)
            {
                gamewindow.Title = "Project Cobalt (" + gamewindow.Width + " x " + gamewindow.Height + ")" + " -" + frames;
                frames = 0;
                w1.Restart();
            }

        }
    }
}
