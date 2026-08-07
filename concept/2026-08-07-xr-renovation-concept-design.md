# XR Renovation Previewer — Concept Design (v1.1)

**DECO7230 · Semester 2, 2026 · Kaike Nehme**
**Status:** v1.1 — revised 7 Aug 2026 after Week 2 studio peer feedback. Three additions accepted (light control, furniture layout, VR↔MR toggle); the core interaction loop unchanged. Revision history is the ideation evidence for Criterion 3.

---

## 1. Context

Phone apps for previewing a renovation — Dulux Visualizer, Houzz, IKEA Kreativ, Planner 5D — all fail the same way. They show you a candidate colour or object as a flat rectangle on a six-inch screen, lit by a generic renderer, detached from the room it's meant to go in.

But the three things that actually decide whether a renovation works are all spatial:

- **Scale** — colour reads differently across a whole wall than in a thumbnail
- **Adjacency** — what matters is how it sits next to the floor and furniture you are *keeping*
- **Light** — the variable that ruins real renovations, and the one a phone cannot show you

XR fixes all three at once, which makes this a genuine redesign rather than a port.

**Category:** Creation (interior design / space planning tools).

## 2. Pitch

> This project is a phone paint-visualiser app — Dulux Visualizer, Houzz, Home Depot Project Color — but using XR so you can hold a sample up against the surfaces you're *keeping*, at full size, in your own light. You pull samples directly off the things staying in the room: the timber floor, the sofa, the benchtop. Those samples generate what works with them, so nothing is picked from a catalogue — the room you already have decides what fits.

## 3. The decision being supported

Most renovations are not blank slates. There is a floor you can't afford to replace, a sofa you're keeping, a benchtop that's fine. The real question is **"what works with the things that are staying?"**

This is deliberately not "choose a colour from scratch". Anchoring to fixed elements gives the hero gesture something to be held *against*, and in the MR phase those fixed elements come free from passthrough.

## 4. User tasks (Criterion 1)

| # | Task | Goal |
|---|---|---|
| 1 | Distinguish what's staying from what's changing | Establish the constraint set the design must respect |
| 2 | Evaluate a candidate finish against a fixed element — in place, at scale, in real light | Judge a single choice with the context that decides it |
| 3 | Assemble and compare complete schemes under that same constraint | Commit to a whole-room direction, not an isolated colour |

## 5. XR concept

**Immersive environment:** a domestic interior — one room. Virtual in IP1/IP2a; the participant's own real room via Quest passthrough in IP2b.

**Core loop:**

| | Action | Gesture |
|---|---|---|
| Mark | Tag each surface keep or change | Sweep an open hand across it |
| Pull | Take a sample off a kept surface | Touch and draw away — it peels into your hand |
| Hold | Judge a candidate in place | Raise toward a changeable surface; proximity previews it |
| Tune | Move through what works with it | Twist the wrist — warmer, cooler, deeper |
| Commit | Apply | Open the hand / release |
| Compare | Judge whole schemes | Build up to 3, step back, switch between them |

There are no menus in that loop. This is a direct response to the brief's warning that interactions "should not be basic, e.g., limited to simply pushing VR buttons".

## 6. Interactions and affordances (Criterion 2)

| Affordance | Why the phone version fails |
|---|---|
| **1:1 scale** | Colour perception is scale-dependent; a thumbnail misleads |
| **Real adjacency** | The candidate is seen *touching* the kept floor, not in a separate swatch grid |
| **Real light** | Sunlight, time of day and shadow are the variables that ruin real decisions |
| **Proprioception** | Arm's length is already how people judge samples in shops — an existing gesture, digitised |
| **Head motion** | Walk to where the sofa is and look back; colour shifts with viewing angle |

**Twist-to-tune** is the novel affordance: continuous, embodied movement through a *constrained* option space, replacing a scrollable palette. The constraint is what dissolves choice paralysis — options are generated from the kept surfaces rather than browsed.

## 7. Phasing across the three prototypes

The brief is explicit that this is "not a final product with all the functionalities". IP1 is a horizontal prototype: broad, shallow, appearing as complete as the testing aims require.

| | Modality | Genuinely functional | Present but shallow |
|---|---|---|---|
| **IP1** Wk 5 | VR, Quest, **controllers** | Full loop — mark, pull, hold, tune, commit — **on walls only** · **diegetic light control** (warm / cool / daylight) | Furniture as static props; one alternate material displayed but not swappable |
| **IP2a** Wk 9 | VR | Whatever IP1's data shows is weakest, rebuilt properly · scheme comparison · **furniture grab, move and rotate** | — |
| **IP2b** Wk 12 | **MR passthrough** | Same loop, pulling samples off the participant's **real** floor and furniture · **runtime VR↔MR toggle** | — |

**Three additions came from Week 2 studio peer feedback** — see [`2026-08-07-lowfi-prototype-test-plan.md` §9](2026-08-07-lowfi-prototype-test-plan.md):

- **Light control into IP1.** Peers independently asked for it after seeing two LED tea lights on the cardboard model, which confirms the concept's central claim from outside. Built as a lamp you touch or a wall switch — never a floating button — so the no-menus rule holds. Three states rather than on/off, because "does this green survive a warm bulb at 9pm?" is the question worth asking.
- **Furniture repositioning into IP2a.** Peers asked to rotate furniture; the intent was genuine layout change, not a viewing workaround. Grab/move/rotate is its own system and is kept out of IP1 so it cannot compete with light for build time on a hurdle.
- **VR↔MR as a runtime toggle in IP2b.** Originally a one-way progression across prototypes; peers wanted to switch at will. Comparing a scheme in the real room against an idealised virtual one — honest versus aspirational — also serves as the "significantly different" interaction IP2b demands.

IP1 scene: one room. **Keeping** — timber floor, a sofa. **Changing** — four walls, ceiling, trim, door.

**Controllers, not hand tracking, for IP1.** Hand tracking drops out under poor lighting and unusual hand poses; across five back-to-back peer tests on a hurdle assessment that risk isn't worth taking. Controllers preserve everything that matters — arm's length, scale, adjacency, head motion — and losing finger-level nuance becomes a documented candidate for the IP2b escalation.

## 8. Architecture

| Script | Lives on | Responsibility |
|---|---|---|
| `Surface.cs` | each wall / floor / ceiling / prop | Holds `Keep` or `Change` state. Exposes `Preview(Color)`, `Commit(Color)`, `Revert()` |
| `MarkTool.cs` | controller | Sweep across a surface toggles its state |
| `SamplePuller.cs` | controller | Touching a **Keep** surface spawns a `Sample` |
| `Sample.cs` | held object | Data only: `baseColor`, `currentColor`, `sourceSurface` |
| `HarmonyTuner.cs` | held sample | Controller roll walks a harmony curve off `baseColor`, sets `currentColor` |
| `HoldUpPreviewer.cs` | held sample | Nearest **Change** surface in radius gets `Preview(currentColor)`; release commits |
| `SchemeManager.cs` | scene | Snapshot / restore all surface colours. Up to 3 schemes |

**Boundaries:** `Surface` never knows samples exist; `Sample` never knows what a wall is. `HoldUpPreviewer` is the sole coupling point. Any one script can be rebuilt without touching the others — which matters, because IP2a is explicitly "rebuild whatever tested worst".

**Harmony rule.** Read the pulled sample's hue via `Color.RGBToHSV`, then walk a relationship off it:

```
analogous      hue ± 30°     safe, tonal
complementary  hue + 180°    high contrast
triadic        hue ± 120°    bold
```

Controller roll maps to position along that curve. Roughly thirty lines, backed by real colour-theory literature — the seed for the five academic references IP2b requires of postgraduate students.

**Stack:** Unity 6000.0.80f1 · URP (required for Quest framerate) · XR Interaction Toolkit 3.x · Android build target · `XRGrabInteractable` for the sample · `Renderer.material.color` for surfaces.

## 9. Edge cases

| Situation | Behaviour |
|---|---|
| No changeable surface in range | Sample sits in hand, nothing previews |
| Two surfaces in range | Nearest by centre distance wins |
| Participant moves away mid-preview | `Revert()` — a preview must never stick |
| Released in mid-air | Sample discarded |
| Pull attempted from a **Change** surface | Refused. Only kept things are sources — this *is* the concept |

The stuck-preview case is the one that will embarrass you in a live test: a participant sweeps a sample past three walls and leaves a trail of half-applied colour. Handle it before Week 5.

## 10. Testing plan (Criterion 4)

**Assumptions under test:**

- **A1** — People understand "pull a sample off what you're keeping, hold it against what you're changing" without being told.
- **A2** — Constraining options to what matches the kept surfaces *helps* people decide, rather than frustrating them.

A2 is the one worth the testing time, because it can genuinely fail. A negative result there makes a strong Evaluation 1.

**Protocol — five minutes per participant, minimum five participants:**

| Time | Step | Observing |
|---|---|---|
| 0:30 | Brief, headset on | — |
| 0:30 | Free look, **no instruction** | What is reached for first? Is the floor touched? |
| 2:00 | "Repaint this room so it works with the floor and sofa you're keeping" | Time to first commit; prompts needed; is twist discovered? |
| 1:00 | "Now make a second version you'd prefer" | Is comparison used at all? |
| 1:00 | Post-test questions | Confidence; frustration at limited choice |

**Data collected:**

| Measure | Type | Validates |
|---|---|---|
| Time to first unprompted commit | quantitative | A1 |
| Facilitator prompts needed (tally) | quantitative | A1 |
| Twist-to-tune discovered unprompted? | boolean | A1 |
| Samples tried before committing | quantitative | A2 |
| "How confident are you in this choice?" 1–5 | quantitative | A2 |
| "Did the limited options feel helpful or restrictive?" | qualitative | **A2 — the decisive one** |
| Think-aloud on *why* a candidate was rejected | qualitative | A2 |

Paper sheet per participant, one row per measure. Quest headset screen-recording with consent — strong appendix material for Evaluation 1.

Standardised instruments (SUS, presence questionnaires) are deliberately excluded from IP1: they are built for longer sessions and would report noise at five minutes. They belong in IP2b, where the brief points to them directly.

## 11. Open questions

The Week 2 studio session produced **peer design feedback, not protocol data** — the Wizard-of-Oz run was not executed against the timed protocol, so no behavioural measures were captured. The interaction questions therefore remain open and carry to IP1 in Week 5:

1. Does **twist-to-tune** feel natural when physically performed, or arbitrary?
2. Is **proximity-preview** legible without instruction, or does it need a visible trigger?
3. Is **sweep-to-mark** distinguishable from an accidental hand movement?
4. Does pulling a sample off a kept surface read as obvious, or does it need a visual cue on kept surfaces?
5. Do three schemes feel like enough for comparison?

Two further questions arise from the accepted feedback:

6. Can **tuning rotation** (twisting the held sample) and **layout rotation** (turning a piece of furniture) share a gesture vocabulary without colliding, now that both exist?
7. Are three lighting states sufficient, or is continuous time-of-day needed to make the light claim properly testable?

Question 6 is the one to watch. Two different meanings for the same wrist movement, distinguished only by what is in your hand, is exactly the kind of ambiguity that reads as a bug to a participant.

## 12. Risks

| Risk | Mitigation |
|---|---|
| Zero Unity experience, 35% hurdle on 28 Aug, no extension | Horizontal prototype; one system deep; ramp began Week 2 |
| Quest build pipeline fails late | IP1 targets the headset in Week 4, proving the pipeline five weeks before it is critical |
| Concept reads as a passive viewer | No menus anywhere in the core loop; every change is a physical act |
| Scope creep back toward full renovation functionality | Brief explicitly disclaims a complete product — cite it when tempted |
| Colour fidelity in MR passthrough (IP2b) | Quest 3 required; treated as a documented finding rather than a defect if it degrades |
