using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    public GameObject panel;
    public Button attackButton;
    public Button healButton;
    public Button speedButton;

    private PlayerController player;
    private PlayerHealth playerHealth;
    private CorruptionSystem corruptionSystem;
    private TMP_Text attackLabel;
    private TMP_Text healLabel;
    private TMP_Text speedLabel;
    private readonly SkillOffer[] activeOffers = new SkillOffer[3];

    private enum SkillEffectType
    {
        AttackSpeed,
        MoveSpeed,
        Heal,
        MaxHealth,
        CorruptionBurst,
        Purify
    }

    private struct SkillOffer
    {
        public string title;
        public string description;
        public bool isBreakOath;
        public SkillEffectType effectType;
    }

    private void Start()
    {
        CacheUiReferences();
        EnsureGameplayReferences();
    }

    public void Show()
    {
        EnsureGameplayReferences();
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
    }

    private void GenerateOffers()
    {
        List<SkillOffer> pool = new List<SkillOffer>
        {
            new SkillOffer
            {
                title = "Oath: Vanguard Step",
                description = "Move faster and keep control.",
                isBreakOath = false,
                effectType = SkillEffectType.MoveSpeed
            },
            new SkillOffer
            {
                title = "Oath: Purify",
                description = "Restore health and reduce corruption.",
                isBreakOath = false,
                effectType = SkillEffectType.Heal
            },
            new SkillOffer
            {
                title = "Oath: Iron Vow",
                description = "Gain max health for later floors.",
                isBreakOath = false,
                effectType = SkillEffectType.MaxHealth
            },
            new SkillOffer
            {
                title = "Break Oath: Ashen Flurry",
                description = "Attack faster and deepen corruption.",
                isBreakOath = true,
                effectType = SkillEffectType.AttackSpeed
            },
            new SkillOffer
            {
                title = "Break Oath: Crimson Pulse",
                description = "Huge burst speed, heavier corruption.",
                isBreakOath = true,
                effectType = SkillEffectType.CorruptionBurst
            },
            new SkillOffer
            {
                title = "Oath: Pale Mercy",
                description = "Cleanse corruption and recover a little.",
                isBreakOath = false,
                effectType = SkillEffectType.Purify
            }
        };

        for (int i = 0; i < activeOffers.Length; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            activeOffers[i] = pool[randomIndex];
            pool.RemoveAt(randomIndex);
        }
    }

    private void RefreshOfferViews()
    {
        RefreshOfferView(attackButton, attackLabel, activeOffers[0]);
        RefreshOfferView(healButton, healLabel, activeOffers[1]);
        RefreshOfferView(speedButton, speedLabel, activeOffers[2]);
    }

    private void RefreshOfferView(Button button, TMP_Text label, SkillOffer offer)
    {
        if (button == null || label == null)
        {
            return;
        }

        label.text = offer.title + "\n<size=70%>" + offer.description + "</size>";

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = offer.isBreakOath
                ? new Color(0.36f, 0.14f, 0.16f, 1f)
                : new Color(0.18f, 0.24f, 0.29f, 1f);
        }

        label.color = offer.isBreakOath
            ? new Color(0.98f, 0.90f, 0.92f, 1f)
            : new Color(0.96f, 0.94f, 0.86f, 1f);
    }

    private void ApplyOffer(int offerIndex)
    {
        EnsureGameplayReferences();

        if (offerIndex < 0 || offerIndex >= activeOffers.Length || player == null)
        {
            Hide();
            return;
        }

        SkillOffer offer = activeOffers[offerIndex];
        switch (offer.effectType)
        {
            case SkillEffectType.AttackSpeed:
                player.attackInterval = Mathf.Max(0.1f, player.attackInterval - 0.2f);
                corruptionSystem?.AddCorruption(15);
                break;
            case SkillEffectType.MoveSpeed:
                player.speed += 1f;
                corruptionSystem?.ReduceCorruption(4);
                break;
            case SkillEffectType.Heal:
                if (playerHealth != null)
                {
                    playerHealth.Heal(2);
                }

                corruptionSystem?.ReduceCorruption(8);
                break;
            case SkillEffectType.MaxHealth:
                if (playerHealth != null)
                {
                    playerHealth.maxHealth += 2;
                    playerHealth.Heal(2);
                }
                break;
            case SkillEffectType.CorruptionBurst:
                player.attackInterval = Mathf.Max(0.1f, player.attackInterval - 0.35f);
                corruptionSystem?.AddCorruption(22);
                break;
            case SkillEffectType.Purify:
                corruptionSystem?.ReduceCorruption(14);
                if (playerHealth != null)
                {
                    playerHealth.Heal(1);
                }
                break;
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
}
