## Current Ticket

## Ticket 0019 - Save/Load v2 (format versioning + migrations + autosave debounce)

**Goal:**  
Make saves future-proof and reduce progress loss by adding:
- Save format versioning
- Migration path for older saves
- Autosave (debounced) on key gameplay events

**Non-goals:**  
- No cloud saves
- No encryption beyond existing integrity hash
- No UI save slots menu (later ticket)

**Acceptance criteria:**

### A) Save format versioning
- [ ] Add a `SaveFormatVersion` integer to the save envelope (preferred) or payload:
  - [ ] Current version constant: `public const int CurrentSaveVersion = 2;`
- [ ] On load:
  - [ ] If version missing (older saves), treat as version 1
  - [ ] If version > CurrentSaveVersion, fail gracefully with a clear error message
- [ ] Keep existing atomic write + .bak + SHA-256 integrity behavior intact

### B) Migrations
- [ ] Introduce a migration pipeline:
  - [ ] `SaveMigrationService` (plain C#) with:
    - [ ] `SaveGameData Migrate(SaveGameData data, int fromVersion, int toVersion)`
  - [ ] Implement at least:
    - [ ] v1 -> v2 migration (fill defaults for new fields like Credits/InstalledAppIds/OwnedAppIds/OsSessionData if missing)
- [ ] Ensure migration happens AFTER integrity is verified (if applicable) and BEFORE game systems initialize

### C) Autosave (debounced)
- [ ] Add an `AutoSaveService` (plain C#) owned by `GameBootstrapper`:
  - [ ] Subscribes to EventBus and requests saves on:
    - [ ] `MissionCompletedEvent`
    - [ ] `MissionRewardGrantedEvent` (credits)
    - [ ] `StorePurchaseCompletedEvent`
    - [ ] `AppInstalledEvent`
    - [ ] (Optional) `SaveSessionCaptureEvent` when window positions change
  - [ ] Debounce window: e.g. 1.5 seconds (multiple events coalesce into one save)
  - [ ] Uses existing SaveService to perform the save
  - [ ] Logs one line per autosave (Editor/Dev only): reason + timestamp
- [ ] Ensure autosave does not run every frame and does not allocate per frame

### D) Tests
- [ ] EditMode tests:
  - [ ] Loading v1 save migrates to v2 with expected defaults
  - [ ] Autosave debounce: multiple events within debounce triggers exactly one Save call (mock SaveService)
  - [ ] Autosave triggers on AppInstalledEvent / MissionCompletedEvent

### E) Documentation discipline
- [ ] Append **ADR-0019** to `Docs/ADR.md` describing:
  - versioning location (envelope vs payload)
  - migration strategy
  - autosave debounce approach
- [ ] Update `Docs/TICKETS.md` completed section when done

**Files allowed to edit:**
- `Assets/Scripts/Infrastructure/**` (SaveService / envelope / migrations)
- `Assets/Scripts/Systems/**` (AutoSaveService if that’s where services live)
- `Assets/Scripts/Game/**` (Bootstrap wiring)
- `Assets/Tests/EditMode/**`
- `Docs/ADR.md`
- `Docs/TICKETS.md`

**Test plan:**
1. Run EditMode tests (all green)
2. Play Bootstrap
3. Complete a mission and observe autosave log
4. Buy + download installer, install app: observe autosave log(s) (debounced)
5. Stop/Start Play: progress persists
6. Confirm Console has 0 errors/warnings


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
- **0016** - Credits rewards + mission chaining
- **0016A** - Missions window: fix overlapping text + add scrolling + consistent styling
- **0016B** - Toast notifications + mission progression UI + window resizing
- **0016C** - Window instance safety + drag/resize clamp
- **0016D** - Resize reliability via root pointer capture
- **0016E** - Resize hot zone + hover highlight + debug logs
- **0017** - Store + Purchase + Install pipeline
- **0017A** - Store/Install boundary fix (move to Systems, remove UI deps)
- **0018** - Installer file pipeline (purchase drops installer; install via File Manager/Terminal)
- **0019** - Save versioning + migration + autosave
