# DECO7230 — Semester Schedule

Digital Prototyping and Extended Reality · Semester 2, 2026 · UQ St Lucia

**Studio session:** Friday · **Unity experience:** none as of 4 Aug · **Quest access:** borrow from UQ/studio

> **Buffer rule:** every 12:00 PM deadline gets submitted by 10:00 AM.

---

## Assessment at a glance

| # | Item | Weight | Due | Hurdle | AI |
|---|---|---|---|---|---|
| 1 | Design Concept Report — 4 staged docs | **30%** | 14 Aug · 2 Sep · 7 Oct · 28 Oct — all **12:00 PM** | No | Permitted (open assessment) |
| 2 | Interactive Prototype 1 — Unity horizontal | **35%** | **Fri 28 Aug**, Studio | **YES** | Secure — must reference all AI used |
| 3 | Interactive Prototype 2b — Unity + Quest | **35%** | **Fri 23 Oct**, Studio | **YES** | Secure — must reference all AI used |

All items individual. No exam. No team component.

### Two hurdle rules — both must be met

1. At least a **Pass on 1 of the 2 Interactive Prototypes**
2. **Present** at both the Week 5 and Week 12 identity-verified Studio sessions

Missing a session without prior approval = **zero for that prototype AND hurdle failure**. There is **no extension** for either prototype, and **no supplementary assessment** exists in this course.

### The 30% is four separate documents

| Sub | Doc | Due | Limit | Content |
|---|---|---|---|---|
| 1 | Design Concept | Fri 14 Aug 12:00 | **2 pages max** [1] | Mundane app redesigned in XR · 3+ user tasks/goals · ideation evidence · XR concept + environment + interactions/affordances · **sketches mandatory** · initial testing plan. 4 criteria @ 0/1/2 |
| 2 | Evaluation 1 (post-IP1) | Wed 2 Sep 12:00 | **3 pages** + appendices | Objective & validation metrics → Results → Analysis/Insights → Evaluation of Aims → Concept Iteration → Reflection. 6 criteria @ 0/1/2 |
| 3 | Evaluation 2 (post-IP2a) | Wed 7 Oct 12:00 | **3 pages** + appendices | Same 6-section template |
| 4 | Evaluation 3 + Reflection | Wed 28 Oct 12:00 | **4 pages** (3 results + 1 reflection) | Same template **plus** 4 reflection areas: Prototype Session Review (pick IP2a *or* IP2b) · Methodological Reflection · Concept Evaluation (validated/invalidated/partial) · Improvements & Extensions incl. a *different* XR modality. +3 extra criteria |

[1] The brief contradicts itself — body says "one-page summary report", Submission Information says "2-page maximum". Write to 2, keep it tight.

Every one submits to **Blackboard AND GitHub**.

### The three prototypes

**IP1 — Fri 28 Aug (35%, hurdle).** *Horizontal* prototype: must "appear as complete as necessary" for the testing aims, **not** be functionally deep. Functional Unity 3D build including programming. Testing plan complete **before** class (template on p.3 of the brief). Tested in Studio with tutor + peers, ~5 min per test, aim 5+ participants. **Results posted to GitHub before you leave the room.** Statement of Originality + AI reference table committed to Git.

**IP2a — Fri 25 Sep (Week 9). NOT formally graded.** Tested in class anyway; testing plan + results still submitted and become the raw material for Evaluation 2. Treat it as a free dress rehearsal for the Quest pipeline.

**IP2b — Fri 23 Oct (35%, hurdle).** Must run **on the Meta Quest headset** — *"Testing in unity or simulator will result in your grade being capped."* Interactions must be **significantly different** from IP2a; cannot reuse with minor changes. Must evaluate against the **five design principles** from lectures — address all five, explaining any that don't apply. May use established VR questionnaires (Week 6 lecture slides).

> **Postgrad-only extra — the DECO2300 cohort does not have this.**
> The IP2b testing plan needs a **one-paragraph research summary with a minimum of 5 academic references**, strongly linked to your prototype and testing. It is **its own rubric criterion**: missing, or fewer than 5 references, scores **0**. Submitted via Blackboard at the end of the session.

### Late / extension rules

| Item | Extension | Late penalty |
|---|---|---|
| Concept + Evaluations | up to 7 days (24-hr multiples) | 1 grade per 24 hrs, then 0 after 7 days |
| Both Prototypes | **none available** | first hour free, then 10%/hr to 8 hrs (max 70%), then **zero** |

---

## Week-by-week

### Week 2 · 4–9 Aug — Unblock everything

Three blockers die this week. None are course content; all of them stop work if left.

- [ ] Install Unity Hub + Unity 6 LTS **with Android Build Support** (SDK + NDK + OpenJDK — needed for Quest in W7, grab it now)
- [ ] Create GitHub repo `deco7230-xr` (private) + Unity `.gitignore`; push briefs and skeleton `/concept /ip1 /ip2a /ip2b /evaluations /testing-data`
- [ ] Unity ramp block 1 (~6–8 hrs): scene, GameObject, MonoBehaviour, `Update()`, move an object by script. No XR yet
- [ ] Ideate 3 candidate apps to redesign — Productivity / Creation / Editing / Social Connection / Meeting & Scheduling / game, **must support active interaction** (Lecture 1: no passive VR experiences)
- [ ] **Photograph the Week 2 Studio low-fi activity** (paper/cardboard/playdoh) — accepted ideation evidence for Submission 1, impossible to recreate later
- [ ] Email `DECO2300@eecs.uq.edu.au` re: Quest borrowing/booking mechanism

Note: there is an accidental zero-commit git repo rooted at `$HOME` holding staged files from a Semester 2 OOP assignment. It is unrelated — do not commit coursework into it. Root the new repo at this folder.

### Week 3 · 10–16 Aug — Concept due Fri 14 Aug 12:00

Wed 12 Aug = Ekka, campus closed, free writing day.

| Day | Task |
|---|---|
| Mon 10 | **Lock the concept.** Scope it honestly against "I have never used Unity." Write the 3+ user tasks/goals |
| Tue 11 | Sketches / storyboards / design visuals — **mandatory**, explicit rubric criterion |
| Wed 12 | Draft: XR concept + specific immersive environment + interactions & affordances. Interactions must not be basic — "simply pushing VR buttons" is called out as insufficient |
| Thu 13 | Initial testing plan section. Cut to 2 pages. Fill the AI acknowledgement coversheet |
| Fri 14 | **Submit by 10:00** — Blackboard + git commit |

- [ ] Unity ramp block 2 (parallel): install XR Interaction Toolkit, sample scene running in editor

### Week 4 · 17–23 Aug — IP1 build sprint

Heaviest week of the semester. *Horizontal* = breadth over depth. It must look coherent, not be deep — that's the brief's own definition, not a shortcut.

- [ ] Mon 17 – Tue 18: scene blockout + **core interaction #1 working**
- [ ] Wed 19 – Thu 20: interactions #2 and #3 + UI/feedback pass so the whole reads as complete
- [ ] Fri 21: write the IP1 testing plan from the brief's p.3 template — 3-sentence pitch ("This project is [mundane app] but using XR for…"), objective, methodology, prototype description, data collection method, setup, **timed** script e.g. "(30 seconds)"

### Week 5 · 24–28 Aug — IP1 TEST Fri 28 Aug · 35% · HURDLE

| Day | Task |
|---|---|
| Mon 24 | **Feature freeze.** Bug-fixing only from here |
| Tue 25 | Pilot test on one person. Whatever breaks the script, fix |
| Wed 26 | Finalise testing plan; print data-collection sheets; write the participant briefing script |
| Thu 27 | **Statement of Originality + AI reference table committed to Git** (UQ Library AI acknowledgement table format). Build a standalone runnable copy. Prep hardware |
| **Fri 28** | **STUDIO TEST.** 5+ participants, ~5 min each. **Post raw results to GitHub before leaving the room** |

### Week 6 · 31 Aug – 6 Sep — Evaluation 1 due Wed 2 Sep 12:00

- [ ] Mon 31: transcribe + tabulate raw data (raw data lives in appendices)
- [ ] Tue 1 Sep: write to the 6 template sections. Results section stays factual — interpretation belongs in Analysis
- [ ] Wed 2: submit by 10:00 — Blackboard + commit

### Week 7 · 7–13 Sep — Quest pipeline (the real technical risk)

- [ ] **Get a hello-world Unity build running ON a physical Quest this week.** First-time device setup — developer mode, ADB, Android build target, signing — reliably burns a full day. Doing this in Week 11 is how people fail IP2b
- [ ] Start IP2a build, informed by Evaluation 1 insights

### Week 8 · 14–20 Sep — IP2a build

- [ ] Deepen interactions
- [ ] Fri: IP2a testing plan drafted

### Week 9 · 21–25 Sep — IP2a test Fri 25 Sep (ungraded, feeds Evaluation 2)

- [ ] Mon–Wed: freeze + pilot
- [ ] Fri 25: test in Studio, post results to GitHub

**Scope IP2a with IP2b in mind.** IP2b must be *significantly different* — you cannot resubmit with minor changes. Deliberately leave a second interaction direction unexplored here so IP2b has somewhere real to go.

### Mid-semester break · 28 Sep – 4 Oct

- [ ] Draft Evaluation 2 (due 3 days after classes resume — don't leave it)
- [ ] Choose IP2b direction: improve / alternative interaction / different XR modality
- [ ] **Start the 5 academic references** — postgrad-only, own rubric criterion, easiest thing on this page to forget

### Week 10 · 6–11 Oct — Evaluation 2 due Wed 7 Oct 12:00

Mon 5 Oct = King's Birthday; classes resume Tue 6 Oct.

- [ ] Wed 7: submit by 10:00 — Blackboard + commit
- [ ] Lock IP2b design; **book the Quest for the whole Week 11 build window**

### Week 11 · 12–18 Oct — IP2b build, on-device

- [ ] Every build tested **on the headset**, not the simulator — grade-cap risk
- [ ] Testing plan **including the 5-reference research paragraph**
- [ ] Evaluate against the **five design principles** from lectures — address all five, explain any that don't apply

### Week 12 · 19–23 Oct — IP2b TEST Fri 23 Oct · 35% · HURDLE

**Collision week.** DECO7381 capstone final + demo and INFS7203 land in the same window. Everything above must be genuinely finished by Mon 19.

| Day | Task |
|---|---|
| Mon 19 | Feature freeze |
| Tue 20 | Pilot on device |
| Wed 21 | Research summary final; testing plan final |
| Thu 22 | Hardware check, backup build, charge everything |
| **Fri 23** | **STUDIO TEST.** Post results to GitHub end of class. **Submit research summary via Blackboard at end of session** |

### Week 13 · 26 Oct – 1 Nov — Evaluation 3 + Reflection due Wed 28 Oct 12:00

- [ ] Mon 26 – Tue 27: 4 pages (3 results + 1 reflection); cover all four named reflection areas — Extensions explicitly wants a *different* XR technology, e.g. AR instead of VR
- [ ] Wed 28: submit by 10:00 — Blackboard + commit

Semester done. No exam.

---

## Risk register

| Risk | Severity | Mitigation |
|---|---|---|
| Zero Unity, 35% hurdle in 24 days | **Critical** | Ramp starts W2; concept scoped to skill level Mon 10 Aug; horizontal prototype = fake the depth |
| Quest build pipeline fails late | **Critical** | Hello-world on device in W7, not W11; IP2a is the rehearsal |
| Missing a Friday Studio session | **Critical** | Hurdle failure regardless of grade. No extension exists |
| Concept scoped too ambitiously | High | Explicit scope gate Mon 10 Aug |
| W12 triple collision (7230 + 7381 + INFS7203) | High | Hard freeze Mon 19 Oct |
| 5-reference postgrad paragraph forgotten | Medium | Started in mid-sem break, finalised Wed 21 Oct |
| GitHub repo missing at submission time | Medium | Created W2 |

---

## Verify these dates

The three **Evaluation** due dates (2 Sep · 7 Oct · 28 Oct, 12:00 PM), all weightings, the hurdle rules and the late/extension policies come **only from a hand-written course note** (Obsidian vault, last modified 28 Jul) — not from any official document held locally. The task sheets in this folder carry **no dates at all** for the Evaluations, and no ECP/course-profile PDF exists on this machine.

Task sheets are more recent (4 Aug) than the course note, so where they conflict they win. Two known conflicts, resolved in favour of the briefs:

- The course note calls the Week 12 item "Interactive Prototype 2"; the brief calls it **IP2b** and defines a separate ungraded **IP2a** in Week 9.
- The course note labels Submission 2 "paper prototype evaluation"; the brief ties every Evaluation to the in-class testing sessions (Wk 5 / 9 / 12).

**Cross-check against Blackboard or the ECP** before treating the Evaluation dates as fixed:
https://course-profiles.uq.edu.au/course-profiles/DECO7230-60802-7660

---

## Source documents in this folder

| File | Covers |
|---|---|
| `Design Concept (1).docx` | Submission 1 — concept brief + 4-criterion rubric |
| `IP1 - Unity Prototype1.docx` | Interactive Prototype 1 + testing-plan template + rubric |
| `IP2 - Functional Prototype.docx` | IP2a (ungraded) and IP2b + postgrad research requirement + rubric |
| `Design Evaluation 1 and 2.docx` | Submissions 2 and 3 — template + 6-criterion rubric |
| `Design Evaluation 3 and Reflection.docx` | Submission 4 — template + reflection areas + 3 extra criteria |
| `AI_Acknowledgment_Coversheet_Template.docx` | Generic UQ AI acknowledgement form (Tool / Use / Prompt / Section / Date) |
