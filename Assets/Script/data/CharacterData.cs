using UnityEngine;

[CreateAssetMenu(fileName = "New Character Data", menuName = "Combat/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Basic Info")]
    public string characterName;
    public Sprite portrait;
    public int level = 1;

    [Header("Combat Stats")]
    public int maxHealth;
    public int minSpeed;
    public int maxSpeed;
    public int defenseLevel;

    // [수정됨] 직접 수치 입력 방식으로 변경 (float -> int)
    [Header("Stagger Settings")]
    [Tooltip("흐트러짐이 발생하는 정확한 체력 수치를 내림차순으로 입력하세요. (예: 150, 100, 50)")]
    public int[] staggerThresholds;

    [Header("Sanity")]
    public int maxSanity = 45;
    public int minSanity = -45;

    [Header("Skills")]
    public SkillData skill1;
    public SkillData skill2;
    public SkillData skill3;
    public SkillData defenseSkill;

    [Header("Resistances")]
    public float slashRes = 1f;
    public float pierceRes = 1f;
    public float bluntRes = 1f;

    public float wrathRes = 1f;
    public float lustRes = 1f;
    public float slothRes = 1f;
    public float gluttonyRes = 1f;
    public float gloomRes = 1f;
    public float prideRes = 1f;
    public float envyRes = 1f;
}