using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ProjectCobalt
{
    class Shaders
    {
        public static Matrix4 CurrentTransformation = Matrix4.Identity;

        public static string VSkybox =
            @"
                #version 450
                layout (location = 0) in vec4 position;

                out vec3 TexCoord;

                uniform mat4 CurrentTransformation;
                uniform mat4 view;

                void main()
                {
                    TexCoord = position.xyz;
                    vec4 newposition = CurrentTransformation * position;
                    gl_Position = view * newposition;
                }  
";

        public static string FSkybox =
            @"
                #version 450
                out vec4 FragColor;

                in vec3 TexCoord;

                uniform samplerCube SkyBox;

                void main()
                {    
                    FragColor = texture(SkyBox, TexCoord);
                }
";


        public static string VTextured =
            @"
                #version 450
                layout(location = 0) in vec4 position;
                layout(location = 1) in vec2 texCoord;
                layout(location = 2) in vec3 normal;
                layout (location = 3) in vec3 Tangent;

                uniform mat4 view;
                uniform mat4 CurrentTransformation;
                
                out vec4 newposition;
                out vec3 TransformedNormal;
                out vec3 TransformedTangent;
                out float flogz;
                out vec2 TexCoord;

                void main()
                {
                 newposition = CurrentTransformation * position;
                 TexCoord = texCoord;
                 TransformedNormal = normalize(vec3(CurrentTransformation * vec4(normal,0)));
                 TransformedTangent = normalize(vec3(CurrentTransformation * vec4(Tangent,0)));
                 gl_Position = view * newposition;
                 flogz = 1.0 + gl_Position.w;
                 gl_Position.z = log2(max(1e-6, flogz)) * 0.0782 - 1.0;
                }
                ";

        public static string FTextured =
            @"  #version 450 
                
                in float flogz;  
                in vec2 TexCoord;
                in vec4 newposition;
                in vec3 TransformedNormal;    
                in vec3 TransformedTangent;
                
                uniform sampler2D DiffuseMap;
                uniform sampler2D SpecularMap;
                uniform sampler2D NormalMap;
                uniform vec3 cameraposition;
                uniform int lightcount;
                vec3 UpdatedNormal = TransformedNormal;
               
                out vec4 FinalColor;  
                
                struct Light 
                {    
                    vec3 Position;
                    vec3 AttenuationFactor;
                    vec3 Direction;
                    vec4 Color;
                    float Angle;
                    int SpecularSize;
                    float MinimumAmbient;
                };
                uniform Light Lights[10];

                vec3 ReadNormalMap()
                {
                    vec3 TransformedTangent0 = normalize(TransformedTangent - dot(TransformedTangent, -TransformedNormal) * -TransformedNormal);
                    vec3 Bitangent = cross(TransformedTangent0,TransformedNormal);
                    vec3 MapNormal = texture(NormalMap, TexCoord).xyz;
                    MapNormal = 2.0 * MapNormal - vec3(1.0, 1.0, 1.0);
                    mat3 TBN = mat3(TransformedTangent0, Bitangent, TransformedNormal);
                    return normalize(TBN * MapNormal);
                }

                vec4 GetSpecular(Light l1,vec3 newlightvector)
                {
                    vec3 cameravector = normalize(cameraposition-newposition.xyz);
                    vec3 halfwayvector = normalize(newlightvector+cameravector);
                    float specularity = pow(max(0,dot(UpdatedNormal,halfwayvector)),l1.SpecularSize);
                    return vec4(specularity,specularity,specularity,1f);
                }
               
                vec4 GetDiffuse(Light l1,vec3 newlightvector)
                {
                    float diffuse = clamp(dot(newlightvector,UpdatedNormal),l1.MinimumAmbient,1f);
                    return vec4(diffuse,diffuse,diffuse,1f);
                }
                
                void main()
                {
                    UpdatedNormal = ReadNormalMap();
                    vec4 lightresult = vec4(0,0,0,0);
                    for(int i = 0; i < 10; i++)
                    {
                        if(i == lightcount){break;}
                        Light l1 = Lights[i];
                        vec3 LightVector = l1.Position - newposition.xyz;
                        float distance = length(LightVector);
                        LightVector = normalize(LightVector);
                        float attenuation = 1.0 /(l1.AttenuationFactor.x+(l1.AttenuationFactor.y*distance)+(l1.AttenuationFactor.z*distance*distance));  
                        lightresult += (GetDiffuse(l1,LightVector)*attenuation) * texture(DiffuseMap,TexCoord);
                        lightresult += GetSpecular(l1,LightVector) * (texture(SpecularMap,TexCoord)*3);
                        lightresult *= l1.Color;
                    }
                    gl_FragDepth = log2(flogz) * 0.0391;
                    FinalColor = lightresult;  
                   // FinalColor =  texture(DiffuseMap,TexCoord);
                }";

        public static string Vbasic =
            @"
                #version 450
                layout(location = 0) in vec4 position;
                layout(location = 3) in vec4 color;
                uniform mat4 view;
                uniform mat4 CurrentTransformation;
                
                out vec4 newposition;
                out vec4 colorV;

                void main()
                {
                 newposition = CurrentTransformation * position;
                 gl_Position = (view * CurrentTransformation) * position;
                 colorV = color;
                }
                ";

        public static string Fbasic =
    @"
                #version 450
                in vec4 colorV;
                out vec4 outputF;    
                void main()
                {
                    outputF = colorV;
                }";

        public static string VUnTextured =
            @"
                #version 450
                layout(location = 0) in vec4 position;
                layout(location = 3) in vec4 color;
                layout(location = 2) in vec3 normal;
                uniform mat4 view;
                uniform mat4 CurrentTransformation;
                
                out vec4 newposition;
                out vec3 TransformedNormal;
                out float flogz;
                out vec4 Color;

                void main()
                {
                 newposition = CurrentTransformation * position;
                 TransformedNormal = normalize(vec3(CurrentTransformation * vec4(normal,0)));
                 Color = color;
                 gl_Position = view * newposition;
                 flogz = 1.0 + gl_Position.w;
                 gl_Position.z = log2(max(1e-6, flogz)) * 0.0782 - 1.0;
                }
                ";

        public static string FUnTextured =
            @"  #version 450 
                
                in float flogz;  
                in vec4 Color;
                in vec4 newposition;
                in vec3 TransformedNormal;          
                
                uniform vec3 cameraposition;
                uniform int lightcount;
               
                out vec4 FinalColor;  
                
                struct Light 
                {    
                    vec3 Position;
                    vec3 AttenuationFactor;
                    vec3 Direction;
                    vec4 Color;
                    float Angle;
                    int SpecularSize;
                    float MinimumAmbient;
                };
                uniform Light Lights[10];
                
                vec4 GetSpecular(Light l1,vec3 newlightvector)
                {
                    vec3 reflectedlightvector = reflect(-newlightvector,TransformedNormal);
                    vec3 cameravector = normalize(cameraposition-newposition.xyz);
                    float specularity = max(0,dot(reflectedlightvector,cameravector));
                    specularity = pow(specularity,l1.SpecularSize);
                    return vec4(specularity,specularity,specularity,1f);
                }
               
                vec4 GetDiffuse(Light l1,vec3 newlightvector)
                {
                    float diffuse = clamp(dot(newlightvector,TransformedNormal),l1.MinimumAmbient,1f);
                    return vec4(diffuse,diffuse,diffuse,1f);
                }
                
                void main()
                {
                    vec4 lightresult = vec4(0,0,0,0);
                    for(int i = 0; i < 10; i++)
                    {
                        if(i == lightcount){break;}
                        Light l1 = Lights[i];
                        vec3 LightVector = l1.Position - newposition.xyz;
                        float distance = length(LightVector);
                        LightVector = normalize(LightVector);
                        float attenuation = 1.0 /(l1.AttenuationFactor.x+(l1.AttenuationFactor.y*distance)+(l1.AttenuationFactor.z*distance*distance));  
                        lightresult += (GetDiffuse(l1,LightVector)*attenuation) * Color;
                        lightresult += (GetSpecular(l1,LightVector)*attenuation);
                        lightresult *= l1.Color;
                    }
                    gl_FragDepth = log2(flogz) * 0.0391;
                    FinalColor = lightresult;
                }";




        public static int LoadShaders(string VertexShader, string FragmentShader, string GeometryShader = null)
        {
            int ShaderProgram = GL.CreateProgram();
            int shader1 = GL.CreateShader(ShaderType.VertexShader);
            int shader2 = GL.CreateShader(ShaderType.FragmentShader);
            int shader3 = GL.CreateShader(ShaderType.GeometryShader);

            GL.ShaderSource(shader1, VertexShader);
            GL.CompileShader(shader1);
            GL.ShaderSource(shader2, FragmentShader);
            GL.CompileShader(shader2);
            if (GeometryShader != null)
            {
                GL.ShaderSource(shader3, GeometryShader);
                GL.CompileShader(shader3);
            }

            string shadererror1 = GL.GetShaderInfoLog(shader1);
            if (shadererror1 != "") { throw new Exception("Vertex Shader Failed to Compile: " + shadererror1); }
            string shadererror2 = GL.GetShaderInfoLog(shader2);
            if (shadererror2 != "") { throw new Exception("Fragment Shader Failed to Compile: " + shadererror2); }
            if (GeometryShader != null)
            {
                string shadererror3 = GL.GetShaderInfoLog(shader3);
                if (shadererror3 != "") { throw new Exception("Geometry Shader Failed to Compile: " + shadererror3); }
            }

            GL.AttachShader(ShaderProgram, shader1);
            GL.AttachShader(ShaderProgram, shader2);
            if (GeometryShader != null) { GL.AttachShader(ShaderProgram, shader3); }
            GL.LinkProgram(ShaderProgram);
            string shaderProgramerror = GL.GetProgramInfoLog(ShaderProgram);
            int statuscode;
            GL.GetProgram(ShaderProgram, GetProgramParameterName.LinkStatus, out statuscode);
            if (statuscode != 1) { throw new Exception("shaders failed to link:" + shaderProgramerror); }
            return ShaderProgram;
        }

        public static void UseShader(int ShaderProgram, Content.Object.Model g1 = null)
        {
            GL.UseProgram(ShaderProgram);
            if (ShaderProgram != 0)
            {
                GL.UniformMatrix4(GL.GetUniformLocation(ShaderProgram, "view"), false, ref Camera.lookat);
                GL.UniformMatrix4(GL.GetUniformLocation(ShaderProgram, "CurrentTransformation"), false, ref CurrentTransformation);
                GL.Uniform3(GL.GetUniformLocation(ShaderProgram, "cameraposition"), Camera.Position);

                if (ShaderProgram == Program.SkyboxShader)
                {
                    if (Content.SkyboxTexture == -1) { throw new Exception(); }
                    GL.Uniform1(GL.GetUniformLocation(ShaderProgram, "SkyBox"), 0);
                }
                else
                {
                    GL.Uniform1(GL.GetUniformLocation(ShaderProgram, ("lightcount")), Lighting.LightList.Count);
                    for (int i = 0; i < Lighting.LightList.Count; i++)
                    {
                        string number = i.ToString();
                        GL.Uniform3(GL.GetUniformLocation(ShaderProgram, ("Lights[" + number + "].Position")), Lighting.LightList[i].Position);
                        GL.Uniform3(GL.GetUniformLocation(ShaderProgram, ("Lights[" + number + "].AttenuationFactor")), Lighting.LightList[i].AttenuationFactor);
                        GL.Uniform4(GL.GetUniformLocation(ShaderProgram, ("Lights[" + number + "].Color")), Lighting.LightList[i].Color);
                        GL.Uniform3(GL.GetUniformLocation(ShaderProgram, ("Lights[" + number + "].Direction")), Lighting.LightList[i].Direction);
                        GL.Uniform1(GL.GetUniformLocation(ShaderProgram, ("Lights[" + number + "].Angle")), Lighting.LightList[i].Angle);
                        GL.Uniform1(GL.GetUniformLocation(ShaderProgram, ("Lights[" + number + "].SpecularSize")), Lighting.LightList[i].SpecularSize);
                        GL.Uniform1(GL.GetUniformLocation(ShaderProgram, ("Lights[" + number + "].MinimumAmbient")), Lighting.LightList[i].MinimumAmbient);
                    }
                    if (g1 != null && g1.DiffuseTexture != -1) { GL.Uniform1(GL.GetUniformLocation(ShaderProgram, "DiffuseMap"), 0); }
                    if (g1 != null && g1.SpecularTexture != -1) { GL.Uniform1(GL.GetUniformLocation(ShaderProgram, "SpecularMap"), 1); }
                    if (g1 != null && g1.NormalTexture != -1) { GL.Uniform1(GL.GetUniformLocation(ShaderProgram, "NormalMap"), 2); }
                }
            }
            Program.error = GL.GetError();
            if (Program.error != ErrorCode.NoError) { throw new Exception(Program.error.ToString()); }
        }

    }
}
