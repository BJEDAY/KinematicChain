using System.Diagnostics;
using System.Drawing;
using DearImGui;
using DearImGui.OpenTK;
using DearImGui.OpenTK.Extensions;
using DearImPlot;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
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
    MrRobot robot;

    public MyGameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
        Controller = new ImGuiController(this, "Roboto-Regular.ttf", 20.0f);

        ImPlotContext = ImPlot.CreateContext();

        ImPlot.SetCurrentContext(ImPlotContext);

        ImPlot.SetImGuiContext(Controller.Context);

        SetupShaders();
        SetupCamera();
        robot = new MrRobot(new Vector2(0.5f, 1.0f), new Vector2(MathHelper.DegreesToRadians(0), MathHelper.DegreesToRadians(0)));
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
    }

    protected void SetupShaders()
    {
        // instead of using path "Shaders/ShaderVerts.glsl and using option "copy to output directory" the path is given directly to the source of shaders (every change gonna be instant)
        shader = new Shader("../../../Shaders/ShaderVert.glsl", "../../../Shaders/ShaderFrag.glsl");
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

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 500), ImGuiCond.Once);


        if(ImGui.Begin("Settings"))
        {
            ImGui.Text("Select program working mode:");
            if(ImGui.Checkbox("Edit", ref SimulationSettings.EditMode))
            {
                SimulationSettings.PathFindingMode = false;
            }
            ImGui.SameLine();
            if(ImGui.Checkbox("Path finding", ref SimulationSettings.PathFindingMode))
            {
                SimulationSettings.EditMode = false;
            }

            ImGui.Text("Edit options:");
            if (ImGui.TreeNode("Lenghts"))
            {
                if(ImGui.SliderFloat("Arm1 Len", ref RobotSett.ArmLen1, 0.0f, 10.0f)) { robot.len.X = RobotSett.ArmLen1; }                
                if(ImGui.SliderFloat("Arm2 Len", ref RobotSett.ArmLen2, 0.0f, 10.0f)) { robot.len.Y = RobotSett.ArmLen2; }
                ImGui.TreePop();
            }
            if(ImGui.TreeNode("Start"))
            {
                if(ImGui.SliderAngle("Arm1 Angle", ref RobotSett.StartArmAngle1)) {  robot.angle.X = RobotSett.StartArmAngle1; }
                if(ImGui.SliderAngle("Arm2 Angle", ref RobotSett.StartArmAngle2)) { robot.angle.Y = RobotSett.StartArmAngle2; }
                ImGui.Checkbox("Alternative Start", ref RobotSett.AlternativeStart);
                ImGui.TreePop();
            }
            if (ImGui.TreeNode("End"))
            {
                ImGui.SliderAngle("Arm1 Angle", ref RobotSett.EndArmAngle1);
                ImGui.SliderAngle("Arm2 Angle", ref RobotSett.EndArmAngle2);
                ImGui.Checkbox("Alternative Start", ref RobotSett.AlternativeEnd);
                ImGui.TreePop();
            }

            ImGui.Text("Path finding options:");
            if(ImGui.TreeNode("Path finding"))
            {
                if(ImGui.Button("Update configuration space"))
                {
                    // odpal funkcje która to ogarnie
                }
                if(ImGui.Button("Flood fill"))
                {
                    // odpal funkcje która to ogarnie
                }
                ImGui.TreePop();
            }
            if(ImGui.TreeNode("Simulation"))
            {
                ImGui.Text("Simulation: "); ImGui.SameLine(); ImGui.Text(SimulationSettings.IsSimulating.ToString());
                ImGui.Text("Time: "); ImGui.SameLine(); ImGui.Text(SimulationSettings.SimulationTime.ToString());
                if (ImGui.Button("Start")) { }
                ImGui.SameLine();
                if (ImGui.Button("Pause")) { }
                ImGui.SameLine();
                if (ImGui.Button("Stop")) { }
                ImGui.SliderFloat("Delta", ref SimulationSettings.Delta, 0.01f, 1.0f);
                ImGui.SliderFloat("Speed", ref SimulationSettings.SimulationSpeed, 0.01f, 1.0f);
                ImGui.TreePop();
            }
        }


        ImGui.End();

        // Not working anymore...
        //GL.LineWidth(5);
        robot.Draw(shader,camera.viewMatrix,camera.projectionMatrix);

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