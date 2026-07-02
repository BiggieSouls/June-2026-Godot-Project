using Godot;

public partial class Pickup_Coin : Pickup_Base
{
    [Export] public int Value = 1;

    private Node3D _bronze;
    private Node3D _silver;
    private Node3D _gold;
    private Area3D _area;
    private AnimationPlayer _anim;
    private bool _picked = false;

    //Called by player_movement.cs when it detects an Area3D attached to a script (on the Node3D) that inherits Pickup_Base (which inherits Node3D).
    public override void Pickup(player_movement player)
    {
        if (_picked)
            return;

        _picked = true;

        GD.Print("Score +" + Value);
        player.Score += Value;

        player._scoreVisible = 5f;
        //player._scoreVisible = true;
        //void Result() { player._scoreVisible = false; }; player.AddTimer(5, true, Result, "Score", true);

        QueueFree();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _type = "Coin";
        _sound = GD.Load<AudioStream>("res://assets/sounds/coin.mp3");

        _bronze = GetNode<Node3D>("Model_1");
        _silver = GetNode<Node3D>("Model_5");
        _gold = GetNode<Node3D>("Model_10");

        _area = GetNode<Area3D>("Area3D");

        switch (Value)
        {
            default:
                break;
            case 1:
                _bronze.Visible = true;
                _silver.Visible = false;
                _gold.Visible = false;
                _anim = GetNode<AnimationPlayer>("Model_1/AnimationPlayer");
                break;
            case 5:
                _bronze.Visible = false;
                _silver.Visible = true;
                _gold.Visible = false;
                _anim = GetNode<AnimationPlayer>("Model_5/AnimationPlayer");
                break;
            case 10:
                _bronze.Visible = false;
                _silver.Visible = false;
                _gold.Visible = true;
                _anim = GetNode<AnimationPlayer>("Model_10/AnimationPlayer");
                break;
        }

        GD.Print(_anim);
        _anim.Play("ArmatureAction");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    /*public override void _Process(double delta)
    {
    }*/
}
