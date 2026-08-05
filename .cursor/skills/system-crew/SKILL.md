---
name: system-crew
description: >-
  On-demand game-system crew: turn user descriptions and reference videos/games
  into similar systems (Producer → Analyst → Implementer → Fidelity QA),
  reference asset docs, and objective idea evaluation. Splits mixed prompts
  into ordered Stages; declines out-of-protocol asks (skill docs, tooling-only)
  and reroutes. Use ONLY when the user explicitly invokes system-crew /
  시스템 크루 / Producer / Systems Analyst / 참고 재현 / reference asset /
  아이디어 평가, or asks to build a system from a reference. Do not use for
  ordinary feature work in this project.
---

# system-crew (on-demand)

이 스킬이 호출되었을 때만 system-crew 프로토콜을 적용한다.  
평소 프로젝트 룰·워크플로를 덮어쓰지 않는다.

## 활성화 시 따를 것

1. `.cursor/system-crew/AGENTS.md`
2. `.cursor/system-crew/workflows/from-reference.md`
3. `.cursor/system-crew/workflows/reference-assets.md`
4. `.cursor/system-crew/workflows/idea-evaluation.md`
5. 역할 파일: `.cursor/system-crew/roles/`

## 역할

| 요청 | 역할 |
|------|------|
| 범위·라우팅 | Producer |
| 프로토콜 밖(스킬 문서화·일상 툴링 등) | Producer → decline / 프로젝트 스킬로 재라우팅 |
| 참고 분해·스펙·자산화 | Systems Analyst |
| 구현 | Implementer |
| 충실도 검수 | Fidelity QA |
| 아이디어 판단 후 적용/거절 | Idea evaluation |

## 혼합 요청 → 단계 분할

한 프롬프트에 **성격이 다른 일**이 섞여 있고, 한 패스로 처리하면 범위·역할·승인이 흐려진다고 판단되면 **반드시 단계를 나눈다.** 한꺼번에 전부 수행하지 않는다.

### 분할 트리거 (해당하면 분할)

- 역할이 둘 이상: 분석 / 스펙 / 아이디어 평가 / 구현 / QA
- 시스템·수직 슬라이스가 둘 이상
- “분석만”과 “구현까지”, “평가”와 “바로 적용”이 한 문장에 공존
- 참고 재현과 무관한 부가 작업(리팩터, UI, 툴링 등)이 같이 들어옴

### 절차

1. Producer Intake에서 `Request type: mixed`로 표시
2. 일을 **순서 있는 Stage**로 쪼개고, Stage마다 담당 역할·산출물·승인 게이트를 적는다
3. 사용자에게 Stage 계획을 **짧게** 제시한 뒤, **Stage 1만** 진행한다 (사용자가 특정 Stage를 지정하면 그것부터)
4. Stage가 끝나기 전에 다음 Stage의 넘어가지 않는다. 끝나면 Next를 다시 선언한다

### Stage 계획 출력 (짧게)

```markdown
## Stages (mixed)
1. [역할] — 할 일 / 산출물 / 게이트
2. ...
→ Now: Stage 1 only
```

예: “이 영상 분석해서 자산화하고, 상태머신 아이디어도 판단해서 괜찮으면 구현해줘”  
→ Stage 1 Analyst 자산화 → Stage 2 Idea evaluation → Stage 3 (ADOPT*면) Implementer. 지금은 Stage 1만.

## 프로토콜 밖

요청이 참고 재현·시스템 스펙/구현·아이디어 평가가 아니면 (`roles/01-producer.md` “프로토콜 밖”):

- `Request type: out_of_scope`
- 4역할 루프·`docs/references/` 자산을 **만들지 않음**
- 왜 맞지 않는지 짧게 설명하고 프로젝트 스킬/일반 구현으로 재라우팅

예: “이 채팅의 MCP 검증 습관을 스킬로 남겨줘” → create-skill / project-workflows 경로.

## 문서 경로

- 참고 자산: `docs/references/`
- 아이디어 판정: `docs/decisions/ideas/`

## 비활성 기본값

사용자가 system-crew를 부르지 않으면 이 스킬·프로토콜을 사용하지 않는다.
