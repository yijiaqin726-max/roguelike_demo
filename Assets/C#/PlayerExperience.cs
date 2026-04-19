using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    public int currentExp = 0;
    public int expToNextLevel = 5;
    public int level = 1;

    [SerializeField] private int expIncreasePerLevel = 3;
    [SerializeField] private LevelUpUI levelUpUI;

    private void Awake()
    {
        if (levelUpUI == null)
        {
            levelUpUI = FindObjectOfType<LevelUpUI>();
        }
    }

    public void GainExp(int amount)
    {
        currentExp += amount;

        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        currentExp = 0;
        expToNextLevel += expIncreasePerLevel;

        if (levelUpUI == null)
        {
            levelUpUI = FindObjectOfType<LevelUpUI>();
        }

        if (levelUpUI != null)
        {
            levelUpUI.Show();
        }
    }
}
