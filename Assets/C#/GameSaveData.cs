using System;

[Serializable]
public class GameSaveData
{
    public int currentFloor = 1;
    public int playerLevel = 1;
    public int currentExp = 0;
    public int expToNextLevel = 5;
    public float moveSpeed = 5f;
    public float attackInterval = 1f;
    public int maxHealth = 10;
    public int currentHealth = 10;
    public int currentCorruption = 0;
    public int corruptionValue = 0;
    public int oathValue = 0;
}
