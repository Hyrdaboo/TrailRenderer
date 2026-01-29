using Godot;

public partial class TrailToggle : Node
{
    private TrailRenderer tr;

    public override void _Ready()
    {
        tr = GetChild<TrailRenderer>(0);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Middle && mouseEvent.Pressed && !mouseEvent.IsEcho())
        {
            tr.Emitting = !tr.Emitting;
        }
    }
}
