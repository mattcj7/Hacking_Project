## Current Ticket

## Ticket 0016C - Fix window resize/drag interaction (no snapping, per-window, clamp onscreen)

**Goal:**  
Fix UI Toolkit window interactions:
- Resize handle works (bottom-right)
- Resizing does NOT trigger titlebar dragging or snap windows
- Drag affects only the active window
- Clamp windows so the titlebar can’t be lost off-screen

**Background / Issue (repro):**
- Resize handle is visible but dragging it does nothing
- On release, window snaps to the left edge / toggles positions
- With two windows open, interacting with resize on one can move the other or push it above the game view

**Acceptance criteria:**
- [ ] Resize handle captures pointer and receives move events (PointerCapture)
- [ ] Resize handler stops event propagation so titlebar drag never runs:
  - [ ] StopImmediatePropagation + StopPropagation on down/move/up
- [ ] Resizing changes only `style.width` / `style.height` (min 320x200) and does not modify left/top
- [ ] Drag handler ignores input while window is resizing
- [ ] No shared/static drag/resize state across windows
- [ ] After drag end (and after restore), clamp window position inside desktop bounds:
  - [ ] Keep titlebar reachable (top >= 0; left within bounds)
- [ ] Unity compiles with 0 errors and behavior verified with two windows open

**Files allowed to edit:**
- `Assets/Scripts/UI/Desktop/**` (WindowView/WindowManager interactions)
- `Assets/UI/**` (window template UXML/USS if needed)
- `Docs/ADR.md`
- `Docs/TICKETS.md`

**Test plan:**
1. Play Bootstrap
2. Open Terminal + Missions
3. Resize each using handle: width/height changes live
4. Drag each: only that one moves
5. Resize does not cause snap/jump or move the other window
6. Try to drag near edges: titlebar stays recoverable



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
