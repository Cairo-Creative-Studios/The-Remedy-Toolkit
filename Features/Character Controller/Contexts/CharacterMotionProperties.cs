//using SaintsField;
using UnityEngine;

public class CharacterMotionProperties : ScriptableObjectWithID<CharacterMotionProperties>
{
    public PhysicsMaterial PhysicsMaterial;

    [Tooltip("If the Horizontal Velocity is within this distance away from the desired moving velocity, it will be snapped to the desired moving velocity. (reduces motion innacuracy and idle drifting).")]
    public float MovementSnapThreshold = 0.1f;

    public bool SlideOnWall = true;

    [Header("Collisions")]
    [Tooltip("The amount of time to fully recover from a Collision's Push")]
    public float CollisionRecoveryDuration = 1f;
    [Tooltip("The max amount of influence over moving velocity a Collision's push can have")]
    public float CollisionPushInfluence = 0.25f;
    [Tooltip("Required speed of the moving object before the collision is considered a 'push'")]
    public float CollisionPushVelocityThreshold = 3f;

    [Header("Springs")]
    public float SpringStrength = 100f;
    public float SpringDamper = 30f;
    public float SpringMaxForce = 10;
    public float SpringEpsilon = 1f;

    public float MinDistanceFromGround = 0.25f;
}