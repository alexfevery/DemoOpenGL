using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;

namespace ProjectCobalt
{
    public class Content
    {
        public static int FloatSizeInBytes = 4;
        public static int VertexAttributeCount = 11;
        private static Dictionary<string, Object> Meshlist = new Dictionary<string, Object>();

        public static int SkyboxTexture = -1;
        public static int LoadCubeMap(string[] faces)
        {
            int textureID = GL.GenTexture();

            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);
            for (int i = 0; i < faces.Length; i++)
            {
                Bitmap bmp = new Bitmap(faces[i]);
                //bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureParameterName.ClampToEdge);
            }
            return textureID;
        }

        public class Object
        {
            public string name;
            public string path;
            public string modelpath;
            public List<Component> Components = new List<Component>();
            public Matrix4 Transformation = Matrix4.Identity;
            public List<Instance> Instances = new List<Instance>();

            public void Draw()
            {
                foreach (Instance g1 in Instances)
                {
                    GL.CullFace(g1.MirrorMode);
                    Shaders.CurrentTransformation = g1.matrix * Transformation;
                    DrawModel(g1, Shaders.CurrentTransformation);
                }
            }

            public void DrawModel(Instance c1,Matrix4 Transformation)
            {
                   //  Shaders.CurrentTransformation = (c1 as Subcomponent).matrix * Transformation;
                if(c1.component.model != null)
                {
                    Shaders.UseShader(c1.component.model.ShaderProgram, c1.component.model);
                    GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                    if (c1.component.model.DiffuseTexture != -1)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, c1.component.model.DiffuseTexture);
                    }
                    if (c1.component.model.SpecularTexture != -1)
                    {
                        GL.ActiveTexture(TextureUnit.Texture1);
                        GL.BindTexture(TextureTarget.Texture2D, c1.component.model.SpecularTexture);
                    }
                    if (c1.component.model.NormalTexture != -1)
                    {
                        GL.ActiveTexture(TextureUnit.Texture2);
                        GL.BindTexture(TextureTarget.Texture2D, c1.component.model.NormalTexture);
                    }
                    GL.BindVertexArray(c1.component.model.VertexArrayObjectID);
                    Program.error = GL.GetError();
                    if (Program.error != ErrorCode.NoError) { throw new Exception(Program.error.ToString()); }
                    GL.DrawElements(PrimitiveType.Triangles, c1.component.model.IndexBuffer.Count(), DrawElementsType.UnsignedInt, 0);
                    Shaders.UseShader(0);
                }
                foreach (Instance c2 in c1.component.Children)
                {
                    DrawModel(c2, Shaders.CurrentTransformation);
                }
            }

            public static Object LoadObj(string path, bool OverrideNormalsSmoothShading)
            {
                Object m1 = null;
                if (!Meshlist.TryGetValue(path, out m1))
                {
                    m1 = ReadCCF(path);
                    m1.path = path;
                    m1.modelpath = Path.Combine(Path.GetDirectoryName(path), "Models");
                    foreach(Component c1 in m1.Components)
                    {
                        LinkModels(m1,c1);
                    }
                    foreach(Instance i1 in m1.Instances)
                    {
                        LinkComponents(m1, i1);
                        
                    }
                    Meshlist.Add(path, m1);
                }
                return m1;
            }

            public static void LinkModels(Object m1,Component c1)
            {
                if(c1.model != null)
                {
                    c1.model = Model.ReadCPF(Path.Combine(m1.modelpath,c1.model.name+".cpf"));
                    c1.model.ShaderProgram = Program.TexturedShader;
                }
            }

            public static void LinkComponents(Object m1, Instance i1)
            {
                i1.component = m1.Components.Where(x => x.ID == i1.ComponentID).First();
                foreach(Instance i2 in i1.component.Children)
                {
                    LinkComponents(m1, i2);
                }
            }

            public static Object ReadCCF(string path)
            {
                Object t1 = new Object();
                TextReader tx1 = new StreamReader(path);
                XmlSerializer tx2 = new XmlSerializer(typeof(Object));
                t1 = (Object)tx2.Deserialize(tx1);
                tx1.Close();
                return t1;
            }

            //public static Mesh LoadHeightMap(string path)
            //{
            //    Bitmap BM1 = new Bitmap(path);
            //    int zcount = BM1.Height;
            //    int xcount = BM1.Width;
            //    List<Vertex> vlist = new List<Vertex>();
            //    int x = 0;
            //    int z = 0;
            //    while (z < zcount - 1)
            //    {
            //        float c1 = BM1.GetPixel(x, z).R / 255f;
            //        vlist.Add(new Vertex(x, c1, z, 0, 0));
            //        z++;
            //        c1 = BM1.GetPixel(x, z).R / 255f;
            //        vlist.Add(new Vertex(x, c1, z, 0, 0));
            //        x++;
            //        c1 = BM1.GetPixel(x, z).R / 255f;
            //        vlist.Add(new Vertex(x, c1, z, 0, 0));
            //        x--;
            //        z--;
            //        c1 = BM1.GetPixel(x, z).R / 255f;
            //        vlist.Add(new Vertex(x, c1, z, 0, 0));
            //        x++;
            //        z++;
            //        c1 = BM1.GetPixel(x, z).R / 255f;
            //        vlist.Add(new Vertex(x, c1, z, 0, 0));
            //        z--;
            //        c1 = BM1.GetPixel(x, z).R / 255f;
            //        vlist.Add(new Vertex(x, c1, z, 0, 0));
            //        if (x == xcount - 1)
            //        {
            //            x = 0;
            //            z++;
            //        }
            //    }
            //    Dictionary<Vertex, uint> checkset1 = new Dictionary<Vertex, uint>();
            //    uint i1 = 0;
            //    List<Vertex> newpoints = new List<Vertex>();
            //    //g1.ShaderProgram = Shaders.LoadShaders(Shaders.VUntextured, Shaders.FUntextured);
            //    List<uint> ilist = new List<uint>();
            //    foreach (Vertex v1 in vlist)
            //    {
            //        Vertex newtt = new Vertex(v1.Position.X / 10, v1.Position.Y * 5, v1.Position.Z / 10, 0, 0);
            //        if (v1.Position.Y < .001f) { continue; }
            //        if (checkset1.ContainsKey(newtt)) { ilist.Add(checkset1[newtt]); }
            //        else
            //        {
            //            newpoints.Add(newtt);
            //            checkset1.Add(newtt, i1);
            //            ilist.Add(i1);
            //            i1++;
            //        }
            //    }
            //    vlist = newpoints;
            //    Mesh m1 = new Mesh(vlist);
            //    Group g1 = new Group();
            //    g1.Parentmesh = m1;
            //    g1.IndexBuffer = ilist;
            //    g1.LoadToGPU();
            //    m1.groups.Add(g1);
            //    return m1;
            //}

            public class Instance
            {
                public string Name;
                public string ComponentID;
                public Component component;
                public CullFaceMode MirrorMode;
                public Matrix4 matrix;
                private string matrixstring;
                public string Matrix
                {
                    set
                    {
                        matrixstring = value;
                        string[] split = value.Split(',');
                        float[] t1 = split.Select(x => float.Parse(x, CultureInfo.InvariantCulture.NumberFormat)).ToArray();
                        matrix = new Matrix4(new Vector4(t1[0], t1[1], t1[2], t1[3]), new Vector4(t1[4], t1[5], t1[6], t1[7]), new Vector4(t1[8], t1[9], t1[10], t1[11]), new Vector4(t1[12], t1[13], t1[14], t1[15]));
                        Matrix4 t2 = new Matrix4();
                        t2.Row0 = new Vector4(1, 0, 0, 0);
                        t2.Row1 = new Vector4(0, 0, -1, 0);
                        t2.Row2 = new Vector4(0, 1, 0, 0);
                        t2.Row3 = new Vector4(0, 0, 0, 1);
                        matrix = matrix * t2;
                        if(matrix.M11 < 0) { MirrorMode = CullFaceMode.Front; }
                        else { MirrorMode = CullFaceMode.Back; }
                    }
                    get { return matrixstring; }
                }
            }

            public class Component
            {
                public string ID;
                public Model model;
                public List<Instance> Children;
            }

            public class Model
            {
                public string name;
                public string ID;
                public string path;
                public List<Vertex> VertexList = new List<Vertex>();

                public int ShaderProgram;
                public int VertexBufferID;
                public List<float> VertexBuffer;

                public int VertexArrayObjectID;
                public int IndexBufferID;
                public List<uint> IndexBuffer = new List<uint>();
                public int DiffuseTexture = -1;
                public int SpecularTexture = -1;
                public int NormalTexture = -1;
                public List<Face> Faces = new List<Face>();

                public void LoadToGPU()
                {
                    VertexBuffer = new List<float>();
                    foreach (Vertex vert1 in VertexList)
                    {
                        for (int i = 0; i < VertexAttributeCount; i++) { VertexBuffer.Add(vert1.data[i]); }
                    }
                    VertexBufferID = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferID);
                    GL.BufferData(BufferTarget.ArrayBuffer, (FloatSizeInBytes * VertexBuffer.Count()), VertexBuffer.ToArray(), BufferUsageHint.StaticDraw);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


                    if (VertexBufferID == 0) { throw new Exception("Cannot create index buffer until parent class vertex buffer is created."); }
                    VertexArrayObjectID = GL.GenVertexArray();
                    IndexBufferID = GL.GenBuffer();
                    GL.BindVertexArray(VertexArrayObjectID);
                    GL.EnableVertexAttribArray(0);
                    GL.EnableVertexAttribArray(1);
                    GL.EnableVertexAttribArray(2);
                    GL.EnableVertexAttribArray(3);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferID);
                    GL.VertexPointer(3, VertexPointerType.Float, FloatSizeInBytes * VertexAttributeCount, 0);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, FloatSizeInBytes * VertexAttributeCount, 0);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, FloatSizeInBytes * VertexAttributeCount, 12);
                    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, FloatSizeInBytes * VertexAttributeCount, 12);
                    GL.NormalPointer(NormalPointerType.Float, FloatSizeInBytes * VertexAttributeCount, 20);
                    GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, FloatSizeInBytes * VertexAttributeCount, 20);
                    GL.VertexPointer(3, VertexPointerType.Float, FloatSizeInBytes * VertexAttributeCount, 32);
                    GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, FloatSizeInBytes * VertexAttributeCount, 32);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferID);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (FloatSizeInBytes * IndexBuffer.Count()), IndexBuffer.ToArray(), BufferUsageHint.StaticDraw);
                    GL.BindVertexArray(0);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }

                public static Model ReadCPF(string path)
                {
                    List<string[]> TextToData = File.ReadAllLines(path).Select(x => x.Split('\t')).ToList();
                    bool SmoothShading = TextToData[0][1].Contains("shading:smooth");
                    Model g1 = new Model();
                    g1.name = TextToData[0][0];
                    g1.LoadTextures(Path.GetDirectoryName(path), g1.name);
                    foreach (string[] item in TextToData.Skip(1))
                    {
                        Vertex v1;
                        if (SmoothShading) { v1 = new Vertex(new Vector3(float.Parse(item[0], CultureInfo.InvariantCulture), float.Parse(item[1], CultureInfo.InvariantCulture), float.Parse(item[2], CultureInfo.InvariantCulture)), new Vector2(float.Parse(item[3], CultureInfo.InvariantCulture), float.Parse(item[4], CultureInfo.InvariantCulture)), new Vector3(float.Parse(item[8], CultureInfo.InvariantCulture), float.Parse(item[9], CultureInfo.InvariantCulture), float.Parse(item[10], CultureInfo.InvariantCulture)), new Vector3(float.Parse(item[11], CultureInfo.InvariantCulture), float.Parse(item[12], CultureInfo.InvariantCulture), float.Parse(item[13], CultureInfo.InvariantCulture))); }
                        else { v1 = new Vertex(new Vector3(float.Parse(item[0], CultureInfo.InvariantCulture), float.Parse(item[1], CultureInfo.InvariantCulture), float.Parse(item[2], CultureInfo.InvariantCulture)), new Vector2(float.Parse(item[3], CultureInfo.InvariantCulture), float.Parse(item[4], CultureInfo.InvariantCulture)), new Vector3(float.Parse(item[5], CultureInfo.InvariantCulture), float.Parse(item[6], CultureInfo.InvariantCulture), float.Parse(item[7], CultureInfo.InvariantCulture)), new Vector3(float.Parse(item[11], CultureInfo.InvariantCulture), float.Parse(item[12], CultureInfo.InvariantCulture), float.Parse(item[13], CultureInfo.InvariantCulture))); }
                        g1.VertexList.Add(v1);
                        g1.IndexBuffer.Add(Convert.ToUInt32(g1.VertexList.Count() - 1));
                    }
                    g1.LoadToGPU();
                    return g1;
                }

                public class Face
                {
                    public List<Vertex> Verticies = new List<Vertex>();
                    public Vector3 Normal;
                }

                public class Vertex
                {
                    public Vector3 Position;
                    public Vector2 TexCoord;
                    public Vector3 Normal;
                    public Vector3 Tangent;

                    public List<float> data
                    {
                        get { return new List<float> { Position.X, Position.Y, Position.Z, TexCoord.X, TexCoord.Y, Normal.X, Normal.Y, Normal.Z, Tangent.X, Tangent.Y, Tangent.Z }; }
                    }

                    public Vertex(Vector3 pos, Vector2 Tex, Vector3 Norm, Vector3 Tan)
                    {
                        Position = pos;
                        TexCoord = Tex;
                        Normal = Norm;
                        Tangent = Tan;
                    }

                    public Vertex()
                    {

                    }

                    public Vertex(float posx, float posy, float posz, float Textx, float texty, float normx, float normy, float normz, float tanx, float tany, float tanz)
                    {
                        Position = new Vector3(posx, posy, posz);
                        TexCoord = new Vector2(Textx, texty);
                        Normal = new Vector3(normx, normy, normz);
                        Tangent = new Vector3(tanx, tany, tanz);
                    }
                }

                public void LoadTextures(string folder, string groupname)
                {
                    List<string> textures = Directory.GetFiles(folder).Where(x => Path.GetFileName(x).ToLower().Contains(groupname.ToLower() + "_")).ToList();
                    try { DiffuseTexture = LoadTexture(textures.Where(x => x.ToLower().Contains("_diffuse")).First()); } catch { DiffuseTexture = -1; }
                    try { SpecularTexture = LoadTexture(textures.Where(x => x.ToLower().Contains("_specular")).First()); } catch { SpecularTexture = -1; }
                    try { NormalTexture = LoadTexture(textures.Where(x => x.ToLower().Contains("_normal")).First()); } catch { NormalTexture = -1; }
                }

                private int LoadTexture(string path)
                {
                    int id = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, id);
                    Bitmap bmp = new Bitmap(path);
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    return id;
                }


            }
        }

    }

}

