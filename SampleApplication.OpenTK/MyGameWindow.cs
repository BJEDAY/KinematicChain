using System.Diagnostics;
using System.Drawing;
using DearImGui;
using DearImGui.OpenTK;
using DearImGui.OpenTK.Extensions;
using DearImPlot;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

//using Vector2 = System.Numerics.Vector2;
using Vector2 = OpenTK.Mathematics.Vector2;

namespace SampleApplication.OpenTK;

struct RobotSettings
{
    public float ArmLen1;
    public float ArmLen2;
    public float StartArmAngle1;
    public float StartArmAngle2;
    public float EndArmAngle1;
    public float EndArmAngle2;
    public bool AlternativeStart;
    public bool AlternativeEnd;
    public RobotSettings()
    {
        StartArmAngle1 = 0.0f;
        StartArmAngle2 = 0.0f;
        EndArmAngle1 = 0.0f;
        EndArmAngle2 = 0.0f;
        ArmLen1 = 0.0f;
        ArmLen2 = 0.0f;
        AlternativeStart = false;
        AlternativeEnd = false;
    }
}

struct SimulationSettings
{
    public bool EditMode;
    public bool PathFindingMode;
    public bool IsSimulating;
    public float SimulationTime;
    public float SimulationSpeed;
    public float Delta;

    public SimulationSettings()
    {
        EditMode = true; PathFindingMode = false;
        IsSimulating = false; SimulationTime = 0.0f;
        SimulationSpeed = 0.0f; Delta = 0.0f;
    }
}

struct ViewPerspectiveSettings
{
    public float fov, f, n;
    public ViewPerspectiveSettings(float Fov, float F, float N) { fov = Fov; f = F; n = N; }
}

internal sealed class MyGameWindow : GameWindowBaseWithDebugContext
{
    private static readonly double[] SampleData1 = Enumerable.Range(0, 256).Select(s => Math.Cos(s / 2.0d / Math.PI)).ToArray();

    private static readonly double[] SampleData2 = Enumerable.Range(0, 256).Select(s => Math.Sin(s / 2.0d / Math.PI)).ToArray();

    private readonly ImGuiController Controller;

    private readonly ImPlotContext ImPlotContext;

    private Color4 Color1 = Color4.Crimson;

    private Color4 Color2 = Color4.DeepSkyBlue;

    private bool ShowImGuiDemo = true;

    private bool ShowImPlotDemo = true;

    private bool ShowDockingDemo = true;

    // My data
    RobotSettings RobotSett = new RobotSettings();
    SimulationSettings SimulationSettings = new SimulationSettings();
    Shader shader; Camera camera; ViewPerspectiveSettings perspectiveSettings;
    MrRobot robot; Line testLine; List<Obstacle> obstacles;  Shader shader2D; Axis axis; bool CreatingObstacle;
    int ObstacleCount; ConfigurationSpace space; Shader TexViewer; Texture test_texture; TextureViewer texViewer; SimulationController SimulationController;
    bool EditMode; bool PathFindingMode;

    public MyGameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
        Controller = new ImGuiController(this, "Roboto-Regular.ttf", 20.0f);

        ImPlotContext = ImPlot.CreateContext();

        ImPlot.SetCurrentContext(ImPlotContext);

        ImPlot.SetImGuiContext(Controller.Context);

        SetupShaders();
        SetupCamera();
        SetupOpenGL();
        robot = new MrRobot(new Vector2(0.5f, 1.0f), new Vector2(MathHelper.DegreesToRadians(0), MathHelper.DegreesToRadians(0)));
        testLine = new Line();
        obstacles = new List<Obstacle>();
        //obstacles.Add(new Obstacle());
        axis = new Axis();
        space = new ConfigurationSpace();
        CreatingObstacle = false;
        ObstacleCount = 0;
        //test_texture = new Texture(1500,TextureUnit.Texture0);
        test_texture = new Texture(GenerateTexData(360),TextureUnit.Texture0);
        texViewer = new TextureViewer();
        SimulationController = new SimulationController(ref robot);
        EditMode = true;
        PathFindingMode = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Controller.Dispose();
            ImPlot.DestroyContext(ImPlotContext);
        }

        base.Dispose(disposing);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        SampleExtraFontImpl();

        Controller.Update((float)args.Time);
        SimulationController.deltaTime = (float)args.Time;
    }

    protected void SetupShaders()
    {
        // instead of using path "Shaders/ShaderVerts.glsl and using option "copy to output directory" the path is given directly to the source of shaders (every change gonna be instant)
        shader = new Shader("../../../Shaders/ShaderVert.glsl", "../../../Shaders/ShaderFrag.glsl");
        shader2D = new Shader("../../../Shaders/2DShaderVert.glsl", "../../../Shaders/2DShaderFrag.glsl");
        TexViewer = new Shader("../../../Shaders/TexViewerVert.glsl", "../../../Shaders/TexViewerFrag.glsl");
    }

    protected void SetupOpenGL()
    {
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.ProgramPointSize);
        //glDisable(GL_DEPTH_TEST);
        //glEnable(GL_BLEND); //Enable blending.
        //glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA); //Set blending function.
        //glEnable(GL_PROGRAM_POINT_SIZE);
    }
    protected Vector3[,] GenerateTexData(int size)
    {
        var res = new Vector3[size,size];
        for(int i=0; i<size; i++)
        {
            for(int j=0; j<size; j++)
            {
                if(j%2==0) res[i, j] = new Vector3(0f, 1f, 0f);
                else res[i, j] = new Vector3(0f,0f, 0f);    
            }
        }
        return res;
    }

    protected void SetupCamera()
    {
        //Camera initialization
        camera = new Camera();
        perspectiveSettings = new ViewPerspectiveSettings(45.0f, 30.0f, 0.5f);
        camera.UpdateProjectionMatrix((float)ClientSize.X, (float)ClientSize.Y, perspectiveSettings.fov, perspectiveSettings.n, perspectiveSettings.f);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.ClearColor(Color.CornflowerBlue);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 500), ImGuiCond.Once);


        if(ImGui.Begin("Settings"))
        {
            ImGui.Text("Select program working mode:");
            if(ImGui.Checkbox("Edit", ref EditMode))
            {
                //SimulationSettings.PathFindingMode = false;
                PathFindingMode = !EditMode;
            }
            ImGui.SameLine();
            if(ImGui.Checkbox("Path finding", ref PathFindingMode))
            {
                //SimulationSettings.EditMode = false;
                EditMode = !PathFindingMode;
            }

            ImGui.BeginDisabled(PathFindingMode);
            ImGui.Text("Edit options:");
            if (ImGui.TreeNode("Lenghts"))
            {
                ImGui.SliderFloat("Arm1 Len", ref robot.len.X, 0.0f, 10.0f); //{ robot.len.X = RobotSett.ArmLen1; }                
                ImGui.SliderFloat("Arm2 Len", ref robot.len.Y, 0.0f, 10.0f); //{ robot.len.Y = RobotSett.ArmLen2; }
                ImGui.TreePop();
            }
            if(ImGui.TreeNode("Start"))
            {
                ImGui.SliderAngle("Arm1 Angle", ref robot.angle.X);   // ref RobotSett.StartArmAngle1)) {  //robot.angle.X = RobotSett.StartArmAngle1; }
                ImGui.SliderAngle("Arm2 Angle", ref robot.angle.Y);  // { robot.angle.Y = RobotSett.StartArmAngle2; }
                ImGui.Checkbox("Alternative Start", ref robot.alt_angle);
                ImGui.TreePop();
            }
            if (ImGui.TreeNode("End"))
            {
                ImGui.SliderAngle("Arm1 Angle", ref robot.endAngle.X);
                ImGui.SliderAngle("Arm2 Angle", ref robot.endAngle.Y);
                ImGui.Checkbox("Alternative End", ref robot.end_alt_angle);
                ImGui.TreePop();
            }
            ImGui.EndDisabled();

            ImGui.BeginDisabled(EditMode);
            ImGui.Text("Path finding options:");
            if(ImGui.TreeNode("Path finding"))
            {
                
                if(ImGui.Button("Update configuration space"))
                {
                    // odpal funkcje która to ogarnie
                    space.UpdateSpace(obstacles, robot.len, robot.alt_angle, robot.angle,robot.endAngle);
                }
                ImGui.BeginDisabled(space.DisabledToFlood);
                if (ImGui.Button("Flood fill"))
                {
                    // odpal funkcje która to ogarnie
                    space.FloodFill(robot.angle, robot.endAngle);
                }
                ImGui.EndDisabled();
                ImGui.TreePop();
            }
            if(ImGui.TreeNode("Simulation"))
            {
                ImGui.Text("Simulation: "); ImGui.SameLine(); ImGui.Text(SimulationSettings.IsSimulating.ToString());
                ImGui.Text("Time: "); ImGui.SameLine(); ImGui.Text(SimulationSettings.SimulationTime.ToString());
                ImGui.BeginDisabled(space.DisabledToSimulate);
                if (ImGui.Button("Start")) 
                {
                    //SimulationController.TestInstance(new Vector2(0, MathHelper.DegreesToRadians(70)));
                    SimulationController.Start();
                    SimulationController.path = space.path;
                }
                ImGui.SameLine();
                if (ImGui.Button("Pause")) { SimulationController.pause = true; SimulationController.run = false; }
                ImGui.SameLine();
                if (ImGui.Button("Stop")) { SimulationController.Stop(); }
                ImGui.SliderFloat("Time", ref SimulationController.animationTime, 0.01f, 10.0f);
                ImGui.EndDisabled();
                //ImGui.SliderFloat("Speed", ref SimulationSettings.SimulationSpeed, 0.01f, 1.0f);
                ImGui.TreePop();
            }
            ImGui.EndDisabled();

            ImGui.BeginDisabled(PathFindingMode);
            ImGui.Text("Obstacles:");
            if(ImGui.Button("Check collision"))
                {
                if (obstacles.Count > 0)
                    {
                        bool FoundAny = false;
                        for(int i=0;  i<obstacles.Count; i++)
                            {
                                var check = space.CheckCollision(obstacles[i], robot);
                                if (check) 
                                    {
                                        FoundAny = true;
                                        Console.WriteLine($"Collision detected with obstacle {i+1}");
                                    }
                            } 
                        if ( !FoundAny ) Console.WriteLine($"Collision not detected");
                    } 
                else Console.WriteLine("There are no obstacles!");
                    
                }
            for (int i = 0; i < obstacles.Count; i++)
            {
                var currentObstacle = obstacles[i];
                //ImGui.Text($"{obstacles[i].Name}");

                if (ImGui.TreeNode($"{obstacles[i].Name}"))
                {
                    //ImGui.DragFloat2("Position", new Span<float>(new float[]{currentObstacle.pos.X, currentObstacle.pos.Y}) );
                    ImGui.DragFloat2("Position", currentObstacle.Position, 0.05f);
                    ImGui.DragFloat2("Size", currentObstacle.Size, 0.05f);
                    ImGui.DragFloat3("Color", currentObstacle.Color, 0.001f, 0.0f, 1.0f, "%.2f");
                    if (ImGui.Button("Delete"))
                    {
                        obstacles.RemoveAt(i);
                    } 
                    ImGui.TreePop();
                }
            }
            ImGui.EndDisabled();



            //if (ImGui.TreeNode("Obstacles"))
            //{
            //ImGui.TreePop();
            //}
        }


        ImGui.End();

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 420), ImGuiCond.Once);

        if (ImGui.Begin("Texture"))
        {
            //ImGui.Image((IntPtr)test_texture.Handle, new System.Numerics.Vector2(500, 500));
            ImGui.Image((IntPtr)space.spaceTex.Handle, new System.Numerics.Vector2(360, 360));
        }
        ImGui.End();

        //texViewer.Draw(TexViewer, test_texture);

        // Not working anymore...
        //GL.LineWidth(5);

        axis.Draw(shader2D, camera.viewMatrix, camera.projectionMatrix);

        
        //testLine.Draw(shader, camera.viewMatrix, camera.projectionMatrix);
        foreach(var obs in obstacles) { obs.Draw(shader, camera.viewMatrix, camera.projectionMatrix); }

        robot.Draw(shader, camera.viewMatrix, camera.projectionMatrix);

        SimulationController.Run();

        Controller.Render();

        

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        if(camera != null) { camera.UpdateProjectionMatrix((float)ClientSize.X, (float)ClientSize.Y, perspectiveSettings.fov, perspectiveSettings.n, perspectiveSettings.f); }
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    #region SampleDocking

    private ImGuiDockNodeFlags dockspace_flags = ImGuiDockNodeFlags.None;

    void DrawDockSpaceOptionsBar(ref bool p_open)
    {
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), dockspace_flags);
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Options"))
            {
                if (ImGui.MenuItem("Enable Docking", "IO.ConfigFlags.DockingEnable", ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.DockingEnable))) {
                    ImGui.GetIO().ConfigFlags ^= ImGuiConfigFlags.DockingEnable;
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Require Shift For Docking", "IO.ConfigDockingWithShift",
                    ImGui.GetIO().ConfigDockingWithShift)) { ImGui.GetIO().ConfigDockingWithShift = !ImGui.GetIO().ConfigDockingWithShift; }

                ImGui.Separator();

                if (ImGui.MenuItem("Flag: NoSplit", "", (dockspace_flags & ImGuiDockNodeFlags.NoSplit) != 0)) {  dockspace_flags ^= ImGuiDockNodeFlags.NoSplit; }
                if (ImGui.MenuItem("Flag: NoResize", "", (dockspace_flags & ImGuiDockNodeFlags.NoResize) != 0)) { dockspace_flags ^= ImGuiDockNodeFlags.NoResize; }
                if (ImGui.MenuItem("Flag: NoDockingInCentralNode", "", (dockspace_flags & ImGuiDockNodeFlags.NoDockingInCentralNode) != 0)) { dockspace_flags ^= ImGuiDockNodeFlags.NoDockingInCentralNode; }
                if (ImGui.MenuItem("Flag: AutoHideTabBar", "", (dockspace_flags & ImGuiDockNodeFlags.AutoHideTabBar) != 0)) { dockspace_flags ^= ImGuiDockNodeFlags.AutoHideTabBar; }
                if (ImGui.MenuItem("Flag: PassthruCentralNode", "", (dockspace_flags & ImGuiDockNodeFlags.PassthruCentralNode) != 0)) { dockspace_flags ^= ImGuiDockNodeFlags.PassthruCentralNode; }
                ImGui.EndMenu();
            }
            HelpMarker(
                @"When docking is enabled, you can ALWAYS dock MOST window into another! Try it now!
    
- Drag from window title bar or their tab to dock/undock.
    
- Drag from window menu button (upper-left button) to undock an entire node (all windows).
    
- Hold SHIFT to disable docking (if io.ConfigDockingWithShift == false, default)
    
- Hold SHIFT to enable docking (if io.ConfigDockingWithShift == true)");

            ImGui.EndMainMenuBar();
        }

        static void HelpMarker(string desc)
        {
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }

    

    #endregion

    #region SampleExtraFont

    private bool? SampleExtraFontFlag;

    private unsafe void SampleExtraFontImpl()
    {
        if (SampleExtraFontFlag is false)
            return;

        SampleExtraFontFlag = false;

        var io = ImGui.GetIO();

        var fonts = io.Fonts;

        var ranges = fonts.GlyphRangesDefault;

        var size = Controller.GetDpiScaledFontSize(12.0f);

        var font = fonts.AddFontFromFileTTF("Roboto-Regular.ttf", size, null, ref *ranges);

        Debug.WriteLine(font);

        fonts.Build();

        Controller.UpdateFontsTextureAtlas();
    }

    private void SampleExtraFontDemo()
    {
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 100), ImGuiCond.Once);

        if (ImGui.Begin("Sample: extra font"))
        {
            if (SampleExtraFontFlag is false)
            {
                ImGui.Text("Change font from Tools/Style Editor.");
            }
            else
            {
                if (ImGui.Button("Click to add an extra font"))
                {
                    SampleExtraFontFlag = true;
                }
            }
        }

        ImGui.End();
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);

       
        //if(this.MouseState.IsButtonPressed(MouseButton.Right))
        if (this.MouseState[MouseButton.Right] && (!this.KeyboardState[Keys.O]))
        {
            var delta = e.Delta.Y;
            camera.ChangeDistance((float)(delta * 0.01f));
        }

        if (this.MouseState[MouseButton.Right] && this.KeyboardState[Keys.O] && EditMode)
        {
            var spacePos = GetSpacePos();
            spacePos.Y = -spacePos.Y;
            if(CreatingObstacle)
            {
                EditCurrentObstacle(spacePos);
            }
            else
            {
                CreateObstacle(spacePos);
            }
        }

        if (this.MouseState[MouseButton.Middle] && this.KeyboardState[Keys.S] && EditMode)
        {
            //Console.WriteLine("Middle Man");
            var res = GetSpacePos();

            var (c1, c2) = InverseKinematic(res);
            //Console.WriteLine($"Pozycja w przestrzeni sceny: {res}");
            if (!float.IsNaN(c1.X) && !float.IsNaN(c2.X))
            {
                robot.angle.X = c1.X;
                robot.angle.Y = c1.Y;
                robot.alternative_angle.X = c2.X;
                robot.alternative_angle.Y = c2.Y;
            }
        }

        if (this.MouseState[MouseButton.Middle] && this.KeyboardState[Keys.E] && EditMode)
        {
            var res = GetSpacePos();

            var (c1, c2) = InverseKinematic(res);
            //Console.WriteLine($"Pozycja w przestrzeni sceny: {res}");
            if (!float.IsNaN(c1.X) && !float.IsNaN(c2.X))
            {
                robot.endAngle = c1;
                robot.endAlternativeAngle = c2;
            }
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
         
        if (e.Button == MouseButton.Right) 
        { 
            CreatingObstacle = false;
            //Console.WriteLine("PPM UP!");
        }
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        //if (this.MouseState[MouseButton.Left])
        //{
        //    //Console.WriteLine(this.MousePosition);
        //    // need to modify this value so in center it's 0,0 (need to subsract half of screen size X and Y)

        //    var res = GetSpacePos();
            
        //    var (c1,c2) = InverseKinematic(res);
        //    Console.WriteLine($"Pozycja w przestrzeni sceny: {res}");
        //    if(!float.IsNaN(c1.X)  && !float.IsNaN(c2.X))
        //    {
        //        robot.angle.X = c1.X;
        //        robot.angle.Y = c1.Y;
        //        robot.alternative_angle.X = c2.X;
        //        robot.alternative_angle.Y = c2.Y;
        //    }
        //}
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (this.KeyboardState[Keys.A])
        {
            robot.alt_angle = !robot.alt_angle;
        }

        if (this.KeyboardState[Keys.P])
        {
            obstacles.Add(new Obstacle(new(1.0f, 0.0f), new(1.0f, 1.0f)));
            
        }
    }
    protected Vector2 GetSpacePos()
    {
        var pos1 = new Vector2(MousePosition.X - ClientSize.X / 2, ClientSize.Y / 2 - MousePosition.Y);
        //Console.WriteLine($"Remaped values: {pos1}");

        var test_pos = new Vector4(0, 1, 0, 1);
        var test_screen_pos = test_pos * camera.viewMatrix * camera.projectionMatrix;
        test_screen_pos /= test_screen_pos.W;
        float deep = (float)test_screen_pos.Z;


        //test_screen_pos = test_screen_pos * camera.projectionMatrix.Inverted() * camera.viewMatrix.Inverted();
        //test_screen_pos /= test_screen_pos.W;


        //Console.WriteLine($"W przestrzeni NDC ekranu wektor (0,1,0) to {test_screen_pos.X}, {test_screen_pos.Y}");
        //Console.WriteLine($"Pozycja odczytana przez MousePosition to {MousePosition}");

        var screen_pos = MousePosition;
        // Step 1: Change position in pixels to NDC
        var NDC = new Vector2((2 * MousePosition.X / ClientSize.X - 1), -(2 * MousePosition.Y / ClientSize.Y - 1));
        //Console.WriteLine($"Pozycja NDC obliczona z MousePosition to {NDC}");
        // Step 2: Change screen NDC position to space position
        Vector4 pos = new Vector4(NDC.X, NDC.Y, deep, 1);
        pos = pos * camera.projectionMatrix.Inverted() * camera.viewMatrix.Inverted();
        pos.X /= pos.W;
        pos.Y /= pos.W;
        pos.Z /= pos.W;
        pos.W /= pos.W;
        Vector2 res = new(pos.X, -pos.Y);
        return res;
    }

    protected (Vector2 config, Vector2 alternativeConfig) InverseKinematic(Vector2 endPos)
    {
        Vector2 conf = new(0, 0);
        Vector2 altConf = new(0, 0);

        float l1 = robot.len.X;
        float l2 = robot.len.Y;
        float lsquared = endPos.X*endPos.X+endPos.Y*endPos.Y;

        var Beta = -Math.Atan2(endPos.Y, endPos.X);
        //Console.WriteLine($"Beta angle: {MathHelper.RadiansToDegrees(Beta)}");

        var Alfa2 = Math.Acos((lsquared - l1 * l1 - l2 * l2) / (-2 * l1 *l2));
        //Console.WriteLine($"Beta angle: {MathHelper.RadiansToDegrees(Alfa2)}");
        var MyAlfa2 = Math.PI - Alfa2;

        var Phi = Math.Acos((l2 *l2 - l1*l1 - lsquared)/(-2*l1*Math.Sqrt(lsquared)));
        //Console.WriteLine($"Phi angle: {MathHelper.RadiansToDegrees(Phi)}");

        var Alfa1 = Beta + Phi;
        var MyAlfa1 = Math.PI/2 - Alfa1;

        var SecondAlfa1 = Beta - Phi;
        var MySecondAlfa1 = Math.PI / 2 - SecondAlfa1;

        //Console.WriteLine($"MyAlfa1 angle: {MathHelper.RadiansToDegrees(MyAlfa1)}");
        //Console.WriteLine($"MyAlfa2 angle: {MathHelper.RadiansToDegrees(MyAlfa2)}");

        // angles are defined by integer values, so when angle in radians is not integer in degrees it must be changed
        //MyAlfa1 = MathHelper.DegreesToRadians((int)MathHelper.RadiansToDegrees(MyAlfa1));
        //MyAlfa2 = MathHelper.DegreesToRadians((int)MathHelper.RadiansToDegrees(MyAlfa2));
        //MySecondAlfa1 = MathHelper.DegreesToRadians((int)MathHelper.RadiansToDegrees(MySecondAlfa1));

        conf = new((float)MyAlfa1, (float)MyAlfa2);
        altConf = new((float)MySecondAlfa1, (float)-MyAlfa2);

        //conf = new((int)MathHelper.RadiansToDegrees(MyAlfa1), (float)MathHelper.RadiansToDegrees(MyAlfa2));
        //altConf = new((int)MathHelper.RadiansToDegrees(MySecondAlfa1), (int)MathHelper.RadiansToDegrees(- MyAlfa2));

        return (conf, altConf);
    }

    protected void CreateObstacle(Vector2 pos)          // creates new obstacle object and adds it on list (with size of 0)
    {
        CreatingObstacle = true;
        Obstacle obstacle = new Obstacle();
        obstacle.pos = pos;
        obstacle.size = new(0.0f, 0.0f);
        
        ObstacleCount ++;
        obstacle.Name = $"Obstacle {ObstacleCount}";
        obstacles.Add(obstacle);
    }

    protected void EditCurrentObstacle(Vector2 pos)     // current means last object on obstacles list
    {
        var currentObstacle = obstacles[obstacles.Count-1];
        var size = pos - currentObstacle.pos;
        currentObstacle.size = size;
    }

    #endregion
}



// NOT WORKING :) Don't know why.

//if(ImGui.BeginTabBar("TestTabBar",ImGuiTabBarFlags.None))
//{
//      if(ImGui.BeginTabItem("1"))
//        {
//            ImGui.SliderAngle("Arm1 Angle", ref Arm1Angle);
//            ImGui.EndTabItem();
//        }

//      if(ImGui.BeginTabItem("2"))
//        {
//            ImGui.SliderAngle("Arm1 Angle", ref Arm1Angle);
//            ImGui.EndTabItem();
//        }
//        ImGui.EndTabBar();
//}