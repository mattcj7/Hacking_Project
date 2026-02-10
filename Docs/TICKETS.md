## Current Ticket

## Ticket 0016C - Fix window layout + clamp (absolute positioning, no multi-window shifts, no snap-left)

**Goal:**
Fix window behavior so:
- Clicking/focusing a window doesn’t cause other windows to shift
- Resizing doesn’t move other windows
- Window doesn’t snap to left when resized wider than desktop
- Windows remain recoverable (title bar always reachable)

**Acceptance criteria:**
- [ ] Window root is forced to `position: absolute` (code and/or USS)
- [ ] Resize handle is positioned/sized (bottom-right, clickable)
- [ ] Clamp logic allows windows wider than desktop by keeping a minimum visible portion (does not force left=0)
- [ ] Clamp uses `_windowsLayer` bounds (not parent) and runs after layout (ExecuteLater(0))
- [ ] Verified with 2 windows open: focusing one does not shift the other; resizing one does not move the other
- [ ] Update `Docs/ADR.md` with ADR-0016C and add ticket to Completed section when done

**Files allowed:**
- `Assets/Scripts/UI/Windows/WindowView.cs`
- `Assets/Scripts/UI/Windows/WindowManager.cs`
- `Assets/UI/Windows/**` (template USS/UXML if needed)
- `Docs/ADR.md`
- `Docs/TICKETS.md`




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
