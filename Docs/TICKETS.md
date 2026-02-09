## Current Ticket

## Ticket 0016 - Rewards + Credits + Mission chaining (persisted progression loop)

**Goal:**  
Turn missions into a real progression loop by adding:
- Credits (wallet) as the first reward currency
- Auto-award credits on mission completion
- Persist credits via existing SaveGameData / SaveService
- Mission chaining: after completion, move to Completed and start the next mission (or allow selection)

**Non-goals:**  
- No store/shop UI yet
- No item inventory yet
- No balance tuning
- No branching narrative
- No real-world hacking functionality (keep everything fictional and simulated)

**Acceptance criteria:**

### Credits / Wallet
- [ ] Add `Credits` as the canonical currency field in `SaveGameData` (if it already exists, standardize on it)
- [ ] Add a small runtime service (plain C#) e.g. `WalletService` owned by `GameBootstrapper`:
  - [ ] `int Credits { get; }`
  - [ ] `void AddCredits(int amount, string reason)`
  - [ ] Publishes `CreditsChangedEvent` via EventBus
- [ ] On startup:
  - [ ] WalletService initializes from loaded SaveGameData.Credits
  - [ ] Saving writes updated Credits back into SaveGameData

### Mission rewards
- [ ] Extend `MissionDefinitionSO` to include reward data:
  - [ ] `int RewardCredits` (default 0)
- [ ] On mission completion:
  - [ ] MissionService awards credits via WalletService exactly once
  - [ ] MissionService ensures rewards are not double-awarded (guard/state)
  - [ ] Publishes `MissionRewardGrantedEvent` (includes amount + mission id)

### Mission chaining / mission list
- [ ] Add mission status tracking:
  - [ ] Track Completed mission ids (in memory at minimum)
  - [ ] Missions UI shows:
    - Active mission
    - Completed missions list (at least mission title + “Completed”)
- [ ] Chaining behavior (pick one; implement minimal):
  - Option A (recommended): MissionCatalog order-based chain:
    - After completing mission i, automatically start mission i+1 if exists
  - Option B: UI selection:
    - Missions app shows available missions; player clicks “Start”
- [ ] Add at least one additional mission asset in `MissionCatalog_Default`:
  - [ ] `M002_DownloadsAudit` (example objectives):
    - Terminal: `cd downloads`
    - Terminal: `ls`
    - File Manager: open `todo.txt` (or Terminal `cat todo.txt`)
  - Ensure the default VFS contains the needed file(s) under `/home/user/downloads/`

### UI
- [ ] Taskbar shows Credits value (same theme as Terminal/FileManager)
  - [ ] Updates when CreditsChangedEvent fires
- [ ] Missions app UI displays reward for active mission and/or completion summary:
  - [ ] Show “Reward: +<credits> credits”
  - [ ] When mission completes, show “MISSION COMPLETE” and “Reward Granted”
- [ ] (Optional but small) Add a simple toast/notification:
  - On reward grant, show “+X Credits” somewhere non-intrusive

### Tests
- [ ] EditMode tests:
  - [ ] Completing mission triggers MissionCompletedEvent and awards credits once
  - [ ] CreditsChangedEvent fires with correct delta
  - [ ] Chaining starts the next mission (if Option A), or mission list exposes missions for UI (if Option B)
- [ ] Unity compiles with 0 errors and tests are green

**Files allowed to edit:**  
- `Assets/Scripts/Infrastructure/**`
- `Assets/Scripts/Game/**`
- `Assets/Scripts/UI/**`
- `Assets/UI/**`
- `Assets/ScriptableObjects/**`
- `Assets/Tests/EditMode/**`
- `Docs/ADR.md`

**Implementation notes:**
- Keep everything instance-based (no static globals)
- Avoid per-frame allocations; UI updates should be event-driven
- Keep reward logic deterministic and idempotent

**Test plan:**
1. Run EditMode tests (all green)
2. Play Bootstrap
3. Open Missions app and confirm mission 1 is active with reward displayed
4. Complete mission 1 objectives
5. Confirm:
   - Mission shows completed
   - Credits increase and appear in taskbar
   - Next mission starts (or becomes selectable)
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
- **0015** - Mission System v1 (SO missions + objectives + Missions app UI)
- **0015A** - Mission completion state + UI indicator