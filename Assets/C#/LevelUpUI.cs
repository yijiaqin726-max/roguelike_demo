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

    private sealed class OfferCardView
    {
        public Button button;
        public TMP_Text categoryText;
        public TMP_Text titleText;
        public TMP_Text descriptionText;
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
    [SerializeField] private Vector2 panelSize = new Vector2(760f, 540f);
    [SerializeField] private Vector2 optionButtonSize = new Vector2(212f, 250f);
    [SerializeField] private Vector2 optionsAreaSize = new Vector2(684f, 290f);
    [SerializeField] private float optionCardSpacing = 16f;
    [SerializeField] private Color buttonHighlightColor = new Color(0.92f, 0.84f, 0.74f, 0.14f);
    [SerializeField] private Color buttonPressedColor = new Color(0.96f, 0.88f, 0.8f, 0.22f);
    [SerializeField] private float buttonFadeDuration = 0.08f;
    [SerializeField] private Color frameColor = new Color(0.56f, 0.4f, 0.28f, 0.62f);
    [SerializeField] private Color headerAccentColor = new Color(0.52f, 0.16f, 0.17f, 0.82f);
    [SerializeField] private Color subtitleColor = new Color(0.76f, 0.74f, 0.8f, 1f);
    [SerializeField] private Color tendencyPlateColor = new Color(0.14f, 0.08f, 0.11f, 0.82f);
    [SerializeField] private Color panelBackPlateColor = new Color(0.015f, 0.012f, 0.02f, 0.82f);
    [SerializeField] private Color cardHoverFrameColor = new Color(0.9f, 0.82f, 0.72f, 0.26f);

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
    private readonly OfferCardView[] offerCardViews = new OfferCardView[3];

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
        CacheUiReferences();
        EnsureGameplayReferences();
        EnsurePresentation();
        GenerateOffers();
        RefreshOfferViews();
        if (panel == null)
        {
            return;
        }

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (panel == null)
        {
            Time.timeScale = 1f;
            return;
        }

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

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = panelSize;
        }

        EnsureOptionsLayout();
        EnsurePanelChrome();

        titleLabel = EnsureTextElement("UpgradeTitle", UpgradeTitleText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(400f, 36f), 28, new Color(0.96f, 0.9f, 0.78f, 1f));
        subtitleLabel = EnsureTextElement("UpgradeSubtitle", UpgradeSubtitleText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(470f, 28f), 14, subtitleColor);

        string tendencyText = corruptionSystem != null ? corruptionSystem.GetAlignmentSummary() : UnknownAlignmentText;
        tendencyLabel = EnsureTextElement("AlignmentHint", tendencyText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(450f, 24f), 14, new Color(0.9f, 0.84f, 0.84f, 1f));
        StyleHeaderText(titleLabel, 0.4f);
        StyleHeaderText(subtitleLabel, 0.22f);
        StyleHeaderText(tendencyLabel, 0.2f);

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

        RefreshOfferView(offerCardViews[0], activeOffers[0]);
        RefreshOfferView(offerCardViews[1], activeOffers[1]);
        RefreshOfferView(offerCardViews[2], activeOffers[2]);
    }

    private void RefreshOfferView(OfferCardView cardView, SkillOffer offer)
    {
        if (cardView == null || cardView.button == null || offer.data == null)
        {
            return;
        }

        string categoryLabel = BuildCategoryLabel(offer.data.category);
        if (cardView.categoryText != null)
        {
            cardView.categoryText.text = categoryLabel;
        }

        if (cardView.titleText != null)
        {
            cardView.titleText.text = offer.data.title;
        }

        if (cardView.descriptionText != null)
        {
            cardView.descriptionText.text = offer.data.description;
        }

        Image image = cardView.button.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        switch (offer.data.category)
        {
            case UpgradeCategory.Oath:
                image.color = new Color(0.18f, 0.24f, 0.29f, 1f);
                ApplyCardTextColors(cardView, new Color(0.79f, 0.84f, 0.9f, 0.95f), new Color(0.96f, 0.94f, 0.86f, 1f), new Color(0.83f, 0.87f, 0.9f, 0.92f));
                UpdateButtonChrome(cardView.button, new Color(0.63f, 0.69f, 0.76f, 0.8f), new Color(0.22f, 0.31f, 0.39f, 0.55f));
                break;
            case UpgradeCategory.Corruption:
                image.color = new Color(0.36f, 0.14f, 0.16f, 1f);
                ApplyCardTextColors(cardView, new Color(0.9f, 0.66f, 0.64f, 0.95f), new Color(0.98f, 0.90f, 0.92f, 1f), new Color(0.94f, 0.81f, 0.83f, 0.92f));
                UpdateButtonChrome(cardView.button, new Color(0.77f, 0.43f, 0.37f, 0.85f), new Color(0.34f, 0.1f, 0.14f, 0.62f));
                break;
            default:
                image.color = new Color(0.22f, 0.22f, 0.22f, 1f);
                ApplyCardTextColors(cardView, new Color(0.8f, 0.78f, 0.69f, 0.95f), new Color(0.92f, 0.92f, 0.92f, 1f), new Color(0.84f, 0.84f, 0.82f, 0.92f));
                UpdateButtonChrome(cardView.button, new Color(0.66f, 0.62f, 0.52f, 0.72f), new Color(0.2f, 0.2f, 0.2f, 0.48f));
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
        text.textWrappingMode = TextWrappingModes.Normal;
        return text;
    }

    private void EnsureOptionsLayout()
    {
        RectTransform optionsRoot = EnsureRectElement(
            "OptionsRoot",
            panel.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -56f),
            optionsAreaSize);

        HorizontalLayoutGroup layout = optionsRoot.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = optionsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        }
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = optionCardSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        SetupOfferCard(0, attackButton, optionsRoot, "OathCard");
        SetupOfferCard(1, healButton, optionsRoot, "CorruptionCard");
        SetupOfferCard(2, speedButton, optionsRoot, "CommonCard");
    }

    private void SetupOfferCard(int index, Button button, RectTransform parent, string fallbackName)
    {
        if (button == null || parent == null)
        {
            return;
        }

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        button.gameObject.name = fallbackName;
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = optionButtonSize;

        LayoutElement layout = button.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = button.gameObject.AddComponent<LayoutElement>();
        }
        layout.preferredWidth = optionButtonSize.x;
        layout.preferredHeight = optionButtonSize.y;
        layout.minWidth = optionButtonSize.x;
        layout.minHeight = optionButtonSize.y;

        offerCardViews[index] = EnsureOfferCardView(button);
        StyleButton(button);
    }

    private OfferCardView EnsureOfferCardView(Button button)
    {
        OfferCardView cardView = new OfferCardView();
        cardView.button = button;
        cardView.categoryText = EnsureCardText(button.transform, "CategoryLabel", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -18f), new Vector2(-20f, -18f), 12, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        cardView.titleText = EnsureCardText(button.transform, "TitleLabel", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -54f), new Vector2(-20f, -54f), 19, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        cardView.descriptionText = EnsureCardText(button.transform, "DescriptionLabel", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(24f, 22f), new Vector2(-20f, -110f), 14, FontStyles.Normal, TextAlignmentOptions.TopLeft);

        cardView.categoryText.verticalAlignment = VerticalAlignmentOptions.Top;
        cardView.titleText.verticalAlignment = VerticalAlignmentOptions.Top;
        cardView.descriptionText.verticalAlignment = VerticalAlignmentOptions.Top;

        return cardView;
    }

    private RectTransform EnsureRectElement(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        Transform existing = parent.Find(objectName);
        RectTransform rect;

        if (existing == null)
        {
            GameObject rectObject = new GameObject(objectName, typeof(RectTransform));
            rect = rectObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
        }
        else
        {
            rect = existing as RectTransform;
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        return rect;
    }

    private TMP_Text EnsureCardText(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        Transform existing = parent.Find(objectName);
        TextMeshProUGUI text;

        if (existing == null && objectName == "DescriptionLabel")
        {
            TMP_Text legacy = parent.GetComponentInChildren<TMP_Text>(true);
            if (legacy != null)
            {
                existing = legacy.transform;
            }
        }

        if (existing == null)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            text = textObject.AddComponent<TextMeshProUGUI>();
        }
        else
        {
            text = existing.GetComponent<TextMeshProUGUI>();
        }

        if (text == null)
        {
            return null;
        }

        text.gameObject.name = objectName;
        RectTransform textRect = text.GetComponent<RectTransform>();
        if (textRect != null)
        {
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.offsetMin = offsetMin;
            textRect.offsetMax = offsetMax;
        }

        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.richText = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.margin = Vector4.zero;
        StyleBodyText(text);
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

        EnsureButtonChrome(button);

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0f);
        colors.highlightedColor = buttonHighlightColor;
        colors.pressedColor = buttonPressedColor;
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.04f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = buttonFadeDuration;
        button.colors = colors;

        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        Transform hoverOverlay = button.transform.Find("HoverOverlay");
        if (hoverOverlay != null)
        {
            button.targetGraphic = hoverOverlay.GetComponent<Image>();
        }
    }

    private void EnsurePanelChrome()
    {
        Image backPlate = EnsureImageElement("BackPlate", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), panelSize + new Vector2(12f, 12f), panelBackPlateColor);
        Image innerShade = EnsureImageElement("InnerShade", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), panelSize - new Vector2(36f, 40f), new Color(0f, 0f, 0f, 0.08f));
        EnsureImageElement("TopAccent", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(64f, 6f), headerAccentColor);
        EnsureImageElement("HeaderDivider", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -116f), new Vector2(430f, 2f), frameColor);
        EnsureImageElement("AlignmentPlate", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(260f, 24f), tendencyPlateColor);
        EnsureImageElement("OptionsDivider", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -144f), new Vector2(620f, 1.5f), new Color(frameColor.r, frameColor.g, frameColor.b, frameColor.a * 0.65f));

        EnsureFrameEdge("FrameTop", new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(0f, -12f), new Vector2(-16f, 2f));
        EnsureFrameEdge("FrameBottom", new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(0f, 12f), new Vector2(-16f, 2f));
        EnsureFrameEdge("FrameLeft", new Vector2(0f, 0.5f), new Vector2(0f, 1f), new Vector2(12f, 0f), new Vector2(2f, -16f));
        EnsureFrameEdge("FrameRight", new Vector2(1f, 0.5f), new Vector2(1f, 1f), new Vector2(-12f, 0f), new Vector2(2f, -16f));

        if (backPlate != null)
        {
            backPlate.transform.SetAsFirstSibling();
        }

        if (innerShade != null)
        {
            innerShade.transform.SetSiblingIndex(1);
        }
    }

    private void EnsureFrameEdge(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        EnsureImageElement(name, anchorMin, anchorMax, anchoredPosition, sizeDelta, frameColor);
    }

    private Image EnsureImageElement(string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        Transform existing = panel.transform.Find(objectName);
        Image image;

        if (existing == null)
        {
            GameObject imageObject = new GameObject(objectName);
            imageObject.transform.SetParent(panel.transform, false);
            RectTransform rect = imageObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            image = imageObject.AddComponent<Image>();
            image.raycastTarget = false;
        }
        else
        {
            image = existing.GetComponent<Image>();
        }

        if (image != null)
        {
            image.color = color;
        }

        return image;
    }

    private void EnsureButtonChrome(Button button)
    {
        if (button == null)
        {
            return;
        }

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        EnsureChildImage(button.transform, "CardShade", new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-8f, -8f), new Color(0f, 0f, 0f, 0.16f));
        EnsureChildImage(button.transform, "HoverOverlay", new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-6f, -6f), new Color(1f, 1f, 1f, 0f));
        EnsureChildImage(button.transform, "HoverFrame", new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-4f, -4f), cardHoverFrameColor);
        EnsureChildImage(button.transform, "AccentBar", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -34f), new Vector2(6f, 58f), headerAccentColor);
        EnsureChildImage(button.transform, "TopTrim", new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(0f, -10f), new Vector2(-18f, 2f), frameColor);
        EnsureChildImage(button.transform, "BottomTrim", new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(0f, 10f), new Vector2(-18f, 2f), new Color(frameColor.r, frameColor.g, frameColor.b, frameColor.a * 0.7f));
        EnsureChildImage(button.transform, "CategoryPlate", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -16f), new Vector2(-44f, 24f), new Color(0f, 0f, 0f, 0.1f));
        EnsureChildImage(button.transform, "DescriptionShade", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 18f), new Vector2(-24f, 62f), new Color(0f, 0f, 0f, 0.08f));
    }

    private void UpdateButtonChrome(Button button, Color accentColor, Color trimColor)
    {
        Transform accent = button.transform.Find("AccentBar");
        if (accent != null)
        {
            Image accentImage = accent.GetComponent<Image>();
            if (accentImage != null)
            {
                accentImage.color = accentColor;
            }
        }

        Transform topTrim = button.transform.Find("TopTrim");
        if (topTrim != null)
        {
            Image topImage = topTrim.GetComponent<Image>();
            if (topImage != null)
            {
                topImage.color = trimColor;
            }
        }

        Transform bottomTrim = button.transform.Find("BottomTrim");
        if (bottomTrim != null)
        {
            Image bottomImage = bottomTrim.GetComponent<Image>();
            if (bottomImage != null)
            {
                bottomImage.color = trimColor;
            }
        }

        Transform categoryPlate = button.transform.Find("CategoryPlate");
        if (categoryPlate != null)
        {
            Image categoryImage = categoryPlate.GetComponent<Image>();
            if (categoryImage != null)
            {
                categoryImage.color = new Color(trimColor.r, trimColor.g, trimColor.b, 0.16f);
            }
        }

        Transform hoverFrame = button.transform.Find("HoverFrame");
        if (hoverFrame != null)
        {
            Image hoverFrameImage = hoverFrame.GetComponent<Image>();
            if (hoverFrameImage != null)
            {
                hoverFrameImage.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.18f);
            }
        }
    }

    private static void ApplyCardTextColors(OfferCardView cardView, Color categoryColor, Color titleColor, Color descriptionColor)
    {
        if (cardView == null)
        {
            return;
        }

        if (cardView.categoryText != null)
        {
            cardView.categoryText.color = categoryColor;
        }

        if (cardView.titleText != null)
        {
            cardView.titleText.color = titleColor;
        }

        if (cardView.descriptionText != null)
        {
            cardView.descriptionText.color = descriptionColor;
        }
    }

    private static Image EnsureChildImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        Transform existing = parent.Find(name);
        Image image;

        if (existing == null)
        {
            GameObject imageObject = new GameObject(name);
            imageObject.transform.SetParent(parent, false);
            RectTransform rect = imageObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            image = imageObject.AddComponent<Image>();
            image.raycastTarget = false;
        }
        else
        {
            image = existing.GetComponent<Image>();
        }

        if (image != null)
        {
            image.color = color;
        }

        return image;
    }

    private static void StyleHeaderText(TMP_Text text, float underlayAlpha)
    {
        if (text == null)
        {
            return;
        }

        text.fontStyle = FontStyles.Bold;
        text.outlineWidth = 0.18f;
        text.outlineColor = new Color(0.09f, 0.02f, 0.03f, 0.82f);
        text.textWrappingMode = TextWrappingModes.Normal;
        text.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        text.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0.05f, 0.01f, 0.02f, underlayAlpha));
        text.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 1f);
        text.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -1f);
    }

    private static void StyleBodyText(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        text.outlineWidth = 0.16f;
        text.outlineColor = new Color(0.08f, 0.02f, 0.03f, 0.82f);
        text.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        text.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0.04f, 0.01f, 0.02f, 0.6f));
        text.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 1f);
        text.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -1f);
    }

    private static string BuildCategoryLabel(UpgradeCategory category)
    {
        switch (category)
        {
            case UpgradeCategory.Oath:
                return "OATH PATH";
            case UpgradeCategory.Corruption:
                return "FALLEN PATH";
            default:
                return "COMMON RITE";
        }
    }
}
