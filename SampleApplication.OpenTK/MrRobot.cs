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
        public Vector2 alternative_angle;
        public bool alt_angle;

        // Basic line vertices and indices
        float[] verts;
        int[] indices;

        public MrRobot(Vector2 lenghts, Vector2 degrees) 
        {
            len = lenghts;
            angle = degrees;
            alternative_angle = degrees;
            alt_angle = false;
            //verts = new float[6] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f };
            verts = new float[12] { -0.02f, 0f, 0f,      0.02f, 0.0f, 0.0f,       0.02f, 1.0f, 0.0f,       -0.02f, 1.0f, 0.0f };
            //indices = new int[2] { 0, 1 };
            indices = new int[6] { 0, 2, 1,     0,3,2 };
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

        public void Draw(Shader shader, Matrix4 view, Matrix4 perspective)
        {
            shader.Use();
            shader.SetMatrix4("persp", perspective);
            shader.SetMatrix4("view", view);
            var scale = Matrix4.CreateScale(1.0f, len.X, 1.0f);
            float deg = 0.0f;
            if(alt_angle) deg = -alternative_angle.X;
            else deg = -angle.X;
            var rot = Matrix4.CreateRotationZ(deg);
            shader.SetMatrix4("model", scale*rot);
            shader.SetVec3("color", new Vector3(0, 1, 1));
            GL.BindVertexArray(VAO);
            //GL.DrawElements(PrimitiveType.Lines, 2, DrawElementsType.UnsignedInt, 0);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            float deg2 = 0.0f;
            if (alt_angle) deg2 = -alternative_angle.Y;
            else deg2 = -angle.Y;
            var trans = Matrix4.CreateTranslation((float)(Math.Sin(-deg) * len.X), (float)(Math.Cos(-deg) * len.X), 0);
            rot = Matrix4.CreateRotationZ(deg2+deg);
            scale = Matrix4.CreateScale(1.0f, len.Y, 1.0f);
            shader.SetMatrix4("model", scale * rot*trans);
            shader.SetVec3("color", new Vector3(1, 1, 0));
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }
    }
}
