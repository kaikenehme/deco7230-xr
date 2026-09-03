# XR Renovation Previewer — Testing plan for Interactive Prototype 2a

**DECO7230 · Kaike Nehme · Studio test: Fri 25 Sep 2026 (Week 9) · v1 (3 Sep, draft — freeze Mon 21 Sep)**
Template: IP1 brief p.3 (IP2a has no template of its own; results feed Evaluation 2). Concept: `../concept/2026-08-07-xr-renovation-concept-design.md` (v1.1). What changed since IP1 and why: `../evaluations/evaluation-1-print.html` §05–06, `../testing-data/ip1/findings.md` §5–6.

## Pitch

This project is a phone paint-visualiser app — Dulux Visualizer, Houzz, Home Depot Project Color — but using XR so you can hold a colour sample up against the surfaces you're keeping, at full size, in your own light. Samples are pulled directly off the things staying in the room, and those samples generate the options that work with them. IP1 found that nobody pulled a sample: kept surfaces gave nothing back when touched (0/5). IP2a rebuilds that first contact, starts the main task with a sample already in hand so the constrained set is finally reached, and adds the things participants asked for: a pre-furnished room to rearrange, more furniture, and a window whose light changes through the day.

## Testing Objective

From IP1's results and Evaluation 1, four assumptions need testing:

- **A1′** — With first-contact feedback (a swatch tab at the touch point, the controller glowing, the sample visibly peeling off), people pull a sample from a kept surface **without being told**. IP1 baseline: 0/5.
- **A2** — Constraining the colour options to what harmonises with the kept surfaces **helps** people decide, rather than frustrating them. **Primary objective, untested after IP1** because the gate to it never opened. Task 1 now starts sample-in-hand so every participant reaches it.
- **A3** — Given both the constrained hold-up loop and the open catalogue menu, which do people use for the choice they keep, and which do they say they trust?
- **A4** — Six times of day through the window are used to check a colour, and change at least one decision (Q7: are three lighting states enough?).

Secondary, on the furniture changes participants asked for: does grip-hold / release-place-at-ray succeed first time, and is thumbstick rotate discovered?

## Testing Methodologies

Structured observation with a timed task protocol, think-aloud, and a short post-test interview, on a Unity prototype. Phase 1 is uninstructed (free look) and measures **entry discovery only** — whether the tab and glow get someone to pull, with no help. Phase 2 starts Task 1 **with a sample already in the participant's hand** (facilitator holds X for one second), so A2 is measured on everyone regardless of Phase 1. Phase 3 is the furniture / light task. The interview branches: people who chose from the constrained set are asked whether it felt helpful or restrictive; people who chose from the menu are asked why the menu. Every row is logged live on one sheet per participant; prompt **numbers** (not ticks) go in the tally boxes so it is known which prompt unstuck whom. Sessions are screen recorded with consent (QuickTime for the simulator, Quest's own recorder on device). Standardised questionnaires stay out at seven minutes per session; they belong to IP2b.

## Prototype description / requirements

A 9 × 7 m virtual living room in VR (Quest, controllers). One of three **preset rooms** is loaded at start (Scandi, Mid-century, Coastal — different kept floor, wall colours and furniture); framed pictures on the left wall switch between them. The floor and one piece of furniture are **kept**; walls, ceiling, door, trim and window frame are **changeable**. It supports:

- **First contact** — moving a controller toward a kept surface shows a swatch tab at that point and the controller glows; pressing trigger peels a sample off the tab into the hand. *(new: IP1's weakest thing, rebuilt)*
- **Hold / Tune / Commit** — as IP1: near a changeable surface previews the colour; twisting the wrist walks seven harmonised options; release commits, mid-air discards. A thread runs from the sample back to the surface it came from, so the constraint shows what it is doing. *(thread new)*
- **Light** — a window in the far wall; touching the wall clock beside it steps the sun through 07:00, 10:00, 13:00, 16:00, 19:00, 22:00 (night: the lamp is the only light). Touching the lamp's pull cord cycles warm / cool / off. *(new)*
- **Presets** — touching a framed picture re-dresses the room. *(new)*
- **Menu** — as IP1: point at a surface, trigger; paint colours, materials, and 24 pieces of furniture in three pages (seating / tables / storage, decor, lighting).
- **Move** — grip on a piece holds it; it sits where the ray meets the floor and follows the ray; the thumbstick of the holding hand rotates it; release places it. Hovering a piece shows an outline. *(placement, rotate and outline new — P2, P4, P1, P3's requests)*
- **Onboarding** — on entry the kept surfaces pulse twice under a "staying" label and the lamp glows, for ten seconds. No panels. *(new)*
- **Cut from this test** — scheme save/cycle (found by 0/4 in IP1). The buttons still work; the script does not mention them.

## Data collection method

Silent observation through Phase 1; Task prompts read verbatim; scripted prompts only after ~20 s stuck, each logged by number. One paper sheet per participant (`data-collection-sheet.md`), screen recording with consent. Results transcribed to `../testing-data/ip2a/README.md` before leaving the room.

| Measure | Type | Validates |
|---|---|---|
| Tab / glow noticed in free look? (remark or visible reaction) | boolean | A1′ |
| Sample pulled **unprompted** in free look | boolean | **A1′ — decisive** |
| Time to first unprompted commit (Task 1, sample already in hand) | quantitative | A1′ / A2 |
| Twist-to-tune discovered unprompted? | boolean | A1′ |
| Harmonised options tried before first commit (tally) | quantitative | A2 |
| Confidence in the kept choice (1–5) | quantitative | A2 |
| Constrained-set users: "helpful or restrictive?" (verbatim) | qualitative | **A2 — decisive** |
| Menu users: "why the menu rather than pulling from the floor or sofa?" (verbatim) | qualitative | A3 |
| Which colour did you reject, and why? (verbatim) | qualitative | A2 |
| Method used for the kept choice (hold-up / menu / both) | categorical | A3 |
| "Which did you trust more, and why?" (verbatim) — **printed this time** | qualitative | A3 |
| Clock touched? stops visited (tally) · did a time change a choice? | quantitative + boolean | A4 |
| Lamp found (unprompted / prompted / no) | categorical | discoverability |
| Furniture: placed where intended first try? rotate discovered? | boolean × 2 | secondary |
| Preset switched unprompted? | boolean | secondary |
| Prompts needed — **numbers**, not ticks | quantitative | A1′ |
| Seen this project before? (yes / no) | boolean | sample |

## Testing Setup

Quest charged, `Builds/ip2a.apk` sideloaded and launched to the room (see `quest-sideload.md`; if the headset is still `unauthorized`, run on the XR Device Simulator and write the platform on every sheet). Print 7 sheets, clipboard, stopwatch. Restart the app between participants (committed colours, furniture moves and the time of day must not leak). Preset 1 (Scandi) loads by default; keep it for everyone so the kept floor and sofa are the same across participants. Consent line before recording. 2 m × 2 m clear floor.

## Testing process (~7 min per participant, aim ≥5, recruit beyond the flat)

- Brief: "You're in a living room you're renovating. The floor and the sofa are staying." Consent for recording. Ask: "Have you seen this project before?" Headset on. **(45 s)**
- **Phase 1, free look, no instruction.** Silent. Log: what is reached for first; tab/glow noticed; sample pulled unprompted. Prompts allowed only after 20 s stuck, by number. **(45 s)**
- **Phase 2, Task 1.** Facilitator holds X for one second: a floor sample appears in the participant's right hand. Read verbatim: **"That's a sample of the floor you're keeping. Repaint this room so it works with the floor and sofa."** Start stopwatch. Log time to first unprompted commit, options tried, twist discovery, prompts, method for the kept choice. **(2 min)**
- **Phase 3, Task 2.** Read verbatim: **"Now move the armchair to where you'd want it, add one more piece of furniture, and check how your wall colour looks at a different time of day."** Log placement success, rotate discovery, clock use, lamp use, preset switching if it happens. **(2 min)**
- **Post-test**, in order: confidence 1–5 · row 10 (branched by row 14) · "which colour did you reject and why?" · "which did you trust more, pulling from the room or the menu, and why?" · "did changing the time of day change your mind about anything?" · one thing that confused you. **(1 min 30)**

**Results posted to this repo (`testing-data/ip2a/`) before leaving the room.**

## Known limits, stated up front

- Platform: if the headset cannot be authorised in time, the simulator replaces it and every count is reported as simulator data, as in IP1.
- The sun's path is a six-stop approximation, not a real hemisphere; the question is whether time of day is *used*, not whether it is accurate.
- Lighting has one floor-standing option (Poly Haven has no floor lamps under CC0); the Lighting page is thin and participants may say so.
