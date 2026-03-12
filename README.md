# Limbus Company 전투 시스템 재현 프로젝트

> 림버스 컴퍼니(Project Moon)의 전투 메커니즘을 분석하고 Unity로 재현한 포트폴리오 프로젝트입니다.

---

## 프로젝트 소개

림버스 컴퍼니의 핵심 전투 공식과 시스템을 역공학(Reverse Engineering)하여 Unity C#으로 구현했습니다.
단순한 모방이 아닌, 실제 게임과 **동일한 난수 환경에서 동일한 데미지**가 산출되는 것을 검증하였습니다.

---

## 개발 환경

- **Engine** : Unity (URP, New Input System)
- **Language** : C#
- **설계 패턴** : State Machine, Event Bus, ScriptableObject 기반 데이터 주도 설계

---

## 재현한 시스템

### 1. 전투 상태 머신 (State Machine)

```
BattleWaitState → BattleExecuteState → BattleClashState → BattleWaitState
```

- `IBattleState` 인터페이스로 상태를 추상화하여 각 상태가 독립적으로 동작
- `BattleManager`가 상태 전환을 중앙 관리

---

### 2. 합(Clash) 시스템

림버스 컴퍼니의 핵심 전투 방식인 코인 대결 구조를 재현했습니다.

- **코인 판정** : 정신력(Sanity)이 앞면 확률에 직접 영향
  ```
  앞면 확률 = Clamp(50 + 현재 정신력, 5, 95)
  ```
- **합 위력** : `기본 위력 + (앞면 코인 수 × 코인 위력) + 합 보너스 + 레벨 보정`
- **코인 파괴** : 합에서 진 쪽의 코인이 1개씩 제거되며, 한쪽 코인이 전부 소진되면 승패 결정
- **클래시 보너스** : 합 횟수당 3% 데미지 보너스 적용

---

### 3. 데미지 계산 공식

```
최종 데미지 = 코인 위력 × (1 + 물리 내성 보정 + 속죄 내성 보정 + 레벨 보정 + 합 보너스)
```

- **레벨 보정** : `(공격 레벨 - 방어 레벨) / (25 + 절댓값(레벨 차))`
- **내성 보정** : 물리 타입(참/관통/타격) + 속죄 속성(분노/욕망/나태 등) 이중 적용
- **흐트러짐 배율** : 단계에 따라 내성 배율 2x / 2.5x / 3x 적용

---

### 4. 스킬 덱 시스템

- 1번 스킬 3장 / 2번 스킬 2장 / 3번 스킬 1장 = 6장을 1블록으로 셔플
- 초기화 시 2블록(12장)을 미리 큐에 적재
- 큐 잔량이 6장 이하가 되면 자동으로 1블록 보충

---

### 5. 공명(Resonance) 시스템

- **일반 공명** : 같은 속죄 속성 스킬이 2개 이상 등록될 때 누적 공격 레벨 보너스 부여
- **완전 공명** : 동일 속죄 속성이 3개 이상 연속 배치될 경우 연관 스킬 전체에 최대 보너스 일괄 적용

---

### 6. 정신력(Sanity) 시스템

| 상황 | 효과 |
|------|------|
| 합 승리 | `10 + (합 횟수 - 1) × 2` 정신력 회복 |
| 강적(동급 이상) 처치 | 처치자 +10, 아군 +5 |
| 강적(동급 이상) 아군 사망 | 생존자 `-(10 + 레벨 차 × 10)` |

---

### 7. 흐트러짐(Stagger) 시스템

- 캐릭터 HP가 특정 수치 이하로 내려갈 때마다 흐트러짐 단계 상승
- 흐트러진 캐릭터는 턴 행동 불가, 받는 데미지 배율 증가
- 임계치는 ScriptableObject에서 배열로 설정 가능

---

### 8. 상태이상 시스템

| 상태이상 | 동작 방식 |
|----------|-----------|
| 화상(Burn) | 턴 종료 시 고정 피해 |
| 출혈(Bleed) | 코인 판정마다 고정 피해 |
| 전동(Tremor) | 폭발 시 흐트러짐 선 전진 |
| 파열(Rupture) | 피격 시 추가 고정 피해 |
| 침잠(Sinking) | 피격 시 정신력 또는 HP 감소 |
| 호흡(Poise) | 타격 시 확률적 치명타 (1.2배) |
| 충전(Charge) | 스킬 효과 트리거용 (최대 20) |

---

### 9. 이고 기프트(EGO Gift) 시스템

ScriptableObject 기반 **모듈러 구조**로 설계하여, 조건과 효과를 에디터에서 조합 가능합니다.

```
EgoGiftBase
├── GiftCondition (조건 모듈)
│   ├── Condition_DamageThreshold  - 피해량 기준
│   ├── Condition_SkillProperty    - 데미지 타입 / 속죄 속성 일치
│   └── Condition_TargetStatus     - 대상의 상태이상 보유 여부
└── GiftEffect (효과 모듈)
    ├── Effect_ApplyStatus         - 상태이상 부여
    ├── Effect_DamageByStatus      - 상태이상 위력 기반 데미지
    └── Effect_DealDamage          - 고정 피해
```

- 기본 조건 충족 시 강화 효과, 미충족 시 대체 효과 발동하는 이중 경로(Path A/B) 구조
- `GiftManager`가 장착/해제 시 이벤트 구독/해제를 자동 처리

---

## 아키텍처 설명

### 이벤트 버스 구조

```
BattleEventManager (전역 이벤트 허브)
├── OnClashWin    → BattleSanitySystem, EgoGiftBase
├── OnHit         → EgoGiftBase
├── OnTurnEnd     → EgoGiftBase
└── OnCharacterDeath → BattleSanitySystem
```

`BattleManager` ↔ UI 간에는 `BattleEvents` (로컬 이벤트)를 사용하고,
기프트·정신력 등 게임플레이 시스템은 `BattleEventManager` (전역 이벤트)를 사용하여 결합도를 분리했습니다.

### 데이터 / 런타임 분리

- `CharacterData`, `SkillData` : 불변 ScriptableObject (원본 데이터)
- `BattleCharacter`, `RuntimeSkill` : 전투 중 변하는 런타임 인스턴스

---

## 검증

- 실제 림버스 컴퍼니와 **동일한 난수 환경(1v1)** 에서 동일한 데미지 수치 산출 확인
- 코인 위력, 레벨 보정, 내성 배율, 합 보너스 등 각 공식 수치를 게임 내 데이터와 대조 검증

---

## 조작법

| 키 | 동작 |
|----|------|
| 스킬 아이콘 드래그 → 적 드롭 | 공격 명령 등록 |
| `Space` | 등록된 행동 실행 |
| `G` | 기프트 창 열기/닫기 |
| `F1` | 전투 로그 표시/숨기기 |
