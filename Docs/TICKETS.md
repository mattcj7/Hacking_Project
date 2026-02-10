## Current Ticket

## Ticket 0016G - Window resizing applies visually (resize the window frame element)

**Goal:**  
Fix window resizing where resize math/logs run but the window does not visually change size.

**Background / Issue:**  
- Resize start/end logs show size changing, but the window’s visible frame does not resize.
- Indicates width/height are being applied to an element that is not the visible window frame.

**Acceptance criteria:**
- [ ] Update `Assets/UI/Windows/Window.uxml` to include an explicit frame element:
  - [ ] Wrap all window visuals (title bar, content, resize handle) in a container named `window-frame`
- [ ] Update `WindowView` to store `Frame` (the resizable target):
  - [ ] `public VisualElement Frame { get; }`
  - [ ] `Create()` queries `window-frame` and throws if missing
- [ ] Resizing updates `Frame.style.width` and `Frame.style.height` (min size enforced)
  - [ ] Optionally also set Root width/height to match Frame for consistency
- [ ] Dragging continues to move the window using Root left/top
- [ ] ClampWindowToBounds uses Frame size (or Root if Frame missing) when clamping
- [ ] Add a temporary Editor-only debug log at resize end:
  - computed size + Frame.resolvedStyle.width/height (to confirm it applied)
- [ ] No new USS warnings (no z-index, no unsupported properties)
- [ ] Append ADR-0016G to Docs/ADR.md and update Docs/TICKETS.md completed section when done
- [ ] Verified with 2 windows open: both visibly resize

**Files allowed:**
- `Assets/UI/Windows/Window.uxml`
- `Assets/UI/Windows/Window.uss` (only if needed)
- `Assets/Scripts/UI/Windows/WindowView.cs`
- `Assets/Scripts/UI/Windows/WindowManager.cs`
- `Docs/ADR.md`
- `Docs/TICKETS.md`

**Test plan:**
1. Play Bootstrap
2. Open Terminal + File Manager
3. Resize each window; confirm visible resizing
4. Confirm logs show Frame.resolvedStyle matches computed size
5. Confirm Console has 0 warnings/errors




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
