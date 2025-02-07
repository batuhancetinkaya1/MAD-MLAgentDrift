using UnityEngine;

public class RaycastSensor : MonoBehaviour
{
    [SerializeField] private float sensorLength = 5f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] public int angleStep = 15;
    [SerializeField] private bool drawDebugLines = true;

    private int rayCount;
    private Vector2[] localRayDirections;
    public float[] distances { get; private set; }

    private void Awake()
    {
        rayCount = Mathf.RoundToInt(360f / angleStep);
        distances = new float[rayCount];
        localRayDirections = new Vector2[rayCount];
        for (int i = 0; i < rayCount; i++)
        {
            float angle = (i * 360f / rayCount);
            localRayDirections[i] = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
            distances[i] = sensorLength;
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < rayCount; i++)
        {
            Vector2 worldDirection = transform.rotation * localRayDirections[i];
            distances[i] = sensorLength;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, worldDirection, sensorLength, layerMask);
            if (hit.collider != null) distances[i] = hit.distance;
            if (drawDebugLines)
            {
                if (hit.collider != null)
                    Debug.DrawLine(transform.position, hit.point, Color.red);
                else
                    Debug.DrawLine(transform.position, transform.position + (Vector3)worldDirection * sensorLength, Color.HSVToRGB((float)i / rayCount, 1f, 1f));
            }
        }
    }

    public float GetSensorLength()
    {
        return sensorLength;
    }

    public Vector2[] GetLocalRayDirections()
    {
        return localRayDirections;
    }
}
