using UnityEngine;

public class CorruptionSystem : MonoBehaviour
{
    public enum CorruptionStage
    {
        Low,
        Mid,
        High
    }

    [Header("Corruption Values")]
    public int currentCorruption = 0;
    public int maxCorruption = 100;
    public int midCorruptionThreshold = 35;
    public int highCorruptionThreshold = 70;

    public void AddCorruption(int amount)
    {
        currentCorruption = Mathf.Clamp(currentCorruption + amount, 0, maxCorruption);
        Debug.Log("Corruption Increased: " + currentCorruption + "/" + maxCorruption);
    }

    public void ReduceCorruption(int amount)
    {
        currentCorruption = Mathf.Clamp(currentCorruption - amount, 0, maxCorruption);
        Debug.Log("Corruption Reduced: " + currentCorruption + "/" + maxCorruption);
    }

    public float GetCorruptionRatio()
    {
        if (maxCorruption <= 0)
        {
            return 0f;
        }

        return (float)currentCorruption / maxCorruption;
    }

    public CorruptionStage GetCurrentStage()
    {
        if (currentCorruption >= highCorruptionThreshold)
        {
            return CorruptionStage.High;
        }

        if (currentCorruption >= midCorruptionThreshold)
        {
            return CorruptionStage.Mid;
        }

        return CorruptionStage.Low;
    }
}
