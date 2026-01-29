using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class LineRenderer : Node3D
{
    // TransformZ - faces look at current global Z axis; Static - faces look at global Z axis at the time of spawning
    public enum Alignment { View, TransformZ, Static }
    public enum TextureMode { Stretch, Tile, DistributePerSegment, RepeatPerSegment, Static }
    public class Point
    {
        public Vector3 Position;
        public Vector3 Normal;
        /// <summary>
        /// DO NOT MODIFY THIS. Used internally by the LineRenderer.
        /// </summary>
        public float textureOffset;
        public readonly float Time;

        public Point(Vector3 position, float textureOffset = 0, Vector3? normal = null)
        {
            this.textureOffset = textureOffset;
            normal ??= Vector3.Up;

            Position = position;
            this.Normal = normal.Value.Normalized();
            Time = Godot.Time.GetTicksMsec() / 1000.0f;
        }
    }

    [Export] private Curve curve;
    [Export] private Alignment alignment = Alignment.TransformZ;
    [Export(PropertyHint.Range, "0, 3, 1")] private int bevelIterations = 0;
    [Export(PropertyHint.Range, "0.01, 0.49")] private float bevelAmount = 0.25f;
    [ExportGroup("Appearance")]
    [Export] private Material material;
    [Export] private GeometryInstance3D.ShadowCastingSetting castShadows = GeometryInstance3D.ShadowCastingSetting.Off;
    [Export] private Gradient colorGradient;
    [Export] private TextureMode textureMode;

    private List<Point> points = new List<Point>();
    private ImmediateMesh mesh = new ImmediateMesh();
    private MeshInstance3D meshInstance;
    private Camera3D camera;

    /// <summary>
    /// Set to true by TrailRenderer to avoid overwriting textureOffset.
    /// Do not modify this manually, unless you know what you're doing.
    /// </summary>
    public bool isModifiedByTrailRenderer = false;

    public Curve Curve
    {
        get => curve;
        set
        {
            if (value != null)
                curve = value;
        }
    }

    public Alignment LineAlignment
    {
        get => alignment;
        set => alignment = value;
    }

    public float BevelAmount
    {
        get => bevelAmount;
        set => bevelAmount = Mathf.Clamp(value, 0.01f, 0.49f);
    }

    public int BevelIterations
    {
        get => bevelIterations;
        set => bevelIterations = Mathf.Clamp(value, 0, 3);
    }

    public Material Material
    {
        get => material;
        set
        {
            material = value;
            meshInstance.MaterialOverride = material;
        }
    }

    public GeometryInstance3D.ShadowCastingSetting CastShadows
    {
        get => castShadows;
        set => castShadows = value;
    }

    public Gradient ColorGradient
    {
        get => colorGradient;
        set
        {
            if (value != null)
                colorGradient = value;
        }
    }

    public TextureMode TextureSamplingMode
    {
        get => textureMode;
        set => textureMode = value;
    }

    public List<Point> Points
    {
        get => points;
    }

    public void CopyValues(LineRenderer lr)
    {
        Curve = lr.curve;
        LineAlignment = lr.alignment;
        Material = lr.material;
        CastShadows = lr.castShadows;
        ColorGradient = lr.colorGradient;
        TextureSamplingMode = lr.textureMode;
        BevelAmount = lr.bevelAmount;
        BevelIterations = lr.bevelIterations;
    }

    private List<Point> ChaikinsSubdivide(List<Point> originalPath)
    {
        if (originalPath.Count < 3)
            return originalPath;

        List<Point> output = originalPath;

        for (int iter = 0; iter < bevelIterations; iter++)
        {
            List<Point> newPath = new List<Point>();

            for (int i = 0; i < output.Count; i++)
            {
                if (i > 0 && i < output.Count - 1)
                {
                    Vector3 p0 = output[i].Position.Lerp(output[i - 1].Position, bevelAmount);
                    Vector3 p1 = output[i].Position.Lerp(output[i + 1].Position, bevelAmount);
                    float textureOffset0 = Mathf.Lerp(output[i].textureOffset, output[i - 1].textureOffset, bevelAmount);
                    float textureOffset1 = Mathf.Lerp(output[i].textureOffset, output[i + 1].textureOffset, bevelAmount);
                    newPath.Add(new Point(p0, textureOffset0));
                    newPath.Add(new Point(p1, textureOffset1));
                }
                else
                {
                    newPath.Add(output[i]);
                }
            }

            output = newPath;
        }

        return output;
    }

    public override void _Ready()
    {
        if (colorGradient == null)
        {
            colorGradient = new Gradient();
            colorGradient.AddPoint(0, Colors.White);
            colorGradient.AddPoint(1, Colors.White);
        }
        if (curve == null)
        {
            curve = new Curve();
            curve.AddPoint(new Vector2(0, 0.5f), 0, 0, Curve.TangentMode.Free, Curve.TangentMode.Linear);
            curve.AddPoint(new Vector2(1, 0.5f), 0, 0, Curve.TangentMode.Linear);
        }

        meshInstance = new MeshInstance3D();
        AddChild(meshInstance);
        meshInstance.Mesh = mesh;
        meshInstance.MaterialOverride = material;
        meshInstance.TopLevel = true;
    }

    public override void _Process(double delta)
    {
        camera = GetViewport().GetCamera3D();
        meshInstance.CastShadow = castShadows;
        meshInstance.GlobalTransform = Transform3D.Identity;

        mesh.ClearSurfaces();
        if (this.points.Count < 2)
            return;

        mesh.SurfaceBegin(Mesh.PrimitiveType.TriangleStrip);


        List<Point> points = ChaikinsSubdivide(this.points);

        Vector3 prevBitangent = Vector3.Zero;
        float totalLength = points.Zip(points.Skip(1), (a, b) => a.Position.DistanceTo(b.Position)).Sum();
        float accumulatedLength = 0;
        for (int i = 0; i < points.Count; i++)
        {
            Point currentPoint = points[i];

            Vector3 tangent = i == 0 ? currentPoint.Position.DirectionTo(points[1].Position) : -currentPoint.Position.DirectionTo(points[i - 1].Position);
            tangent = tangent.Normalized();

            Vector3 alignmentVec;
            if (alignment == Alignment.View)
                alignmentVec = currentPoint.Position.DirectionTo(camera.GlobalPosition).Normalized();
            else if (alignment == Alignment.TransformZ)
                alignmentVec = GlobalBasis.Z.Normalized();
            else
                alignmentVec = currentPoint.Normal;

            Vector3 bitangent = alignmentVec.Cross(tangent).Normalized();
            Vector3 normal = tangent.Cross(bitangent).Normalized();

            if (i > 0)
            {
                Point previous = points[i - 1];
                accumulatedLength += currentPoint.Position.DistanceTo(previous.Position);
            }

            switch (textureMode)
            {
                case TextureMode.Stretch:
                    currentPoint.textureOffset = accumulatedLength / totalLength;
                    break;
                case TextureMode.DistributePerSegment:
                    currentPoint.textureOffset = i / (points.Count - 1.0f);
                    break;
                case TextureMode.Tile:
                    currentPoint.textureOffset = 1 - (totalLength - accumulatedLength);
                    break;
                case TextureMode.RepeatPerSegment:
                    currentPoint.textureOffset = i;
                    break;
                case TextureMode.Static:
                    if (!isModifiedByTrailRenderer)
                        currentPoint.textureOffset = 1 - (totalLength - accumulatedLength);
                    break;
                default:
                    break;
            }

            float t = accumulatedLength / totalLength;
            Color color = colorGradient.Sample(t);
            bitangent *= curve.Sample(t);

            prevBitangent = bitangent;
            
            mesh.SurfaceSetUV(new Vector2(0, 1 - currentPoint.textureOffset));
            mesh.SurfaceSetNormal(normal);
            mesh.SurfaceSetColor(color);
            mesh.SurfaceAddVertex(currentPoint.Position - bitangent);

            mesh.SurfaceSetUV(new Vector2(1, 1 - currentPoint.textureOffset));
            mesh.SurfaceSetNormal(normal);
            mesh.SurfaceSetColor(color);
            mesh.SurfaceAddVertex(currentPoint.Position + bitangent);
        }

        mesh.SurfaceEnd();
    }
}