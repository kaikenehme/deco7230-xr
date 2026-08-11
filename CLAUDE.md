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

## Current state — 7 Aug 2026, Week 2

**Done:** repo scaffolded · Unity + Android chain installed · concept spec v1.1 · low-fi prototype built (cardboard living room, photos in `concept/`) · test plan + 5-page PDF · Week 2 peer feedback folded in.

**Next:** Design Concept Report **drafted** — `concept/Concept-Report-Kaike-Nehme.pdf`, exactly 2 pages, source in `concept-report-print.html`. Still needs: Kaike's own voice pass, student number if required, and the **AI acknowledgement coversheet** (open assessment — AI is permitted but must be declared). Due **Fri 14 Aug 12:00**, Blackboard + this repo.

**Not started:** any Unity code. Kaike has **zero Unity experience** — budget ramp time accordingly.

**Biggest open question:** assumption **A2** — that constraining options to what matches the kept surfaces *helps* rather than frustrates. Nothing has tested it. The whole concept rests on it. IP1's testing minutes should go here.

**Also open:** Q1–Q5 (discoverability of the loop) carried from Week 2 · Q6 — tuning rotation vs layout rotation now share a wrist gesture and may collide · Q7 — whether three lighting states suffice.
