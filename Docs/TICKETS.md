## Current Ticket

## Ticket 0018 - Installer file pipeline (purchase drops installer into VFS; install via File Manager/Terminal)

**Goal:**  
Make purchases feel like real OS activity:
- Buying an app creates an installer file in the VFS (e.g., `/home/user/downloads/<appId>.installer`)
- Installing happens via File Manager (open file) or Terminal (`install <path>`), not directly from Store UI

**Non-goals:**  
- No real executable code
- No sandboxing beyond current fictional model
- No cryptography beyond existing integrity

**Acceptance criteria:**
- [ ] Add `InstallerPackage` model:
  - [ ] Installer file format is a small JSON text payload stored in VFS file contents:
    - appId, displayName, version (int), pricePaid (optional), createdAt (optional)
- [ ] Store purchase behavior:
  - [ ] On successful purchase, write installer file to:
    - `/home/user/downloads/<appId>.installer`
  - [ ] If file exists, create a unique name or overwrite safely (deterministic)
  - [ ] Post notification: “Downloaded installer: <file>”
- [ ] File Manager integration:
  - [ ] Double-click/open `.installer` file triggers install prompt:
    - [ ] “Install <displayName>?” Confirm/Cancel
  - [ ] Confirm triggers InstallService.Install(appId)
- [ ] Terminal integration:
  - [ ] Add `install <path>` command:
    - [ ] Validates file exists and ends with `.installer`
    - [ ] Parses payload and calls InstallService.Install(appId)
    - [ ] Writes terminal output for success/fail
- [ ] InstallService remains idempotent and persists InstalledAppIds
- [ ] Store UI:
  - [ ] After purchase, button becomes “Downloaded” or “Owned” (no direct install)
- [ ] Tests:
  - [ ] Purchase creates installer file in VFS with correct payload
  - [ ] Terminal install from installer path installs app (idempotent)
- [ ] Docs:
  - [ ] Append ADR-0018 to Docs/ADR.md
  - [ ] Update Docs/TICKETS.md completed sections when done

**Files allowed:**
- `Assets/Scripts/Systems/Store/**`
- `Assets/Scripts/Systems/VFS/**`
- `Assets/Scripts/UI/Apps/Store/**`
- `Assets/Scripts/UI/Apps/FileManager/**`
- `Assets/Scripts/UI/Apps/Terminal/**`
- `Assets/Tests/EditMode/**`
- `Docs/ADR.md`
- `Docs/TICKETS.md`

**Test plan:**
1. Play Bootstrap
2. Open Store and buy an app
3. Open File Manager → Downloads; see `<appId>.installer`
4. Double-click installer; confirm install; app appears in taskbar
5. Repeat using Terminal: `install /home/user/downloads/<appId>.installer`
6. Stop/Start Play: installed app persists





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
