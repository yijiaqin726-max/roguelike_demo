using System;

public enum SkillEffectType
{
    AttackSpeed,
    MoveSpeed,
    Heal,
    MaxHealth,
    CorruptionBurst,
    Purify,
    OathDamageShield,
    OathExpHeal,
    OathPurificationPulse,
    FallenKillLeech,
    FallenLowHealthFrenzy,
    FallenDeathBurst
}

public enum UpgradeCategory
{
    Oath,
    Corruption,
    Common
}

[Serializable]
public class UpgradeOptionData
{
    public string title;
    public string description;
    public UpgradeCategory category;
    public SkillEffectType effectType;
    public int oathChange;
    public int corruptionChange;
    public float baseWeight = 1f;
    public float oathAffinity = 0.2f;
    public float corruptionAffinity = 0.2f;
}
