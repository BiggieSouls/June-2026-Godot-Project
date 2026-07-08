using Godot;
using System;
using static Godot.WebSocketPeer;

public partial class Guillotine : Reacts
{
    //public Actor_Character _target = null;
    //private Actor_Player _player = null;
    private AnimationPlayer _anim;
    //private AnimationTree _animTree;
    //private AnimationNodeStateMachinePlayback _playback;
    //private string _animCurrent;
    private Area3D _frontline;
    private Area3D _backline;
    private string _state = "Untouched";
    private Controller _controller;
    [Export] public float ResetDistance = 20.0f;

    public override void _Ready()
    {
        _frontline = GetNode<Area3D>("FrontLine");
        _backline = GetNode<Area3D>("BackLine");
        _anim = GetNode<AnimationPlayer>("AnimationPlayer");
        _controller = Controller.Instance;
    }

    public override void DoThingDrawCard(player_movement player, Area3D area)
    {
        if (_state.Equals("Untouched"))
            _state = area.Name.Equals("FrontLine") ? "TouchedFront" : area.Name.Equals("BackLine") ? "TouchedBack" : "Untouched";
        else if ((_state.Equals("TouchedFront") && area.Name.Equals("BackLine")) || (_state.Equals("TouchedBack") && area.Name.Equals("FrontLine")))
        {
            _state = "Score";
            player.Score += 5;
            player._scoreVisible = 5.0f;
            GD.Print("Score +5!");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var _currentAnim = _anim.CurrentAnimation.ToString();
        if (_state.Equals("Score"))
        {
            _anim.Play("Idle", 1f);
            _state = "Cooldown";
        }
        else if (!_state.Equals("Cooldown") && !_currentAnim.Equals("Swing"))
            _anim.Play("Swing", 0.5f);

        if(_controller.Player == null)
            return;

        if (!_state.Equals("Untouched"))
            if ((_controller.Player.GlobalPosition - GlobalPosition).Length() >= ResetDistance)
                _state = "Untouched";
    }
}
