using Godot;

public abstract partial class Reacts : Node3D
{
    public string _type = "unassigned";
    public AudioStream _sound = null;

    //Currently, only the player will trigger a reaction.
    public abstract void DoThingDrawCard(player_movement player);
}
