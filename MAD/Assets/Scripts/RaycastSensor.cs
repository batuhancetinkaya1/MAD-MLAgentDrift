using UnityEngine;

public class RaycastSensor : MonoBehaviour
{
    [Header("Sensor Settings")]
    [SerializeField] private float sensorLength = 5f;    // Raycast uzunlu�u
    [SerializeField] private LayerMask layerMask;        // Engeller / duvarlar hangi layer'da?

    public float[] distances; // Her bir sens�r i�in buldu�umuz mesafeler

    private void Update()
    {
        distances = new float[8]; // 8 y�n

        // �rnek: 0 - �n, 1 - arka, 2/3/4 - sol y�nler, 5/6/7 - sa� y�nler
        distances[0] = GetSensorDistance(Vector2.right);
        distances[1] = GetSensorDistance(Vector2.left);

        distances[2] = GetSensorDistance(Vector2.up);
        distances[3] = GetSensorDistance(Quaternion.Euler(0, 0, 45) * Vector2.up);
        distances[4] = GetSensorDistance(Quaternion.Euler(0, 0, -45) * Vector2.up);

        distances[5] = GetSensorDistance(Vector2.down);
        distances[6] = GetSensorDistance(Quaternion.Euler(0, 0, 135) * Vector2.up);
        distances[7] = GetSensorDistance(Quaternion.Euler(0, 0, -135) * Vector2.up);
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
            return sensorLength; // Maksimum mesafe
        }
    }
}
