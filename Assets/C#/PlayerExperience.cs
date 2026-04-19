using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    public int currentExp = 0;
    public int expToNextLevel = 5;
    public int level = 1;

    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void GainExp(int amount)
    {
        currentExp += amount;
        Debug.Log("Current EXP: " + currentExp + "/" + expToNextLevel);

        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

void LevelUp()
{
    level++;
    currentExp = 0;
    expToNextLevel += 3;

    LevelUpUI ui = FindObjectOfType<LevelUpUI>();
    if (ui != null)
    {
        ui.Show();
    }
}
}