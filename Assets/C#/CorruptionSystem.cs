using UnityEngine;
using UnityEngine.Serialization;

public class CorruptionSystem : MonoBehaviour
{
    public enum CorruptionStage
    {
        Low,
        Mid,
        High
    }

    public enum AlignmentBias
    {
        Balanced,
        Oathbound,
        Fallen
    }

    [Header("Corruption Values")]
    [FormerlySerializedAs("currentCorruption")]
    public int corruptionValue = 0;
    public int maxCorruption = 100;
    public int midCorruptionThreshold = 35;
    public int highCorruptionThreshold = 70;

    [Header("Oath Values")]
    public int oathValue = 0;
    public int maxOath = 100;

    public void AddCorruption(int amount)
    {
        corruptionValue = Mathf.Clamp(corruptionValue + amount, 0, maxCorruption);
    }

    public void ReduceCorruption(int amount)
    {
        corruptionValue = Mathf.Clamp(corruptionValue - amount, 0, maxCorruption);
    }

    public void AddOath(int amount)
    {
        oathValue = Mathf.Clamp(oathValue + amount, 0, maxOath);
    }

    public void ReduceOath(int amount)
    {
        oathValue = Mathf.Clamp(oathValue - amount, 0, maxOath);
    }

    public float GetCorruptionRatio()
    {
        if (maxCorruption <= 0)
        {
            return 0f;
        }

        return (float)corruptionValue / maxCorruption;
    }

    public float GetOathRatio()
    {
        if (maxOath <= 0)
        {
            return 0f;
        }

        return (float)oathValue / maxOath;
    }

    public CorruptionStage GetCurrentStage()
    {
        if (corruptionValue >= highCorruptionThreshold)
        {
            return CorruptionStage.High;
        }

        if (corruptionValue >= midCorruptionThreshold)
        {
            return CorruptionStage.Mid;
        }

        return CorruptionStage.Low;
    }

    public AlignmentBias GetAlignmentBias()
    {
        int difference = oathValue - corruptionValue;
        if (difference >= 10)
        {
            return AlignmentBias.Oathbound;
        }

        if (difference <= -10)
        {
            return AlignmentBias.Fallen;
        }

        return AlignmentBias.Balanced;
    }

    public string GetAlignmentSummary()
    {
        AlignmentBias bias = GetAlignmentBias();
        string biasLabel = "Balanced";
        if (bias == AlignmentBias.Oathbound)
        {
            biasLabel = "Oathbound";
        }
        else if (bias == AlignmentBias.Fallen)
        {
            biasLabel = "Fallen";
        }

        return "Oath " + oathValue + " | Corruption " + corruptionValue + " | Bias: " + biasLabel;
    }
}
