using Godot;

public partial class NoclipCamera : Camera3D
{
    [Export]
    private float mouseSensitivity = 3.0f;
    [Export]
    private float speed = 100.0f;
    [Export]
    private float smooth = 5.0f;

    bool pressing = false;
    float xRot, yRot;
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseBtnEvt)
        {
            if (mouseBtnEvt.ButtonIndex == MouseButton.Right)
            {
                if (mouseBtnEvt.Pressed)
                {
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                    pressing = true;
                }
                else if (mouseBtnEvt.IsReleased())
                {
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                    pressing = false;
                }
            }
        }

        if (@event is InputEventMouseMotion mouseMotionEvt && pressing)
        {
            Vector2 mouseDelta = -mouseMotionEvt.Relative / GetViewport().GetWindow().Size;

            xRot += mouseSensitivity * mouseDelta.Y;
            yRot += mouseSensitivity * mouseDelta.X;

            GlobalRotation = Quaternion.FromEuler(new Vector3 { X = xRot, Y = yRot }).GetEuler();
        }
    }

    public override void _Process(double delta)
    {
        float fDelta = (float)delta;
        Vector3 inputDir = new Vector3(Input.GetAxis("left", "right"), Input.GetAxis("down", "up"), Input.GetAxis("forward", "backward"));

        Vector3 desired = GlobalPosition + (inputDir.X * GlobalTransform.Basis.X + inputDir.Y * GlobalTransform.Basis.Y + inputDir.Z * GlobalTransform.Basis.Z) * fDelta * speed;
        GlobalPosition = GlobalPosition.Lerp(desired, fDelta * smooth);

        if (!pressing)
        {
            xRot = GlobalRotation.X;
            yRot = GlobalRotation.Y;
        }
    }
}
