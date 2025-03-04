using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

[RequireComponent(typeof(BehaviorParameters))]
[RequireComponent(typeof(DecisionRequester))]
public class CarAgent : Agent
{
    [Header("References")]
    [SerializeField] private Car car;
    [SerializeField] private RaycastSensor raycastSensor;
    [SerializeField] private CheckPointManager checkpointManager;
    [SerializeField] private LayerMask trackLayer;

    [Header("Rewards")]
    [SerializeField] private float forwardSpeedReward = 0.0015f;
    [SerializeField] private float checkpointReward = 15f;
    [SerializeField] private float lapCompletionReward = 20f;

    [Header("Episode Settings")]
    [SerializeField] private float maxEpisodeTime = 10f;

    [Header("Punishment Open")]
    [SerializeField] private bool isPunishmentAllowed = false;

    [Header("Çarpma Cezaları")]
    [SerializeField] private float frontCollisionPenalty = -2.0f;
    [SerializeField] private float slightFrontCollisionPenalty = -1.0f;
    [SerializeField] private float sideCollisionPenalty = -0.5f;
    [SerializeField] private float backCollisionPenalty = -0.2f;
    [SerializeField] private float collisionMultiplier = -0.2f;
    [SerializeField] private float backwardPenalty = -25.0f;
    [SerializeField] private float maxCollisionMultipiliedPenalty = -3.0f;

    [Header("Çarpma Ayarları")]
    [SerializeField] private int maxCollisionsBeforeBigPenalty = 3;
    [SerializeField] private float minImpactSpeedThreshold = 2f;

    [Header("Inspector Observation")]
    [SerializeField] private int collisionCount = 0;
    [SerializeField] private int lap = 1;

    private int nextCheckpointIndex;
    int expectedCheckpointIndex;
    int backwardCheckpointIndex;
    private bool isCounterClockwise;
    private float episodeTime;

    public float maxCheckpointDistance = 50f;

    [Header("Initial Rotation (Euler)")]
    [SerializeField] private bool randomOrientation = true;
    [SerializeField] private float forwardOrientation = 0f;
    [SerializeField] private float backwardOrientation = 180f;

    public override void Initialize()
    {
        var decisionRequester = GetComponent<DecisionRequester>();
        decisionRequester.DecisionPeriod = 1;
        decisionRequester.TakeActionsBetweenDecisions = true;
    }

    public override void OnEpisodeBegin()
    {
        if (randomOrientation)
        {
            isCounterClockwise = (Random.value > 0.5f);
        }

        CheckPoint startCheckpoint = checkpointManager.GetCheckpointByIndex(0);
        if (startCheckpoint == null) return;

        car.ResetPhysics();
        car.transform.position = startCheckpoint.transform.position;
        float chosenAngle = isCounterClockwise ? backwardOrientation : forwardOrientation;
        car.transform.rotation = Quaternion.Euler(0f, 0f, chosenAngle);
        checkpointManager.ResetAllCheckpoints();
        int totalCheckpoints = checkpointManager.TotalCheckpoints;
        nextCheckpointIndex = isCounterClockwise ? 1 : totalCheckpoints - 1;
        collisionCount = 0;
        lap = 1;
        episodeTime = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (raycastSensor != null && raycastSensor.distances != null && raycastSensor.distances.Length > 0)
        {
            int totalRays = raycastSensor.distances.Length;
            float directSum = 0f, frontSum = 0f, leftSum = 0f, rightSum = 0f, backSum = 0f;
            int directCount = 0, frontCount = 0, leftCount = 0, rightCount = 0, backCount = 0;

            Vector2[] localRayDirections = raycastSensor.GetLocalRayDirections();

            for (int i = 0; i < totalRays; i++)
            {
                float normalizedDist = raycastSensor.distances[i] / raycastSensor.GetSensorLength();
                Vector2 worldDirection = transform.rotation * localRayDirections[i];
                float signedAngle = Vector2.SignedAngle(transform.up, worldDirection);
                float importance = 1f;
                if (Mathf.Abs(signedAngle) == 0f)
                {
                    importance = 2f;
                    directSum += normalizedDist * importance;
                    directCount++;
                }
                else if (Mathf.Abs(signedAngle) <= 15f)
                {
                    importance = 1.5f;
                    frontSum += normalizedDist * importance;
                    frontCount++;
                }
                else if (Mathf.Abs(signedAngle) <= 90f)
                {
                    if (signedAngle >= 0f)
                    {
                        rightSum += normalizedDist * importance;
                        rightCount++;
                    }
                    else
                    {
                        leftSum += normalizedDist * importance;
                        leftCount++;
                    }
                }
                else if (Mathf.Abs(signedAngle) <= 135f)
                {
                    importance = 0.5f;
                    backSum += normalizedDist * importance;
                    backCount++;
                }
                else
                {
                    importance = 0.2f;
                    backSum += normalizedDist * importance;
                    backCount++;
                }
            }

            float dAvg = directCount > 0 ? directSum / directCount : 0f;
            float fAvg = frontCount > 0 ? frontSum / frontCount : 1f;
            float lAvg = leftCount > 0 ? leftSum / leftCount : 1f;
            float rAvg = rightCount > 0 ? rightSum / rightCount : 1f;
            float bAvg = backCount > 0 ? backSum / backCount : 1f;

            sensor.AddObservation(dAvg);
            sensor.AddObservation(fAvg);
            sensor.AddObservation(lAvg);
            sensor.AddObservation(rAvg);
            sensor.AddObservation(bAvg);
            sensor.AddObservation(lAvg - rAvg);
        }
        else
        {
            sensor.AddObservation(1f);
            sensor.AddObservation(1f);
            sensor.AddObservation(1f);
            sensor.AddObservation(1f);
            sensor.AddObservation(1f);
            sensor.AddObservation(0f);
        }

        Vector2 velocity = car.rb.linearVelocity;
        float forwardSpeed = Vector2.Dot(velocity, car.transform.up);
        float lateralSpeed = Vector2.Dot(velocity, car.transform.right);

        sensor.AddObservation(Mathf.Clamp(forwardSpeed / car.maxSpeed, 0f, 1f));
        sensor.AddObservation(Mathf.Clamp(lateralSpeed / car.maxSpeed, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp01(velocity.magnitude / car.maxSpeed));
        sensor.AddObservation(Mathf.Clamp(car.rb.angularVelocity / car.turnSpeed, -1f, 1f));

        Vector2 upDir = car.transform.up.normalized;
        sensor.AddObservation(upDir.x);
        sensor.AddObservation(upDir.y);

        CheckPoint nextCP = checkpointManager.GetCheckpointByIndex(nextCheckpointIndex);
        if (nextCP != null)
        {
            Vector2 cpPosition = (Vector2)nextCP.transform.position;
            Vector2 relativePos = cpPosition - (Vector2)car.transform.position;
            float distance = relativePos.magnitude;
            Vector2 directionToCP = relativePos.normalized;
            float angle = Vector2.SignedAngle(car.transform.up, directionToCP);
            sensor.AddObservation(Mathf.Clamp01(distance / maxCheckpointDistance));
            sensor.AddObservation(angle / 180f);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float turnInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        car.SetInputs(moveInput, turnInput);

        Vector2 forwardDir = car.transform.up;
        float forwardSpeed = Vector2.Dot(car.rb.linearVelocity, forwardDir);

        if (forwardSpeed > 0f)
        {
            AddReward(forwardSpeed * forwardSpeedReward);
        }

        episodeTime += Time.fixedDeltaTime;
        if (episodeTime > maxEpisodeTime)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var contActions = actionsOut.ContinuousActions;
        contActions[0] = Input.GetAxis("Vertical");
        contActions[1] = Input.GetAxis("Horizontal");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isPunishmentAllowed) return;
        if (!collision.gameObject.CompareTag("Grid"))
            return;

        Vector2 collisionNormal = collision.contacts[0].normal;
        Vector2 carForward = transform.up;
        float impactAngle = Vector2.SignedAngle(carForward, -collisionNormal);
        float impactSpeed = car.rb.linearVelocity.magnitude;
        float penalty = 0f;

        if (Mathf.Abs(impactAngle) < 0.1f)
        {
            penalty = frontCollisionPenalty;
        }
        else if (Mathf.Abs(impactAngle) <= 30f)
        {
            penalty = slightFrontCollisionPenalty;
        }
        else if (Mathf.Abs(impactAngle) <= 90f)
        {
            penalty = sideCollisionPenalty;
        }
        else
        {
            penalty = backCollisionPenalty;
            return;
        }

        if (impactSpeed < minImpactSpeedThreshold)
        {
            penalty *= 0.5f;
        }

        if (collisionCount < maxCollisionsBeforeBigPenalty)
        {
            AddReward(penalty);
        }
        else
        {
            float additionalPenalty = Mathf.Min(collisionMultiplier * collisionCount, maxCollisionMultipiliedPenalty);
            AddReward(penalty + additionalPenalty);
        }

        collisionCount++;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckPoint cp = other.GetComponent<CheckPoint>();
        if (cp == null) return;

        int total = checkpointManager.TotalCheckpoints;
        expectedCheckpointIndex = nextCheckpointIndex;
        backwardCheckpointIndex = isCounterClockwise
            ? (nextCheckpointIndex - 2 + total) % total
            : (nextCheckpointIndex + 2) % total;

        if (cp.CheckpointIndex == expectedCheckpointIndex)
        {
            float timeTaken = episodeTime;
            episodeTime = 0f;
            float timeBonus = Mathf.Max(0, (maxEpisodeTime - timeTaken));
            float totalReward = checkpointReward + timeBonus;
            AddReward(totalReward);

            if (nextCheckpointIndex == 0)
            {
                AddReward(lapCompletionReward);
                lap++;
                if (lap > 5)
                {
                    EndEpisode();
                    return;
                }
            }

            nextCheckpointIndex = isCounterClockwise
                ? (nextCheckpointIndex + 1) % total
                : (nextCheckpointIndex - 1 + total) % total;
        }
        else if (cp.CheckpointIndex == backwardCheckpointIndex && isPunishmentAllowed)
        {
            AddReward(backwardPenalty);
            EndEpisode();
        }
    }

}
