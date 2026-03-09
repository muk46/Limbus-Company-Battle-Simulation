using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Dropdown characterDropdown;

    [Header("Target Settings")]
    [Tooltip("체크하면 아군, 해제하면 적군(허수아비)을 교체합니다.")]
    public bool isPlayerTeam = true;
    public int slotIndex = 0;

    [Header("Character Database")]
    [Tooltip("드롭다운 리스트에 표시할 모든 캐릭터 데이터(ScriptableObject)를 할당하십시오.")]
    public List<CharacterData> allCharacters = new List<CharacterData>();

    private void Start()
    {
        // BattleManager가 캐릭터 스폰을 완료할 수 있도록 아주 짧은 지연 후 UI 동기화 실행
        Invoke(nameof(InitializeDropdown), 0.1f);
    }

    private void InitializeDropdown()
    {
        if (characterDropdown == null) return;

        characterDropdown.ClearOptions();
        List<string> options = new List<string>();

        // 1. 드롭다운 옵션 텍스트 채우기
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i] != null)
                options.Add(allCharacters[i].characterName);
            else
                options.Add("Empty Data");
        }
        characterDropdown.AddOptions(options);

        // 2. 게임 시작 시 현재 맵에 배치된 캐릭터와 드롭다운 표기값 동기화
        if (BattleManager.Instance != null)
        {
            List<CharacterData> currentTeamData = isPlayerTeam ? BattleManager.Instance.playerTeamData : BattleManager.Instance.enemyTeamData;

            // 현재 UI의 슬롯 인덱스가 팀 데이터 리스트 범위 안에 존재할 경우
            if (slotIndex < currentTeamData.Count && currentTeamData[slotIndex] != null)
            {
                CharacterData startingChar = currentTeamData[slotIndex];

                // 해당 캐릭터가 전체 데이터베이스(allCharacters)의 몇 번째 인덱스인지 검색
                int startingIndex = allCharacters.IndexOf(startingChar);

                if (startingIndex >= 0)
                {
                    // 값을 변경하되, 교체 로직(이벤트)이 실행되지 않도록 표기만 변경 (무한루프 방지)
                    characterDropdown.SetValueWithoutNotify(startingIndex);
                }
            }
        }

        // 3. 사용자가 직접 클릭하여 값을 바꿀 때만 교체 로직이 실행되도록 리스너 등록
        characterDropdown.onValueChanged.AddListener(OnCharacterSelected);
    }

    private void OnCharacterSelected(int index)
    {
        if (index < 0 || index >= allCharacters.Count) return;

        CharacterData selectedData = allCharacters[index];

        if (selectedData != null && BattleManager.Instance != null)
        {
            BattleManager.Instance.SwapCharacter(isPlayerTeam, slotIndex, selectedData);
            BattleManager.Instance.ResetBattle();

            Debug.Log($"[System] 슬롯 {slotIndex}번: {selectedData.characterName}(으)로 교체 완료.");
        }
    }
}