## Current Ticket

## Ticket 0016B - Notifications + Start Next Mission + Resizable windows + ADR + TICKETS bookkeeping

**Goal:**  
Improve UX and “OS feel” with:
- Toast notifications (mission complete, credits gained)
- Start Next Mission button in Missions app
- Window resizing via drag handle
- Ensure ADR.md and TICKETS.md bookkeeping stays current

**Non-goals:**  
- No new mission logic types
- No store/inventory yet
- No animation-heavy UI (keep lightweight)
- No real OS/network access (fictional only)

**Acceptance criteria:**

### A) Toast notifications (desktop-level)
- [ ] Add a small notification system:
  - [ ] `NotificationService` (plain C#) posts notifications via EventBus (e.g., `NotificationPostedEvent`)
  - [ ] DesktopShell displays a toast stack (top-right or bottom-right)
  - [ ] Toasts auto-dismiss after N seconds (e.g., 3.5s) without per-frame polling
- [ ] Post notifications on:
  - [ ] Mission completion (“Mission Complete: <Title>”)
  - [ ] Credits awarded (“+X Credits”)
- [ ] Toast UI uses shared theme and is readable

### B) Missions app: Start Next Mission button
- [ ] When active mission is Completed and a next mission exists (catalog order):
  - [ ] Show a `Start Next Mission` button
  - [ ] Clicking starts the next mission and refreshes Missions UI
- [ ] If no next mission exists:
  - [ ] Show “No more missions” (minimal) and hide/disable the button

### C) Window resizing
- [ ] Add resize support to windows:
  - [ ] Window template includes a bottom-right resize handle element
  - [ ] Dragging the handle resizes by setting `style.width` and `style.height`
  - [ ] Enforce minimum size (e.g., 320x200)
  - [ ] Uses UI Toolkit pointer events with pointer capture during resize drag
  - [ ] Resizing does not break existing focus/drag/close behaviors
  - [ ] Window contents still layout correctly after resize

### D) Documentation discipline (ADR + Tickets)
- [ ] Update `Docs/ADR.md` by appending:
  - [ ] **ADR-0016A**: Missions UI scroll/non-overlapping layout approach (if not already present)
  - [ ] **ADR-0016B**: Toast notifications + resize handle approach + Start Next Mission UX
  - [ ] Add/append a short process rule: “Each completed ticket must add/update ADR.md”
- [ ] Update `Docs/TICKETS.md`:
  - [ ] Append `0016B` under `## Completed - Support/Fix Tickets (non-ADR)`
  - [ ] If `0016A` is not yet listed there, add it too

**Files allowed to edit:**  
- `Assets/Scripts/Infrastructure/**`
- `Assets/Scripts/UI/**`
- `Assets/UI/**`
- `Docs/ADR.md`
- `Docs/TICKETS.md`
- `AGENTS.md` (optional, only if used for the process rule)

**Test plan:**
1. Play Bootstrap
2. Complete a mission; confirm toasts appear for mission complete + credits
3. Open Missions app; click “Start Next Mission”; verify next mission starts
4. Resize a window via bottom-right handle; confirm min size and content reflows
5. Confirm Console has 0 errors/warnings




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
