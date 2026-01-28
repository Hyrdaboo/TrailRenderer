using Godot;

public partial class HandController : Node3D
{
    [Export] private AudioStreamPlayer audioPlayer;
    private TrailRenderer tr;
    private AnimationPlayer animPlayer;

    private static Node FindChildOfType<T>(Node node)
    {
        if (node is T)
        {
            return node;
        }

        foreach (Node child in node.GetChildren())
        {
            Node result = FindChildOfType<T>(child);
            if (result != null)
                return result;
        }

        return null;
    }

    public override void _Ready()
    {
        animPlayer = FindChildOfType<AnimationPlayer>(this) as AnimationPlayer;
        tr = FindChildOfType<TrailRenderer>(this) as TrailRenderer;

        animPlayer.AnimationFinished += StopEmit;
    }

    private void StopEmit(StringName name)
    {
        tr.Emitting = false;
    }

    private const float AudioPlayDelay = 0.25f;
    private float audioPlayTimer = -AudioPlayDelay;
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Fire1"))
        {
            if (!string.IsNullOrEmpty(animPlayer.CurrentAnimation)) 
                return;

            tr.Emitting = true;
            animPlayer.CurrentAnimation = "Swing_Left";
            animPlayer.SpeedScale = 1.2f;

            float scale = (int)(GD.Randi() % 2 * 2 - 1);
            Scale = new Vector3(scale, 1, 1);

            audioPlayTimer = AudioPlayDelay;
        }

        if (audioPlayTimer < 0 && !audioPlayer.Playing && audioPlayTimer > -0.05f)
        {
            audioPlayer.Play();
            audioPlayer.PitchScale = Mathf.Lerp(1.0f, 1.2f, GD.Randf());
        }
        
        audioPlayTimer -= (float)delta;
    }
}
