## Current Ticket

## Ticket 0017A - Fix Store/Install assembly boundaries (remove UI deps, move to Systems)

**Goal:**  
Fix compile errors by aligning Store/Install pipeline with ADR-0001 assembly boundaries.

**Background / Issue:**  
Store files were created under `Assets/Scripts/Infrastructure/Store/` but reference `HackingProject.UI` and UI types (AppRegistry/AppDefinitionSO/AppId). Infrastructure cannot reference UI, causing CS0234/CS0246 errors.

**Acceptance criteria:**
- [ ] Move Store-related runtime code out of Infrastructure into Systems:
  - [ ] `Assets/Scripts/Systems/Store/**` (or similar Systems folder)
- [ ] Remove all references to `HackingProject.UI` and UI-only types from Store pipeline:
  - [ ] Replace `AppId` with `string appId`
  - [ ] Remove `AppDefinitionSO` from events/services
  - [ ] Remove `AppRegistry` and `AppPackageDatabaseSO` dependencies from InstallService
- [ ] Define Store pipeline events in Systems (depends only on Infrastructure IEvent):
  - [ ] `StorePurchaseCompletedEvent` (itemId, appId, price)
  - [ ] `AppInstalledEvent` (appId)
- [ ] InstallService responsibilities:
  - [ ] Updates `SaveGameData.InstalledAppIds` (idempotent)
  - [ ] Publishes `AppInstalledEvent(appId)`
  - [ ] Does NOT directly manipulate UI registries/taskbar
- [ ] UI integration:
  - [ ] StoreController (UI) subscribes to `AppInstalledEvent` and refreshes AppRegistry/taskbar via existing UI code path
  - [ ] Or AppRegistry listens and rebuilds itself when install events fire
- [ ] Unity compiles with 0 errors and existing tests remain green
- [ ] Docs:
  - [ ] Append ADR-0017A to `Docs/ADR.md` describing the boundary fix (Systems owns Store logic; UI reacts via events)
  - [ ] Update `Docs/TICKETS.md` completed sections when finished

**Files allowed:**
- `Assets/Scripts/Infrastructure/Store/**` (delete/move)
- `Assets/Scripts/Systems/Store/**` (new)
- `Assets/Scripts/UI/Apps/Store/**` (wire UI refresh on install event)
- `Docs/ADR.md`
- `Docs/TICKETS.md`

**Test plan:**
1. Unity recompiles with 0 errors
2. Open Store, buy/install an app
3. Confirm taskbar updates (via UI reaction) and persistence still works




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
