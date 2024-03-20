using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;


namespace SampleApplication.OpenTK
{
    public class ConfigurationSpace
    {
        int size;
        private Vector3[,] colors;
        private float[,] distance;
        public List<Vector2> path;
        public Vector3 Background = new Vector3(0,0,0);
        public Vector3 StartConfigColor = new Vector3(255,0,0);
        public Vector3 EndConfigColor = new Vector3(0, 255, 0);
        public Texture spaceTex;
        public ConfigurationSpace() 
        {
            size = 360;
            colors = new Vector3[size, size];
            spaceTex = new Texture(colors, TextureUnit.Texture0);
            distance = new float[size, size];
            path = new();
        }

        public void UpdateSpace(List<Obstacle> obstacles, Vector2 len, bool alternative_angle, Vector2 startConfig, Vector2 endConfig)
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

            var (alf, beta) = GetCorrectAngles(startConfig);
            colors[alf, beta] = StartConfigColor;

            (alf,beta) = GetCorrectAngles(endConfig);
            colors[alf, beta] = EndConfigColor;

            spaceTex.UpdateTexture(colors);
        }

        public void FloodFill(Vector2 startConfig, Vector2 endConfig)
        {
            var startAngles = GetCorrectAngles2(startConfig);
            var endAngles = GetCorrectAngles2(endConfig);

            for(int i=0; i<360;i++)
            {
                for(int j=0; j<360;j++)
                {
                    if (colors[i, j] == Background || colors[i,j] == StartConfigColor || colors[i,j] == EndConfigColor) distance[i, j] = -1; // jak nie ma przeszkody to -1
                    else distance[i, j] = -2;   // jak jest tam przeszkoda to -2 
                }
            }

            //Stack<Tuple<Vector2,int>> stack = new Stack<Tuple<Vector2,int>>();
            //stack.Push(Tuple.Create(startConfig, 0)); 

            // stack holds info about angles and distance from starting config (one move to upper, lower, left or right position on space tex increase value of distance by 1)
            //Stack<(Vector2 angles, int dist)> stack = new Stack<(Vector2, int)>();
            //stack.Push((startAngles, 0));

            Queue<(Vector2 angles, int dist)> queue = new Queue<(Vector2, int dist)> ();
            queue.Enqueue((startAngles,0));
            int maxDist = 0;

            //int currentX=0, currentY=0;

            while(queue.Count>0)
            {
                // generalnie w skrócie zdejmuję aktualny element, sprawdzam czy się mieście w dziedzinie i czy nie ma tam przeszkody, a jak tak to dodaję info o odległości do tego elementu w distance
                // czyli w skrócie cały ten stos nie dość że bada czy w ogóle można gdzieś dotrzeć to do tego aktualizuje informację o odległości (za pomoca której potem będzie można się cofnąć z końcą do poczatku)
                //var currentElem = stack.Pop();
                var currentElem = queue.Dequeue();

                // check czy nie wyszliśmy poza 'teksturkę'
                if (!AngleDomain(currentElem.angles)) continue;

                // check czy tam jest przeszkoda lub już algorytm tam wpisał dane
                if (distance[(int)currentElem.angles.X, (int)currentElem.angles.Y] != -1) continue;

                // jeśli rzeczywiście nie jesteśmy poza dziedziną i trafiliśmyn na pole gdzie info jeszcze nie zostało zapisane to trzeba to naprawić 
                //currentX = (int)currentElem.angles.X;
                //currentY = (int)currentElem.angles.Y;
                distance[(int)currentElem.angles.X, (int)currentElem.angles.Y] = currentElem.dist;
                //break;
                if(currentElem.dist > maxDist)  maxDist = currentElem.dist;

                // jak dotarliśmy flood fillem do pozycji końcowej to finito, wiecęj info nie potrzebne
                if (currentElem.angles == endAngles) break;

                //if (maxDist > 1500) break;

                //stack.Push((WrapVector2(currentElem.angles + new Vector2(0, 1), size), currentElem.dist + 1));
                //stack.Push((WrapVector2(currentElem.angles + new Vector2(0, -1), size), currentElem.dist + 1));
                //stack.Push((WrapVector2(currentElem.angles + new Vector2(1, 0), size), currentElem.dist + 1));
                //stack.Push((WrapVector2(currentElem.angles + new Vector2(-1, 0), size), currentElem.dist + 1));



                //stack.Push((currentElem.angles + new Vector2(1, 0), currentElem.dist + 1));
                //stack.Push((currentElem.angles + new Vector2(-1, 0), currentElem.dist + 1));
                //stack.Push((currentElem.angles + new Vector2(0, 1), currentElem.dist + 1));
                //stack.Push((currentElem.angles + new Vector2(0, -1), currentElem.dist + 1));

                queue.Enqueue((WrapVector2(currentElem.angles + new Vector2(1, 0),360), currentElem.dist + 1));
                queue.Enqueue((WrapVector2(currentElem.angles + new Vector2(-1, 0), 360), currentElem.dist + 1));
                queue.Enqueue((WrapVector2(currentElem.angles + new Vector2(0, 1), 360), currentElem.dist + 1));
                queue.Enqueue((WrapVector2(currentElem.angles + new Vector2(0, -1), 360), currentElem.dist + 1));


                //queue.Enqueue((currentElem.angles + new Vector2(1, 0), currentElem.dist + 1));
                //queue.Enqueue((currentElem.angles + new Vector2(-1, 0), currentElem.dist + 1));
                //queue.Enqueue((currentElem.angles + new Vector2(0, 1), currentElem.dist + 1));
                //queue.Enqueue((currentElem.angles + new Vector2(0, -1), currentElem.dist + 1));

            }

            //var value = distance[currentX, currentY];

            //if (distance[currentX,currentY] == 0)
            //{
            //    colors[currentX,currentY] = new Vector3(0, 255, 0); 
            //}

            for(int i=0; i<size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (distance[i, j] == -1 || distance[i,j] == -2) continue; // w trakcie robienia flood filla udało się dojśc do endConfig zanim do tego pola więc tu nie ma info (zostanie zwykły background)
                    var currentColorValue = 1.0f-(distance[i, j] /(float)maxDist);
                    //if (distance[i, j] == 0)
                    //{
                    //    colors[i, j] = new Vector3(0, 255, 0);
                    //}
                        
                    colors[i, j] = new Vector3(currentColorValue);
                }
            }

            FindPath(endAngles);

            spaceTex.UpdateTexture(colors);
        }

        public void FindPath(Vector2 endAngles)
        {
            // 1. Starts at the endConfig
            // 2. Search for element to the left,right,up or bottom thats distance value is one less (and add it as next path value)
            // 3. Do step 2 unless value of distance is 0 (path from endConfig do startConfig is found)
            path.Clear();
            path.Add(endAngles);

            int currentDistance = (int)distance[(int)endAngles.X, (int)endAngles.Y];

            while(currentDistance>0)
            {
                Vector2 right = new Vector2((int)Wrap(path.Last().X + 1, 360), (int)Wrap(path.Last().Y, 360));
                Vector2 left = new Vector2((int)Wrap(path.Last().X - 1, 360), (int)Wrap(path.Last().Y, 360));
                Vector2 up = new Vector2((int)Wrap(path.Last().X, 360), (int)Wrap(path.Last().Y + 1, 360));
                Vector2 bottom = new Vector2((int)Wrap(path.Last().X, 360), (int)Wrap(path.Last().Y - 1, 360));
                // searching for any neighbour with distance one lower (it' don't matters if there are multiple neightbours with one lower value which algorithm gonna choose, at the end it's gonna travel to startConfig)
                if (distance[(int)right.X,(int)right.Y] == currentDistance-1)
                {
                    path.Add(right);
                }
                else if(distance[(int)left.X, (int)left.Y] == currentDistance - 1)
                {
                    path.Add(left);
                }
                else if(distance[(int)up.X, (int)up.Y] == currentDistance - 1)
                {
                    path.Add(up);
                }
                else if(distance[(int)bottom.X, (int)bottom.Y] == currentDistance - 1)
                {
                    path.Add(bottom);
                }

                currentDistance--;
            }

            path.Reverse();
            var pathColor = new Vector3(255, 255, 0);

            foreach(var elem in path)
            {
                colors[(int)elem.X, (int)elem.Y] = pathColor;
            }
        }

        public Vector2 WrapVector2(Vector2 val, float max)
        {
            var a = (val.X < 0 ? val.X + max : val.X % max);
            var b = (val.Y < 0 ? val.Y + max : val.Y % max);
            return new Vector2(a, b);
        }

        public float Wrap(float val, float max)
        {
            var a = (val < 0 ? val + max : val % max);
            return a;
        }
        protected bool AngleDomain(Vector2 angles)
        {
            if(angles.X >=0 && angles.X < size && angles.Y >=0 && angles.Y < size) return true;
            else return false;
        }

        public (int a, int b) GetCorrectAngles(Vector2 config)
        {
            var alf = MathHelper.RadiansToDegrees(config.X);
            var bet = MathHelper.RadiansToDegrees(config.Y);
            //var alf = config.X;
            //var bet = config.Y;
            alf = alf > 0 ? alf : 360 + alf;
            bet = bet > 0 ? bet : 360 + bet;
            alf = alf == 360 ? 0 : alf;
            bet = bet == 360 ? 0 : bet;
            return ((int)alf, (int)bet);
        }

        public Vector2 GetCorrectAngles2(Vector2 config)
        {
            var alf = MathHelper.RadiansToDegrees(config.X);
            var bet = MathHelper.RadiansToDegrees(config.Y);
            //var alf = config.X;
            //var bet = config.Y;
            alf = alf > 0 ? alf : 360 + alf;
            bet = bet > 0 ? bet : 360 + bet;
            alf = alf == 360 ? 0 : alf;
            bet = bet == 360 ? 0 : bet;
            return new Vector2((int)alf, (int)bet);
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


        public Vector2 GetLine(Vector2 start, Vector2 end)
        {
            var rangeX = start.X - end.X;
            var rangeY = start.Y - end.Y;
            float a, b;
            if (Math.Abs(rangeX) < 0.01f)   // vertical line
            {
                a = float.MaxValue;
                b = start.X;  // b now hold info about position x of line 
            }
            else
            {
                a = (start.Y - end.Y) / (start.X - end.X);
                b = start.Y - a * start.X;
            }
            
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
            float xBottom = 0.0f;
            float xTop = 0.0f;
            float yLeft = 0.0f;
            float yRight = 0.0f;

            var GetInterX = (float a, float b, float yVal) =>
            {
                return (yVal - b) / a;
            };

            var GetInterY = (float a, float b, float xVal) =>
            {
                return (a * xVal + b);
            };

            if (line.X != float.MaxValue)
            {
                xBottom = GetInterX(line.X, line.Y, Corners.bottomLeft.Y);
                xTop = GetInterX(line.X, line.Y, Corners.topLeft.Y);

                yLeft = GetInterY(line.X, line.Y, Corners.topLeft.X);
                yRight = GetInterY(line.X, line.Y, Corners.topRight.X);
            }
            else
            {
                xBottom = line.Y;   // when case of vertical line is detected then info about x pos of line is hold inside b
                xTop = line.Y;
            }

            bool FoundIntersection = false;

            if(line.X != float.MaxValue) // if line is vertical then lowest y is the bottom of obstacle and highest the top of obstacle so CheckLineDomain will do the job
            {
                if (yLeft >= Corners.bottomLeft.Y && yLeft <= Corners.topLeft.Y)
                {
                    if (CheckLineDomain(start, end, new Vector2(Corners.topLeft.X, yLeft))) FoundIntersection = true;
                }
                if (yRight >= Corners.bottomLeft.Y && yRight <= Corners.topLeft.Y)
                {
                    if (CheckLineDomain(start, end, new Vector2(Corners.topRight.X, yRight))) FoundIntersection = true;
                }

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


//public float GetIntersectionX(float a, float b, float yVal)
//{
//    return (yVal - b) / a;
//}

//public float GetIntersectionY(float a, float b, float xVal)
//{
//    return (a * xVal + b);
//}