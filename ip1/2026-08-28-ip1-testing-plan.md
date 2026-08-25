# XR Renovation Previewer — Testing plan for Interactive Prototype 1

**DECO7230 · Kaike Nehme · Studio test: Fri 28 Aug 2026 (Week 5) · v2 (menu + catalogue, 25 Aug)**
Template: IP1 brief p.3. Concept: `../concept/2026-08-07-xr-renovation-concept-design.md` (v1.1).

## Pitch

This project is a phone paint-visualiser app — Dulux Visualizer, Houzz, Home Depot Project Color — but using XR so you can hold a colour sample up against the surfaces you're keeping, at full size, in your own light. Samples are pulled directly off the things staying in the room — the timber floor, the sofa — and those samples generate the options that work with them. Alongside that, a menu on the left controller offers the full catalogue: paint colours, floor and wall materials, and furniture to swap or add. The test asks which of the two people actually use, and which they trust.

## Testing Objective

From my concept, I have identified three assumptions that need testing:

- **A1** — People understand "pull a sample off what you're keeping, hold it against what you're changing" without being told.
- **A2** — Constraining the colour options to what harmonises with the kept surfaces **helps** people decide, rather than frustrating them.
- **A3** — When both are available, people reach for the constrained hold-up loop *or* the open catalogue menu — and the open catalogue raises, or lowers, their confidence in the final choice compared with the constrained options.

**A2 is the primary objective** — the whole concept rests on it and it can genuinely fail; a negative result is still a strong Evaluation 1. This test aims to discover (a) whether the core loop is discoverable without instruction (time to first unprompted commit, prompts needed, twist-to-tune discovery), and (b) whether the constrained option space increases or decreases decision confidence (samples tried, self-reported confidence, helpful-vs-restrictive judgement), and (c) which method is used first and which produces the choice the participant keeps (A3).

## Testing Methodologies

This testing plan uses **structured observation with a timed task protocol, think-aloud, and a short post-test interview** to evaluate a digital prototype made in Unity. The first phase is deliberately uninstructed (free exploration) to measure discoverability before any priming; the second phase is task-directed; the interview captures the A2 judgement in the participant's own words. Quantitative measures are logged live on a paper sheet, one row per measure; the headset view is screen-recorded with consent as backup and appendix material. Standardised instruments (SUS, presence questionnaires) are deliberately excluded: at five minutes per session they would report noise; they belong to IP2b, where the brief points to them.

## Prototype description / requirements

The prototype was designed to make the full core loop testable at horizontal breadth: a single furnished virtual living room (timber floor and sofa are **kept**; four walls, ceiling, door and trim are **changeable**) in VR on Quest with controllers. It supports:

- **Mark** — sweeping a controller across a surface while holding grip toggles it between keep and change.
- **Pull** — touching a kept surface (floor, sofa) and pressing trigger pulls a colour sample off it into the hand.
- **Hold** — bringing the held sample near a changeable surface previews that colour on the whole surface; moving away always reverts.
- **Tune** — twisting the wrist while holding the sample walks through seven options harmonised to the sample (analogous, tonal, triadic, complementary).
- **Commit** — releasing the sample against a surface applies the colour; releasing in mid-air discards it.
- **Light** — touching the lamp cycles warm / cool / daylight, relighting the whole room.
- **Compare** — controller A saves the current scheme (up to 3), B cycles between saved schemes.
- **Menu** — pointing the right controller at a surface or piece of furniture and pressing trigger opens a menu on the left hand: paint colours (Dulux), materials (tiles, timber, carpet, plaster) and furniture to add; hovering previews, clicking commits, moving away reverts. Furniture can be swapped, removed, grabbed and moved.

Every control is either a physical act on the room or a controller input; the menu is the one non-diegetic element and exists so the two approaches can be compared (A3). The lamp cannot be swapped.

## Data collection method

During the testing process, I will observe silently through the first uninstructed phase, then issue the two task prompts verbatim, tallying on a paper data sheet (one sheet per participant, one row per measure) and noting think-aloud remarks. The headset view is screen-recorded with consent. I will only intervene with a scripted prompt when a participant is stuck for ~20 seconds, and each prompt is tallied.

| Measure | Type | Validates |
|---|---|---|
| Time to first unprompted commit | quantitative (stopwatch) | A1 |
| Facilitator prompts needed (tally) | quantitative | A1 |
| Twist-to-tune discovered unprompted? | boolean | A1 |
| Samples tried before first commit | quantitative | A2 |
| "How confident are you in this choice?" (1–5) | quantitative | A2 |
| "Did the limited options feel helpful or restrictive?" | qualitative | **A2 — decisive** |
| Think-aloud on why a candidate was rejected | qualitative | A2 |
| Method used first after Task 1 prompt (hold-up / menu) | boolean | A3 |
| Method that produced the kept choice (hold-up / menu / both) | categorical | A3 |

## Testing Setup

Charge the Quest and controllers; sideload the current `ip1.apk` and verify it launches to the room scene. Print 6 data sheets and clip to a board with a stopwatch. Start headset screen-recording at the start of each session. Clear the previous participant's schemes by restarting the app between sessions (committed colours and saved schemes must not leak between participants). Have the consent line ready before recording. One chair placed for the participant to orient from; 2m × 2m clear floor space.

## Testing process (~5 min per participant, aim ≥5 participants)

- Brief the participant: "You're in a living room you're renovating. The floor and sofa are staying." Consent for screen recording. Headset on, controllers in hand. **(30 seconds)**
- Free look, no instruction. Observe silently: what is reached for first? Is the floor touched? Log any unprompted discovery of pull / preview / twist. **(30 seconds)**
- Task 1, read verbatim: **"Repaint this room so it works with the floor and sofa you're keeping."** Start stopwatch. Log time to first unprompted commit, prompts, samples tried, twist discovery. **(1 minute 30)**
- Task 2, read verbatim: **"Now change the floor to tiles and add one piece of furniture, then save this as a second version and switch between the two."** Log menu discovery, spawn/move success, whether saving/cycling is used. **(1 minute 30)**
- Post-test, ask in order: confidence 1–5 in the final choice; "did the limited options feel helpful or restrictive — why?"; "you had two ways to choose — pulling from the room and the menu — which did you trust more, and why?"; one thing that confused you. **(1 minute)**

**Results are posted to this repo (`testing-data/ip1/`) before leaving the room** (brief requirement).
