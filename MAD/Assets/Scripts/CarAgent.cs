using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(BehaviorParameters))]
public class CarAgent : Agent
{
    [Header("References")]
    [SerializeField] private Car car;                     // Car hareket scripti (Car.cs)
    [SerializeField] private RaycastSensor raycastSensor; // Mesafe sensörü (RaycastSensor.cs)
    [SerializeField] private LayerMask trackLayer;        // Duvar/pist dışı layer

    [Header("Rewards")]
    [SerializeField] private float speedRewardMultiplier = 0.001f;
    [SerializeField] private float driftRewardMultiplier = 0.0005f;
    [SerializeField] private float collisionPenalty = -1f;
    [SerializeField] private float checkpointReward = 1f;
    [SerializeField] private float finalLapReward = 5f;

    [Header("Drift Settings")]
    [Tooltip("Minimum açı (derece) araç yanlamaya başlayınca kabul edilsin.")]
    [SerializeField] private float minDriftAngle = 15f;
    [Tooltip("Maksimum açı (derece) araç hâlâ drift kabul edilsin.")]
    [SerializeField] private float maxDriftAngle = 80f;

    // Checkpoint takibi
    private int nextCheckpointIndex;
    private float lastCheckpointTime;

    // Varsayalım 8 adet checkpoint var
    private const int totalCheckpoints = 8;

    public override void Initialize()
    {
        // Burada MaxStep vs. ayarlamak isterseniz
        // MaxStep = 5000;
    }

    public override void OnEpisodeBegin()
    {
        // Her episode başlarken aracı sıfırla (konum, hız, açı)
        ResetCar();
        // Checkpoint takibini sıfırla
        nextCheckpointIndex = 0;
        lastCheckpointTime = Time.time;
    }

    private void ResetCar()
    {
        // Aracı sahnede istediğiniz konuma ve açıya yerleştirebilirsiniz.
        // Örneğin basitçe (0,0) konumuna ve 0 derece dönüklüğe:
        car.transform.position = Vector3.zero;
        car.transform.rotation = Quaternion.identity;

        // Rigidbody hızları sıfırla
        car.rb.linearVelocity = Vector2.zero;
        car.rb.angularVelocity = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1) Raycast sensör mesafeleri
        if (raycastSensor != null)
        {
            foreach (float dist in raycastSensor.distances)
            {
                sensor.AddObservation(dist);
            }
        }

        // 2) Aracın hızı
        float speed = car.rb.linearVelocity.magnitude;
        sensor.AddObservation(speed);

        // 3) Sıradaki checkpoint indeksi
        sensor.AddObservation(nextCheckpointIndex);

        // 4) Aracın yönü (Z euler ya da forward vektör)
        sensor.AddObservation(car.transform.rotation.eulerAngles.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // ContinuousActions[0] = ileri/fren (gaz değeri)
        // ContinuousActions[1] = direksiyon
        float moveInput = actions.ContinuousActions[0];
        float turnInput = actions.ContinuousActions[1];

        car.SetInputs(moveInput, turnInput);

        // --- Ödül Mantıkları ---

        // 1) Hız ödülü
        float speed = car.rb.linearVelocity.magnitude;
        AddReward(speed * speedRewardMultiplier);

        // 2) Drift ödülü
        if (speed > 1f)
        {
            // Araç "transform.up" vektörü ile hız vektörü arasındaki açı
            float angle = Vector2.Angle(car.transform.up, car.rb.linearVelocity.normalized);
            if (angle > minDriftAngle && angle < maxDriftAngle)
            {
                float driftFactor = angle / 90f; // 0-1 arasına normalize bir değer
                AddReward(driftFactor * driftRewardMultiplier * speed);
            }
        }
    }

    /// <summary>
    /// Checkpoint objeleri (isTrigger collider) üzerinden geçiş yapıldığında çalışır.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Checkpoint scriptini al
        CheckPoint cp = other.GetComponent<CheckPoint>();
        if (cp != null)
        {
            // Beklediğimiz checkpoint mi?
            if (cp.CheckpointIndex == nextCheckpointIndex)
            {
                // Basit bir zaman bazlı değerlendirme
                float currentTime = Time.time;
                float timeSinceLast = currentTime - lastCheckpointTime;
                lastCheckpointTime = currentTime;

                // Checkpoint ödülü
                AddReward(checkpointReward);

                // Opsiyonel: Daha hızlı geldiyse küçük bir bonus (örnek)
                float bonus = Mathf.Clamp(3f - timeSinceLast, 0f, 3f) * 0.1f;
                AddReward(bonus);

                nextCheckpointIndex++;

                // Son checkpoint'e ulaştıysak tur tamam
                if (nextCheckpointIndex >= totalCheckpoints)
                {
                    AddReward(finalLapReward);
                    EndEpisode();
                }
            }
            else
            {
                // Yanlış checkpoint (sırayı bozdu)
                AddReward(-0.1f);
            }
        }
    }

    /// <summary>
    /// Duvara/pist dışına çarparsa ceza ve episode sonlandırma
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // LayerMask ile kontrol
        if (((1 << collision.gameObject.layer) & trackLayer) != 0)
        {
            // Çarpma cezası
            AddReward(collisionPenalty);
            EndEpisode();
        }
    }
}
