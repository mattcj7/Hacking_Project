## Ticket 0010A - Auto-assign AppCatalog (Setup tool + Editor self-heal)

**Goal:**  
Ensure the taskbar app buttons always appear by guaranteeing `DesktopShellController.AppCatalog` is assigned automatically.

**Background / Issue:**  
Runtime warning:
`[DesktopShellController] App catalog is not assigned.`
Result: Terminal and File Manager buttons do not appear.

**Non-goals:**  
- No new app functionality
- No UI redesign
- No package changes
- No Player Settings changes

**Acceptance criteria:**
- [ ] `Tools > Hacking Project > Setup Desktop UI` assigns the default AppCatalog to `DesktopShellController` if missing:
  - `Assets/ScriptableObjects/Apps/AppCatalog_Default.asset`
- [ ] Setup tool persists the assignment (Undo + SetDirty + MarkSceneDirty) and logs one confirmation message.
- [ ] `DesktopShellController` includes an Editor-only self-heal:
  - If AppCatalog is null in Editor, attempts to load the default catalog via `AssetDatabase.LoadAssetAtPath<AppCatalogSO>(...)`
  - If still null, logs a single ERROR with expected path and scene/object name
- [ ] Play Mode no longer logs "App catalog is not assigned" under normal conditions
- [ ] Taskbar shows Terminal + File Manager buttons sourced from the catalog
- [ ] Unity compiles with 0 errors and tests remain green

**Files allowed to edit:**
- `Assets/Editor/**`
- `Assets/Scripts/UI/Desktop/**`
- `Assets/Scripts/UI/Apps/**` (only if needed)
- `Assets/Scenes/**` (only if needed)

**Test plan:**
1. Set DesktopShellController.AppCatalog = None (unassigned)
2. Run: Tools > Hacking Project > Setup Desktop UI
3. Press Play in Bootstrap
4. Confirm no "App catalog is not assigned" warning and taskbar buttons appear
5. Confirm Console has 0 errors
