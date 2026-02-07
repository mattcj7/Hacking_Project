## Ticket 0007B - Remove unsupported USS pseudo-class :last-child (fix spacing without warnings)

**Goal:**  
Eliminate the USS warning "Unknown pseudo class last-child" while keeping taskbar button spacing.

**Background / Issue:**  
Unity UI Toolkit does not support `:last-child`, causing:
`Unknown pseudo class "last-child" in StyleSheet DesktopShell`

**Non-goals:**  
- No UI redesign
- No UXML changes unless required

**Acceptance criteria:**
- [ ] Remove any `:last-child` usage from `Assets/UI/Desktop/DesktopShell.uss`
- [ ] Maintain spacing between taskbar app buttons
- [ ] Unity Console shows **0 USS warnings** about `last-child`
- [ ] (Optional) If desired, implement “no trailing gap” by setting the last button marginRight to 0 in code

**Files allowed to edit:**
- `Assets/UI/Desktop/DesktopShell.uss`
- `Assets/Scripts/UI/**` (only if implementing optional code spacing fix)

**Test plan:**
1. Open Unity project
2. Confirm Console shows no USS warnings about `last-child`
3. Press Play in Bootstrap and verify taskbar app buttons are spaced correctly
