using System.Collections.Generic;
using UnityEngine;

// Enums.cs 내부의 StatusEffectType을 아래와 같이 변경합니다.
public enum StatusEffectType { None, Burn, Bleed, Tremor, Rupture, Sinking, Poise, Charge }
public enum EffectCondition { OnUse, OnClashWin, OnClashLose, BeforeHit, OnHit, OnHeadsHit, OnTailsHit, OnTurnEnd }
public enum SinAffinity { Wrath, Lust, Sloth, Gluttony, Gloom, Pride, Envy }
public enum DamageType { Slash, Pierce, Blunt }

// 림버스 컴퍼니 4대 위력 구분
public enum PowerModifierType
{
    BasePower,
    CoinPower,
    ClashPower,
    FinalPower
}

// [신규] 조건 검사 대상
public enum ConditionTarget { Attacker, Target }

// [신규] 검사할 스탯 종류
public enum ConditionCheckType
{
    StatusPotency,
    StatusCount,
    Sanity,
    Speed,
    SpeedDiff
}

// [신규] 비교 연산자
public enum CompareOperator { GreaterOrEqual, LessOrEqual, Equal }

[System.Serializable]
public class CoinData
{
    public string coinDescription;
    public bool isUnbreakable;

    public List<BaseCoinEffect> coinEffects = new List<BaseCoinEffect>();
}

public struct ClashResult
{
    public bool LeftWon;
    public RuntimeSkill LeftSkill;
    public RuntimeSkill RightSkill;
}

public struct DamageResult
{
    public int finalDamage;
    public bool targetStaggered;
}

public struct ClashRoundResult
{
    public int leftPower;
    public int rightPower;
    public string leftCoinLog;
    public string rightCoinLog;
    public int winnerFlag;
}