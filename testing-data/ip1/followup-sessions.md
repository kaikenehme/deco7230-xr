# IP1 follow-up sessions — running 3 more before Wed 2 Sep

> **Status Sun 30 Aug:** done — P3–P5 run with flatmates on the printed sheet, transcribed into `README.md` (supplementary table) and folded into `findings.md`. The instrument fixes below were **not** applied before running; the gaps they targeted recur in P3–P5 and are listed in `findings.md` §4. Rule 4 (tutor check) still open.

**Purpose:** the studio session produced n=2 and never reached A2. Three more real sessions this weekend / Monday would (a) raise n to 5, (b) give A2 a chance to be observed. They must be reported as **out-of-class supplementary sessions**, clearly separated from the Fri 28 Aug studio data.

## Rules — non-negotiable

1. **Real participants, real sessions.** Housemates, friends, classmates from other courses. Not you. Not invented.
2. **Label honestly** in Evaluation 1: date, location, platform (simulator or Quest), n. Example line: *"Three supplementary sessions (P3–P5) were run on Mon 31 Aug at home on the XR Device Simulator with the same protocol; they are reported separately from the studio session."*
3. **Same protocol, same sheet** — `ip1/data-collection-sheet.md` — so rows are comparable. Fix the two instrument gaps below *before* running.
4. **Check with the tutor** (email or Ed, Monday morning) that supplementary sessions may appear in the Evaluation 1 appendix. Cheap to ask; expensive to be told no after submitting.
5. **No identifying info** on sheets. First names not written down.

## Instrument fixes before running (5 min)

| Gap found on Fri | Fix |
|---|---|
| "Which method did you trust more, and why?" was in the plan but not on the sheet | Add as row 17, verbatim answer |
| Row 10 asked about "limited options" to people who used the menu | Branch: if row 14 = hold-up → ask row 10 as printed. If row 14 = menu → ask *"Why the menu rather than pulling a colour from the floor or sofa?"* |
| Which scripted prompts were used wasn't logged | Write the prompt numbers in row 4 boxes instead of ticks (e.g. `2 3 5`) |
| Rows lost while facilitating solo | Screen-record (QuickTime on the laptop for the simulator; Quest's built-in recording if on headset). Tick the consent box. |

## Per-session runbook (~7 min each)

| Step | Time | Do | Record |
|---|---|---|---|
| Setup | before | Restart Play mode so schemes/colours don't leak from previous participant. Sheet numbered P3/P4/P5, date, platform written at top | — |
| Brief | 30 s | "You're in a living room you're renovating. The floor and sofa are staying." Ask consent to record | consent box |
| Free look | 30 s | Silent. Watch what they reach for | rows 1, 2 |
| Task 1 | 90 s | *"Repaint this room so it works with the floor and sofa you're keeping."* Start stopwatch. Prompt only after ~20 s stuck; write prompt number | rows 3, 4, 5, 6, 13, 14 |
| Task 2 | 90 s | *"Now change the floor to tiles, add one piece of furniture and move it where you'd want it, then save this as a second version and switch between the two."* | rows 7, 8, 15, 16 |
| Post-test | 60 s | In order: confidence 1–5 · row 10 (branched) · row 17 trusted method · one confusing thing | rows 9, 10, 11, 12, 17 |
| Notes | 30 s | Anything broken, hesitations, gestures — write while fresh | observer notes |

## Platform choice

| | Simulator (laptop) | Quest (if `adb` cooperates) |
|---|---|---|
| Comparability with Fri data | same platform, directly comparable | different platform — report as a separate condition |
| A2 chance | low — Fri showed the loop isn't found via mouse | higher — body interaction is what the loop was designed for |
| Setup risk | none | `unauthorized` still unsolved; budget 30 min, then fall back |

Recommendation: try the Quest for 30 min Monday morning (`adb devices`; accept the USB-debugging dialog inside the headset; if still `unauthorized`, `adb kill-server && adb start-server`, different cable). If it works, run all three on Quest and report them as a second condition — that comparison (mouse vs body) is itself an insight. If not, run on the simulator and keep it comparable.

## After running

1. Add P3–P5 as a **second table** in `README.md` under a heading that names the date/place/platform.
2. Update `findings.md` §3 verdicts only if the new sessions change them; keep the studio-only verdict visible.
3. Append a row to `ip1/ai-use-log.md` if AI helps with the write-up (it will — Evaluation 1 is an open assessment but still needs the acknowledgement coversheet).
4. Commit sheets (scans, no names) to `sheets/` and push.
