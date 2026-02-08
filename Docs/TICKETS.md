## Current Ticket

## Ticket 0015 - Mission System v1 (ScriptableObject missions + objective evaluation + Mission app UI)

**Goal:**  
Introduce a minimal, scalable **Mission System** that drives gameplay using existing simulated systems:
- Data-driven missions via ScriptableObjects
- Objective evaluation driven by events (Terminal/VFS/UI)
- A simple Mission app window to view the active mission + objectives

**Non-goals:**  
- No networking, no real hacking
- No mission rewards/store/economy yet (later ticket)
- No branching dialog
- No persistence of mission progress yet (optional stub only)

**Acceptance criteria:**
- [ ] Mission data is ScriptableObject-driven:
  - [ ] `MissionDefinitionSO` (id, title, description, list of objectives)
  - [ ] `MissionCatalogSO` (list of missions)
  - [ ] Create assets under `Assets/ScriptableObjects/Missions/`:
    - [ ] `MissionCatalog_Default.asset`
    - [ ] One starter mission asset: `M001_ReadTheDocs`
- [ ] Mission runtime service:
  - [ ] `MissionService` (plain C# class) created/owned by `GameBootstrapper` (no global static)
  - [ ] Can set an active mission and evaluate objective completion
  - [ ] Publishes events via EventBus:
    - [ ] `MissionStartedEvent`
    - [ ] `MissionObjectiveCompletedEvent`
    - [ ] `MissionCompletedEvent`
- [ ] Objective model:
  - [ ] Keep it minimal and testable:
    - Option A (recommended): objectives are SOs that implement an interface like `IMissionObjective` with `OnEvent(...)`
    - Option B: objectives are simple serializable structs evaluated by MissionService
  - [ ] Must support at least these objective types for v1:
    - [ ] Terminal command executed (e.g. `cat /home/user/docs/readme.txt`)
    - [ ] File viewed in File Manager (e.g. user navigated to `/home/user/docs` and selected `readme.txt`)
- [ ] Integration points (events to drive objectives):
  - [ ] Terminal publishes `TerminalCommandExecutedEvent` (command + args + resolved cwd path)
  - [ ] File Manager publishes `FileManagerOpenedFileEvent` (full path)
  - [ ] MissionService subscribes to these events and updates objective state
- [ ] Mission app UI:
  - [ ] UI Toolkit assets:
    - `Assets/UI/Apps/Missions/MissionsView.uxml`
    - `Assets/UI/Apps/Missions/MissionsView.uss`
  - [ ] Controller:
    - `Assets/Scripts/UI/Apps/Missions/MissionsController.cs`
  - [ ] Shows:
    - Active mission title + description
    - List of objectives with completed state
- [ ] App catalog:
  - [ ] Add a new app definition asset for "Missions" and include it in `AppCatalog_Default`
  - [ ] Clicking Missions opens the Mission window (single-instance)
- [ ] Starter mission content:
  - Mission `M001_ReadTheDocs` objectives (example):
    1) In Terminal, `cd docs`
    2) In Terminal, `cat readme.txt` (or absolute path)
  - Ensure the default VFS includes `/home/user/docs/readme.txt` with some text content
- [ ] EditMode tests:
  - [ ] TerminalCommandExecutedEvent completes the expected objective(s)
  - [ ] Mission completes when all objectives are satisfied
- [ ] Unity compiles with 0 errors, tests pass, Play Mode mission works

**Files allowed to edit:**  
- `Assets/Scripts/Infrastructure/**`
- `Assets/Scripts/Game/**`
- `Assets/Scripts/UI/**`
- `Assets/UI/**`
- `Assets/ScriptableObjects/**`
- `Assets/Tests/EditMode/**`
- `Docs/ADR.md`

**Implementation notes:**
- Keep mission evaluation event-driven (no polling).
- Keep it deterministic and fictional: objectives are about *simulated* files/commands only.
- Avoid allocations per frame; only allocate on command execution / UI interactions.

**Test plan:**
1. Run EditMode tests (all green)
2. Play Bootstrap
3. Open Missions app and confirm mission is visible
4. Open Terminal:
   - `pwd` (optional)
   - `cd docs`
   - `cat readme.txt`
5. Confirm objectives tick off and mission completes
6. Confirm Console has 0 errors


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