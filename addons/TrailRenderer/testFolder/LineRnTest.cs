using Godot;


public partial class LineRnTest : Node3D
{
    [Export] private Path3D path;
    [Export] private LineRenderer lr;
    [Export] private int resolution = 9;

    int prevResolution = -1;
    float t;
    float px = -3;
    public override void _Process(double delta)
    {
        lr.Points.Clear();
        lr.Points.Add(new LineRenderer.Point(new Vector3(px, 0, 0)));
        lr.Points.Add(new LineRenderer.Point(new Vector3(1, 0, 0)));
        lr.Points.Add(new LineRenderer.Point(new Vector3(2, 0, 0)));
        lr.Points.Add(new LineRenderer.Point(new Vector3(3, 0.5f, 0)));

        if (Input.IsKeyPressed(Key.A))
            px += (float)delta * 2;
        if (Input.IsKeyPressed(Key.D))
            px -= (float)delta * 2;

        /*t += (float)delta;
        path.Curve.SetPointPosition(0, new Vector3(Mathf.Sin(t * 5) * 3, 0, 0));

        lr.Points.Clear();
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
