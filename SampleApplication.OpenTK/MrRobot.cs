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


        // so the simulation gonna work in diffrent thread and it's gonna be made inside MyGameWindow, robot object gonna only get currentFrame nad display it (if animation is on)
        public Vector2 currentFrame;
        public bool animate;

        public bool end_alt_angle;
        public Vector2 endAngle;
        public Vector2 endAlternativeAngle;

        // Basic line vertices and indices
        float[] verts;
        int[] indices;

        public MrRobot(Vector2 lenghts, Vector2 degrees) 
        {
            len = lenghts;
            angle = degrees;
            alternative_angle = degrees;
            alt_angle = false;
            end_alt_angle = false;
            endAngle = degrees;
            endAlternativeAngle = degrees;
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

            //// after setting up all data change it to default
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

        public Vector2 Arm1End
        {
            get
            {
                float deg = 0.0f;
                if (alt_angle) deg = alternative_angle.X;
                else deg = angle.X;
                return new Vector2((float)(Math.Sin(deg) * len.X), (float)(Math.Cos(deg) * len.X));
            }
        }

        public Vector2 Arm2End
        {
            get
            {
                float deg = 0.0f;
                if (alt_angle) deg = alternative_angle.X;
                else deg = angle.X;

                float deg2 = 0.0f;
                if (alt_angle) deg2 = alternative_angle.Y;
                else deg2 = angle.Y;

                Vector2 end1 = new Vector2((float)(Math.Sin(deg) * len.X), (float)(Math.Cos(deg) * len.X));
                Vector2 vec = new Vector2((float)(Math.Sin(deg+deg2) * len.Y), (float)(Math.Cos(deg+deg2) * len.Y));
                Vector2 end2 = end1 + vec;

                return end2;
            }
        }

        public void Draw(Shader shader, Matrix4 view, Matrix4 perspective)
        {
            shader.Use();
            shader.SetMatrix4("persp", perspective);
            shader.SetMatrix4("view", view);


            shader.SetFloat("opacity", 1.0f);
            //if (alt_angle) Render(shader, new Vector2(MathHelper.DegreesToRadians(alternative_angle.X), MathHelper.DegreesToRadians(alternative_angle.Y)));
            //else Render(shader, new Vector2(MathHelper.DegreesToRadians(angle.X), MathHelper.DegreesToRadians(angle.Y)));
            if (alt_angle) Render(shader, alternative_angle);
            else Render(shader,angle);

            shader.SetFloat("opacity", 0.5f);
            //if (end_alt_angle) Render(shader, new Vector2(MathHelper.DegreesToRadians(endAlternativeAngle.X),MathHelper.DegreesToRadians(endAlternativeAngle.Y)));
            //else Render(shader, new Vector2(MathHelper.DegreesToRadians(endAngle.X), MathHelper.DegreesToRadians(endAngle.Y)));

            if (end_alt_angle) Render(shader,endAlternativeAngle);
            else Render(shader, endAngle);

            if (animate)
            {
                shader.SetFloat("opacity", 1.0f);
                Render(shader, new Vector2(MathHelper.DegreesToRadians(currentFrame.X), MathHelper.DegreesToRadians(currentFrame.Y)));
            }

            //// after setting up all data change it to default
            //GL.BindVertexArray(0);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Render(Shader shader, Vector2 angles)
        {
            var scale = Matrix4.CreateScale(1.0f, len.X, 1.0f);

            var deg = -angles.X;
            var rot = Matrix4.CreateRotationZ(deg);
            shader.SetMatrix4("model", scale * rot);
            shader.SetVec3("color", new Vector3(0, 1, 1));
            GL.BindVertexArray(VAO);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            var deg2 = -angles.Y;
            var trans = Matrix4.CreateTranslation((float)(Math.Sin(-deg) * len.X), (float)(Math.Cos(-deg) * len.X), 0);
            rot = Matrix4.CreateRotationZ(deg2 + deg);
            scale = Matrix4.CreateScale(1.0f, len.Y, 1.0f);
            shader.SetMatrix4("model", scale * rot * trans);
            shader.SetVec3("color", new Vector3(1, 1, 0));
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

    }
}
