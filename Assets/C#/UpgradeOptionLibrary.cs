using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeOptionLibrary", menuName = "Oathbreaker/Upgrade Option Library")]
public class UpgradeOptionLibrary : ScriptableObject
{
    public List<UpgradeOptionData> options = new List<UpgradeOptionData>();
}
