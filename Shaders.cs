using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ProjectCobalt
{
    class Shaders
    {
        public static Matrix4 CurrentTransformation = Matrix4.Identity;

        public static string VTextured =
            @"
                #version 450
                layout(location = 0) in vec4 position;
                layout (location = 1) in vec2 texCoord;
                layout(location = 0) uniform mat4 view;
                layout(location = 1) uniform mat4 CurrentTransformation;

                out float flogz;
                out vec2 TexCoord;
                void main()
                {
                 gl_Position = (view * CurrentTransformation) * position;
                 flogz = 1.0 + gl_Position.w;
                 gl_Position.z = log2(max(1e-6, flogz)) * 0.0782 - 1.0;
                 TexCoord = texCoord;
                }
                ";

        public static string FTextured =
            @"
                #version 450
                in float flogz;  
                in vec2 TexCoord;
                out vec4 outputF;    
                uniform sampler2D ourTexture;

                void main()
                {
                 gl_FragDepth = log2(flogz) * 0.0391;
                 outputF = texture(ourTexture,TexCoord);
                }";

        public static string VUntextured =
            @"
                #version 450
                layout(location = 0) in vec4 position;
                layout(location = 3) in vec4 color;
                layout(location = 0) uniform mat4 view;
                layout(location = 1) uniform mat4 CurrentTransformation;
                
                out float flogz;
                out vec4 colorV;

                void main()
                {
                 gl_Position = (view * CurrentTransformation) * position;
                 flogz = 1.0 + gl_Position.w;
                 gl_Position.z = log2(max(1e-6, flogz)) * 0.0782 - 1.0;
                 colorV = color;
                }


                ";

        public static string FUntextured =
            @"
                #version 450
                in float flogz;  
                in vec4 colorV;
                out vec4 outputF;    

                void main()
                {
                 gl_FragDepth = log2(flogz) * 0.0391;
                 outputF = colorV;
                }";


        public static int LoadShaders(string VertexShader, string FragmentShader)
        {
            int ShaderProgram = GL.CreateProgram();
            int shader1 = GL.CreateShader(ShaderType.VertexShader);
            int shader2 = GL.CreateShader(ShaderType.FragmentShader);
            
            GL.ShaderSource(shader1, VertexShader);
            GL.CompileShader(shader1);
            GL.ShaderSource(shader2, FragmentShader);
            GL.CompileShader(shader2);

            string shadererror1 = GL.GetShaderInfoLog(shader1);
            if (shadererror1 != ""){throw new Exception("Vertex Shader Failed to Compile: " + shadererror1);}
            string shadererror2 = GL.GetShaderInfoLog(shader2);
            if (shadererror2 != "") { throw new Exception("Fragment Shader Failed to Compile: "+ shadererror2); }

            GL.AttachShader(ShaderProgram, shader1);
            GL.AttachShader(ShaderProgram, shader2);
            GL.LinkProgram(ShaderProgram);
            int statuscode;
            GL.GetProgram(ShaderProgram, GetProgramParameterName.LinkStatus, out statuscode);
            if (statuscode != 1) { throw new Exception("shaders failed to link"); }
            return ShaderProgram;
        }

        public static void UseShader(int ShaderProgram)
        {
            GL.UseProgram(ShaderProgram);
            if (ShaderProgram != 0)
            {
                GL.UniformMatrix4(0, false, ref Camera.lookat);
                GL.UniformMatrix4(1, false, ref CurrentTransformation);
            }
            Program.error = GL.GetError();
            if (Program.error != ErrorCode.NoError) { throw new Exception(Program.error.ToString()); }
        }

    }
}
