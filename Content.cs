using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using FileFormatWavefront;
using FileFormatWavefront.Model;
using System.IO;
using System.Threading;

namespace ProjectCobalt
{
    class Content
    {
        public static int FloatSizeInBytes = 4;
        public static int VertexAttributeCount = 5;

        public class Vertex
        {
            public Vector3 Position;
            public Vector2 TexCoord;

            public List<float> data
            {
                get { return new List<float> { Position.X, Position.Y, Position.Z, TexCoord.X, TexCoord.Y }; }
            }

            public Vertex(Vector3 pos, Vector2 Texcord)
            {
                Position = pos;
                TexCoord = Texcord;
            }

            public Vertex(float posx, float posy, float posz, float Textx, float texty)
            {
                Position = new Vector3(posx, posy, posz);
                TexCoord = new Vector2(Textx, texty);
            }
        }

        public class Group
        {
            public List<uint> IndexList;
            public VBO VBO;
            //public int ShaderProgram = Shaders.LoadShaders(Shaders.VUntextured, Shaders.FUntextured);
        }

        public class Mesh
        {
            public List<Group> groups;
            public List<Vertex> VertexList;
            public int Texture = -1;
            public int ShaderProgram;

            public static int LoadTexture()
            {
                int id = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, id);
                Bitmap bmp = new Bitmap("GameContent/TestTexture.png");

                bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                return id;
            }

            public void Draw(Matrix4 PosScaleOreint, PolygonMode mode)
            {
                foreach (Group g1 in groups)
                {
                    if (g1.VBO == null) { g1.VBO = VBO.LoadToGPU(VertexList, g1.IndexList); }
                    Shaders.CurrentTransformation = PosScaleOreint;
                    Shaders.UseShader(ShaderProgram);
                    GL.PolygonMode(MaterialFace.FrontAndBack, mode);
                    if (Texture != -1) { GL.BindTexture(TextureTarget.Texture2D, Texture); }
                    GL.BindVertexArray(g1.VBO.VertexArrayObjectID);
                    Program.error = GL.GetError();
                    if (Program.error != ErrorCode.NoError) { throw new Exception(Program.error.ToString()); }
                    GL.DrawElements(PrimitiveType.Triangles, g1.VBO.IndexBuffer.Count(), DrawElementsType.UnsignedInt, 0);
                    Shaders.CurrentTransformation = Matrix4.Identity;
                    Shaders.UseShader(0);
                }
            }

            public static Mesh LoadObj(string path)
            {
                Mesh m1 = new Mesh();
                bool loadTextureImages = true;
                FileLoadResult<Scene> objdata = FileFormatObj.Load(path, loadTextureImages);
                m1.VertexList = objdata.Model.Vertices.Select(x => new Vertex(new Vector3(x.x, x.y, x.z), new Vector2(-1, -1))).ToList();

                m1.groups = new List<Group>();
                if (objdata.Model.Groups.Count() == 0)
                {
                    Group g1 = new Group();
                    g1.IndexList = new List<uint>();
                    foreach (Face f1 in objdata.Model.UngroupedFaces)
                    {
                        g1.IndexList.Add((uint)f1.Indices[0].vertex);
                        if ((objdata.Model.Uvs.Count != 0) && m1.VertexList[f1.Indices[0].vertex].TexCoord.X < 0)
                        {
                            m1.VertexList[f1.Indices[0].vertex].TexCoord.X = objdata.Model.Uvs[(int)f1.Indices[0].uv].u;
                            m1.VertexList[f1.Indices[0].vertex].TexCoord.Y = objdata.Model.Uvs[(int)f1.Indices[0].uv].v;
                        }
                        g1.IndexList.Add((uint)f1.Indices[1].vertex);
                        if ((objdata.Model.Uvs.Count != 0) && m1.VertexList[f1.Indices[1].vertex].TexCoord.X < 0)
                        {
                            m1.VertexList[f1.Indices[1].vertex].TexCoord.X = objdata.Model.Uvs[(int)f1.Indices[1].uv].u;
                            m1.VertexList[f1.Indices[1].vertex].TexCoord.Y = objdata.Model.Uvs[(int)f1.Indices[1].uv].v;
                        }
                        g1.IndexList.Add((uint)f1.Indices[2].vertex);
                        if ((objdata.Model.Uvs.Count != 0) && m1.VertexList[f1.Indices[1].vertex].TexCoord.X < 0)
                        {
                            m1.VertexList[f1.Indices[2].vertex].TexCoord.X = objdata.Model.Uvs[(int)f1.Indices[2].uv].u;
                            m1.VertexList[f1.Indices[2].vertex].TexCoord.Y = objdata.Model.Uvs[(int)f1.Indices[2].uv].v;
                        }
                    }
                    m1.groups.Add(g1);
                }
                else
                {
                    foreach (FileFormatWavefront.Model.Group g1 in objdata.Model.Groups)
                    {
                        Group meshgroup = new Group();
                        meshgroup.IndexList = new List<uint>();
                        foreach (Face f1 in g1.Faces)
                        {
                            meshgroup.IndexList.Add((uint)f1.Indices[0].vertex);
                            if ((objdata.Model.Uvs.Count != 0) && m1.VertexList[f1.Indices[0].vertex].TexCoord.X < 0)
                            {
                                m1.VertexList[f1.Indices[0].vertex].TexCoord.X = objdata.Model.Uvs[(int)f1.Indices[0].uv].u;
                                m1.VertexList[f1.Indices[0].vertex].TexCoord.Y = objdata.Model.Uvs[(int)f1.Indices[0].uv].v;
                            }
                            meshgroup.IndexList.Add((uint)f1.Indices[1].vertex);
                            if ((objdata.Model.Uvs.Count != 0) && m1.VertexList[f1.Indices[1].vertex].TexCoord.X < 0)
                            {
                                m1.VertexList[f1.Indices[1].vertex].TexCoord.X = objdata.Model.Uvs[(int)f1.Indices[1].uv].u;
                                m1.VertexList[f1.Indices[1].vertex].TexCoord.Y = objdata.Model.Uvs[(int)f1.Indices[1].uv].v;
                            }
                            meshgroup.IndexList.Add((uint)f1.Indices[2].vertex);
                            if ((objdata.Model.Uvs.Count != 0) && m1.VertexList[f1.Indices[1].vertex].TexCoord.X < 0)
                            {
                                m1.VertexList[f1.Indices[2].vertex].TexCoord.X = objdata.Model.Uvs[(int)f1.Indices[2].uv].u;
                                m1.VertexList[f1.Indices[2].vertex].TexCoord.Y = objdata.Model.Uvs[(int)f1.Indices[2].uv].v;
                            }
                        }
                        m1.groups.Add(meshgroup);
                    }
                }



                return m1;
            }

            public static Mesh LoadHeightMap(string path)
            {
                Mesh m1 = new Mesh();
                Bitmap BM1 = new Bitmap(path);
                int zcount = BM1.Height;
                int xcount = BM1.Width;
                m1.VertexList = new List<Vertex>();
                int x = 0;
                int z = 0;
                while (z < zcount - 1)
                {
                    float c1 = BM1.GetPixel(x, z).R / 255f;
                    m1.VertexList.Add(new Vertex(x, c1, z, 0, 0));
                    z++;
                    c1 = BM1.GetPixel(x, z).R / 255f;
                    m1.VertexList.Add(new Vertex(x, c1, z, 0, 0));
                    x++;
                    c1 = BM1.GetPixel(x, z).R / 255f;
                    m1.VertexList.Add(new Vertex(x, c1, z, 0, 0));
                    x--;
                    z--;
                    c1 = BM1.GetPixel(x, z).R / 255f;
                    m1.VertexList.Add(new Vertex(x, c1, z, 0, 0));
                    x++;
                    z++;
                    c1 = BM1.GetPixel(x, z).R / 255f;
                    m1.VertexList.Add(new Vertex(x, c1, z, 0, 0));
                    z--;
                    c1 = BM1.GetPixel(x, z).R / 255f;
                    m1.VertexList.Add(new Vertex(x, c1, z, 0, 0));
                    if (x == xcount - 1)
                    {
                        x = 0;
                        z++;
                    }
                }
                Dictionary<Vertex, uint> checkset1 = new Dictionary<Vertex, uint>();
                uint i1 = 0;
                List<Vertex> newpoints = new List<Vertex>();
                m1.groups = new List<Group>();
                Group g1 = new Group();
                //g1.ShaderProgram = Shaders.LoadShaders(Shaders.VUntextured, Shaders.FUntextured);
                g1.IndexList = new List<uint>();
                foreach (Vertex v1 in m1.VertexList)
                {
                    Vertex newtt = new Vertex(v1.Position.X / 10, v1.Position.Y * 5, v1.Position.Z / 10, 0, 0);
                    if (checkset1.ContainsKey(newtt)) { g1.IndexList.Add(checkset1[newtt]); }
                    else
                    {
                        newpoints.Add(newtt);
                        checkset1.Add(newtt, i1);
                        g1.IndexList.Add(i1);
                        i1++;
                    }
                }
                m1.VertexList = newpoints;
                m1.groups.Add(g1);
                return m1;
            }

        }

        public class VBO
        {
            public int VertexArrayObjectID;
            public int VertexBufferID;
            public int IndexBufferID;
            public List<float> VertexBuffer;
            public List<uint> IndexBuffer;

            public static VBO LoadToGPU(List<Vertex> Vertexdata, List<uint> Indexdata)
            {
                VBO v1 = new VBO();
                v1.VertexBuffer = new List<float>();
                foreach (Vertex vert1 in Vertexdata)
                {
                    v1.VertexBuffer.Add(vert1.data[0]);
                    v1.VertexBuffer.Add(vert1.data[1]);
                    v1.VertexBuffer.Add(vert1.data[2]);
                    v1.VertexBuffer.Add(vert1.data[3]);
                    v1.VertexBuffer.Add(vert1.data[4]);
                }
                v1.IndexBuffer = Indexdata;

                v1.VertexArrayObjectID = GL.GenVertexArray();
                v1.VertexBufferID = GL.GenBuffer();

                GL.BindVertexArray(v1.VertexArrayObjectID);

                GL.EnableVertexAttribArray(0);//
                GL.EnableVertexAttribArray(1);//

                GL.BindBuffer(BufferTarget.ArrayBuffer, v1.VertexBufferID);
                GL.BufferData(BufferTarget.ArrayBuffer, (FloatSizeInBytes * v1.VertexBuffer.Count()), v1.VertexBuffer.ToArray(), BufferUsageHint.StaticDraw);

                GL.VertexPointer(3, VertexPointerType.Float, FloatSizeInBytes * VertexAttributeCount, 0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, FloatSizeInBytes * VertexAttributeCount, 0);

                GL.TexCoordPointer(2, TexCoordPointerType.Float, FloatSizeInBytes * VertexAttributeCount, 12);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, FloatSizeInBytes * VertexAttributeCount, 12);

                v1.IndexBufferID = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, v1.IndexBufferID);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (FloatSizeInBytes * v1.IndexBuffer.Count()), v1.IndexBuffer.ToArray(), BufferUsageHint.StaticDraw);
                GL.BindVertexArray(0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                
                return v1;
            }



        }


    }
}
