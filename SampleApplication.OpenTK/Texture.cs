using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


namespace SampleApplication.OpenTK
{
    public class Texture
    {
        public int Handle;
        TextureUnit Unit;
        
        // default constructor to generate 2D texture with random bytes
        public Texture(int size, TextureUnit unit) 
        {
            Unit = unit;
            Handle = GL.GenTexture();
            GL.ActiveTexture(Unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, size, size, 0, PixelFormat.Rgba, PixelType.UnsignedByte, GenerateRandom2DBytes(size));
        }

        public Texture(float[,] data, TextureUnit unit)
        {
            Unit = unit;
            Handle = GL.GenTexture();
            GL.ActiveTexture(Unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, data.GetLength(0), data.GetLength(1), 0, PixelFormat.Red, PixelType.Float, data);
        }

        public Texture(Vector3[,] data, TextureUnit unit) 
        {
            Unit = unit;
            Handle = GL.GenTexture();
            GL.ActiveTexture(Unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.GetLength(0), data.GetLength(1), 0, PixelFormat.Rgb, PixelType.Float, data);
        }

        public void Use()
        {
            GL.ActiveTexture(Unit);
            GL.BindTexture(TextureTarget.Texture2D,Handle);
        }

        public void ChangeUnit(TextureUnit unit) 
        {
            Unit = unit;
        }

        private byte[] GenerateRandom2DBytes(int size)
        {
            var rand = new Random();
            var res = new byte[4 * (int)Math.Pow(size, 2)];
            rand.NextBytes(res);
            return res;
        }

        public void UpdateTexture(Vector3[,] data)
        {
            GL.ActiveTexture(Unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.GetLength(0), data.GetLength(1), 0, PixelFormat.Rgb, PixelType.Float, data);
        }
    }
}
