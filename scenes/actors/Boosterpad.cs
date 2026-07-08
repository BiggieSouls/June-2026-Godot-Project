using Godot;
using System;

public partial class Boosterpad : Reacts
{
    [Export] public float SpeedMin = 60;
    //[Export] public float SpeedMax = 100;
    [Export] public float BoostAmount = 1000;
    [Export] public bool BoostBasedOnScore = true;
    [Export] public int BoostBasedOnScore_RatioOneInX = 4;
    [Export] public bool CanBoostVertical = false;

    public override void DoThingDrawCard(player_movement player, Area3D area)
    {
        Vector3 velocity = player.LinearVelocity;
        if(BoostBasedOnScore)
            SpeedMin *= Math.Max(1, player.Score/ BoostBasedOnScore_RatioOneInX);

        // Horizontal movement only
        Vector3 horizontalVelocity = new Vector3(
            velocity.X,
            CanBoostVertical ? velocity.Y : 0,
            velocity.Z
        );

        float currentSpeed = horizontalVelocity.Length();

        // Don't boost stationary objects
        if (currentSpeed < 0.1f)
            return;

        Vector3 direction = horizontalVelocity.Normalized();

        float newSpeed;

        if (currentSpeed < SpeedMin)
        {
            // Force up to minimum speed
            newSpeed = SpeedMin;
        }
        else
        {
            // Add boost
            newSpeed = currentSpeed + BoostAmount;
        }

        // Cap maximum speed
        //newSpeed = Mathf.Min(newSpeed, MaximumBoostSpeed);

        GD.Print(newSpeed);

        player.LinearVelocity = new Vector3(
            direction.X * newSpeed,
            CanBoostVertical ? direction.Y * newSpeed : velocity.Y,
            direction.Z * newSpeed
        );
    }
}
