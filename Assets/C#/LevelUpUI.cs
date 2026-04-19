using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    public GameObject panel;
    private PlayerController player;
    private PlayerHealth playerHealth;
    private CorruptionSystem corruptionSystem;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        corruptionSystem = FindObjectOfType<CorruptionSystem>();

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    public void Show()
    {
        panel.SetActive(true);
        Time.timeScale = 0f; // 暂停游戏
    }

    public void Hide()
    {
        panel.SetActive(false);
        Time.timeScale = 1f; // 恢复游戏
    }

    public void OnAttackSpeed()
    {
        player.attackInterval = Mathf.Max(0.1f, player.attackInterval - 0.2f);
        if (corruptionSystem != null)
        {
            corruptionSystem.AddCorruption(15);
        }
        Hide();
    }

    public void OnMoveSpeed()
    {
        player.speed += 1f;
        if (corruptionSystem != null)
        {
            corruptionSystem.ReduceCorruption(4);
        }
        Hide();
    }

    public void OnHeal()
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(2);
        }

        if (corruptionSystem != null)
        {
            corruptionSystem.ReduceCorruption(8);
        }

        Hide();
    }
}
