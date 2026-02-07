## Ticket 0008A - Taskbar clock shows seconds (HH:mm:ss)

**Goal:**  
Change the taskbar clock display format from `HH:mm` to `HH:mm:ss`.

**Non-goals:**  
- No changes to TimeService tick rate (still once per second)
- No UI layout changes

**Acceptance criteria:**
- [ ] Taskbar clock displays `HH:mm:ss`
- [ ] EditMode tests updated accordingly
- [ ] Unity compiles with 0 errors and tests pass

**Files allowed to edit:**
- `Assets/Scripts/UI/Desktop/**`
- `Assets/Tests/EditMode/**`

**Test plan:**
1. Run EditMode tests (all green)
2. Play Bootstrap scene and confirm seconds tick visibly
