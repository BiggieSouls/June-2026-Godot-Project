using System;
using Godot;

public partial class player_movement : RigidBody3D
{
	public RayCast3D Collider_Below = null;
	public Camera3D Camera = null;

	public override void _Ready()
	{
		AddToGroup("Player");

		Collider_Below = GetNode<RayCast3D>("RayCast3D");
		Collider_Below.TopLevel = true;
		if(GetParent() != null)
			Camera = GetParent().GetNode<Camera3D>("Camera/YawPivot/PitchPivot/Camera3D");
	}

	// How fast the player moves in meters per second.
	[Export]
	public int Speed { get; set; } = 20;
	[Export]
	public float Friction { get; set; } = 0.99f;

	private bool anyInput = false;
	private bool onGround = false;
	private float jumpStrength = 9f;

	public override void _PhysicsProcess(double delta)
	{
		//After making the raycast top-level, it, uh, doesn't inherit position anymore
		//So we fix that by moving it XD
		Collider_Below.GlobalPosition = this.GlobalPosition;

		//Grab the camera basis so we can adjust control direction based on which way the camera is pointing
		Basis camBasis = Camera.GlobalBasis;

		Vector3 forward = -camBasis.Z;
		forward.Y = 0;
		forward = forward.Normalized();

		Vector3 right = camBasis.X;
		right.Y = 0;
		right = right.Normalized();

		// We check for each move input and update the direction accordingly.
		anyInput = false;
		var direction = Vector3.Zero;
		if (Input.IsActionPressed("move_right"))
		{
			direction.X += 1.0f;
			anyInput = true;
		}
		if (Input.IsActionPressed("move_left"))
		{
			direction.X -= 1.0f;
			anyInput = true;
		}
		if (Input.IsActionPressed("move_back"))
		{
			direction.Z -= 1.0f;
			anyInput = true;
		}
		if (Input.IsActionPressed("move_forward"))
		{
			direction.Z += 1.0f;
			anyInput = true;
		}

		//Normalise it
		if (direction != Vector3.Zero)
		{
			direction = direction.Normalized();
		}

		Vector3 moveDirection =
			forward * direction.Z +
			right * direction.X;

		ApplyCentralForce(moveDirection * (onGround ? Speed : Speed/3)); //No air control for you (1/3rd speed)

		if(onGround && Input.IsActionJustPressed("jump"))
		{
			ApplyImpulse(new Vector3(0, jumpStrength, 0));
		}
	}

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		// Apply friction per frame
		if (!anyInput)
			state.LinearVelocity *= Friction;

		Vector3 vel = state.LinearVelocity;
		if (onGround)
		{
			vel.X *= Friction;
			vel.Y *= Friction;
		}
		else
		{
			vel.X *= 0.999999f;
			vel.Y *= 0.999999f;
			//vel.X *= Friction*1.09f;
			//vel.Y *= Friction * 1.09f;
		}
		state.LinearVelocity = vel;

		if (Collider_Below == null)
			return;
		else if (Collider_Below.IsColliding())
			onGround = true;
		else
			onGround = false;

		GD.Print("Can Jump? " + onGround);
	}
}
