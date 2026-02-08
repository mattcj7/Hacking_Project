## Ticket 0014 - Persist OS session (open apps + window positions + last paths) via SaveService

**Goal:**  
Persist and restore the player’s “OS session”:
- Which apps are open
- Window positions (x,y)
- Last known working directory for Terminal
- Last visited path for File Manager

**Non-goals:**  
- No encryption/anti-tamper beyond existing integrity
- No multi-slot UI
- No deep app state (only minimal session state)

**Acceptance criteria:**
- [ ] Extend `SaveGameData` to include `OsSessionData`:
  - [ ] `List<OpenWindowData>` where each entry includes:
    - [ ] `string AppId` (or enum/string id used by AppDefinitionSO)
    - [ ] `float X`, `float Y` (window position in pixels)
    - [ ] Optional `float Width`, `float Height` (if available; else omit for now)
    - [ ] `int ZOrder` (optional; restore order if easy)
  - [ ] `string TerminalCwdPath` (start default `/home/user`)
  - [ ] `string FileManagerPath` (start default `/home/user`)
- [ ] WindowManager exposes:
  - [ ] A way to query current open windows + positions
  - [ ] A way to spawn windows at a position (restore)
- [ ] AppLauncher integrates session restore:
  - [ ] On load, restores open apps/windows from save session
  - [ ] Single-instance rule still enforced (don’t duplicate windows)
- [ ] Terminal + FileManager integrate session:
  - [ ] When they change directory/path, update the session state in memory
  - [ ] On open, initialize from session state if present
- [ ] Saving:
  - [ ] F5 save trigger still works and includes session state
  - [ ] On application quit (Editor play mode stop), attempt an autosave (optional)
- [ ] Tests:
  - [ ] EditMode test: building session data from a mock set of windows produces expected SaveGameData
  - [ ] EditMode test: restoring session calls spawn/focus with correct AppIds and positions (can be logic-only with mocks)
- [ ] Unity compiles with 0 errors and tests pass
- [ ] Play Mode:
  - [ ] Open Terminal + File Manager, move windows, navigate paths, press F5
  - [ ] Stop Play, start Play: windows re-open and positions + paths restore

**Files allowed to edit:**  
- `Assets/Scripts/Infrastructure/**`
- `Assets/Scripts/Game/**`
- `Assets/Scripts/UI/**`
- `Assets/Tests/EditMode/**`
- `Docs/ADR.md`

**Implementation notes:**
- Keep session state as simple DTOs (serializable by JsonUtility)
- Avoid globals: store current `SaveGameData` in bootstrap/context and let systems update it
- If Z-order restore is hard, skip for now (positions + open apps are the key)

**Test plan:**
1. Run EditMode tests (all green)
2. Play Bootstrap
3. Open Terminal + File Manager, move them, cd/navigate
4. Press F5 to save
5. Stop Play, Play again
6. Confirm windows + positions + paths restore
7. Confirm Console has 0 errors
