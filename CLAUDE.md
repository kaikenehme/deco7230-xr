# DECO7230 — Digital Prototyping and Extended Reality

Coursework repo for **XR Renovation Previewer**. Semester 2, 2026 · UQ St Lucia · Kaike Nehme.
Repo: `github.com/kaikenehme/deco7230-xr` (private).

## Read these before proposing anything

| File | What it is |
|---|---|
| `concept/2026-08-07-xr-renovation-concept-design.md` | **Source of truth for the design.** v1.1 |
| `concept/2026-08-07-lowfi-prototype-test-plan.md` | Week 2 studio session, method, and §9 findings |
| `SCHEDULE.md` | Week-by-week plan and every deadline |
| `*.docx` in root | Official task sheets and rubrics, as issued |

Course note and schedule also live in the Obsidian vault at
`/Users/kaikenehme/Desktop/ClaudeCode/The Valt/Semester 4/DECO7230 - Digital Prototyping and Extended Reality/`.

## Hard constraints — do not design around these, design *within* them

- **Two hurdles.** (1) At least a Pass on 1 of the 2 Interactive Prototypes. (2) Physically present at **both** the Week 5 and Week 12 identity-verified Studio sessions. Missing one = zero for that prototype **and** hurdle failure.
- **No extension exists for either prototype.** No supplementary assessment in this course.
- **Studio session is FRIDAY.**
- **IP2b must run on Meta Quest hardware.** "Testing in unity or simulator will result in your grade being capped."
- **Postgrad-only:** IP2b's testing plan needs a one-paragraph research summary with **≥5 academic references**. It is its own rubric criterion — missing or <5 scores 0. DECO2300 students don't have this, so nobody around Kaike will mention it.
- Every assessment item submits to **Blackboard AND this repo**.
- IP1 and IP2b are **secure assessments** — all AI use must be declared (UQ Library acknowledgement table). IP1 also needs a Statement of Originality committed here.

### Dates

| Item | Due | Weight |
|---|---|---|
| Design Concept | Fri 14 Aug, 12:00 | part of 30% |
| **Interactive Prototype 1** | **Fri 28 Aug**, Studio | **35%, HURDLE** |
| Evaluation 1 | Wed 2 Sep, 12:00 | part of 30% |
| Interactive Prototype 2a | Fri 25 Sep, Studio | ungraded |
| Evaluation 2 | Wed 7 Oct, 12:00 | part of 30% |
| **Interactive Prototype 2b** | **Fri 23 Oct**, Studio | **35%, HURDLE** |
| Evaluation 3 + Reflection | Wed 28 Oct, 12:00 | part of 30% |

Submit every 12:00 item by **10:00**. ⚠️ The three Evaluation dates come only from a hand-written vault note — no task sheet carries them and no ECP is held locally. Verify against Blackboard before treating as fixed.

## Locked design decisions — don't relitigate these

| Decision | Choice | Why |
|---|---|---|
| Rubric category | **Creation** | Interior design / room planning tools. Rubric's criterion 1 omits "or a game" from its list — anchor to one of the five it names |
| App being redesigned | Phone paint-visualiser (Dulux Visualizer, Houzz, IKEA Kreativ) | Mundane, widely used, fails at scale/adjacency/light |
| Decision supported | **Match against what's staying** | Not "choose from scratch". Gives the hero gesture something to be held against |
| Hero interaction | **Hold it up against the surface** | Proximity previews; release commits. Zero learning curve |
| Sample source | **Pull off the surfaces being kept** | Closes the loop, kills menus, constraint generates the options |
| Modality | VR (IP1, IP2a) → MR (IP2b), with a runtime toggle in IP2b | Maps onto the three prototypes; de-risks MR failure |
| Scope | Full renovation as *concept*, phased as *build* | IP1 is horizontal — brief says explicitly "not a final product with all the functionalities" |
| IP1 input | **Controllers, not hand tracking** | Hand tracking drops out; too fragile for 5 back-to-back tests on a hurdle |

**No menus. Ever.** The brief warns that interactions must not be "limited to simply pushing VR buttons". Every control is diegetic — a lamp you touch, a switch on the wall. This is the concept's main defence on rubric criterion 2.

## Phasing

| | Modality | Functional | Shallow / absent |
|---|---|---|---|
| **IP1** Wk 5 | VR, controllers | Full loop (mark → pull → hold → tune → commit) **on walls only** · diegetic light control (warm / cool / daylight) | Furniture static; layout absent |
| **IP2a** Wk 9 | VR | Weakest thing from IP1 rebuilt · scheme comparison · furniture grab/move/rotate | — |
| **IP2b** Wk 12 | MR passthrough | Same loop on the participant's **real** room · runtime VR↔MR toggle | — |

## Environment

- **Unity 6000.0.80f1** — `/Applications/Unity/Hub/Editor/6000.0.80f1`. Android Build Support, SDK (platforms 34/35/36), NDK r27c, OpenJDK 17 all installed. `adb` at `PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`.
- **Unity project: `ip2a/RenovationPreviewer`** (moved from `ip1/` on 3 Sep 2026; IP1 as tested = tag `ip1-submitted`). The scene is **generated** by `Assets/Editor/SceneBuilder.cs` — edit the builder, never `Room.unity`. Headless: `$UNITY -batchmode -quit -projectPath <proj> -executeMethod SceneBuilder.Build`; tests `-runTests -testPlatform EditMode|PlayMode` (no `-quit`).
- **URP is mandatory** — built-in pipeline won't hold framerate on Quest.
- **XR Interaction Toolkit 3.x**, Android build target.
- Unity licence needs a one-time Hub sign-in (GUI, user-side).
- **Quest is borrowed from UQ/studio**, not owned. Book ahead of Week 11.
- ⚠️ **Disk was 98% full.** ~12 GB free after the Unity install; a Unity XR project's `Library/` runs 3–8 GB. Check before large operations.
- ⚠️ This folder was renamed from `Digital_protpotype ` (trailing space) because Gradle breaks on it. **Never reintroduce spaces into build paths.**
- There is an unrelated zero-commit git repo rooted at `$HOME`. Don't commit coursework into it.

## Generating the PDF

`weasyprint` is installed but **broken** (missing `libpango`). Use Chrome headless:

```bash
cd concept
# inline the images as data URIs first (Chrome + file:// is unreliable otherwise)
python3 - "$SCRATCH" <<'PY'
import base64, sys, pathlib
scratch = pathlib.Path(sys.argv[1])
html = pathlib.Path("lowfi-plan-print.html").read_text()
for token, fn in (("IMG1_B64","prototype-01-front.jpeg"), ("IMG2_B64","prototype-02-eyelevel.jpeg")):
    b64 = base64.b64encode(pathlib.Path(fn).read_bytes()).decode()
    html = html.replace(token, f"data:image/jpeg;base64,{b64}")
(scratch / "lowfi-inlined.html").write_text(html)
PY
"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" \
  --headless=new --disable-gpu --no-pdf-header-footer --virtual-time-budget=4000 \
  --print-to-pdf="XR-Renovation-Previewer-Lowfi-Plan.pdf" "file://$SCRATCH/lowfi-inlined.html"
```

Print CSS gotchas already solved in `lowfi-plan-print.html`: `page-break-after:avoid` on headings stops orphans; flex rows overflow their borders in print, so legends use tables; photos need a fixed `height` + `object-fit:cover` or portrait shots eat a whole page.

## How to work here

- **Don't overclaim test results.** Record exactly what was and wasn't measured. The Week 2 session produced peer feedback, not protocol data, and the docs say so. Markers read a lot of these.
- **Interrogate feature requests before accepting them.** All three Week 2 requests arrived as "add a button"; two had better answers underneath. That interrogation is what Criterion 3 rewards.
- Prefer concise, executable docs. Tables over prose. Lead with paste-ready content.
- Scope discipline is the top risk — zero Unity experience against a 35% hurdle with no extension.
- Caveman mode is usually active in these sessions (terse, fragments OK). Code, commits and security warnings still get written normally.

## Current state — 22 Sep 2026, Week 9 (IP2a test Fri 25 Sep, 3 days out)

**Done:** IP1 tested Fri 28 Aug (n=5, simulator; hold-up loop entered 0/5) · Evaluation 1 submitted Wed 2 Sep · IP2a build T0–T14 green (3 Sep) · 11 Sep simulator tooling (`EditorGazeAim`, menu closes on right B / `Backspace` / left Y) · **22 Sep feel pass round 1** (`fba9f0c`, `f0a0c38`): both hands visible (left rides the head as a follower; offset 19° below eye line, the old 41° was off-screen) · surface hover = amber rectangle on the hit face (`SurfaceFrame`), menu's current target keeps frame/outline, no whole-object glow · menu 26 cm up / 16 cm ahead of the left hand · gaze aim defers to the simulator's `T`/`Y`/`Tab` hand choice · **Input System settings asset: keys reach the game without Game view focus** (was the cause of "T/Y/G not working", twice) · Pipeline package 0.7.0. EditMode 149, PlayMode 22 (not re-run 22 Sep). Plan + STATUS: `docs/superpowers/plans/2026-09-03-ip2a-build.md`.

**Simulator keys (editor):** mouse = look, click = trigger, `G` = grip, scroll = twist, `B` = A/X, `N` = B/Y, `T` = buttons to left hand, `Y` = right hand, `F1–F3` presets, `F5–F10` time stops, `Backspace` close menu, `F4` raw simulator. **Avoid `Esc`** (simulator "stop manipulation": target None → nothing responds until `T`/`Y`). Game view `Gizmos` toggle off, or RectTransforms draw as white rectangles.

**Live-editor workflow:** `unity open <proj>`; global flags go **before** `command` (`unity --no-banner --non-interactive --format json command <name> ...`), `--timeout` after `command`. `console_status` for compile state, `run_tests --mode EditMode` (sync works), `eval` (expressions/statements only, no class defs — `run_script --file --entry` for a MonoBehaviour), `capture_game_view --source screen --save_path` (lands under `Assets/`, delete after), `editor_play`/`editor_stop`. Synthetic key events from `eval` do **not** reach play-mode action maps — real presses only. The editor was quit 5× on 22 Sep between looks; each relaunch ~1 min and Play state lost. Do not run a batchmode editor while the GUI editor has the project open.

**Headless visual check exists:** PlayMode `SnapshotTests` renders 14 views to `ip2a/RenovationPreviewer/Builds/snapshots/*.png` (gitignored).

**Open bug, first thing Tue 23 Sep:** furniture grab "still bugged" after keys were confirmed reaching the simulator (`G` logged, outline reportedly appears). Not diagnosed — no probe run yet on `FurnitureInput`/`FurnitureSlot` with the ray on a chair. Start there, with Kaike aiming and a probe of `FurnitureSlot.IsHeld` + the interactor's select state.

**Kaike next (Wed–Fri):** finish the lap (sample pull → twist → hold-up → commit; presets on Wall_W; clock + lamp; menu furniture pages) · voice pass on `ip2a/2026-09-25-ip2a-testing-plan.md` + `data-collection-sheet.md` · AI-log consolidation + T15 sweep (AI) · Thu freeze, print sheets · **Fri: first 10 min with staff on the Quest (`ip2a/quest-sideload.md`), then test (simulator fallback is fine for IP2a), push `testing-data/ip2a/` before leaving.** Pilot #1 (Sat 19) and freeze (Mon 21) were missed. **APK: build only when Kaike says go, right before a headset session.**

**Open:** A2 still untested, now reachable (Task 1 starts sample-in-hand) · A1′ (does first-contact feedback get people to pull?) · Q6 tuning-twist vs furniture-rotate: rotate is now on the thumbstick, so the wrist means one thing · Q7 becomes A4 (six time stops).
