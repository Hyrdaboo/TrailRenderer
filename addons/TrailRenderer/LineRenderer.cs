using Godot;
using System.Collections.Generic;

public partial class LineRenderer : Node3D
{
    public enum Alignment { View, TransformZ, Static }
    public enum TextureMode { Stretch, Tile, PerSegment }
    public class Point
    {
        public Vector3 Position;
        public Vector3 Bitangent;
        /// <summary>
        /// DO NOT MODIFY THIS. Used internally by the LineRenderer.
        /// </summary>
        public float textureOffset;
        public readonly float Time;

        public Point(Vector3 position , Vector3? bitangent = null)
        {
            bitangent ??= Vector3.Forward;

            Position = position;
            Bitangent = bitangent.Value.Normalized();
            Time = Godot.Time.GetTicksMsec() / 1000.0f;
        }
    }

    [Export] private Curve curve;
    [Export] private Alignment alignment = Alignment.TransformZ;
    [Export] private bool worldSpace = true;
    [ExportGroup("Appearance")]
    [Export] private Material material;
    [Export] private GeometryInstance3D.ShadowCastingSetting castShadows = GeometryInstance3D.ShadowCastingSetting.Off;
    [Export] private Gradient colorGradient;
    [Export] private TextureMode textureMode;

    private List<Point> points = new List<Point>();
    private ImmediateMesh mesh = new ImmediateMesh();
    private MeshInstance3D meshInstance;
    private Camera3D camera;

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

    public bool WorldSpace
    {
        get => worldSpace;
        set => worldSpace = value;
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
        WorldSpace = lr.worldSpace;
        Material = lr.material;
        CastShadows = lr.castShadows;
        ColorGradient = lr.colorGradient;
        TextureSamplingMode = lr.textureMode;
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
        meshInstance.GlobalTransform = worldSpace ? Transform3D.Identity : GlobalTransform;

        mesh.ClearSurfaces();
        if (points.Count < 2)
            return;

        mesh.SurfaceBegin(Mesh.PrimitiveType.TriangleStrip);

        for (int i = 0; i < points.Count; i++)
        {
            Point currentPoint = points[i];

            Vector3 tangent = i == 0 ? currentPoint.Position.DirectionTo(points[1].Position) : -currentPoint.Position.DirectionTo(points[i-1].Position);
            Vector3 bitangent;
            if (alignment == Alignment.View && worldSpace)
            {
                bitangent = camera.Basis.Z.Cross(tangent).Normalized();
            }
            else if (alignment == Alignment.TransformZ && worldSpace)
                bitangent = GlobalBasis.Orthonormalized().Z;
            else
                bitangent = currentPoint.Bitangent;
            Vector3 normal = tangent.Cross(bitangent);
            
            float t = i / (points.Count - 1.0f);
            Color color = colorGradient.Sample(t);
            bitangent *= curve.Sample(t);

            switch (textureMode)
            {
                case TextureMode.Stretch:
                    currentPoint.textureOffset = i / (points.Count - 1.0f);
                    break;
                case TextureMode.Tile:
                    if (i > 0)
                    {
                        Point previous = points[i - 1];
                        currentPoint.textureOffset = currentPoint.Position.DistanceTo(previous.Position) + previous.textureOffset;
                    }
                    break;
                case TextureMode.PerSegment:
                    currentPoint.textureOffset = i;
                    break;
                default:
                    break;
            }

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