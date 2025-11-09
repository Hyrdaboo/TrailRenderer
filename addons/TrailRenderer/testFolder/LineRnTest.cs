using Godot;
using System;
using System.Collections.Generic;


public partial class LineRnTest : Node3D
{
    [Export] private Path3D path;
    [Export] private LineRenderer lr;
    [Export] private int resolution = 9;

    private Vector3[] conePoints;

    public static Vector3 SampleQuadraticMultiple(Vector3[] points, float t)
    {
        if (points.Length < 3)
            return Vector3.Zero;

        List<Vector3> curvePoints = new List<Vector3>(points);

        int i = 0;
        while (curvePoints.Count > 1)
        {
            curvePoints[i] = curvePoints[i].Lerp(curvePoints[i+1], t);
            i++;

            if (i == curvePoints.Count - 1)
            {
                i = 0;
                curvePoints.RemoveAt(curvePoints.Count - 1);
            }
        }

        return curvePoints[0];
    }

    

    int prevResolution = -1;
    float t;
    float px = -4;
    Vector2 smoothDir;
    public override void _Process(double delta)
    {
        /*lr.Points.Clear();
        lr.Points.Add(new LineRenderer.Point(new Vector3(px, 0, 0)));
        lr.Points.Add(new LineRenderer.Point(new Vector3(0, 0, 0)));
        lr.Points.Add(new LineRenderer.Point(new Vector3(1, 0, 0)));
        lr.Points.Add(new LineRenderer.Point(new Vector3(2, 1.0f, 0)));

        if (Input.IsKeyPressed(Key.A))
            px += (float)delta * 2;
        if (Input.IsKeyPressed(Key.D))
            px -= (float)delta * 2;*/


        lr.Points.Clear();
        float radius = 5.0f;
        float angleStep = 2.0f * Mathf.Pi / resolution;
        for (int i = 0; i < resolution; i++)
        {
            float x = radius * Mathf.Cos(angleStep * i);
            float y = radius * Mathf.Sin(angleStep * i);
            lr.Points.Add(new LineRenderer.Point(new Vector3(x, y, 0)));
        }


        /*Vector2 mousePosition = (GetViewport().GetMousePosition() / GetViewport().GetVisibleRect().Size) * Vector2.One * 2 - Vector2.One;
        Vector2 dirToMouse = Vector2.Zero.DirectionTo(mousePosition).Normalized();
        Vector2 desiredDir = Vector2.Up;
        float angleLimitDegrees = 15.0f;
        Vector2 limitLeft = desiredDir.Rotated(Mathf.DegToRad(-angleLimitDegrees));
        Vector2 limitRight = desiredDir.Rotated(Mathf.DegToRad(angleLimitDegrees));

        float angleToTarget = Mathf.RadToDeg(dirToMouse.AngleTo(desiredDir));
        if (angleToTarget < angleLimitDegrees && angleToTarget >= 0)
            dirToMouse = limitLeft;
        else if (angleToTarget > -angleLimitDegrees && angleToTarget < 0)
            dirToMouse = limitRight;
        
        smoothDir = smoothDir.Slerp(dirToMouse, 0.1f);
        
        Vector3 toCamPos(Vector2 p)
        {
            Camera3D cam = GetViewport().GetCamera3D();
            Vector3 localPos = new Vector3(p.X, -p.Y, -3.0f);
            localPos = cam.Transform * localPos;
            
            return localPos;
        }*/

        Vector3[] points =
        [
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(1.0f, 0.0f, -1.0f),
            new Vector3(1.5f, 0.0f, 0.5f),
            new Vector3(2.5f, 0.0f, -1.5f)
        ];

        int res = 16;
        //int res = points.Length;
        Vector3[] path = new Vector3[res];

        for (int i = 0; i < res; i++)
        {
            //path[i] = SampleQuadraticBezier(points[0], points[2], points[1], i / (float)(res - 1), out _);
            path[i] = SampleQuadraticMultiple(points, i / (float)(res - 1));
        }

        /*DateTime time = DateTime.Now;
        Vector3[] path = ChaikinsSubdivide(points);
        GD.Print((DateTime.Now - time).TotalMilliseconds);*/

        /*using (DebugDraw3D.NewScopedConfig()
            .SetThickness(0.01f)
            .SetNoDepthTest(true)
            .SetCenterBrightness(0.75f))
        {
            DebugDraw3D.DrawLinePath(points, new Color("red"));
            DebugDraw3D.DrawLinePath(path, new Color("green"));
        }*/


        /* using (DebugDraw3D.NewScopedConfig()
             .SetThickness(0.02f)
             .SetNoDepthTest(true)
             .SetCenterBrightness(0.75f))
         {
             DebugDraw3D.DrawArrow(toCamPos(Vector2.Zero), toCamPos(desiredDir), new Color("black"), 0.1f);
             DebugDraw3D.DrawArrow(toCamPos(Vector2.Zero), toCamPos(limitLeft), new Color("red"), 0.1f);
             DebugDraw3D.DrawArrow(toCamPos(Vector2.Zero), toCamPos(limitRight), new Color("red"), 0.1f);
             DebugDraw3D.DrawArrow(toCamPos(Vector2.Zero), toCamPos(smoothDir), new Color("gray"), 0.1f);
         }*/


        /*lr.Points.Clear();
        prevResolution = resolution;

        float curveLength = path.Curve.GetBakedLength();
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            float dist = t * curveLength;
            lr.Points.Add(new LineRenderer.Point(path.Curve.SampleBaked(dist)));
        }*/


    }
}
