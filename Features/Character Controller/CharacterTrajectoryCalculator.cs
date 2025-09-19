using Remedy.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace Remedy.CharacterControllers
{
    // TODO: Try to remove the helper functions for height changes, they may be causing the slight innaccuracies.
    // Trajectories render correctly for a split second when jumping 

    /// <summary>
    /// Calculates trajectory predictions for the PhysicsBasedCharacterController
    /// </summary>
    [SelectionBase]
    public class CharacterTrajectoryCalculator : Singleton<CharacterTrajectoryCalculator>
    {
        public PhysicsBasedCharacterController controller;
        public float Contrast = 3f;
        private CharacterRaycastContext _raycastContext => controller.GetComponent<CharacterRaycastContext>();

        /// <summary>
        /// Calculates the predicted trajectory points for a given initial velocity
        /// </summary>
        /// <param name="controller">The character controller to simulate</param>
        /// <param name="initialVelocity">The velocity to set at the start of simulation</param>
        /// <param name="timeStep">Physics simulation time step (usually Time.fixedDeltaTime)</param>
        /// <param name="maxTime">Maximum time to simulate</param>
        /// <param name="maxPoints">Maximum number of trajectory points to return</param>
        /// <returns>Array of predicted positions</returns>
        public static Vector3[] CalculateTrajectory(
            Vector3 startingPosition,
            Vector3 initialVelocity,
            float timeStep = 0.02f,
            float maxTime = 5f,
            int maxPoints = 100,
            bool includeHangtime = true)
        {
            List<Vector3> trajectoryPoints = new List<Vector3>();

            // Get initial state
            Vector3 position = startingPosition;
            Vector3 velocity = initialVelocity;
            if (Instance.controller == null)
                Instance.controller = FindAnyObjectByType<PhysicsBasedCharacterController>();
            Rigidbody rb = Instance.controller.gameObject.GetComponent<Rigidbody>();
            float mass = rb.mass;

            // Simulation parameters from controller

            Vector3 baseGravity = (Physics.gravity * mass * 2f) + (Physics.gravity * mass * ((Instance.controller.Properties.FallSpeed + 1f))) ;
            Vector3 gravity = baseGravity;
            LayerMask terrainLayer = Instance._raycastContext.Properties.CollisionMask;

            trajectoryPoints.Add(position);

            float currentTime = 0f;
            int pointCount = 0;
            float simulatedHangTime = 0f;

            while (currentTime < maxTime && pointCount < maxPoints)
            {

                // Check if we hit ground
                (bool hitGround, RaycastHit groundHit) = SimulateGroundRaycast(
                    position,
                    2f,
                    terrainLayer);

                Vector3 totalForce = Vector3.zero;

                // Apply gravity (doubled like in the controller)
                totalForce += gravity;



                Vector3 acceleration = totalForce;
                velocity += acceleration * timeStep;
                // Apply wall collision forces if moving fast enough
                if (velocity.magnitude > 0.1f)
                {
                    Vector3 wallForce = CalculateWallCollisionForce(position, velocity);
                    if (wallForce != Vector3.zero)
                        velocity = wallForce;
                }

                float yVel = velocity.y;
                velocity = Vector3.Lerp(velocity, Vector3.zero, timeStep);
                velocity.y = yVel;

                position += velocity * timeStep;

                if (velocity.y <= 0)
                {
                    if (simulatedHangTime > Instance.controller.Properties.HangTimeDuration || !includeHangtime)
                    {
                        // If we're very close to ground, we might be landing
                        if (hitGround && groundHit.distance <= Instance.controller.Properties.RideHeight * 1.3f)
                        {
                            // Character has landed, trajectory ends here
                            trajectoryPoints.Add(groundHit.point + Vector3.up * Instance.controller.Properties.RideHeight);
                            break;
                        }

                        gravity.y += baseGravity.y * timeStep;
                    }
                    else
                    {
                        gravity.y = 0;
                        simulatedHangTime += timeStep;
                    }
                }
                else
                {
                    gravity.y += baseGravity.y * timeStep;
                }

                trajectoryPoints.Add(position);

                currentTime += timeStep;
                pointCount++;
            }

            return trajectoryPoints.ToArray();
        }

        /// <summary>
        /// Simulates the ground raycast from the character controller
        /// </summary>
        private static (bool, RaycastHit) SimulateGroundRaycast(
            Vector3 position,
            float rayLength,
            LayerMask terrainLayer)
        {
            RaycastHit hit;
            Ray ray = new Ray(position, Vector3.down);
            bool hitGround = Physics.SphereCast(ray, 0.5f, out hit, rayLength, terrainLayer);

            if (!hitGround)
            {
                hitGround = Physics.SphereCast(ray, 0.15f, out hit, rayLength, terrainLayer);
            }

            return (hitGround, hit);
        }


        /// <summary>
        /// Calculates potential wall collision forces
        /// </summary>
        private static Vector3 CalculateWallCollisionForce(
            Vector3 position,
            Vector3 velocity)
        {
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            Vector3 movementDirection = horizontalVelocity.normalized;

            // Get wall collision parameters through reflection (since they're private)
            LayerMask wallLayer = Instance._raycastContext.Properties.CollisionMask;
            float wallSlideDistance = 2;

            RaycastHit hit;
            bool hitWall = Physics.SphereCast(
                position,
                0.1f,
                movementDirection,
                out hit,
                wallSlideDistance,
                wallLayer
            );

            if (hitWall)
            {
                Vector3 wallNormal = hit.normal;

                Vector3 newVel = Vector3.ProjectOnPlane(velocity, wallNormal);

                return newVel;
            }

            return Vector3.zero;
        }
    }
}