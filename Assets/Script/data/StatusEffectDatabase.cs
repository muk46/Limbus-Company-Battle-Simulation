using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// StatusEffectType → StatusEffectData 매핑 레지스트리.
/// BattleManager.Awake()에서 Initialize()를 호출해 싱글톤으로 등록합니다.
/// </summary>
[CreateAssetMenu(fileName = "StatusEffectDatabase", menuName = "Combat/Status Effect Database")]
public class StatusEffectDatabase : ScriptableObject
{
    public static StatusEffectDatabase Instance { get; private set; }

    [SerializeField] private List<StatusEffectData> _effects = new List<StatusEffectData>();

    private Dictionary<StatusEffectType, StatusEffectData> _lookup;

    public void Initialize()
    {
        Instance = this;
        _lookup = new Dictionary<StatusEffectType, StatusEffectData>();

        foreach (var effect in _effects)
        {
            if (effect != null)
                _lookup[effect.effectType] = effect;
        }

        Debug.Log($"[StatusEffectDatabase] {_lookup.Count}개 상태이상 등록 완료.");
    }

    /// <summary>등록된 StatusEffectData를 반환. 미등록 타입이면 null 반환.</summary>
    public StatusEffectData Get(StatusEffectType type)
    {
        _lookup.TryGetValue(type, out var data);
        return data;
    }
}
