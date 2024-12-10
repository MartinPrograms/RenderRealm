using System.Numerics;
using RenderRealm.Graphics.Shaders;
using ShaderGen;

namespace RenderRealm.Shaders
{
    [Obsolete("This is a test shader, it is not used in the project. I have concluded that ShaderGen is not the way to go for this project. All source will remain, as it might be of interest to some.")]
    public class TestShader
    {
        public float u_time;
        public Vector2 u_resolution;
        public float u_aspect;
        
        public struct VertexInput
        {
            [PositionSemantic]
            public Vector3 Position;
            [TextureCoordinateSemantic]
            public Vector2 TexCoord;
        }
    
        public struct FragmentInput
        {
            [SystemPositionSemantic]
            public Vector4 Position;
            [TextureCoordinateSemantic]
            public Vector2 TexCoord;
        }
    
        [VertexShader]
        public FragmentInput VS(VertexInput input)
        {
            FragmentInput output;
            output.Position = new Vector4(input.Position, 1);
            output.TexCoord = input.TexCoord;
            return output;
        }
    
        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            var uv = input.TexCoord;
            uv.Y *= u_aspect;
            // UV is from top left, so to get it in the center we need to do this
            uv *= 2;
            uv -= Vector2.One;
            
            var dist = Vector2.Distance(uv, Vector2.Zero);
            var color = Vector4.One; 
            if (dist < 0.5)
            {
                color = Vector4.One;
            }
            else
            {
                color = Vector4.Zero;
            }
            
            return new Vector4(uv,0, 1);
            
        }
    }
}