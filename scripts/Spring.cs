using Godot;

public partial class Spring : Reacts
{
    [Export] public float BounceForce = 5.0f;

    private Area3D _area;

    public override void _Ready()
    {
        _type = "Spring";
        _sound = GD.Load<AudioStream>("res://assets/sounds/spring.mp3");

        _area = GetNode<Area3D>("StaticBody3D/Area3D");

        _area.BodyEntered += OnBodyEntered;
    }

    public override void DoThingDrawCard(player_movement player, Area3D area)
    {
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is RigidBody3D rigidBody)
        {
            Vector3 launchDirection = rigidBody.LinearVelocity.Bounce(GlobalTransform.Basis.Y.Normalized());

            rigidBody.ApplyCentralImpulse(
                launchDirection * BounceForce
            );
        }
    }
}