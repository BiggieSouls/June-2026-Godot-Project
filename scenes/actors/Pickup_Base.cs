using Godot;

public abstract partial class Pickup_Base : Node3D
{
    public string _type = "unassigned";
    public AudioStream _sound = null;

    //Currently, only the player can pick things up.
    public abstract void Pickup(player_movement player);
}
