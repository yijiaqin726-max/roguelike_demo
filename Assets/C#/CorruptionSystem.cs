using UnityEngine;
using UnityEngine.Serialization;
using System;

public class CorruptionSystem : MonoBehaviour
{
    public enum CorruptionStage
    {
        Ordered,
        Unsteady,
        Uncontrolled
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
    public int unsteadyCorruptionThreshold = 30;
    public int uncontrolledCorruptionThreshold = 60;

    [Header("Dark Pulse Pressure")]
    [Range(0f, 1f)] public float pulseChanceUnsteady = 0.2f;
    [Range(0f, 1f)] public float pulseChanceUncontrolled = 0.45f;
    public float pulseRadiusUnsteady = 1.8f;
    public float pulseRadiusUncontrolled = 2.4f;
    public int pulseDamageUnsteady = 1;
    public int pulseDamageUncontrolled = 2;
    public float pulseKnockbackUnsteady = 0.2f;
    public float pulseKnockbackUncontrolled = 0.3f;
    [Range(0f, 1f)] public float eliteRateBonusUnsteady = 0.08f;
    [Range(0f, 1f)] public float eliteRateBonusUncontrolled = 0.18f;
    [Range(-1f, 0f)] public float spawnIntervalModifierUnsteady = -0.1f;
    [Range(-1f, 0f)] public float spawnIntervalModifierUncontrolled = -0.22f;
    [Range(0f, 1f)] public float dangerEnemyWeightBonusUncontrolled = 0.25f;

    [Header("Oath Values")]
    public int oathValue = 0;
    public int maxOath = 100;

    public event Action<CorruptionStage, CorruptionStage> CorruptionStageChanged;

    private CorruptionStage currentStage;

    private void Awake()
    {
        currentStage = EvaluateStage();
    }

    public void AddCorruption(int amount)
    {
        corruptionValue = Mathf.Clamp(corruptionValue + amount, 0, maxCorruption);
        RefreshStage();
    }

    public void ReduceCorruption(int amount)
    {
        corruptionValue = Mathf.Clamp(corruptionValue - amount, 0, maxCorruption);
        RefreshStage();
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
        return currentStage;
    }

    public float GetPulseChance()
    {
        switch (currentStage)
        {
            case CorruptionStage.Unsteady:
                return pulseChanceUnsteady;
            case CorruptionStage.Uncontrolled:
                return pulseChanceUncontrolled;
            default:
                return 0f;
        }
    }

    public float GetPulseRadius()
    {
        switch (currentStage)
        {
            case CorruptionStage.Unsteady:
                return pulseRadiusUnsteady;
            case CorruptionStage.Uncontrolled:
                return pulseRadiusUncontrolled;
            default:
                return 0f;
        }
    }

    public int GetPulseDamage()
    {
        switch (currentStage)
        {
            case CorruptionStage.Unsteady:
                return pulseDamageUnsteady;
            case CorruptionStage.Uncontrolled:
                return pulseDamageUncontrolled;
            default:
                return 0;
        }
    }

    public float GetPulseKnockback()
    {
        switch (currentStage)
        {
            case CorruptionStage.Unsteady:
                return pulseKnockbackUnsteady;
            case CorruptionStage.Uncontrolled:
                return pulseKnockbackUncontrolled;
            default:
                return 0f;
        }
    }

    public float GetEliteRateBonus()
    {
        switch (currentStage)
        {
            case CorruptionStage.Unsteady:
                return eliteRateBonusUnsteady;
            case CorruptionStage.Uncontrolled:
                return eliteRateBonusUncontrolled;
            default:
                return 0f;
        }
    }

    public float GetSpawnIntervalModifier()
    {
        switch (currentStage)
        {
            case CorruptionStage.Unsteady:
                return spawnIntervalModifierUnsteady;
            case CorruptionStage.Uncontrolled:
                return spawnIntervalModifierUncontrolled;
            default:
                return 0f;
        }
    }

    public float GetDangerEnemyWeightBonus()
    {
        return currentStage == CorruptionStage.Uncontrolled ? dangerEnemyWeightBonusUncontrolled : 0f;
    }

    public string GetStageLabel()
    {
        switch (currentStage)
        {
            case CorruptionStage.Unsteady:
                return "Unsteady";
            case CorruptionStage.Uncontrolled:
                return "Uncontrolled";
            default:
                return "Ordered";
        }
    }

    public string GetStageTransitionMessage(CorruptionStage stage)
    {
        switch (stage)
        {
            case CorruptionStage.Unsteady:
                return "Corruption Rising";
            case CorruptionStage.Uncontrolled:
                return "Control Is Slipping";
            default:
                return string.Empty;
        }
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

        return "Oath " + oathValue + " | Corruption " + corruptionValue + " | Bias: " + biasLabel + " | Stage: " + GetStageLabel();
    }

    public void SyncStage()
    {
        CorruptionStage previousStage = currentStage;
        currentStage = EvaluateStage();
        if (previousStage != currentStage)
        {
            CorruptionStageChanged?.Invoke(previousStage, currentStage);
        }
    }

    private void RefreshStage()
    {
        CorruptionStage nextStage = EvaluateStage();
        if (nextStage == currentStage)
        {
            return;
        }

        CorruptionStage previousStage = currentStage;
        currentStage = nextStage;
        CorruptionStageChanged?.Invoke(previousStage, currentStage);
    }

    private CorruptionStage EvaluateStage()
    {
        if (corruptionValue >= uncontrolledCorruptionThreshold)
        {
            return CorruptionStage.Uncontrolled;
        }

        if (corruptionValue >= unsteadyCorruptionThreshold)
        {
            return CorruptionStage.Unsteady;
        }

        return CorruptionStage.Ordered;
    }
}
