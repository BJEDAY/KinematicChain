using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace SampleApplication.OpenTK
{
    public class TextureViewer
    {
        // Vertex array object, vertex buffer object, element buffer object
        public int VAO { get; set; }
        public int VBO { get; set; }
        public int EBO { get; set; }

        // Basic viewer vertices and indices
        float[] verts;
        int[] indices;

        public TextureViewer()
        {
            // first two are 2D position (z is 0 inside shader) of rectangle displaying texture, second two are tex coords
            verts = new float[16] 
            { -1.0f, -1.0f, 0.0f, 0.0f, // bottom left - 0
              1.0f, -1.0f, 1.0f, 0.0f,  // bottom right - 1
              1.0f, 1.0f, 1.0f, 1.0f,   // top right - 2
              -1.0f,1.0f,0.0f,1.0f      // top left - 3
            };
            indices = new int[6] 
            { 
                0, 3, 1,  
                3, 2, 1
            };
            GenerateVAO();
        }

        public void GenerateVAO()
        {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.DynamicDraw);

            EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);


            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        public void UpdateVAO()     // in this case propably Update will never be used
        {
            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        public void Draw(Shader shader, Texture tex)
        {
            shader.Use();
            tex.Use();  //default tex unit is 0
            shader.SetInt("texture0", 0);
            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }
    }
}
