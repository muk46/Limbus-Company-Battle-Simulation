using System.Collections.Generic;
using UnityEngine;

public class SkillDeckSystem
{
    private Queue<RuntimeSkill> skillQueue = new Queue<RuntimeSkill>();
    private SkillData skill1, skill2, skill3; // 원본 스킬 3종

    // 캐릭터의 스킬 3종을 받아 덱 시스템 초기화
    public void Initialize(SkillData s1, SkillData s2, SkillData s3)
    {
        skill1 = s1;
        skill2 = s2;
        skill3 = s3;

        skillQueue.Clear();
        RefillBlock();
        RefillBlock(); // 넉넉하게 2블록(12개) 미리 충전
    }

    // 1스킬 3개, 2스킬 2개, 3스킬 1개의 비율로 생성 후 섞어서 큐에 삽입
    private void RefillBlock()
    {
        List<RuntimeSkill> block = new List<RuntimeSkill>();

        for (int i = 0; i < 3; i++) block.Add(new RuntimeSkill(skill1));
        for (int i = 0; i < 2; i++) block.Add(new RuntimeSkill(skill2));
        block.Add(new RuntimeSkill(skill3));

        // 셔플 알고리즘
        for (int i = block.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            RuntimeSkill temp = block[i];
            block[i] = block[rnd];
            block[rnd] = temp;
        }

        foreach (var skill in block) skillQueue.Enqueue(skill);
    }

    // UI에 보여주거나 사용할 때 가장 앞단의 스킬을 꺼냄
    public RuntimeSkill ConsumeSkill()
    {
        if (skillQueue.Count == 0) return null;

        RuntimeSkill consumed = skillQueue.Dequeue();

        // 큐에 남은 스킬이 1블록(6개) 이하로 떨어지면 새 블록 자동 충전
        if (skillQueue.Count <= 6) RefillBlock();

        return consumed;
    }

    // 큐의 데이터를 훼손하지 않고 화면에 띄울 상위 N개 스킬 확인
    public RuntimeSkill[] GetVisibleSkills(int count = 2)
    {
        RuntimeSkill[] visibleSkills = new RuntimeSkill[count];
        var enumerator = skillQueue.GetEnumerator();

        for (int i = 0; i < count; i++)
        {
            if (enumerator.MoveNext()) visibleSkills[i] = enumerator.Current;
        }
        return visibleSkills;
    }
}