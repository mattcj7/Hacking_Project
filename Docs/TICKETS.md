## Current Ticket

## Ticket 0015A - Mission completion (set state + fire MissionCompletedEvent + UI shows completed)

**Goal:**  
When all objectives are completed, the mission should transition to **Completed**, publish `MissionCompletedEvent`, and the Missions app UI should visibly reflect completion.

**Non-goals:**  
- No rewards/economy yet
- No mission chains/next-mission selection (optional stub only)

**Acceptance criteria:**
- [ ] `MissionService` detects when **all objectives are complete** and then:
  - [ ] sets mission state to `Completed`
  - [ ] publishes `MissionCompletedEvent` exactly once
  - [ ] prevents repeated completion firing if additional events arrive
- [ ] Missions UI updates on completion:
  - [ ] Shows a clear “MISSION COMPLETE” indicator (badge/text)
  - [ ] (Optional) moves mission to a Completed section OR clears Active mission display
- [ ] Ensure objective completion triggers a mission completion check immediately after the last objective completes (no polling)
- [ ] Add/Edit EditMode tests:
  - [ ] Completing final objective fires `MissionCompletedEvent`
  - [ ] Mission state is `Completed` after final objective
- [ ] Unity compiles with 0 errors; tests pass

**Files allowed to edit:**  
- `Assets/Scripts/Infrastructure/**`
- `Assets/Scripts/Game/**`
- `Assets/Scripts/UI/Apps/Missions/**`
- `Assets/Tests/EditMode/**`

**Test plan:**
1. Run EditMode tests (all green)
2. Play Bootstrap
3. Open Missions app and Terminal
4. Complete objectives (`cd docs`, `cat readme.txt`)
5. Confirm UI shows “MISSION COMPLETE” and completion event occurs



## Completed - Support/Fix Tickets (non-ADR)

- **0003A** — Fix legacy Input usage after switching to Input System (remove `UnityEngine.Input` calls)
- **0005A** — Fix UI Toolkit USS unsupported properties (`border-top-style`, `unity-font-style` → supported equivalents)
- **0005B** — Bootstrap rendering fix (ensure at least one enabled Main Camera targeting Display 1)
- **0006A** — Auto-wire Window template + harden setup tool (idempotent / non-destructive)
- **0006B** — Setup tool selection-safe changes (reduce Inspector null-target exceptions)
- **0006C** — Reliable WindowTemplate auto-assign (SerializedObject + MarkSceneDirty persistence)
- **0006D** — Runtime self-heal for WindowTemplate + prevent duplicate setup objects
- **0007A** — Replace unsupported USS `gap` with margins
- **0007B** — Remove unsupported USS `:last-child` selector (no pseudo-class warnings)
- **0008A** — Taskbar clock format update to `HH:mm:ss`
- **0010A** — Auto-assign AppCatalog (setup tool + editor self-heal so taskbar buttons appear)
- **0012A** — Terminal readability + auto-scroll polish (UX improvements)
- **0012B** — Terminal single-Enter submit + focus + auto-scroll attempt
- **0012C** — Terminal Enter submits once without obsolete PreventDefault + readable input
- **0012D** — Terminal auto-scroll + readable input + Enter submit refinements
- **0012E** — Fix UI Toolkit scheduler misuse (`ExecuteLater` called on scheduled item, not scheduler)
- **0013** — Shared theme USS + File Manager matches Terminal color scheme
- **0014** - Persist OS Session State (open apps/windows + last paths persisted/restored)
- **0015** - Mission System v1 (SO missions + objectives + Missions app UI)