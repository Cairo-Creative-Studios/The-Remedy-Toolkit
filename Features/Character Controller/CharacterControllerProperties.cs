using UnityEngine;

public class CharacterControllerProperties : ScriptableObjectWithID<CharacterControllerProperties>
{
    [Header("Movement")]
    public float MoveSpeed = 5f;
    public float Acceleration = 60f;
    public float Deceleration = 30f;

    [Header("Jump and Fall")]
    [Tooltip("The speed at which the character moves toward the desired Jump Height")]
    public float JumpSpeed = 10f;
    [Tooltip("The max height of the jump")]
    public float JumpHeight = 4f;
    public float FallSpeed = 2f;
    [Tooltip("The delay between transitioning from Grounded to Jumping states after the Character has landed")]
    public float JumpResetDelay = 0.25f;
    [Tooltip("The amount of time after the Jump Button has been initially pressed to consider whether a jump should occur")]
    public float JumpBuffer = 1f;
    [Tooltip("The amount of time to remain at the peak of a Jump (or push)")]
    public float HangTimeDuration = 0.75f;
    [Tooltip("Rate of change toward the desired Hang Time Height during Hang Time")]
    public float HangTimeDamper = 10f;

    [Header("Ride Height")]
    public float RideHeight = 1f;
    [Tooltip("Larger numbers can make the character jump innapropriately, but also make it stick to the desired height easier.")]
    public float MaxHoverForce = 2f;
    public float RideSpringStrength = 200f;
    public float VerticalSpringDamper = 30f;

    [Header("Collisions")]
    [Tooltip("The amount of time to fully recover from a Collision's Push")]
    public float CollisionRecoveryDuration = 1f;
    [Tooltip("The max amount of influence over moving velocity a Collision's push can have")]
    //[MinMaxSlider(0.0f, 1f)]
    public float CollisionInfluence = 0.25f;
    [Tooltip("Required speed of the moving object before the collision is considered a 'push'")]
    public float CollisionVelocityThreshold = 3f;
}