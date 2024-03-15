using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;


namespace SampleApplication.OpenTK
{
    public class ConfigurationSpace
    {
        int size;
        private Vector3[,] colors;
        public Vector3 Background = new Vector3(0,0,0);
        public Texture spaceTex;
        public ConfigurationSpace() 
        {
            size = 360;
            colors = new Vector3[size, size];
            spaceTex = new Texture(colors, TextureUnit.Texture0);
        }

        public void UpdateSpace(List<Obstacle> obstacles, Vector2 len, bool alternative_angle)
        {
            MrRobot tester = new MrRobot(len, new Vector2(0, 0));
            tester.alt_angle = alternative_angle;

            for(int i=0;  i<360; i++)
            {
                for(int j=0; j<360; j++)
                {
                    tester.angle = new Vector2(MathHelper.DegreesToRadians(i), MathHelper.DegreesToRadians(j));
                    colors[i, j] = Background;
                    for (int k=0; k<obstacles.Count; k++) 
                    {
                        var res = CheckCollision(obstacles[k], tester);
                        if (res) colors[i, j] = obstacles[k].color;
                    }
                }
            }
            spaceTex.UpdateTexture(colors);
        }

        public bool CheckCollision(Obstacle obstacle, MrRobot robot) 
        {
            var Corners = obstacle.GetCorners();

            Vector2 end1 = robot.Arm1End;
            Vector2 end2 = robot.Arm2End;

            Vector2 line1 = GetLine(new Vector2(0, 0), end1);
            Vector2 line2 = GetLine(end1, end2);

           
            bool res1 =  CheckLineIntersection(line1,new(0,0),end1,Corners);
            bool res2 = CheckLineIntersection(line2, end1, end2, Corners);

            if (!res1 && !res2) { return false; }
            else return true;
        }

        public float GetIntersectionX(float a, float b, float yVal)
        {
            return (yVal - b) / a;
        }

        public float GetIntersectionY(float a, float b, float xVal)
        {
            return (a * xVal + b);
        }

        public Vector2 GetLine(Vector2 start, Vector2 end)
        {
            float a = (start.Y-end.Y) / (start.X-end.X);
            float b = start.Y - a * start.X;
            return new Vector2(a, b);
        }

        public bool CheckLineDomain(Vector2 start, Vector2 end, Vector2 pos)
        {
            float left = start.X < end.X ? start.X : end.X;
            float right = end.X > start.X ? end.X : start.X;
            float bottom = start.Y < end.Y ? start.Y : end.Y;
            float top = end.Y>start.Y? end.Y : start.Y;

            if (pos.X >= left && pos.X <= right && pos.Y >= bottom && pos.Y <= top) return true;
            else return false;
        }

        public bool CheckLineIntersection(Vector2 line,Vector2 start, Vector2 end, (Vector2 bottomLeft, Vector2 bottomRight, Vector2 topLeft, Vector2 topRight) Corners)
        {
            float yLeft = GetIntersectionY(line.X, line.Y, Corners.topLeft.X);
            float yRight = GetIntersectionY(line.X, line.Y, Corners.topRight.X);
            float xBottom = GetIntersectionX(line.X, line.Y, Corners.bottomLeft.Y);
            float xTop = GetIntersectionX(line.X, line.Y, Corners.topLeft.Y);

            bool FoundIntersection = false;

            if (yLeft >= Corners.bottomLeft.Y && yLeft <= Corners.topLeft.Y)
            {
                if (CheckLineDomain(start, end, new Vector2(Corners.topLeft.X, yLeft))) FoundIntersection = true;
            }
            if (yRight >= Corners.bottomLeft.Y && yRight <= Corners.topLeft.Y)
            {
                if (CheckLineDomain(start, end, new Vector2(Corners.topRight.X, yRight))) FoundIntersection = true;
            }
            if (xTop >= Corners.bottomLeft.X && xTop <= Corners.topRight.X)
            {
                if (CheckLineDomain(start, end, new Vector2(xTop, Corners.topRight.Y))) FoundIntersection = true;
            }
            if (xBottom >= Corners.bottomLeft.X && xBottom <= Corners.topRight.X)
            {
                if (CheckLineDomain(start, end, new Vector2(xBottom, Corners.bottomRight.Y))) FoundIntersection = true;
            }

            if (FoundIntersection) return true;


            return false;
        }
    }
}
