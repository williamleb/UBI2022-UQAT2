using UnityEngine;

namespace Units.Camera
{
    public class CameraStrategyMultipleTargets : CameraStrategy
    {
        // Defining the size of the arena, easily accessible by anyone who needs it
        private const float ARENA_Y = 17.7f;
		
        private Vector3 offset;
        private Vector3 averageTarget;

        [Header("Camera Settings")] private const float CAMERA_TILT_ANGLE = 55f;

        private float longestDistance;
        private const float MAX_DIST = 64f;
        private const float MIN_DIST = 50f;

        private float distanceMultiplier = 1f;
        private const float MAX_DISTANCE_MULTIPLIER = 1.4f;

        private const float FURTHEST_TARGET_WEIGHT_MULTIPLIER = 2.08f;

        [Header("Boundaries")] [SerializeField] private CameraBounds cameraBounds;

        private Vector3 averageTargetGizmoPosition;
        private Vector3 weightedTargetGizmoPosition;


        private void Awake()
        {
            Initialize();
        }

        // Use this for initialization
        protected override void Initialize()
        {
            base.Initialize();
            
            UpdateCamera();
            ForceUpdatePositionAndRotation();
        }

        public void ResetCameraPosition()
        {
            transform.position = Vector3.zero;
            MyCamera.transform.localPosition = offset;
            MyCamera.transform.rotation = Quaternion.Euler(CAMERA_TILT_ANGLE, 0, 0);
        }

        [SerializeField] private float singleTargetYAdditionalOffset = 2f;

        private void UpdateCamera()
        {
            CalculateAverages();
            CalculateOffset();

            averageTarget = cameraBounds.StayWithinBounds(averageTarget, offset, CAMERA_TILT_ANGLE, longestDistance, MyCamera.transform);
            if (Targets.Count == 1 && Targets[0] != null)
            {
                float arenaYMultiplier = Mathf.Clamp01(Targets[0].transform.position.z / ARENA_Y);
                float additionalZOffsetForOnlineTeleport = arenaYMultiplier * singleTargetYAdditionalOffset;
                averageTarget += Vector3.forward * additionalZOffsetForOnlineTeleport;
            }

            UpdatePositionAndRotation();
        }

        // Update is called once per frame
        public void LateUpdate()
        {
            UpdateCamera();
        }

        void CalculateAverages()
        {
            //Reset the distance
            longestDistance = 0f;
            float yDifference = 0f;

            //Reset the average target variable
            averageTarget = Vector3.zero;

            //If there are no targets in the target list, then set the distance to 10
            if (Targets.Count == 0)
            {
                longestDistance = MAX_DIST;
                return;
            }

            ActiveTargets.Clear();

            float minY = Mathf.Infinity;
            float maxZ = -Mathf.Infinity;
            float averageZ = 0;

            //Go through each target and calculate its distance to the targets average position and add it to the distance variable
            for (int i = 0; i < Targets.Count; i++)
            {
                GameObject targetGameObject = Targets[i];

                if (targetGameObject != null)
                {
                    //Get current target
                    Transform targetTransform = Targets[i].transform;

                    ActiveTargets.Add(targetTransform.gameObject);

                    //Add target position to average target - gets divided later
                    averageTarget += targetTransform.position;

                    //Loop through the target list again and check distances and y differences
                    for (int j = i; j < Targets.Count; j++)
                    {
                        GameObject subTargetGameObject = Targets[j];
                        if (subTargetGameObject != targetGameObject)
                        {
                            //Find the tanks with the longest distance between them
                            float targetDistance = Vector3.Distance(targetTransform.position, Targets[j].transform.position);
                            if (targetDistance > longestDistance)
                            {
                                longestDistance = targetDistance;
                            }

                            //Compensate for Y Difference between all tanks
                            float yDiff = Mathf.Abs((targetTransform.position.z - Targets[j].transform.position.z));
                            if (yDifference < yDiff)
                            {
                                yDifference = yDiff;
                            }

                            //Keep track of max and min positions on the z-axis 
                            //for moving the boundsCenter around
                            float zPos = targetTransform.position.z;
                            if (zPos > maxZ)
                                maxZ = zPos;
                            if (zPos < minY)
                                minY = zPos;
                            averageZ += zPos;
                        }
                    }
                }
            }

            // Compensate the camera position when driving above the center of the arena
            distanceMultiplier = Mathf.Clamp(yDifference / (cameraBounds.Bounds.z * 2), 0, 1f);
            if (averageZ >= 0)
            {
                cameraBounds.SetBoundsMovementMultiplier = Mathf.Clamp((averageZ) / (cameraBounds.Bounds.z), 0, 1);
            }

            distanceMultiplier = Remap(distanceMultiplier, 0, 1, 1, MAX_DISTANCE_MULTIPLIER);

            if (Targets.Count == 1)
            {
                longestDistance = MAX_DIST;
            }
            else
                longestDistance += MIN_DIST;

            float addOnDistance = (longestDistance / cameraBounds.Diagonal) * ((MAX_DIST * distanceMultiplier) - MIN_DIST);
            longestDistance += (addOnDistance);

            //If target count is greater 3 or more, then we need to use a weighted target position to try to keep all players on the screen 
            if (ActiveTargets.Count > 2)
            {
                //Reset weightedAverageTarget
                Vector3 weightedAverageTarget = Vector3.zero;

                //Calculate the average of all the positions
                averageTarget = averageTarget / ActiveTargets.Count;

                //Save position for gizmo drawing
                averageTargetGizmoPosition = averageTarget;

                float[] distances = new float[ActiveTargets.Count];
                Vector3[] directions = new Vector3[ActiveTargets.Count];
                float localLongestDistance = 0;

                //Find distances to average point
                for (int i = 0; i < ActiveTargets.Count; i++)
                {
                    Vector3 direction = ActiveTargets[i].transform.position - averageTarget;
                    Debug.DrawRay(ActiveTargets[i].transform.position, direction, Color.green);
                    directions[i] = direction;

                    float distanceToAverage = direction.magnitude;
                    distances[i] = distanceToAverage;
                    if (distanceToAverage > localLongestDistance)
                        localLongestDistance = distanceToAverage;
                }

                //Calculate a average target offset with weights between 0-1 based on longestDistance
                //The longer a tank is from the average target, the more impact it will have on the weighted offset
                for (int i = 0; i < ActiveTargets.Count; i++)
                {
                    float multiplier = Remap(distances[i], 0, localLongestDistance, 0, 1);
                    //weightedAverageTarget += (directions[i] * Mathf.Pow(multiplier, furthestTargetWeightMultiplier));
                    weightedAverageTarget += directions[i] * (multiplier * FURTHEST_TARGET_WEIGHT_MULTIPLIER);
                }

                weightedAverageTarget /= ActiveTargets.Count;
                averageTarget += weightedAverageTarget; //Offset the average target with the weights 

                //Save weighted target for gizmo drawing
                weightedTargetGizmoPosition = averageTarget;
            }
            //If there is only 1-2 players, we just use straight up average positioning
            else if (ActiveTargets.Count > 0)
            {
                averageTarget /= ActiveTargets.Count;
            }
            else
            {
                averageTarget = transform.position;
            }
        }

        private static float Remap(float value, float oldFrom, float oldTo, float newFrom, float newTo)
        {
            return (value - oldFrom) / (oldTo - oldFrom) * (newTo - newFrom) + newFrom;
        }

        //Use trigonometry to calculate the distance and height of the camera given a distance and a camera tilt angle
        private void CalculateOffset()
        {
            const float modifiedAngle = CAMERA_TILT_ANGLE - 90;
            float zOffset = (Mathf.Sin(modifiedAngle * Mathf.Deg2Rad) * longestDistance);
            float yOffset = (Mathf.Cos(modifiedAngle * Mathf.Deg2Rad) * longestDistance);
            offset = new Vector3(0, yOffset, zOffset);
        }

        //Update the position and rotation of both the camera and the camera parent
        void UpdatePositionAndRotation()
        {
            //Camera local position and rotation
            Transform camTransform = MyCamera.transform;
            camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, offset, Time.fixedDeltaTime * 10f);
            camTransform.rotation = Quaternion.Lerp(camTransform.rotation, Quaternion.Euler(CAMERA_TILT_ANGLE, 0, 0), Time.fixedDeltaTime * 10f);

            //Camera parent position
            transform.position = Vector3.Lerp(transform.position, averageTarget, Time.fixedDeltaTime * MoveSpeed);
        }

        void ForceUpdatePositionAndRotation()
        {
            //Camera local position and rotation
            MyCamera.transform.localPosition = offset;
            MyCamera.transform.rotation = Quaternion.Euler(CAMERA_TILT_ANGLE, 0, 0);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(averageTarget, 0.4f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(weightedTargetGizmoPosition, 0.4f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(averageTargetGizmoPosition, 0.4f);
        }
    }
}