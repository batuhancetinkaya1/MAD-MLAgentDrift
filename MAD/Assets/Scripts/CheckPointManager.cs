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

    // **Tüm checkpoint’leri sýfýrlamak için yeni metod**
    public void ResetAllCheckpoints()
    {
        foreach (var checkpoint in checkPoints)
        {
            checkpoint.gameObject.SetActive(true);
        }
    }
}
