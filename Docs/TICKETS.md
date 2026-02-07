## Ticket 0013 - File Manager UI matches Terminal theme (shared theme USS)

**Goal:**  
Make the File Manager window match the same dark/legible color scheme as the Terminal window, using a shared theme stylesheet so both apps stay consistent.

**Non-goals:**  
- No new File Manager features (navigation stays as-is)
- No changes to VFS logic
- No layout redesign beyond color/typography/padding adjustments

**Acceptance criteria:**
- [ ] Add a shared theme USS file (single source of truth):
  - `Assets/UI/Theme/HackingOSTheme.uss`
- [ ] Theme defines common look:
  - [ ] Dark panel background
  - [ ] High-contrast text color
  - [ ] Subtle borders/dividers consistent with Terminal
  - [ ] Reasonable font sizing and padding for readability
- [ ] File Manager uses the shared theme:
  - [ ] `FileManagerView.uxml` includes `HackingOSTheme.uss` (preferred) OR controller adds it at runtime
  - [ ] Remove/adjust conflicting styles so File Manager matches Terminal
- [ ] Any input/list elements in File Manager are readable (no white background with light text)
- [ ] No USS warnings (avoid unsupported props like `gap` and unsupported pseudo-classes like `:last-child`)
- [ ] Unity compiles with 0 errors/warnings and tests remain green

**Files allowed to edit:**
- `Assets/UI/Theme/**`
- `Assets/UI/Apps/FileManager/**`
- `Assets/Scripts/UI/Apps/FileManager/**` (only if needed to attach theme at runtime)

**Test plan:**
1. Play Bootstrap
2. Open Terminal and note its scheme
3. Open File Manager and confirm it matches Terminal scheme (background + text + borders)
4. Confirm Console has 0 errors/warnings
