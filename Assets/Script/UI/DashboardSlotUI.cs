using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DashboardSlotUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image[] skillIcons;
    [SerializeField] private TMP_Text[] skillNameTexts;

    private BattleCharacter _targetCharacter;

    public void Setup(BattleCharacter character)
    {
        _targetCharacter = character;

        if (portraitImage != null && _targetCharacter.OriginData.portrait != null)
        {
            portraitImage.sprite = _targetCharacter.OriginData.portrait;
        }

        UpdateSkills();
    }

    public void UpdateSkills()
    {
        if (_targetCharacter == null || skillIcons == null || skillIcons.Length == 0) return;

        RuntimeSkill[] visibleSkills = _targetCharacter.DeckSystem.GetVisibleSkills(skillIcons.Length);

        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (skillIcons[i] == null) continue;

            if (i < visibleSkills.Length && visibleSkills[i] != null)
            {
                skillIcons[i].enabled = true;

                if (visibleSkills[i].OriginData != null)
                {
                    // [추가된 핵심 코드] OriginData에서 스킬 아이콘 이미지를 가져와 할당
                    // (주의: OriginData 스크립트 내부에 선언된 이미지 변수명에 맞춰 'skillIcon' 부분을 수정하십시오)
                    if (visibleSkills[i].OriginData.skillIcon != null)
                    {
                        skillIcons[i].sprite = visibleSkills[i].OriginData.skillIcon;
                    }

                    if (skillNameTexts != null && skillNameTexts.Length > i && skillNameTexts[i] != null)
                    {
                        skillNameTexts[i].text = visibleSkills[i].OriginData.skillName;
                    }
                }

                // 핵심 추가 로직: 스킬 아이콘에 부착된 드래그 컨트롤러에 데이터 주입
                SkillDragController dragCtrl = skillIcons[i].GetComponent<SkillDragController>();
                if (dragCtrl != null)
                {
                    dragCtrl.Setup(_targetCharacter, visibleSkills[i]);
                }
            }
            else
            {
                skillIcons[i].enabled = false;
                skillIcons[i].sprite = null; // [추가된 안전 장치] 슬롯이 비었을 때 이전 이미지 초기화

                if (skillNameTexts != null && skillNameTexts.Length > i && skillNameTexts[i] != null)
                {
                    skillNameTexts[i].text = "";
                }
            }
        }
    }
}