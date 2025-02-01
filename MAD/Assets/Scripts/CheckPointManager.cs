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

    // **T�m checkpoint�leri s�f�rlamak i�in yeni metod**
    public void ResetAllCheckpoints()
    {
        foreach (var checkpoint in checkPoints)
        {
            checkpoint.gameObject.SetActive(true);
        }
    }
}
