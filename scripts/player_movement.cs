using Godot;

public partial class player_movement : RigidBody3D
{
	public RayCast3D Collider_Below = null;
	public RayCast3D Collider_BelowLong = null;
	public Camera3D Camera = null;
	private Area3D _detection;
	private ShapeCast3D _domainExpansion;

	private AudioStream _sfxLandingLight = GD.Load<AudioStream>("res://assets/sounds/landing_light.mp3");
	private AudioStream _sfxLandingHeavy = GD.Load<AudioStream>("res://assets/sounds/landing_heavy.mp3");
	private AudioStreamPlayer3D _sound;

	public int Score = 0;

	[Export] public float LandingThresholdHeavy = 10;

	// How fast the player moves in meters per second.
	[Export] public int Speed { get; set; } = 20;
	[Export] public float Friction { get; set; } = 0.99f;

	private bool anyInput = false;
	private bool onGround = false;
	private bool onGroundLong = false;
	private float jumpStrength = 9f;

	public override void _Ready()
	{
		AddToGroup("Player");

		Collider_Below = GetNode<RayCast3D>("RayCast_Ground");
		Collider_Below.TopLevel = true;
		Collider_BelowLong = GetNode<RayCast3D>("RayCast_GroundLong");
		Collider_BelowLong.TopLevel = true;
		_domainExpansion = GetNode<ShapeCast3D>("ShapeCast3D");
		_domainExpansion.TopLevel = true;
		if (GetParent() != null)
			Camera = GetParent().GetNode<Camera3D>("Camera/YawPivot/PitchPivot/Camera3D");

		_domainExpansion = GetNode<ShapeCast3D>("ShapeCast3D");

		_detection = GetNode<Area3D>("Area3D");
		_detection.AreaEntered += OnAreaEntered;
		//_detection.AreaExited += OnAreaExited;

		_sound = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
	}

	public override void _PhysicsProcess(double delta)
	{
		//After making the raycast top-level, it, uh, doesn't inherit position anymore
		//So we fix that by moving it XD
		Collider_Below.GlobalPosition = GlobalPosition;
		Collider_BelowLong.GlobalPosition = GlobalPosition;
		_domainExpansion.GlobalPosition = GlobalPosition;//new Vector3(GlobalPosition.X, GlobalPosition.Y+1, GlobalPosition.Z);

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
		if (Input.IsActionPressed("move_right_alt"))
		{
			direction.X += 1.0f;
			anyInput = true;
		}
		if (Input.IsActionPressed("move_left_alt"))
		{
			direction.X -= 1.0f;
			anyInput = true;
		}
		if (Input.IsActionPressed("move_back_alt"))
		{
			direction.Z -= 1.0f;
			anyInput = true;
		}
		if (Input.IsActionPressed("move_forward_alt"))
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

		// Remove velocity component perpendicular to the ground
		Vector3 groundNormal = Collider_Below.GetCollisionNormal();
		Vector3 planarVelocity = LinearVelocity - groundNormal * LinearVelocity.Dot(groundNormal);
		float speed = planarVelocity.Length();

		float localSpeedMult = (onGround ? Speed : Speed / 3); //No air control for you (1/3rd speed)
		localSpeedMult *= onGround && speed < 5 ? 2 : 1;
		ApplyCentralForce(moveDirection * localSpeedMult);

		if(onGround && Input.IsActionJustPressed("jump"))
		{
			ApplyImpulse(new Vector3(0, jumpStrength, 0));
			GD.Print("Your score is: " + Score);
		}

		/*if (_domainExpansion.IsColliding() && !onGround)
		{
			Vector3 closestPoint = _domainExpansion.GetCollisionPoint(0);
			//Vector3 normal = _domainExpansion.GetCollisionNormal(0);
			Vector3 newGrav = (closestPoint - GlobalPosition).Normalized();

			//ApplyCentralForce(newGrav * LinearVelocity);
			ApplyCentralForce(newGrav * 5);
		}*/
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

		bool isCollidingLong = Collider_BelowLong.IsColliding();
		bool isColliding = Collider_Below.IsColliding();
		if(isColliding && !onGround)
		{
			//This means it's the first frame of landing, so we play a sound.
			if (!_sound.Playing)
			{
				GD.Print("Vertical speed: " + state.LinearVelocity.Y);
				if (state.LinearVelocity.Y <= -1)
				{
					_sound.Stream = state.LinearVelocity.Y <= -LandingThresholdHeavy ? _sfxLandingHeavy : _sfxLandingLight; //Load the sound
					_sound.Play(); //Play sound
				}
			}
		}
		onGround = isColliding;
		onGroundLong = isCollidingLong;
	}

	public void OnAreaEntered(Area3D area)
	{
		Node3D obj = area.Owner as Node3D;
		GD.Print(obj);
		if (obj.GetType().IsSubclassOf(typeof(Pickup_Base)))
		{
			Pickup_Base o = area.Owner as Pickup_Base;
			_sound.Stream = o._sound;
			_sound.Play();
			o.Call("Pickup", this);
		}
		else if (obj.GetType().IsSubclassOf(typeof(Reacts)))
		{
			GD.Print("HEY");
			Reacts o = area.Owner as Reacts;
			_sound.Stream = o._sound;
			_sound.Play();
			o.Call("DoThingDrawCard", this);
		}
	}
}
