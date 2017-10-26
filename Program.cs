using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Input;
using System.Threading;
using System.Configuration;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

namespace ProjectCobalt
{
    class Program
    {
        public static GameWindow gamewindow = new GameWindow();
        public static Point WindowCenter;
        public static Stopwatch watch = new Stopwatch();
        public static int UntexturedShader = -1;
        public static int BasicShader = -1;
        public static int TexturedShader = -1;
        public static int ShadowShader = -1;
        public static int SkyboxShader = -1;
        public static ErrorCode error;


        public static List<uint> testlist = new List<uint>();

        static void Main()
        {
            using (gamewindow)
            {
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
            Camera.View = Matrix4.CreatePerspectiveFieldOfView(1f, (float)gamewindow.Width / (float)gamewindow.Height, .1f, 1000);
            WindowCenter = new Point(gamewindow.Bounds.Left + (int)(gamewindow.Bounds.Width / 2.0), gamewindow.Bounds.Top + (int)(gamewindow.Bounds.Height / 2.0));
        }

        public static Content.Object testobj1;
        public static Content.Object skybox;
        public static Content.Object lightsphere;
        public static void Load()
        {
            gamewindow.VSync = VSyncMode.On;
            gamewindow.WindowState = WindowState.Maximized;
            gamewindow.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - gamewindow.Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - gamewindow.Height) / 2);
            UntexturedShader = Shaders.LoadShaders(Shaders.VUnTextured, Shaders.FUnTextured);
            BasicShader = Shaders.LoadShaders(Shaders.Vbasic, Shaders.Fbasic);
            TexturedShader = Shaders.LoadShaders(Shaders.VTextured, Shaders.FTextured);
            SkyboxShader = Shaders.LoadShaders(Shaders.VSkybox, Shaders.FSkybox);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            //GL.Enable(EnableCap.FramebufferSrgb);
            gamewindow.CursorVisible = false;
            GL.ClearColor(Color.CornflowerBlue);

            testobj1 = Content.Object.LoadObj(@"C:\Users\alexf\Google Drive\Computer\Desktop\ModelFile\test1\test1.ccf",  true);
            // testobj1.ShaderProgram = TexturedShader;
            // testobj1.Transformation = Matrix4.CreateRotationX(5) *Matrix4.CreateScale(.0005f) *Matrix4.CreateTranslation(0.05f,-0.5f,0);
            testobj1.Transformation = Matrix4.CreateScale(.0005f);
            //skybox = Content.Mesh.LoadObj(@"C:\Users\alexf\Google Drive\Computer\Desktop\skybox1\cubemap.obj", false);
            // skybox.ShaderProgram = SkyboxShader;
            List<string> skyboxfaces = new List<string>();
            skyboxfaces.Add(Directory.GetFiles(@"C:\Users\alexf\Google Drive\Computer\Desktop\skybox1").Where(x => x.Contains("RT.jpg")).First());
            skyboxfaces.Add(Directory.GetFiles(@"C:\Users\alexf\Google Drive\Computer\Desktop\skybox1").Where(x => x.Contains("LF.jpg")).First());
            skyboxfaces.Add(Directory.GetFiles(@"C:\Users\alexf\Google Drive\Computer\Desktop\skybox1").Where(x => x.Contains("UP.jpg")).First());
            skyboxfaces.Add(Directory.GetFiles(@"C:\Users\alexf\Google Drive\Computer\Desktop\skybox1").Where(x => x.Contains("DN.jpg")).First());
            skyboxfaces.Add(Directory.GetFiles(@"C:\Users\alexf\Google Drive\Computer\Desktop\skybox1").Where(x => x.Contains("BK.jpg")).First());
            skyboxfaces.Add(Directory.GetFiles(@"C:\Users\alexf\Google Drive\Computer\Desktop\skybox1").Where(x => x.Contains("FR.jpg")).First());
            //Content.SkyboxTexture = Content.LoadCubeMap(skyboxfaces.ToArray());


            Lighting.Light l1 = new Lighting.Light(new Vector3(0f, 1f, -1f), new Vector3(0f, 0.5f, .5f), new Vector4(1f, 1f, 1f,1f),new Vector3(0,0,0), 0, 200, .01f);
            Lighting.LightList.Add(l1);
        }

        public static void Update()
        {
            if (gamewindow.Focused) { Controls.GetControls(); }
            Camera.lookat = Camera.GetViewMatrix();
            if (!w1.IsRunning || w1.ElapsedMilliseconds >= 1000)
            {
                gamewindow.Title = "Project Cobalt (" + gamewindow.Width + " x " + gamewindow.Height + ")" + " " + frames + (gamewindow.VSync == VSyncMode.On ? " frames/second (Limited to Monitor Refresh rate)" : " frames/second");
                frames = 0;
                w1.Restart();
            }
           // skybox.Transformation = Matrix4.CreateTranslation(Camera.Position);
        }
        public static Stopwatch w1 = new Stopwatch();
        public static int frames = 0;
        
        //public static void ShowNormals()
        //{
        //    Shaders.UseShader(0);
        //    Shaders.CurrentTransformation = testobj1.Transformation;
        //    Shaders.UseShader(BasicShader);
        //    GL.Begin(PrimitiveType.Lines);
        //    foreach (Content.Vertex t1 in testobj1.Vertexlist)
        //    {
        //        GL.Color3(Color.White);
        //        GL.Vertex3(t1.Position);
        //        GL.Color3(Color.Red);
        //        GL.Vertex3(t1.Position + (t1.Normal*10));
        //    }
        //    GL.End();
        //    Shaders.CurrentTransformation = Matrix4.Identity;
        //    Shaders.UseShader(0);
        //}

        public static void Render()
        {
            error = GL.GetError();
            if (error != ErrorCode.NoError) { throw new Exception(error.ToString()); }
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Color3(Color.Blue);
          //  GL.DepthMask(false);
          //  skybox.Draw(PolygonMode.Fill,SkyboxShader);
          //  GL.DepthMask(true);

            testobj1.Draw();
            Shaders.CurrentTransformation = Matrix4.Identity;

            Shaders.UseShader(BasicShader);
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
            foreach (Lighting.Light light in Lighting.LightList)
            {
                GL.Color3(Color.Green);
                GL.Vertex3(light.Position);
                GL.Color3(Color.White);
                GL.Vertex3(light.Direction);
            }
            GL.End();
            //// ShowNormals();
            gamewindow.SwapBuffers();
            frames++;


        }
    }
}
