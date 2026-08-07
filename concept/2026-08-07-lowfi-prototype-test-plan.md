# Low-Fidelity Prototype + Test Plan

**DECO7230 · Week 2 Studio · 7 August 2026 · Kaike Nehme**
Concept: XR Renovation Previewer — see [`2026-08-07-xr-renovation-concept-design.md`](2026-08-07-xr-renovation-concept-design.md)

---

## 1. The part of the concept being tested

**The sample loop** — the single interaction the whole concept rests on:

> pull a sample off something you're **keeping** → hold it up against something you're **changing** → rotate to vary it → release to commit

**Deliberately not tested today:** scheme comparison, marking keep/change, MR passthrough, the colour-harmony maths, anything about the headset.

One part, tested properly, rather than a demo of everything.

## 2. What I'm testing and what I need to find out

The concept assumes this loop is self-evident. If it isn't, the design is wrong and it's far cheaper to learn that from cardboard than from Unity in Week 4.

| # | Question | If the answer is "no" |
|---|---|---|
| Q1 | Do people work out that samples come from the things being **kept**? | Kept surfaces need a visual affordance |
| Q2 | Is holding a sample **near** a surface understood as "preview"? | Proximity-preview fails; needs an explicit trigger |
| Q3 | Do people **rotate** the sample to ask for a variation? | Twist-to-tune is arbitrary, not natural — cut or redesign it |
| Q4 | Does the same gesture read correctly for **furniture** as for wall colour? | The loop doesn't generalise; full-renovation scope is at risk |
| Q5 | Does having **fewer, constrained** options feel helpful or restrictive? | The core premise (constraint dissolves choice paralysis) is wrong |

**Assumptions under challenge:**

- **A1 — Discoverability.** The loop can be performed without instruction.
- **A2 — Constraint as help.** Generating options from what's staying reduces choice paralysis rather than causing frustration.

Q5 / A2 is the one that matters most. It can genuinely fail, and a failure is a finding worth reporting.

## 3. Methodology

**Wizard-of-Oz play-acting on a physical low-fidelity prototype.**

A second person acts as the system: when the participant performs a gesture, the wizard physically swaps the paper to produce what the app *would* have done. The participant is never told how it works.

Three roles:

| Role | Does |
|---|---|
| **Participant** | Uses the prototype. Thinks aloud throughout |
| **Wizard** | Silently swaps paper according to the response rules below. Never explains |
| **Observer** | Times, tallies, writes verbatim quotes. Says nothing |

**Why this method:** it tests the *interaction model* before any code exists — which is what the brief means by using low-fi prototyping to "play-act/enact the interactions". Discoverability cannot be measured if you explain the interface first, so nothing is explained.

**Wizard response rules — follow exactly, every run:**

| Participant does | Wizard does |
|---|---|
| Holds a swatch **near** a surface | Swap that surface's panel to the swatch colour. Hold it there |
| Moves the swatch **away** | Put the original panel back |
| **Rotates** the swatch in hand | Hand over a different swatch from the same harmony set |
| **Releases / puts down** the swatch on a surface | Leave the new panel in place — committed |
| **Taps or presses** a surface | Do nothing. Observer records it |
| Touches a **kept** item (floor, sofa) | Hand them a swatch matching that item's colour |
| Touches a **changeable** item to take a sample | Do nothing. Observer records it |
| **Asks how it works** | "What would you expect?" Nothing more |

## 4. What I paper prototyped

A living room built from cardboard and paper:

- **Room shell** — three cardboard walls plus a floor, open front so the participant can reach in
- **Interchangeable wall panels** — paper sheets in several colours, sized to drop over each wall
- **Furniture** — card sofa, coffee table, rug, lamp, each with swappable coloured paper covers
- **Kept items** — the timber floor and the sofa, physically marked so they read as fixed
- **Sample deck** — paper swatch cards, held in the hand and raised against surfaces

The scale is tabletop rather than 1:1, so this cannot test the scale or lighting claims. It tests the **interaction model only** — which is the point.

> **Before starting, check:** the floor and sofa must be visibly distinguishable as "staying". If nothing marks them, Q1 is untestable — participants can't discover a rule the prototype doesn't express. A strip of tape or a written label is enough.

## 5. Data collected and how

| # | Measure | Type | Captured by |
|---|---|---|---|
| D1 | First three actions attempted, in order | sequence | Observer writes them down |
| D2 | Time to first correct pull-from-kept-item | seconds | Phone stopwatch |
| D3 | Prompts needed before the loop was performed | tally | Observer |
| D4 | Tried to tap/press instead of hold-up? | yes/no | Observer |
| D5 | Rotated the swatch unprompted? | yes/no | Observer |
| D6 | Tried to take a sample off a **changeable** surface? | yes/no | Observer |
| D7 | Confusion quotes, verbatim | qualitative | Observer, in the participant's words |
| D8 | "Did fewer options feel helpful or restrictive?" 1–5 + why | mixed | Asked after the task |
| D9 | Photo of each configuration produced | visual | Phone |

D7 and D8 carry the most weight. Numbers from three or four participants prove nothing statistically — the value is in *why* people got stuck.

## 6. Step-by-step testing process

**One run ≈ 6:30. Aim for 3–4 runs in the session.**

| Time | Step | Script / action |
|---|---|---|
| **0:30** | Setup | Room on table, kept items marked, swatch deck to one side. Reset any previous changes |
| **0:30** | Brief | *"This is a living room. The floor and the sofa are staying. Everything else can change. Think out loud."* **Nothing about how it works.** |
| **1:30** | Free exploration | No task given. Observer records D1, D2, D4, D6 |
| **2:00** | Task 1 — colour | *"Change the room so it works with the floor and sofa you're keeping."* Wizard responds per rules. Observer records D3, D5, D7 |
| **1:00** | Task 2 — furniture | *"Now change a piece of furniture the same way."* Tests Q4 |
| **1:00** | Post-task | D8, then: *"What did you think would happen when you held the card up?"* and *"Where did you think the colours came from?"* |

**Between runs:** reset the room to its starting state and photograph it, so every participant begins identically.

---

## 7. Results — fill in during the session

| | P1 | P2 | P3 | P4 |
|---|---|---|---|---|
| D1 First three actions | | | | |
| D2 Time to first correct pull | | | | |
| D3 Prompts needed | | | | |
| D4 Tapped instead of held up? | | | | |
| D5 Rotated unprompted? | | | | |
| D6 Sampled a changeable surface? | | | | |
| D8 Helpful / restrictive (1–5) | | | | |

**D7 — confusion quotes, verbatim:**

-
-
-

## 8. Findings against each question

| | Answer | Evidence |
|---|---|---|
| Q1 Samples come from kept things | | |
| Q2 Proximity reads as preview | | |
| Q3 Rotation reads as "vary it" | | |
| Q4 Works for furniture too | | |
| Q5 Constraint helps vs restricts | | |

## 9. What changed as a result

*This section is the deliverable.* Criterion 3 of the Design Concept Report rewards showing how the initial idea was **refined**, not how right it was to begin with. A gesture that failed and was redesigned scores better than one asserted to be perfect.

**Changed:**

-

**Kept, and why the testing supports it:**

-

**Still unresolved, to test in IP1:**

-
