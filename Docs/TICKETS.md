## Ticket 0005A - Fix UI Toolkit USS warnings (Unity 6.3 supported properties)

**Goal:**  
Remove UI Toolkit USS warnings by replacing unsupported USS properties with Unity 6.3 (6000.3) supported equivalents.

**Background / Issue:**  
Unity Console warnings:
- `Unknown property 'border-top-style'`
- `Unknown property 'unity-font-style'` (should be `-unity-font-style`)

**Non-goals:**  
- Do not redesign the UI
- Do not change UXML structure
- Keep visual appearance the same (or as close as possible)

**Acceptance criteria:**
- [ ] In `Assets/UI/Desktop/DesktopShell.uss`, remove any `border-top-style` usage
  - [ ] Replace with supported properties (e.g. `border-top-width` + `border-top-color`)
- [ ] Replace `unity-font-style` with `-unity-font-style`
- [ ] Unity Console shows **0 USS warnings** related to `DesktopShell.uss`

**Files allowed to edit:**  
- `Assets/UI/Desktop/DesktopShell.uss`

**Test plan:**
1. Open Unity project
2. Confirm Console has 0 USS warnings (or specifically none referencing `DesktopShell.uss`)
3. Press Play in `Bootstrap` and confirm desktop/taskbar still looks correct
