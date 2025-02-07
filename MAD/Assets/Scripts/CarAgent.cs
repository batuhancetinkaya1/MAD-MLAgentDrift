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
    [SerializeField] private float frontCollisionPenalty = -2.0f; // 0° çarpma (tam ön)
    [SerializeField] private float slightFrontCollisionPenalty = -1.0f; // ±30° çarpma (ön yanlar)
    [SerializeField] private float sideCollisionPenalty = -0.5f; // ±90° çarpma (yan çarpmalar)
    [SerializeField] private float backCollisionPenalty = -0.2f; // ±90° çarpma (yan çarpmalar)
    [SerializeField] private float collisionMultiplier = -0.2f; // Her ek çarpma için ekstra ceza
    [SerializeField] private float backwardPenalty = -25.0f;
    [SerializeField] private float maxCollisionMultipiliedPenalty = -3.0f;

    [Header("Çarpma Ayarları")]
    [SerializeField] private int maxCollisionsBeforeBigPenalty = 3; // Kaç çarpmadan sonra ekstra ceza verilecek?
    [SerializeField] private float minImpactSpeedThreshold = 2f;

    [Header("Inspector Observation")]
    [SerializeField] private int collisionCount = 0; // Kaç kere çarptığını takip ediyoruz
    [SerializeField] private int lap = 1; // Kaç kere çarptığını takip ediyoruz

    private int nextCheckpointIndex;
    int expectedCheckpointIndex;
    int backwardCheckpointIndex;
    private bool isCounterClockwise;
    private float episodeTime;

    public override void Initialize()
    {
        var decisionRequester = GetComponent<DecisionRequester>();
        decisionRequester.DecisionPeriod = 1;
        decisionRequester.TakeActionsBetweenDecisions = true;
    }

    public override void OnEpisodeBegin()
    {
        isCounterClockwise = Random.value > 0.5f;

        CheckPoint startCheckpoint = checkpointManager.GetCheckpointByIndex(0);
        if (startCheckpoint == null) return;

        car.ResetPhysics();

        car.transform.position = startCheckpoint.transform.position;
        car.transform.rotation = isCounterClockwise
            ? Quaternion.Euler(0f, 0f, -90f)
            : Quaternion.Euler(0f, 0f, 90f);

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

            // RaycastSensor içindeki yerel ray yönlerini elde ediyoruz.
            Vector2[] localRayDirections = raycastSensor.GetLocalRayDirections();

            for (int i = 0; i < totalRays; i++)
            {
                // Normalleştirilmiş mesafe: [0,1] arası
                float normalizedDist = raycastSensor.distances[i] / raycastSensor.GetSensorLength();
                // Ray'un yönü ile aracın önü (Vector2.up) arasındaki açıyı hesaplıyoruz.
                // Bu açı, aracın yerel koordinat sistemine göre konumlandırılmıştır.
                Vector2 worldDirection = transform.rotation * localRayDirections[i];
                float signedAngle = Vector2.SignedAngle(transform.up, worldDirection);
                // Varsayılan ağırlık değeri 1
                float importance = 1f;
                if (Mathf.Abs(signedAngle) == 0f)
                {
                    // Tam ön: en büyük önem veriliyor.
                    importance = 2f;
                    directSum += normalizedDist * importance;
                    directCount++;
                }
                // Açıya göre hangi bölgeye ait olduğunu belirliyoruz.
                else if (Mathf.Abs(signedAngle) <= 15f)
                {
                    // Ön: ekstra önem veriliyor.
                    importance = 1.5f;
                    frontSum += normalizedDist * importance;
                    frontCount++;
                }
                else if (Mathf.Abs(signedAngle) <= 90f)
                {
                    // Sağ veya sol bölgeler
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
                    // Arka bölge, orta önem
                    importance = 0.5f;
                    backSum += normalizedDist * importance;
                    backCount++;
                }
                else
                {
                    // Arka bölgenin daha yan kısımları, düşük önem
                    importance = 0.2f;
                    backSum += normalizedDist * importance;
                    backCount++;
                }
            }

            // Her bölgenin ortalamasını hesaplıyoruz (eğer bölgeden ray yoksa varsayılan 1f veriliyor)
            float dAvg = directCount > 0 ? directSum / directCount : 0f;
            float fAvg = frontCount > 0 ? frontSum / frontCount : 1f;
            float lAvg = leftCount > 0 ? leftSum / leftCount : 1f;
            float rAvg = rightCount > 0 ? rightSum / rightCount : 1f;
            float bAvg = backCount > 0 ? backSum / backCount : 1f;

            // Gözlemleri ekliyoruz:
            sensor.AddObservation(dAvg);
            sensor.AddObservation(fAvg);         // Öne yönelik ortalama
            sensor.AddObservation(lAvg);         // Sola yönelik ortalama
            sensor.AddObservation(rAvg);         // Sağa yönelik ortalama
            sensor.AddObservation(bAvg);         // Arkaya yönelik ortalama
            sensor.AddObservation(lAvg - rAvg);    // Sol-sağ farkı
        }
        else
        {
            // Raycast verisi alınamadıysa güvenli (varsayılan) değerler ekleniyor.
            sensor.AddObservation(1f);
            sensor.AddObservation(1f);
            sensor.AddObservation(1f);
            sensor.AddObservation(1f);
            sensor.AddObservation(1f);
            sensor.AddObservation(0f);
        }

        // Ek olarak aracın hız ve açısal hız bilgileri de gözlem olarak ekleniyor.
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

        // Sadece "Grid" tag’ine sahip nesnelerle çarpışmaları kontrol et
        if (!collision.gameObject.CompareTag("Grid"))
            return;

        // Çarpışma temasındaki ilk noktanın normalini alıyoruz
        Vector2 collisionNormal = collision.contacts[0].normal;
        Vector2 carForward = transform.up;
        // Aracın ön yönü ile duvar normali arasında açıyı hesaplıyoruz.
        // (Negatif normal kullanarak, arabanın neresine çarptığını yorumluyoruz.)
        float impactAngle = Vector2.SignedAngle(carForward, -collisionNormal);
        float impactSpeed = car.rb.linearVelocity.magnitude; // Çarpma anındaki hız

        float penalty = 0f;

        // Açıya göre ceza belirleme:
        // Eğer çarpışma tam önden (0 dereceye çok yakın) gerçekleşiyorsa, en ağır ceza;
        // 0'dan 30 derece arası (ön yanlar) orta ceza;
        // 30 ile 90 derece arası (yan) hafif ceza verilir.
        // 90 dereceden büyük açılar (arka) için ceza uygulanmaz.
        if (Mathf.Abs(impactAngle) < 0.1f)
        {
            // Neredeyse tam önden çarpmada (0°)
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

        // Düşük hızda çarpma durumunda ceza yarıya indiriliyor
        if (impactSpeed < minImpactSpeedThreshold)
        {
            penalty *= 0.5f;
        }

        // Artan ceza: Belirli sayıda çarpmadan sonra (maxCollisionsBeforeBigPenalty)
        // ek ceza uygulanıyor. Aşağıdaki if–else yapısı ile bu durumu yönetiyoruz.
        if (collisionCount < maxCollisionsBeforeBigPenalty)
        {
            AddReward(penalty);
        }
        else
        {
            // collisionCount arttıkça ek ceza ekle, ancak toplam ek ceza -3.0f ile sınırlandırılsın.
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

        // Beklenen doğru checkpoint'in index'i
        expectedCheckpointIndex = nextCheckpointIndex;
        // Geride kalınan (geri dönüş) checkpoint index'i:
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
            }

            // Doğru checkpoint'e geçtikten sonra sonraki checkpoint'i ayarla:
            nextCheckpointIndex = isCounterClockwise
                ? (nextCheckpointIndex + 1) % total
                : (nextCheckpointIndex - 1 + total) % total;
        }
        // Eğer ajan, beklenen checkpoint yerine geride kalan checkpoint'ten geçerse
        else if (cp.CheckpointIndex == backwardCheckpointIndex && isPunishmentAllowed)
        {
            AddReward(backwardPenalty);

            EndEpisode();
        }
    }

}
