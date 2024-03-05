using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace SampleApplication.OpenTK
{
    public class MrRobot
    {
        // Vertex array object, vertex buffer object, element buffer object
        public int VAO { get; set; }
        public int VBO { get; set; }
        public int EBO { get; set; }

        public Vector2 len;
        public Vector2 angle;

        // Basic line vertices and indices
        float[] verts;
        int[] indices;

        public MrRobot(Vector2 lenghts, Vector2 degrees) 
        {
            len = lenghts;
            angle = degrees;
            verts = new float[6] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f };
            indices = new int[2] { 0, 1 };
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

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        public void UpdateVAO()     // in this case propably Update will never be used
        {
            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        //public void Draw(Shader shader, Matrix4 view, Matrix4 perspective)
        //{
        //    shader.Use();
        //    shader.SetMatrix4("persp", perspective);
        //    shader.SetMatrix4("view", view);
        //    shader.SetMatrix4("model", Matrix4.CreateTranslation(0f, 0f, 0f));
        //    GL.BindVertexArray(VAO);
        //    GL.DrawElements(PrimitiveType.Lines, 2, DrawElementsType.UnsignedInt, 0);
        //}
    }
}
