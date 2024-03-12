using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SampleApplication.OpenTK
{
    public class Obstacle
    {
        // Vertex array object, vertex buffer object, element buffer object
        public int VAO { get; set; }
        public int VBO { get; set; }
        public int EBO { get; set; }

        public string Name;
        public Vector2 pos;
        public Vector2 size;
        public Vector3 color;


        public Span<float> Position
        {
            get {
                    return MemoryMarshal.CreateSpan(ref pos.X,2);
                }
        }

        public Span<float> Size
        {
            get
            {
                return MemoryMarshal.CreateSpan(ref size.X, 2);
            }
        }

        public Span<float> Color
        {
            get
            {
                return MemoryMarshal.CreateSpan(ref color.X, 3);
            }
        }


        // Basic line vertices and indices
        float[] verts;
        int[] indices;

        public Obstacle(Vector2 p, Vector2 s)
        {
            pos = p;
            size = s;
            Random rand = new Random();
            color = new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
            //verts = new float[12] { -1.0f, -1.0f, 0.0f, 
            //                        1.0f, -1.0f, 0.0f, 
            //                        1.0f, 1.0f, 0.0f,                    
            //                        -1.0f, 1.0f, 0.0f};

            verts = new float[12] { 0.0f, 0.0f, 0.0f,
                                    1.0f, 0.0f, 0.0f,
                                    1.0f, 1.0f, 0.0f,
                                    0.0f, 1.0f, 0.0f};


            //indices = new int[6] { 0, 3, 1, 
            //                       3, 2, 1 };
            indices = new int[6] { 0, 3, 1,
                                   3, 2, 1 };
            GenerateVAO();
        }

        public Obstacle() : this(new Vector2(1.0f,1.0f), new Vector2(1.0f, 1.0f)) { }

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

            // after setting up all data change it to default
            //GL.BindVertexArray(0);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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
            var t = Matrix4.CreateTranslation(pos.X,pos.Y,0.0f);
            var s = Matrix4.CreateScale(size.X, size.Y,1.0f);
            shader.SetMatrix4("model", s*t);
            shader.SetVec3("color", color);
            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            // after setting up all data change it to default
            //GL.BindVertexArray(0);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
