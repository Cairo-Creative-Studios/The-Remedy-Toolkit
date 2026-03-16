using Remedy.Framework;
using UnityEngine;

public class CharacterRaycastContext : MonoBehaviour
{
    public SignalData CheckForWall;
    public SignalData OnHitWall;

    public bool IsGrounded = false;
    public Vector3 GroundPosition = default;
    public Vector3 WallPosition = default;

    public RaycastUtility.RaycastResult GroundCastResult;
    public RaycastUtility.RaycastResult WallCastResult;
    [SchematicProperties]
    public CharacterRaycastProperties Properties;

    private bool _cached = false;
    private Transform _transform;
    private Vector3 _dirToWall = default;
    private Vector3 _wallNorm = default;

    private void OnEnable()
    {
        _transform = gameObject.GetComponent<Transform>();
    }

    public void PerformGroundCast()
    {
        GroundCastResult = RaycastUtility.SphereCast(_transform, _transform.position, Vector3.down, Properties.GroundCheckSphereRadius, Properties.GroundCheckDistance, Properties.CollisionMask, -1f, Properties.MaxGroundSlopeAngle, Properties.GroundCheckDistance);
        
        if(GroundCastResult.TryGetClosestHit(out RaycastHit hit))
        {
            GroundPosition = hit.point;
        }
        
        IsGrounded = GroundCastResult.HasHit;
    }
    public void PerformWallCast(Vector3 direction)
    {
        WallCastResult = RaycastUtility.SphereCast(_transform, _transform.position, direction, Properties.WallCheckSphereRadius, Properties.WallCheckDistance, Properties.CollisionMask, Properties.MinWallAngle, Properties.MaxWallAngle, Properties.WallCheckDistance);

        if (WallCastResult.TryGetClosestHit(out RaycastHit hit))
        {
            WallPosition = hit.point;
            _dirToWall = direction;
            _wallNorm = hit.normal;
        }
    }

    private void OnDrawGizmos()
    {
        if (!_cached)
            _transform = transform;

        if (GroundCastResult.HasHit)
        {
            CustomGizmos.DrawCircleArrow(
                _transform.position,
                Vector3.down,
                Mathf.Abs(Mathf.Abs(GroundPosition.y) - Mathf.Abs(_transform.position.y)),
                new Color(1, 0, 0, 0.5f)
            );
        }

        // Draw hit point if wall detected
        if (WallCastResult.HasHit)
        {
            CustomGizmos.DrawCircleArrow(
                _transform.position,
                _dirToWall,
                Vector3.Distance(_transform.position, WallPosition),
                new Color(1, 1, 0, 0.5f),
                normal: _wallNorm
            );
        }
    }
}