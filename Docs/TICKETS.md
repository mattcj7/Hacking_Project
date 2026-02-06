## Ticket 0003 - Bootstrap scene + GameStateMachine

**Goal:**  
Create a deterministic startup flow with a `Bootstrap` scene and a simple `GameStateMachine` that transitions **Boot → MainMenu → Gameplay** with clear logging.

**Non-goals:**  
- No UI framework decisions yet (no UI Toolkit setup here)
- No hacking simulation/tools/apps
- No mission system
- No save/load

**Acceptance criteria:**
- [ ] Create folder `Assets/Scenes/` if missing
- [ ] Create scene: `Assets/Scenes/Bootstrap.unity`
- [ ] `Bootstrap` scene contains a single root GameObject (e.g. `Game`) with `GameBootstrapper` component
- [ ] Implement `GameStateMachine` (pure C# class) and 3 states:
  - `BootState`
  - `MainMenuState`
  - `GameplayState`
- [ ] On Play in `Bootstrap`:
  - [ ] Logs show transition `Boot → MainMenu` automatically
  - [ ] Provide a simple way to trigger `MainMenu → Gameplay` (keypress `G` while in MainMenu), with logging
- [ ] No singletons unless justified in code comments (prefer composition via `GameBootstrapper`)
- [ ] Unity compiles with **zero errors**
- [ ] Add a brief ADR entry describing the state-machine approach

**Files allowed to edit:**  
- `Assets/Scenes/**`
- `Assets/Scripts/Game/**`
- `Docs/ADR.md`

**Implementation notes:**  
- Put scripts under `Assets/Scripts/Game/`
- Use namespaces like `HackingProject.Game` (stay consistent with Ticket 0002 assemblies)
- Logging via `Debug.Log` is fine

**Test plan:**  
1. Open `Assets/Scenes/Bootstrap.unity`  
2. Press Play  
3. Confirm Console logs show `Boot → MainMenu`  
4. Press `G` to transition to `Gameplay` and confirm logs  
5. Confirm Console has **0 errors**
