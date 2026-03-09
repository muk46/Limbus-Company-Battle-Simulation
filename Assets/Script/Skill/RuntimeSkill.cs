using UnityEngine;
using System.Collections.Generic;

public class RuntimeCoin
{
    public CoinData originCoinData;
    public bool isDestroyed;

    public RuntimeCoin(CoinData data)
    {
        originCoinData = data;
        isDestroyed = false;
    }
}

public class RuntimeSkill
{
    public SkillData OriginData { get; private set; }

    public int CurrentBasePower { get; set; }
    public int CurrentCoinPower { get; set; }

    public int ClashPowerModifier { get; set; }
    public int FinalPowerModifier { get; set; }

    // [신규 추가] 공명으로 인해 증가한 공격 레벨 보너스
    public int ResonanceAttackLevelBonus { get; set; }

    // [신규 추가] 타격 피해량 증폭 보너스 (디버프 비례 피해량 증가 등)
    public float DamageMultiplierBonus { get; set; }

    public List<RuntimeCoin> RuntimeCoins { get; private set; }

    public RuntimeSkill(SkillData data)
    {
        OriginData = data;
        CurrentBasePower = data.basePower;
        CurrentCoinPower = data.coinPower;

        ClashPowerModifier = 0;
        FinalPowerModifier = 0;
        ResonanceAttackLevelBonus = 0; // 초기화
        DamageMultiplierBonus = 0f;    // [신규 추가] 0으로 초기화

        RuntimeCoins = new List<RuntimeCoin>();
        if (data.coins != null && data.coins.Count > 0)
        {
            foreach (var coinData in data.coins)
                RuntimeCoins.Add(new RuntimeCoin(coinData));
        }
        else if (data.coinCount > 0)
        {
            for (int i = 0; i < data.coinCount; i++)
                RuntimeCoins.Add(new RuntimeCoin(new CoinData()));
        }
    }

    public int GetRemainingCoinCount()
    {
        int count = 0;
        foreach (var coin in RuntimeCoins)
        {
            if (!coin.isDestroyed) count++;
        }
        return count;
    }

    public void DestroyCoin()
    {
        for (int i = RuntimeCoins.Count - 1; i >= 0; i--)
        {
            if (!RuntimeCoins[i].isDestroyed)
            {
                // 파괴 불가 코인 기믹 처리
                if (RuntimeCoins[i].originCoinData != null && RuntimeCoins[i].originCoinData.isUnbreakable)
                {
                    Debug.Log($"[System] 파괴 불가 코인이므로 파괴를 무시합니다.");
                    continue;
                }

                RuntimeCoins[i].isDestroyed = true;
                Debug.Log($"[System] 코인이 파괴되었습니다. 남은 코인: {GetRemainingCoinCount()}");
                break;
            }
        }
    }
}