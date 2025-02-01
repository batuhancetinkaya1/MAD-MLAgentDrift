using UnityEngine;

public class RaycastSensor : MonoBehaviour
{
    [Header("Sensor Settings")]
    [SerializeField] private float sensorLength = 5f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private bool includeBackwardSensor = false;

    private int rayCount;
    public float[] distances;

    private void Awake()
    {
        // 180 dereceyi 15°'lik açýlarla bölerek ray sayýsýný hesapla
        rayCount = (180 / 15) + 1; // 13 ray olacak
        distances = new float[rayCount];

        // Baþlangýçta tüm mesafeleri maksimum olarak kabul et
        for (int i = 0; i < rayCount; i++)
        {
            distances[i] = sensorLength;
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < rayCount; i++)
        {
            float angle = -90f + (i * 15f); // -90'dan baþla, 15°'lik adýmlarla artýr
            Vector2 direction = Quaternion.Euler(0, 0, angle) * transform.up;
            distances[i] = GetSensorDistance(direction);
        }

        if (includeBackwardSensor)
        {
            float backwardDistance = GetSensorDistance(-transform.up);
            System.Array.Resize(ref distances, distances.Length + 1);
            distances[distances.Length - 1] = backwardDistance;
        }
    }

    private float GetSensorDistance(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, sensorLength, layerMask);
        if (hit.collider != null)
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
            return hit.distance;
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position + (Vector3)direction * sensorLength, Color.green);
            return sensorLength;
        }
    }

    public float GetSensorLength()
    {
        return sensorLength;
    }
}
