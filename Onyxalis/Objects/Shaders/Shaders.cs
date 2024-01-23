using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Shaders
{
    public class Shaders
    {
        public enum Shader
        {
            LineOfSight
        }
        public static void SetLineOfSightShaderParameters(Effect lineOfSightShader, Vector2 start, Vector2 end, Texture2D worldTexture, float intensity)
        {
            lineOfSightShader.Parameters["u_Start"].SetValue(start);
            lineOfSightShader.Parameters["u_End"].SetValue(end);
            lineOfSightShader.Parameters["u_WorldTexture"].SetValue(worldTexture);
            lineOfSightShader.Parameters["u_Intensity"].SetValue(intensity);
        }
    }
}
