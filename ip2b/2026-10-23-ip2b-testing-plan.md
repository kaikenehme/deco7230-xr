# XR Renovation Previewer — Testing plan for Interactive Prototype 2b

**DECO7230 · Kaike Nehme · Studio test: Fri 23 Oct 2026 (Week 12) · on the studio Meta Quest 3S**
Design: `../docs/superpowers/specs/2026-10-17-ip2b-design.md`, amended by `../docs/superpowers/specs/2026-10-20-empty-room-design.md` (20 Oct: the MR showcase is replaced by an Empty room). What changed since IP2a and why: `../evaluations/evaluation-2-print.html` §05–06, `../testing-data/ip2a/README.md`.

## Pitch

A phone paint-visualiser (Dulux Visualizer, Houzz, IKEA Kreativ) rebuilt in XR: you stand in a room, pull a colour off anything you like — the sofa, the rug, a print — and it opens in your hand as a paint fan deck. Pick a card, hold it up to a wall or the floor (or throw it), and the room changes at full size, in its own light. IP2b moves from a test level to two rooms that read as homes (a warm contemporary living room and a mid-century bedroom), lets you change anything, and ends in a bare Empty room where you make the room you imagine.

## Research summary (DECO7230)

Choosing a colour by holding a sample against the surface draws on proprioception, since working within arm’s reach and relative to the body is faster and more precise than acting at a distance (Mine et al., 1997). For this reason, hold-up is the default way to commit a choice, and the fan deck opens in the hand rather than on a panel. Throwing in VR is measurably less accurate and more effortful than real throwing (Zindulka et al., 2020), so toss is offered alongside hold-up but evaluated on experience and preference rather than accuracy. Colour judgements and preferred light temperature also differ between a headset and a screen (Siess & Wölfel, 2019). Testing therefore runs on the Quest, and participants can change the room’s light through the clock, lamp and curtains. The final phase opens a bare room with the same tools, following the creativity-support principle of low thresholds, high ceilings and wide walls (Shneiderman, 2007): participants have nothing new to learn, no set target, and as many possible outcomes as they can imagine. Its single question, how close the result came to the room they had in mind, is a one-item rating in the spirit of the Creativity Support Index (Cherry & Latulipe, 2014), which measures how well a tool supports creative work. The full index is too long for a one-minute phase. Experience and comfort are instead measured with two short standardised instruments that fit within the eight-minute session: the 8-item UEQ-S (Schrepp et al., 2017) and the 9-item VRSQ (Kim et al., 2018).

Cherry, E., & Latulipe, C. (2014). Quantifying the creativity support of digital tools through the Creativity Support Index. *ACM Transactions on Computer-Human Interaction, 21*(4), 1–25. https://doi.org/10.1145/2617588
Kim, H. K., Park, J., Choi, Y., & Choe, M. (2018). Virtual reality sickness questionnaire (VRSQ): Motion sickness measurement index in a virtual reality environment. *Applied Ergonomics, 69*, 66–73. https://doi.org/10.1016/j.apergo.2017.12.016
Mine, M. R., Brooks, F. P., & Sequin, C. H. (1997). Moving objects in space: Exploiting proprioception in virtual-environment interaction. *Proceedings of SIGGRAPH '97*, 19–26. https://doi.org/10.1145/258734.258747
Schrepp, M., Hinderks, A., & Thomaschewski, J. (2017). Design and evaluation of a short version of the User Experience Questionnaire (UEQ-S). *International Journal of Interactive Multimedia and Artificial Intelligence, 4*(6), 103–108. https://doi.org/10.9781/ijimai.2017.09.001
Shneiderman, B. (2007). Creativity support tools: Accelerating discovery and innovation. *Communications of the ACM, 50*(12), 20–32. https://doi.org/10.1145/1323688.1323689
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
| B6 *(exploratory)* | In a bare room, the same tools let people make the room they imagine. | "How close is this to the room you had in mind?" 1–5 + why · what they painted with (menu / fan / both) · furniture added? |

Lesson carried from Eval 2: every decisive question is asked of **everyone exposed to the thing it asks about**, whichever path they took.

## Methodology

Structured observation with a timed task protocol and think-aloud, two standardised questionnaires, and a short interview. Phase 1 is uninstructed (B1). Phase 2 is a free creative task (B2, B3). Phase 3 changes the light and layout (B5). Phase 4 is the Empty room (B6). Then UEQ-S and VRSQ on the iPad, then the interview (B2, B3, B4). One record per participant, logged live on the iPad session logger (https://claude.ai/artifact/MSM5HKKZdxh7xLpB1MV3RS); prompts logged **by number**. The Quest view is mirrored and recorded with consent (`record-session.sh P1`).

## Prototype

A 5.5 × 5 m room on the Quest 3S, controllers (not hand tracking). Three rooms, switched by touching a framed picture on the wall behind the start position: **Living** (warm contemporary — curved cream sofa, terracotta armchair, oak floor, jute rug, garden window, arc lamp, woven pendant, TV wall), **Bedroom** (mid-century — walnut, teal wall, mustard duvet, vanity) and the **Empty room** (bare white walls, plain oak boards; no furniture, rug, prints or lamp; always opens in daylight; furniture can be added from the floor menu).

| Interaction | What the participant does | New in IP2b? |
|---|---|---|
| Pull | Bring a controller to **anything** — sofa, rug, a wall, the floor — a swatch tab appears; trigger peels a sample into the hand | **any surface** (IP2a: kept surfaces only) |
| Fan deck | The sample opens as **seven cards, lightest on the left, two off-whites first**; touch a card with the other controller (or point the right ray at it + trigger) to pick it | **new** (replaces twist-to-tune) |
| Hold up | Bring the picked card near a wall or the floor to preview; let go to commit; let go mid-air to discard. The sample stays in the hand with no button held; **let go = squeeze and release the grip** | floor now takes colour |
| Toss | Throw the card at a wall or the floor (swing, letting go of the grip) — it splashes on | **new** |
| Menu | Point at a surface, trigger: full catalogue (paints, floor materials, furniture) | as IP2a |
| Floor | Paint tints the floor's material; the menu swaps the material | tint **new** |
| Move | Grip a piece to move it; thumbstick turns it; release places it | smoother, half-speed turn |
| Light | Touch the clock (time of day), the lamp (warm/cool/off), **the curtains (draw closed/open — daylight fades over 1 s)** | curtains **new** |
| Empty room | Touch the **"Empty room"** frame: a bare white room with the same tools. Colours from the menu, or pull from a wall (once it's painted, its fan follows that colour); furniture from the floor menu's Add tab | **new** (replaces the MR showcase) |

Facilitator (desktop dry run only): `T` then hold `B` = sample in hand · `F1–F3` rooms · `F5–F10` time.

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
| Stepped back to look at the room after a hold-up commit? (observed; logger notes) | boolean | B3 |
| "When you held it up, could you tell how the colour looked in the whole room?" (everyone who used hold-up; logger notes) | qualitative | B3 |
| Floor changed? by tint / material / both | categorical | B2 |
| Curtains / clock / lamp used · did a light change change a colour? | boolean × 3 + boolean | **B5** |
| "Feels like a room I could make mine" 1–5 + why | quantitative + qualitative | **B4** |
| Empty room: "How close is this to the room you had in mind?" 1–5 + why · painted with menu / fan / both · added furniture? | quantitative + categorical + qualitative | B6 |
| UEQ-S (8 items, −3…+3): pragmatic + hedonic quality | quantitative | B2–B4 |
| VRSQ (9 items, 0–3): oculomotor + disorientation | quantitative | comfort principle |
| Prompts needed — numbers | quantitative | all |
| Seen this project before? | boolean | sample |

**UEQ-S items** (7-point, left −3 … right +3): obstructive–supportive · complicated–easy · inefficient–efficient · confusing–clear · boring–exciting · not interesting–interesting · conventional–inventive · usual–leading edge.
**VRSQ items** (none 0 · slight 1 · moderate 2 · severe 3): general discomfort · fatigue · eyestrain · difficulty focusing · headache · fullness of head · blurred vision · dizzy (eyes closed) · vertigo.

## Five design principles (Week 6 lecture)

| Principle | How the prototype addresses it | What we observe |
|---|---|---|
| **Text readability** | Almost no text: cards carry colour only; the three frame labels are large, dark on light | Any squinting / leaning in to read; comments |
| **Ergonomics** | Fan opens at the hand, cards within 16 cm; hold-up at arm's length; floor reached by lowering the arm, not crouching; furniture moved by ray, no lifting | Arm held up > 10 s; crouching; reach complaints |
| **Brightness changes** | Curtains fade daylight over 1 s, never snap; closed room stays lit by lamp + pendant; time-of-day steps are scheduled by touch | Squint / flinch at curtain close; VRSQ eyestrain |
| **Avoiding simulator sickness** | No smooth locomotion (teleport only; room fits the play space) | VRSQ total; any pause / "I feel off" |
| **Button placement** | No menu buttons for core actions: controls are objects (cards, curtains, clock, lamp, frames). Trigger = pull/pick/touch, grip = move furniture or let go of a sample — the same on both hands | Wrong button pressed (tally); asks "which button?" |

## Setup (studio, before the first participant)

1. Quest 3S charged; boundary set in the test area, clear ≥ 2 × 2 m.
2. USB to the Mac, **"Always allow"** for debugging; install the APK (`adb install -r`); launch once.
3. Check the Empty room: touch its frame → bare, lit room; touch Living to return.
4. Mirror running (scrcpy) + recording script; iPad logger open; consent card ready.
5. Restart the app between participants (colours, furniture, time and curtains must not leak).

## Process (~8 min per participant, aim ≥ 5)

Longer than the brief's 5-minute guide: four short phases test B1–B6 in one sitting, and the two questionnaires (UEQ-S, VRSQ) add about 1 min 30. If the queue is long, drop Phase 4 (B6 is exploratory) to get to about 7 min.

- **Brief (30 s).** "Imagine this is your home and you're about to redecorate. You can change anything." Consent for recording. Seen the project before? Fit the headset.
- **Phase 1 — free look, no instruction (45 s).** Silent. Log B1 measures. Prompts only after 20 s stuck, by number.
- **Phase 2 — Task 1 (2 min).** Read verbatim: **"Make this room yours: change the walls and the floor to colours you'd want to live with."** If no sample after 45 s, prompt 1: "Try touching something you like." Log fan, cards, method, hold-up/toss tallies.
- **Phase 3 — Task 2 (1 min).** Read verbatim: **"Now see how it looks in the evening, and move one piece of furniture."** Log curtains/clock/lamp, decision change, furniture.
- **Phase 4 — Empty room (1 min).** Read verbatim: **"Touch the frame that says 'Empty room'. Make the room you're imagining."** Log what they painted with and whether they added furniture; at the end ask the B6 question.
- **Questionnaires (1 min 30).** Headset off. UEQ-S then VRSQ on the iPad.
- **Interview (1 min).** In order: B2 helpful/restrictive (if a fan was opened) · B3 hold-up vs throw (if both were shown) · B3 overview: "When you held it up, could you tell how the colour looked in the whole room?" (if they used hold-up) · B4 rating + why · one thing that confused you.

**Results: logger export + this plan's sheet posted to `testing-data/ip2b/` before leaving the room. Research summary submitted to Blackboard at the end of the session.**

## Known limits, stated up front

- MR was cut on 20 Oct: it could not be tried on a headset before the test, and the editor run showed the participant could get stranded. The Empty room replaces it; MR passthrough is the "different modality" extension in Evaluation 3.
- First run of this build on a headset is in the studio, just before the first participant.
- Designer's desktop lap (23 Oct, morning): hold-up needs you close to the wall, so you judge the colour but lose the view of the whole room. Not changed before the test; observed under B3 (step-back + overview question). Toss and the menu both paint from a distance.
- In the Empty room a tall participant can walk into the ceiling pendant (shade bottom ≈ 1.75 m): steer them round it.
- The three room frames hang on Wall_N. Touching the current room's frame does nothing, but a participant painting that wall can brush another room's frame and switch rooms: watch the frames in Phase 4 and note any switch.
- B6 is one exploratory rating after one minute; it is reported raw, not as a creativity-support score.
- n ≈ 5 peers, mostly design students: counts are reported raw, not as rates.
