using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector2 = OpenTK.Mathematics.Vector2;

namespace SampleApplication.OpenTK
{
    public class SimulationController
    {
        public bool run, pause, stop;
        public float deltaTime;
        float currentTime;
        public float animationTime;
        MrRobot robot;
        public List<Vector2> path;
        public bool endNextFrame;
        public SimulationController(ref MrRobot instance)
        {
            run = false;
            pause = false;
            stop = false;
            deltaTime = 0;
            robot = instance;
            animationTime = 1;
            currentTime = 0;
            endNextFrame = false;
        }

        public void TestInstance(Vector2 newAngles)
        {
            robot.angle = newAngles;
        }

        public void Start()
        {
            if (!pause) currentTime = 0;
            run =true;
            pause = false;
            stop =false;
            robot.animate = true;
        }

        public void Stop()
        {
            run =false;
            pause = false;
            stop = true;
            currentTime = 0;
            robot.animate = false;
        }


        public void Run()
        {
            if(run)
            {
                if(endNextFrame) 
                {
                    run = false;
                    endNextFrame=false;
                    //robot.animate = false;
                }

                currentTime += deltaTime;
                if (currentTime >= animationTime) 
                {
                    currentTime = animationTime;
                    endNextFrame =true;
                }
                
                int size = path.Count;
                int element =(int)((currentTime / animationTime)*(size-1));
                robot.currentFrame = path[element];

            }

        }
    }
}
