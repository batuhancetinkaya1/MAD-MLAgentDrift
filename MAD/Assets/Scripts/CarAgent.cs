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
    [SerializeField] private float forwardSpeedReward = 0.002f;
    [SerializeField] private float checkpointReward = 5f;
    [SerializeField] private float finalLapReward = 10f;

    [Header("Episode Settings")]
    [SerializeField] private int totalCheckpoints = 8;
    [SerializeField] private float maxEpisodeTime = 45f;

    private int nextCheckpointIndex;
    private float episodeTime;

    public override void Initialize()
    {
        var decisionRequester = GetComponent<DecisionRequester>();
        decisionRequester.DecisionPeriod = 1;
        decisionRequester.TakeActionsBetweenDecisions = true;
    }

    public override void OnEpisodeBegin()
    {
        ResetCar();
        checkpointManager.ResetAllCheckpoints();
        nextCheckpointIndex = 1;
        episodeTime = 0f;
    }

    private void ResetCar()
    {
        car.transform.position = Vector3.zero;
        car.transform.rotation = Quaternion.Euler(0, 0, -90);
        car.rb.linearVelocity = Vector2.zero;
        car.rb.angularVelocity = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (raycastSensor == null || raycastSensor.distances == null || raycastSensor.distances.Length == 0)
        {
            Debug.LogWarning("⚠️ Raycast distances array is empty or null! Using default values.");
            for (int i = 0; i < 7; i++) sensor.AddObservation(1f); // Varsayılan max mesafe
            sensor.AddObservation(0f); // Forward speed
            sensor.AddObservation(0f); // Lateral speed
            sensor.AddObservation(1f); // Forward clearance
            sensor.AddObservation(1f); // Right clearance
            sensor.AddObservation(1f); // Left clearance
            sensor.AddObservation(1f); // Front right clearance
            sensor.AddObservation(1f); // Front left clearance
            return;
        }

        float maxSensorLength = raycastSensor.GetSensorLength();
        float maxSpeed = 10f; // Araç için maksimum hız değeri (deneyerek belirleyebilirsin)

        // **Tüm Raycast Mesafelerini Normalize Et**
        foreach (float dist in raycastSensor.distances)
        {
            sensor.AddObservation(dist / maxSensorLength);
        }

        float forwardSpeed = Vector2.Dot(car.rb.linearVelocity, car.transform.up);
        float lateralSpeed = Vector2.Dot(car.rb.linearVelocity, car.transform.right);

        int middleIndex = raycastSensor.distances.Length / 2;
        float forwardClearance = raycastSensor.distances[middleIndex] / maxSensorLength;
        float rightClearance = raycastSensor.distances[raycastSensor.distances.Length - 1] / maxSensorLength;
        float leftClearance = raycastSensor.distances[0] / maxSensorLength;
        float frontRightClearance = raycastSensor.distances[middleIndex + 2] / maxSensorLength;
        float frontLeftClearance = raycastSensor.distances[middleIndex - 2] / maxSensorLength;

        // **Hızları Normalize Et**
        sensor.AddObservation(forwardSpeed / maxSpeed);
        sensor.AddObservation(lateralSpeed / maxSpeed);

        sensor.AddObservation(forwardClearance);
        sensor.AddObservation(rightClearance);
        sensor.AddObservation(leftClearance);
        sensor.AddObservation(frontRightClearance);
        sensor.AddObservation(frontLeftClearance);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveInput = actions.ContinuousActions[0];
        float turnInput = actions.ContinuousActions[1];

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckPoint cp = other.GetComponent<CheckPoint>();
        if (cp != null && cp.CheckpointIndex == nextCheckpointIndex)
        {
            // Checkpoint'e ulaşma süresi
            float timeTaken = episodeTime;
            episodeTime = 0f; // Yeni süreyi sıfırla

            // Süre bazlı ek ödül (hızlı ulaşırsa daha fazla alacak)
            float timeBonus = Mathf.Max(0, (10f - timeTaken)); // 10 saniyenin altı daha fazla ödül

            // **Garantili Checkpoint Ödülü**: (speed reward toplamından büyük olmalı)
            float totalReward = checkpointReward + timeBonus;

            AddReward(totalReward);
            nextCheckpointIndex++;

            if (nextCheckpointIndex >= totalCheckpoints)
            {
                nextCheckpointIndex = 0;
                AddReward(finalLapReward);
            }
        }
    }

}
