using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Combat/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Basic Information")]
    public string skillName;
    public Sprite skillIcon;
    public DamageType damageType;
    public SinAffinity sinAffinity;

    [Header("Combat Stats")]
    public int attackLevel;
    public int basePower;
    public int coinPower;
    public int coinCount;

    [Header("Effects")]
    public List<CoinData> coins = new List<CoinData>();
}