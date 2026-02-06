## Ticket 0006D - Self-heal WindowTemplate at runtime + harden Setup tool (selection-safe, no duplicates)

**Goal:**
1) Ensure windows ALWAYS spawn without requiring manual assignment of DesktopShellController.WindowTemplate.
2) Reduce Unity Editor Inspector exceptions by making the setup tool selection-safe and non-destructive.
3) Prevent duplicate DesktopShellController/UIDocument objects from being created by setup runs.

**Background / Issue:**
- `[DesktopShellController] Window template is not assigned.` still appears and manual assignment is still required.
- Unity Editor exceptions persist:
  - `SerializedObjectNotCreatableException: Object at index 0 is null`
  - `MissingReferenceException: GameObjectInspector m_Targets doesn't exist anymore`
These are typically caused by editor scripts changing scene objects while they are selected/inspected.

**Non-goals:**
- No new window features
- No UI redesign
- No Player Settings changes
- No package upgrades/downgrades

**Acceptance criteria:**
- [ ] DesktopShellController no longer relies on manual WindowTemplate assignment:
  - [ ] If WindowTemplate is null in Editor, auto-load via AssetDatabase from `Assets/UI/Windows/Window.uxml`
  - [ ] If still null, log a single ERROR that includes the GameObject name and scene path
- [ ] The warning `[DesktopShellController] Window template is not assigned.` is removed OR only emitted if auto-load fails.
- [ ] Setup tool (`Tools > Hacking Project > Setup Desktop UI`) is idempotent AND does not create duplicates:
  - [ ] Finds existing UIDocument/DesktopShellController and repairs it instead of creating a new one
  - [ ] Does not use DestroyImmediate on scene objects/components
  - [ ] Is selection-safe: clears Selection before modifications and restores afterwards OR performs modifications inside EditorApplication.delayCall
- [ ] After running setup tool and pressing Play:
  - [ ] Two demo windows spawn without manual assignment
  - [ ] No template warning appears (unless auto-load fails)
- [ ] Unity compiles with 0 errors

**Files allowed to edit:**
- `Assets/Scripts/UI/Desktop/**`
- `Assets/Editor/**`
- `Assets/Scenes/**`

**Test plan:**
1. Remove any manual WindowTemplate assignment on DesktopShellController (set it to None)
2. Run Tools > Hacking Project > Setup Desktop UI
3. Press Play in Bootstrap
4. Confirm windows spawn and no template warning appears
5. Click around Hierarchy/Inspector for ~15 seconds; confirm no repeating Inspector exception spam
