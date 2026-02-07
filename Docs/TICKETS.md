## Ticket 0011 - Simulated filesystem v1 (Virtual FS model + File Manager reads it)

**Goal:**  
Create a lightweight **virtual filesystem (VFS)** model (folders/files) and update the File Manager placeholder window to display the VFS tree/path contents.

**Non-goals:**  
- No real OS file access (must remain simulated)
- No persistence yet (save/load integration later)
- No drag/drop, rename, delete yet
- No terminal commands yet (later ticket)

**Acceptance criteria:**
- [ ] Add VFS core models in Infrastructure:
  - [ ] `VfsNode` base (id, name, parent id)
  - [ ] `VfsFolder` and `VfsFile` (file has size + optional text content)
  - [ ] `VirtualFileSystem` with:
    - [ ] create folder/file APIs
    - [ ] `GetChildren(folderId)` and `TryGetNode(id)`
    - [ ] `ResolvePath("/home/user")` style lookup (basic; supports `/` root + folder names)
- [ ] Create a default VFS factory (pure code or ScriptableObject) that builds:
  - `/home/user/` with a couple folders (e.g. `docs`, `downloads`)
  - at least 3 files (e.g. `readme.txt`, `todo.txt`, `notes.txt`)
- [ ] Integrate into bootstrap:
  - [ ] `GameBootstrapper` owns a single `VirtualFileSystem` instance
  - [ ] Expose it to UI layer via constructor injection or via a small `GameContext` object (no globals)
- [ ] Update File Manager window placeholder:
  - [ ] When File Manager app opens, it shows:
    - current path (start at `/home/user`)
    - list of child entries (folders + files)
  - [ ] Clicking a folder navigates into it and refreshes list
  - [ ] Back button goes to parent (stop at root)
- [ ] UI Toolkit for File Manager view:
  - [ ] Create `Assets/UI/Apps/FileManager/FileManagerView.uxml` + `.uss`
  - [ ] Create controller: `Assets/Scripts/UI/Apps/FileManager/FileManagerController.cs`
- [ ] EditMode tests:
  - [ ] Path resolution works for `/` and `/home/user`
  - [ ] Creating folders/files results in expected children
- [ ] Unity compiles with 0 errors, tests pass, and Play Mode shows File Manager browsing VFS

**Files allowed to edit:**  
- `Assets/Scripts/Infrastructure/**`
- `Assets/Scripts/Game/**`
- `Assets/Scripts/UI/**`
- `Assets/UI/**`
- `Assets/Tests/EditMode/**`
- `Docs/ADR.md`

**Implementation notes:**
- Keep it simple: ids can be GUID strings or ints
- Keep allocations low: avoid rebuilding entire lists per frame; update on navigation only
- Still a placeholder app; focus on correctness and clean separation

**Test plan:**
1. Run EditMode tests (all green)
2. Play Bootstrap
3. Launch File Manager from taskbar
4. Confirm it starts at `/home/user`
5. Click into `docs`/`downloads` and back out
6. Confirm Console has 0 errors
