using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace ProjectCobalt
{
    class Lighting
    {
        public static List<Light> LightList = new List<Light>();
        public class Light
        {
            public Vector3 Position;
            public Vector3 Direction;
            public Vector3 AttenuationFactor;
            public Vector4 Color;
            public float Cutoff;
            public float Angle;
            public int SpecularSize;
            public float MinimumAmbient;
            int FBO;
            int ShadowMap;

            public Light(Vector3 position, Vector3 attenuationFactor, Vector4 color, Vector3 direction, float angle, int specularsize, float minimumambient)
            {
                Position = position;
                AttenuationFactor = attenuationFactor;
                Color = color;
                Direction = direction;
                Angle = angle;
                SpecularSize = specularsize;
                MinimumAmbient = minimumambient;


            }
        }


    }
}
