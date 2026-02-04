## Ticket 0002 — Assets folder structure + asmdefs + editor settings

**Goal:** Create a clean Unity code layout and assembly boundaries so compile times stay fast and modules don’t sprawl.

**Non-goals:**
- No gameplay features
- No UI work
- No package additions unless required for asmdef/test layout

**Acceptance criteria:**
- [ ] Create folders:
  - `Assets/Scripts/Game/`
  - `Assets/Scripts/UI/`
  - `Assets/Scripts/Systems/`
  - `Assets/Scripts/Infrastructure/`
  - `Assets/Tests/` with `EditMode/` and `PlayMode/`
- [ ] Create asmdefs:
  - `HackingProject.Game`
  - `HackingProject.UI`
  - `HackingProject.Systems`
  - `HackingProject.Infrastructure`
  - `HackingProject.Tests.EditMode`
  - `HackingProject.Tests.PlayMode`
- [ ] Assembly references:
  - `Game` references `Infrastructure`, `Systems`
  - `UI` references `Infrastructure`, `Systems`
  - `Systems` references `Infrastructure`
  - `Infrastructure` references none
  - Tests reference the relevant assemblies
- [ ] Unity compiles with **zero** errors
- [ ] Add a short ADR entry noting the module layout

**Files allowed to edit:**
- `Assets/**` (folders + `.asmdef`)
- `Docs/ADR.md`

**Test plan:**
1. Open Unity project.
2. Wait for compilation to complete.
3. Confirm Console shows **0 errors**.
