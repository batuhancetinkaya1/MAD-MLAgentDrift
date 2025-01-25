using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    [SerializeField] private CheckPoint[] checkPoints;
    public int TotalCheckpoints => checkPoints.Length;

    public CheckPoint GetCheckpointByIndex(int index)
    {
        if (index < 0 || index >= checkPoints.Length) return null;
        return checkPoints[index];
    }
}
