using Godot;

public partial class MouseFollow : Node3D
{
    public override void _Process(double delta)
    {
        Vector2 mousePosition = GetViewport().GetMousePosition();

        Camera3D cam = GetViewport().GetCamera3D();
        var rayOrigin = cam.ProjectRayOrigin(mousePosition);
        var rayDir = cam.ProjectRayNormal(mousePosition);
        var worldPos = rayOrigin + rayDir * 5;


        GlobalPosition = worldPos;
    }
}
