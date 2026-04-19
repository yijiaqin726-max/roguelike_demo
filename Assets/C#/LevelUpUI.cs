using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    private struct SkillOffer
    {
        public UpgradeOptionData data;
    }

    private const string DefaultLibraryResourcePath = "DefaultUpgradeOptionLibrary";
    private const string UpgradeTitleText = "\u9009\u62e9\u6210\u957f\u65b9\u5411";
    private const string UpgradeSubtitleText = "\u5b88\u4f4f\u6b8b\u8a93\uff0c\u6216\u8fdb\u4e00\u6b65\u62e5\u62b1\u8150\u5316";
    private const string UnknownAlignmentText = "\u503e\u5411\u672a\u77e5";

    public GameObject panel;
    public Button attackButton;
    public Button healButton;
    public Button speedButton;

    [Header("Upgrade Data")]
    public UpgradeOptionLibrary upgradeOptionLibrary;

    [FormerlySerializedAs("upgradeOptions")]
    [SerializeField] private List<UpgradeOptionData> inlineUpgradeOptions = new List<UpgradeOptionData>();

    [Header("Upgrade Tuning")]
    [SerializeField] private float minimumWeight = 0.05f;
    [SerializeField] private float attackSpeedUpgradeStep = 0.2f;
    [SerializeField] private float corruptionBurstAttackSpeedStep = 0.35f;
    [SerializeField] private float moveSpeedUpgradeStep = 1f;
    [SerializeField] private int healUpgradeAmount = 2;
    [SerializeField] private int maxHealthUpgradeAmount = 2;
    [SerializeField] private int purifyHealAmount = 1;

    [Header("Presentation")]
    [SerializeField] private Color panelTint = new Color(0.05f, 0.03f, 0.05f, 0.94f);
    [SerializeField] private Vector2 optionButtonSize = new Vector2(240f, 72f);
    [SerializeField] private Color buttonHighlightColor = new Color(0.38f, 0.19f, 0.2f, 1f);
    [SerializeField] private Color buttonPressedColor = new Color(0.11f, 0.07f, 0.08f, 1f);
    [SerializeField] private float buttonFadeDuration = 0.08f;

    private PlayerController player;
    private PlayerHealth playerHealth;
    private CorruptionSystem corruptionSystem;
    private PlayerBranchProgression branchProgression;
    private TMP_Text attackLabel;
    private TMP_Text healLabel;
    private TMP_Text speedLabel;
    private TMP_Text titleLabel;
    private TMP_Text subtitleLabel;
    private TMP_Text tendencyLabel;
    private readonly SkillOffer[] activeOffers = new SkillOffer[3];

    private void Start()
    {
        EnsureUpgradeOptionSource();
        CacheUiReferences();
        EnsureGameplayReferences();
        EnsurePresentation();
    }

    public void Show()
    {
        EnsureUpgradeOptionSource();
        EnsureGameplayReferences();
        EnsurePresentation();
        GenerateOffers();
        RefreshOfferViews();
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnAttackSpeed()
    {
        ApplyOffer(0);
    }

    public void OnMoveSpeed()
    {
        ApplyOffer(2);
    }

    public void OnHeal()
    {
        ApplyOffer(1);
    }

    private void CacheUiReferences()
    {
        if (panel == null)
        {
            return;
        }

        if (attackButton == null)
        {
            Transform attackTransform = panel.transform.Find("Button_Attack");
            if (attackTransform != null)
            {
                attackButton = attackTransform.GetComponent<Button>();
            }
        }

        if (healButton == null)
        {
            Transform healTransform = panel.transform.Find("Button_Heal");
            if (healTransform != null)
            {
                healButton = healTransform.GetComponent<Button>();
            }
        }

        if (speedButton == null)
        {
            Transform speedTransform = panel.transform.Find("Button_Speed");
            if (speedTransform != null)
            {
                speedButton = speedTransform.GetComponent<Button>();
            }
        }

        attackLabel = FindLabel(attackButton);
        healLabel = FindLabel(healButton);
        speedLabel = FindLabel(speedButton);
    }

    private void EnsurePresentation()
    {
        if (panel == null)
        {
            return;
        }

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = panelTint;
        }

        titleLabel = EnsureTextElement("UpgradeTitle", UpgradeTitleText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(360f, 36f), 28, new Color(0.96f, 0.9f, 0.78f, 1f));
        subtitleLabel = EnsureTextElement("UpgradeSubtitle", UpgradeSubtitleText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(440f, 28f), 15, new Color(0.76f, 0.74f, 0.8f, 1f));

        string tendencyText = corruptionSystem != null ? corruptionSystem.GetAlignmentSummary() : UnknownAlignmentText;
        tendencyLabel = EnsureTextElement("AlignmentHint", tendencyText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(480f, 24f), 14, new Color(0.86f, 0.82f, 0.84f, 1f));

        StyleButton(attackButton);
        StyleButton(healButton);
        StyleButton(speedButton);
    }

    private void EnsureGameplayReferences()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }

        if (corruptionSystem == null)
        {
            corruptionSystem = FindObjectOfType<CorruptionSystem>();
        }

        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (branchProgression == null && player != null)
        {
            branchProgression = player.GetComponent<PlayerBranchProgression>();
            if (branchProgression == null)
            {
                branchProgression = player.gameObject.AddComponent<PlayerBranchProgression>();
            }
        }
    }

    private void EnsureUpgradeOptionSource()
    {
        if (upgradeOptionLibrary == null)
        {
            upgradeOptionLibrary = Resources.Load<UpgradeOptionLibrary>(DefaultLibraryResourcePath);
        }

        if (inlineUpgradeOptions == null || inlineUpgradeOptions.Count == 0)
        {
            inlineUpgradeOptions = CreateDefaultUpgradeOptions();
        }
    }

    private List<UpgradeOptionData> GetUpgradeOptions()
    {
        if (upgradeOptionLibrary != null && upgradeOptionLibrary.options != null && upgradeOptionLibrary.options.Count > 0)
        {
            return upgradeOptionLibrary.options;
        }

        return inlineUpgradeOptions;
    }

    private static List<UpgradeOptionData> CreateDefaultUpgradeOptions()
    {
        return new List<UpgradeOptionData>
        {
            new UpgradeOptionData
            {
                title = "OATH: Sacred Guard",
                description = "After taking damage, gain brief damage reduction.",
                category = UpgradeCategory.Oath,
                effectType = SkillEffectType.OathDamageShield,
                oathChange = 12,
                corruptionChange = 0,
                baseWeight = 1f,
                oathAffinity = 0.35f,
                corruptionAffinity = -0.1f
            },
            new UpgradeOptionData
            {
                title = "OATH: Mercy Tithe",
                description = "Picking up EXP restores a small amount of health.",
                category = UpgradeCategory.Oath,
                effectType = SkillEffectType.OathExpHeal,
                oathChange = 10,
                corruptionChange = -4,
                baseWeight = 1f,
                oathAffinity = 0.3f,
                corruptionAffinity = -0.2f
            },
            new UpgradeOptionData
            {
                title = "OATH: Purging Pulse",
                description = "Periodically emit a holy shockwave around you.",
                category = UpgradeCategory.Oath,
                effectType = SkillEffectType.OathPurificationPulse,
                oathChange = 14,
                corruptionChange = -6,
                baseWeight = 0.9f,
                oathAffinity = 0.45f,
                corruptionAffinity = -0.05f
            },
            new UpgradeOptionData
            {
                title = "FALL: Blood Appetite",
                description = "Kills have a chance to restore health.",
                category = UpgradeCategory.Corruption,
                effectType = SkillEffectType.FallenKillLeech,
                oathChange = 0,
                corruptionChange = 15,
                baseWeight = 1f,
                oathAffinity = -0.15f,
                corruptionAffinity = 0.35f
            },
            new UpgradeOptionData
            {
                title = "FALL: Last Breath Fury",
                description = "Attack faster as your health gets lower.",
                category = UpgradeCategory.Corruption,
                effectType = SkillEffectType.FallenLowHealthFrenzy,
                oathChange = 0,
                corruptionChange = 16,
                baseWeight = 1f,
                oathAffinity = -0.15f,
                corruptionAffinity = 0.35f
            },
            new UpgradeOptionData
            {
                title = "FALL: Rotburst",
                description = "Enemies explode with corruption when they die.",
                category = UpgradeCategory.Corruption,
                effectType = SkillEffectType.FallenDeathBurst,
                oathChange = 0,
                corruptionChange = 22,
                baseWeight = 0.9f,
                oathAffinity = -0.2f,
                corruptionAffinity = 0.45f
            },
            new UpgradeOptionData
            {
                title = "COMMON: Field Recovery",
                description = "Recover slightly without shifting too hard.",
                category = UpgradeCategory.Common,
                effectType = SkillEffectType.Heal,
                oathChange = 0,
                corruptionChange = 0,
                baseWeight = 1f,
                oathAffinity = 0f,
                corruptionAffinity = 0f
            },
            new UpgradeOptionData
            {
                title = "COMMON: Tempered Stride",
                description = "Reliable movement for any build.",
                category = UpgradeCategory.Common,
                effectType = SkillEffectType.MoveSpeed,
                oathChange = 0,
                corruptionChange = 0,
                baseWeight = 1f,
                oathAffinity = 0f,
                corruptionAffinity = 0f
            },
            new UpgradeOptionData
            {
                title = "COMMON: Battle Rhythm",
                description = "Improve attack speed without shifting alignment.",
                category = UpgradeCategory.Common,
                effectType = SkillEffectType.AttackSpeed,
                oathChange = 0,
                corruptionChange = 0,
                baseWeight = 0.9f,
                oathAffinity = 0f,
                corruptionAffinity = 0f
            }
        };
    }

    private void GenerateOffers()
    {
        activeOffers[0] = CreateOfferForCategory(UpgradeCategory.Oath);
        activeOffers[1] = CreateOfferForCategory(UpgradeCategory.Corruption);
        activeOffers[2] = CreateOfferForCategory(UpgradeCategory.Common);
    }

    private SkillOffer CreateOfferForCategory(UpgradeCategory category)
    {
        List<UpgradeOptionData> pool = GetUpgradeOptions().FindAll(option => option.category == category);
        if (pool.Count == 0)
        {
            return default;
        }

        UpgradeOptionData selected = ChooseWeightedOption(pool);
        return new SkillOffer { data = selected };
    }

    private UpgradeOptionData ChooseWeightedOption(List<UpgradeOptionData> pool)
    {
        float totalWeight = 0f;
        float[] weights = new float[pool.Count];

        float oathRatio = corruptionSystem != null ? corruptionSystem.GetOathRatio() : 0f;
        float corruptionRatio = corruptionSystem != null ? corruptionSystem.GetCorruptionRatio() : 0f;

        for (int i = 0; i < pool.Count; i++)
        {
            UpgradeOptionData option = pool[i];
            float weight = option.baseWeight;
            weight += option.oathAffinity * oathRatio;
            weight += option.corruptionAffinity * corruptionRatio;
            weight = Mathf.Max(minimumWeight, weight);
            weights[i] = weight;
            totalWeight += weight;
        }

        float roll = Random.value * totalWeight;
        float cumulative = 0f;
        for (int i = 0; i < pool.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative)
            {
                return pool[i];
            }
        }

        return pool[pool.Count - 1];
    }

    private void RefreshOfferViews()
    {
        if (tendencyLabel != null && corruptionSystem != null)
        {
            tendencyLabel.text = corruptionSystem.GetAlignmentSummary();
        }

        RefreshOfferView(attackButton, attackLabel, activeOffers[0]);
        RefreshOfferView(healButton, healLabel, activeOffers[1]);
        RefreshOfferView(speedButton, speedLabel, activeOffers[2]);
    }

    private void RefreshOfferView(Button button, TMP_Text label, SkillOffer offer)
    {
        if (button == null || label == null || offer.data == null)
        {
            return;
        }

        label.text = offer.data.title + "\n<size=70%>" + offer.data.description + "</size>";

        Image image = button.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        switch (offer.data.category)
        {
            case UpgradeCategory.Oath:
                image.color = new Color(0.18f, 0.24f, 0.29f, 1f);
                label.color = new Color(0.96f, 0.94f, 0.86f, 1f);
                break;
            case UpgradeCategory.Corruption:
                image.color = new Color(0.36f, 0.14f, 0.16f, 1f);
                label.color = new Color(0.98f, 0.90f, 0.92f, 1f);
                break;
            default:
                image.color = new Color(0.22f, 0.22f, 0.22f, 1f);
                label.color = new Color(0.92f, 0.92f, 0.92f, 1f);
                break;
        }
    }

    private void ApplyOffer(int offerIndex)
    {
        EnsureGameplayReferences();

        if (offerIndex < 0 || offerIndex >= activeOffers.Length || player == null || activeOffers[offerIndex].data == null)
        {
            Hide();
            return;
        }

        UpgradeOptionData option = activeOffers[offerIndex].data;
        switch (option.effectType)
        {
            case SkillEffectType.AttackSpeed:
                player.attackIntervalSeconds = Mathf.Max(player.minimumAttackInterval, player.attackIntervalSeconds - attackSpeedUpgradeStep);
                break;
            case SkillEffectType.MoveSpeed:
                player.moveSpeed += moveSpeedUpgradeStep;
                break;
            case SkillEffectType.Heal:
                if (playerHealth != null)
                {
                    playerHealth.Heal(healUpgradeAmount);
                }
                break;
            case SkillEffectType.MaxHealth:
                if (playerHealth != null)
                {
                    playerHealth.maxHealth += maxHealthUpgradeAmount;
                    playerHealth.Heal(maxHealthUpgradeAmount);
                }
                break;
            case SkillEffectType.CorruptionBurst:
                player.attackIntervalSeconds = Mathf.Max(player.minimumAttackInterval, player.attackIntervalSeconds - corruptionBurstAttackSpeedStep);
                break;
            case SkillEffectType.Purify:
                if (playerHealth != null)
                {
                    playerHealth.Heal(purifyHealAmount);
                }
                break;
            case SkillEffectType.OathDamageShield:
                branchProgression?.UnlockOathDamageShield();
                break;
            case SkillEffectType.OathExpHeal:
                branchProgression?.UnlockOathExpHeal();
                break;
            case SkillEffectType.OathPurificationPulse:
                branchProgression?.UnlockOathPurificationPulse();
                break;
            case SkillEffectType.FallenKillLeech:
                branchProgression?.UnlockFallenKillLeech();
                break;
            case SkillEffectType.FallenLowHealthFrenzy:
                branchProgression?.UnlockFallenLowHealthFrenzy();
                break;
            case SkillEffectType.FallenDeathBurst:
                branchProgression?.UnlockFallenDeathBurst();
                break;
        }

        if (corruptionSystem != null)
        {
            if (option.oathChange > 0)
            {
                corruptionSystem.AddOath(option.oathChange);
            }
            else if (option.oathChange < 0)
            {
                corruptionSystem.ReduceOath(-option.oathChange);
            }

            if (option.corruptionChange > 0)
            {
                corruptionSystem.AddCorruption(option.corruptionChange);
            }
            else if (option.corruptionChange < 0)
            {
                corruptionSystem.ReduceCorruption(-option.corruptionChange);
            }
        }

        Hide();
    }

    private static TMP_Text FindLabel(Button button)
    {
        if (button == null)
        {
            return null;
        }

        return button.GetComponentInChildren<TMP_Text>(true);
    }

    private TMP_Text EnsureTextElement(string objectName, string content, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, float fontSize, Color color)
    {
        Transform existing = panel.transform.Find(objectName);
        TMP_Text text;

        if (existing == null)
        {
            GameObject textObject = new GameObject(objectName);
            textObject.transform.SetParent(panel.transform, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            text = textObject.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            text = existing.GetComponent<TMP_Text>();
        }

        if (text == null)
        {
            return null;
        }

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        return text;
    }

    private void StyleButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = optionButtonSize;
        }

        ColorBlock colors = button.colors;
        colors.highlightedColor = buttonHighlightColor;
        colors.pressedColor = buttonPressedColor;
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = buttonFadeDuration;
        button.colors = colors;
    }
}
