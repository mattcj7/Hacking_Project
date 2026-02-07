## Ticket 0009 - Save/Load v1 (Unity JsonUtility + atomic write + backup + SHA-256 integrity)

**Goal:**  
Implement a minimal Save/Load foundation that is:
- **Performant** (save only on demand, no per-frame allocations)
- **Robust** (atomic write + `.bak` fallback)
- **Tamper/corruption-evident** (SHA-256 hash integrity check)
- **Unity-safe** (use `JsonUtility` for broad platform compatibility)

**Non-goals:**  
- No cloud saves
- No encryption / anti-cheat (integrity check only)
- No save-slot UI (optional debug keybind is fine)
- No full game data yet (just a placeholder field)

**Acceptance criteria:**
- [ ] Create data models:
  - [ ] `SaveGameData` (payload) includes:
    - [ ] `int Version` (start at 1)
    - [ ] `string LastSavedUtcIso` (ISO-8601 UTC string)
    - [ ] At least one placeholder field proving persistence (e.g., `int Credits` OR `string PlayerProfileName`)
  - [ ] `SaveEnvelope` (wrapper stored on disk) includes:
    - [ ] `int Version` (envelope version, also start at 1)
    - [ ] `string PayloadJson` (serialized `SaveGameData` as JSON string)
    - [ ] `string PayloadSha256Hex` (SHA-256 of PayloadJson, hex encoded)
- [ ] Implement `SaveService` (plain C# class) under `Assets/Scripts/Infrastructure/Save/`:
  - [ ] Constructor takes a base directory path (string) so tests can redirect writes
  - [ ] Uses fixed file names:
    - [ ] primary: `savegame.json`
    - [ ] backup:  `savegame.bak.json`
    - [ ] temp:    `savegame.tmp.json`
  - [ ] `bool TryLoad(out SaveGameData data)`:
    - [ ] If primary missing -> return false
    - [ ] If primary exists but fails (IO/parse/hash mismatch) -> attempt `.bak` automatically
    - [ ] If both fail -> return false (no throw)
  - [ ] `void Save(SaveGameData data)`:
    - [ ] Updates `LastSavedUtcIso`
    - [ ] Serializes payload with `JsonUtility.ToJson`
    - [ ] Computes SHA-256 hash of `PayloadJson` (UTF8)
    - [ ] Writes **atomically**:
      - write temp
      - move existing primary to backup (or overwrite backup)
      - replace primary with temp (prefer OS-level replace if available)
- [ ] Integrate into game bootstrap:
  - [ ] `GameBootstrapper` owns `SaveService` instance
  - [ ] On startup: TryLoad -> if missing/fails, create new default SaveGameData
  - [ ] Add a **save trigger** using the **new Input System**:
    - [ ] Press `F5` to Save
    - [ ] Press `F9` to Delete primary+backup (debug only) OR create “New Save” (optional)
- [ ] Publish events via EventBus:
  - [ ] `SaveLoadedEvent` (includes whether loaded from primary or backup)
  - [ ] `SaveCompletedEvent`
  - [ ] `SaveFailedEvent` (optional; includes reason enum like IO/HashMismatch/ParseError)
- [ ] Add EditMode tests (must not touch real persistentDataPath):
  - [ ] Missing file -> TryLoad returns false
  - [ ] Round-trip -> Save then TryLoad returns equivalent data
  - [ ] Corruption -> write invalid primary (or wrong hash) but valid backup; TryLoad loads from backup
  - [ ] Hash mismatch -> detected and rejected
- [ ] Unity compiles with **0 errors**
- [ ] EditMode tests pass (all green)
- [ ] Play Mode:
  - [ ] Logs indicate loaded/created save on startup
  - [ ] Pressing F5 writes save and triggers event/log

**Files allowed to edit:**  
- `Assets/Scripts/Infrastructure/**`
- `Assets/Scripts/Game/**`
- `Assets/Tests/EditMode/**`
- `Docs/ADR.md`

**Implementation notes:**
- Use `JsonUtility` (Unity-friendly) for both envelope and payload.
- Integrity check is for corruption/tamper detection (not “security against a determined attacker”).
- Avoid per-frame allocations: check inputs once per frame; serialize only on Save.
- Keep API and code minimal; future fields can be added via versioning.

**Test plan:**
1. Run EditMode tests (all green)
2. Play Bootstrap
3. Press F5 -> confirm save log and file created in persistentDataPath
4. Stop Play, start Play again -> confirm it loads existing save
5. Confirm Console has 0 errors
