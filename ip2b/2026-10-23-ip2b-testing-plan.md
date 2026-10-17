# XR Renovation Previewer — Testing plan for Interactive Prototype 2b

**DECO7230 · Kaike Nehme · Studio test: Fri 23 Oct 2026 (Week 12) · on the studio Meta Quest 3S**
Design: `../docs/superpowers/specs/2026-10-17-ip2b-design.md`. What changed since IP2a and why: `../evaluations/evaluation-2-print.html` §05–06, `../testing-data/ip2a/README.md`.

## Pitch

A phone paint-visualiser (Dulux Visualizer, Houzz, IKEA Kreativ) rebuilt in XR: you stand in a room, pull a colour off anything you like — the sofa, the rug, a print — and it opens in your hand as a paint fan deck. Pick a card, hold it up to a wall or the floor (or throw it), and the room changes at full size, in its own light. IP2b moves from a test level to two rooms that read as homes (a warm contemporary living room and a mid-century bedroom), lets you change anything, and ends with the same tools on the studio's real walls through passthrough.

## Research summary (DECO7230)

Choosing a colour by holding a sample against the surface draws on proprioception: working within arm's reach and relative to the body is faster and more precise than acting at a distance (Mine et al., 1997), which is why hold-up is the default commit and the fan deck opens in the hand rather than on a panel. Throwing in VR is measurably less accurate and more effortful than real throwing (Zindulka et al., 2020), so toss is offered alongside hold-up and evaluated on experience and preference, not accuracy. Colour judgements made in a headset differ from those made on a screen, and light temperature preferences shift between the two (Siess & Wölfel, 2019) — the reason testing runs on the Quest, and why the room's light can be changed (clock, lamp, curtains). The final showcase moves the same interaction along Milgram and Kishino's (1994) reality–virtuality continuum, from a virtual room to the participant's surroundings through video passthrough; because passthrough limits colour and clarity (de Souza & Tartz, 2024), MR is asked about as intent ("would you use this at home?"), not used for colour accuracy. Experience and comfort are measured with short standardised instruments that fit a seven-minute session: the 8-item UEQ-S (Schrepp et al., 2017) and the 9-item VRSQ (Kim et al., 2018).

de Souza, R. V., & Tartz, R. (2024). Visual perception and user satisfaction in video see-through head-mounted displays: A mixed-methods evaluation. *Frontiers in Virtual Reality, 5*. https://doi.org/10.3389/frvir.2024.1368721
Kim, H. K., Park, J., Choi, Y., & Choe, M. (2018). Virtual reality sickness questionnaire (VRSQ): Motion sickness measurement index in a virtual reality environment. *Applied Ergonomics, 69*, 66–73. https://doi.org/10.1016/j.apergo.2017.12.016
Milgram, P., & Kishino, F. (1994). A taxonomy of mixed reality visual displays. *IEICE Transactions on Information and Systems, E77-D*(12), 1321–1329. https://doi.org/10.1587/e77-d_12_1321
Mine, M. R., Brooks, F. P., & Sequin, C. H. (1997). Moving objects in space: Exploiting proprioception in virtual-environment interaction. *Proceedings of SIGGRAPH '97*, 19–26. https://doi.org/10.1145/258734.258747
Schrepp, M., Hinderks, A., & Thomaschewski, J. (2017). Design and evaluation of a short version of the User Experience Questionnaire (UEQ-S). *International Journal of Interactive Multimedia and Artificial Intelligence, 4*(6), 103–108. https://doi.org/10.9781/ijimai.2017.09.001
Siess, A., & Wölfel, M. (2019). User color temperature preferences in immersive virtual realities. *Computers & Graphics, 81*, 20–31. https://doi.org/10.1016/j.cag.2019.03.018
Zindulka, T., Bachynskyi, M., & Müller, J. (2020). Performance and experience of throwing in virtual reality. *Proceedings of the 2020 CHI Conference on Human Factors in Computing Systems*. https://doi.org/10.1145/3313831.3376639

## Testing objective

| # | Assumption | Decisive measure |
|---|---|---|
| **B1** | With nothing marked as "staying" and the room's largest piece pulsing on entry, people pull a sample **unprompted**. (IP1 0/5 sim · IP2a 4/5 headset) | Pulled in free look, no prompt |
| **B2** | Seeing all seven options at once, light → dark, gets people **choosing from the constrained set** rather than the full menu — and they find it helpful, not restrictive. (IP2a: kept colour from the menu 5/5; A2 never asked) | Method of the kept colour · "helpful or restrictive?" asked of **everyone who opened a fan** |
| **B3** | Given both, people use and prefer **hold-up** for precision and **toss** for fun — or one dominates. | Commits per method (logged per commit) · stated preference + why |
| **B4** | The dressed rooms feel like a home the participant could make their own. | "Feels like a room I could make mine" 1–5 + why |
| **B5** | Changing the light (curtains, clock, lamp) changes at least one colour decision. | Light used? · decision changed? |
| B6 *(exploratory)* | Seeing the tools on real walls makes the concept credible for their own home. | "Could you see yourself using this at home?" verbatim |

Lesson carried from Eval 2: every decisive question is asked of **everyone exposed to the thing it asks about**, whichever path they took.

## Methodology

Structured observation with a timed task protocol and think-aloud, two standardised questionnaires, and a short interview. Phase 1 is uninstructed (B1). Phase 2 is a free creative task (B2, B3). Phase 3 changes the light and layout (B5). Phase 4 is the MR showcase (B6). Then UEQ-S and VRSQ on the iPad, then the interview (B2, B3, B4). One record per participant, logged live on the iPad session logger; prompts logged **by number**. The Quest view is mirrored and recorded with consent (`../ip2a/record-session.sh`).

## Prototype

A 5.5 × 5 m room on the Quest 3S, controllers (not hand tracking). Two rooms, switched by touching a framed picture on the wall behind the start position: **Living** (warm contemporary — curved cream sofa, terracotta armchair, oak floor, jute rug, garden window, arc lamp, woven pendant, TV wall) and **Bedroom** (mid-century — walnut, teal wall, mustard duvet, vanity).

| Interaction | What the participant does | New in IP2b? |
|---|---|---|
| Pull | Bring a controller to **anything** — sofa, rug, a wall, the floor — a swatch tab appears; trigger peels a sample into the hand | **any surface** (IP2a: kept surfaces only) |
| Fan deck | The sample opens as **seven cards, lightest on the left, two off-whites first**; touch a card with the other controller (or point the right ray at it + trigger) to pick it | **new** (replaces twist-to-tune) |
| Hold up | Bring the picked card near a wall or the floor to preview; let go to commit; let go mid-air to discard | floor now takes colour |
| Toss | Throw the card at a wall or the floor — it splashes on | **new** |
| Menu | Point at a surface, trigger: full catalogue (paints, floor materials, furniture) | as IP2a |
| Floor | Paint tints the floor's material; the menu swaps the material | tint **new** |
| Move | Grip a piece to move it; thumbstick turns it; release places it | smoother, half-speed turn |
| Light | Touch the clock (time of day), the lamp (warm/cool/off), **the curtains (draw closed/open — daylight fades over 1 s)** | curtains **new** |
| MR | Touch the **"Your room"** frame: the virtual room disappears and the same cards paint the real walls (passthrough). Touch it again to return | **new** |

Facilitator (desktop dry run only): `T` then hold `B` = sample in hand · `F1–F2` rooms · `F5–F10` time · `M` MR on/off.

## Data collection

| Measure | Type | Validates |
|---|---|---|
| Sample pulled unprompted in free look · what from | boolean + categorical | **B1 — decisive** |
| Hero pulse noticed (remark / reach toward it) | boolean | B1 |
| Fan opened · cards picked before first commit (tally) | quantitative | B2 |
| Method of the colour they keep (fan / menu / both) | categorical | **B2 — decisive** |
| "Seven options picked from what you pulled — helpful or restrictive? Why?" (everyone who opened a fan) | qualitative | **B2 — decisive** |
| Commits by hold-up / by toss (tally each) | quantitative | **B3** |
| "Hold it up or throw it — which would you use, and why?" (everyone shown both) | qualitative | **B3** |
| Floor changed? by tint / material / both | categorical | B2 |
| Curtains / clock / lamp used · did a light change change a colour? | boolean × 3 + boolean | **B5** |
| "Feels like a room I could make mine" 1–5 + why | quantitative + qualitative | **B4** |
| MR: painted a real wall? · "Could you see yourself using this at home?" | boolean + qualitative | B6 |
| UEQ-S (8 items, −3…+3): pragmatic + hedonic quality | quantitative | B2–B4 |
| VRSQ (9 items, 0–3): oculomotor + disorientation | quantitative | comfort principle |
| Prompts needed — numbers | quantitative | all |
| Seen this project before? | boolean | sample |

**UEQ-S items** (7-point, left −3 … right +3): obstructive–supportive · complicated–easy · inefficient–efficient · confusing–clear · boring–exciting · not interesting–interesting · conventional–inventive · usual–leading edge.
**VRSQ items** (none 0 · slight 1 · moderate 2 · severe 3): general discomfort · fatigue · eyestrain · difficulty focusing · headache · fullness of head · blurred vision · dizzy (eyes closed) · vertigo.

## Five design principles (Week 6 lecture)

| Principle | How the prototype addresses it | What we observe |
|---|---|---|
| **Text readability** | Almost no text: cards carry colour only; the two frame labels are large, dark on light | Any squinting / leaning in to read; comments |
| **Ergonomics** | Fan opens at the hand, cards within 16 cm; hold-up at arm's length; floor reached by lowering the arm, not crouching; furniture moved by ray, no lifting | Arm held up > 10 s; crouching; reach complaints |
| **Brightness changes** | Curtains fade daylight over 1 s, never snap; closed room stays lit by lamp + pendant; time-of-day steps are scheduled by touch | Squint / flinch at curtain close; VRSQ eyestrain |
| **Avoiding simulator sickness** | No smooth locomotion (teleport only; room fits the play space); MR keeps the real room visible | VRSQ total; any pause / "I feel off" |
| **Button placement** | No menu buttons for core actions: controls are objects (cards, curtains, clock, lamp, frames). Trigger = pull/pick/touch, grip = move — the same on both hands | Wrong button pressed (tally); asks "which button?" |

## Setup (studio, before the first participant)

1. Quest 3S charged; **Space Setup** run in the test area (walls + floor), boundary clear ≥ 2 × 2 m.
2. USB to the Mac, **"Always allow"** for debugging; install the APK (`adb install -r`); launch once.
3. Check MR: touch "Your room" → walls detected and paintable. If not: run the test without Phase 4 and note it.
4. Mirror running (scrcpy) + recording script; iPad logger open; consent card ready.
5. Restart the app between participants (colours, furniture, time and curtains must not leak).

## Process (~7 min per participant, aim ≥ 5)

- **Brief (30 s).** "Imagine this is your home and you're about to redecorate. You can change anything." Consent for recording. Seen the project before? Fit the headset.
- **Phase 1 — free look, no instruction (45 s).** Silent. Log B1 measures. Prompts only after 20 s stuck, by number.
- **Phase 2 — Task 1 (2 min).** Read verbatim: **"Make this room yours: change the walls and the floor to colours you'd want to live with."** If no sample after 45 s, prompt 1: "Try touching something you like." Log fan, cards, method, hold-up/toss tallies.
- **Phase 3 — Task 2 (1 min).** Read verbatim: **"Now see how it looks in the evening, and move one piece of furniture."** Log curtains/clock/lamp, decision change, furniture.
- **Phase 4 — MR showcase (45 s).** Read verbatim: **"Touch the frame that says 'Your room'."** Then: "Paint one of these walls." Log; ask B6 question.
- **Questionnaires (1 min 30).** Headset off. UEQ-S then VRSQ on the iPad.
- **Interview (1 min).** In order: B2 helpful/restrictive (if a fan was opened) · B3 hold-up vs throw (if both were shown) · B4 rating + why · one thing that confused you.

**Results: logger export + this plan's sheet posted to `testing-data/ip2b/` before leaving the room. Research summary submitted to Blackboard at the end of the session.**

## Known limits, stated up front

- The studio is not the participant's home: B6 is intent, not evidence of use at home.
- Passthrough colour is not accurate (de Souza & Tartz, 2024); colour decisions are measured in VR only.
- First run of this build on a headset is the morning of the test; if MR fails, Phase 4 is dropped and reported as such.
- n ≈ 5 peers, mostly design students: counts are reported raw, not as rates.
