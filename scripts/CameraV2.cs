using Godot;

public partial class CameraV2 : Node3D
{
    /*[Export] public float Distance = 6.0f;
    [Export] public float Height = 2.0f;*/
    [Export] public float MouseSensitivity = 0.003f;

    private RigidBody3D player = null;
    private Node3D _yawPivot;
    private Node3D _pitchPivot;

    private float _yaw;
    private float _pitch;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as RigidBody3D;

        Input.MouseMode = Input.MouseModeEnum.Captured;

        _yawPivot = GetNode<Node3D>("YawPivot");
        _pitchPivot = _yawPivot.GetNode<Node3D>("PitchPivot");
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.MouseMode == Input.MouseModeEnum.Captured && @event is InputEventMouseMotion mouseMotion)
        {
            _yaw -= mouseMotion.Relative.X * MouseSensitivity;
            _pitch -= -mouseMotion.Relative.Y * MouseSensitivity;

            _pitch = Mathf.Clamp(
                _pitch,
                Mathf.DegToRad(-80),
                Mathf.DegToRad(80));
        }
        else if (@event.IsActionPressed("ui_cancel"))
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
                Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public override void _Process(double delta)
    {
        if (player == null)
            return;

        GlobalPosition = player.GlobalPosition;
        //GlobalPosition = player.GlobalPosition + new Vector3(0, 3.85f, -4);

        _yawPivot.Rotation = new Vector3(0, _yaw, 0);
        _pitchPivot.Rotation = new Vector3(_pitch, 0, 0);

        Camera3D camera = _pitchPivot.GetNode<Camera3D>("Camera3D");
    }
}