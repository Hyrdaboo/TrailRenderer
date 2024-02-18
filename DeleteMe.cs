using Godot;

public partial class DeleteMe : Node
{
    [Export] private TrailRenderer target;
	Camera3D cam;

    public override void _Ready()
    {
        cam = GetParent<Camera3D>();
    }

    public override void _Process(double delta)
    {
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            target.Emitting = true;
        }
        else target.Emitting = false;

        target.GlobalPosition = cam.ProjectPosition(GetViewport().GetMousePosition(), 8.0f);
        target.RotateX((float)delta * 1.0f);
    }
}
