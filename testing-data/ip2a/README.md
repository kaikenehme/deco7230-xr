# IP2a testing data — Fri 25 Sep 2026, Week 9 studio

**Status:** raw results, exported 25 Sep from the iPad session logger (https://claude.ai/artifact/SXDX155ZVLBknmy5gvajCD). Not yet analysed — that is Evaluation 2 (due Wed 7 Oct).

**n = 5, all on the Meta Quest 3S** (controllers). The plan (v2) said desktop mode; on the day Kaike switched to the headset once the Quest was authorised over USB. So these are headset results — unlike IP1, which was all simulator.

Plan: `../../ip2a/2026-09-25-ip2a-testing-plan.md` · Rows: `../../ip2a/data-collection-sheet.md` · Raw records: `logger-export/participants/p1–p5.json` · Device log: `device/app-events-2026-09-25.txt`.

## Results as logged

| Row | Measure | P1 | P2 | P3 | P4 | P5 |
|---|---|---|---|---|---|---|
| — | Platform | Quest | Quest | Quest | Quest | Quest |
| — | Seen project before | No | No | No | Yes | Yes |
| — | Recording consent | No | No | No | No | No |
| 1 | Reached for first | The forniture | Walls | Sofa | Forniture | Floor |
| 2 | Tab/glow noticed | Yes | Yes | Yes | Yes | Yes |
| 3 | Pulled a sample (free look) | Unprompted | Unprompted | Unprompted | — | Unprompted |
| 4 | Prompts used, in logged order | 1 → 4 → 3 → 8 | 1 → 3 → 6 → 8 | 3 → 5 → 3 → 2 → 8 | 1 → 2 → 6 → 8 | 3 → 5 → 7 → 6 → 8 |
| 5 | Time to first unprompted commit | 1 min | 2 min | 3 min | 4 min | 4 min |
| 6 | Twist-to-tune found | No | No | Unprompted | Unprompted | Unprompted |
| 7 | Harmonised options tried before commit | 2 | 2 | 2 | 1 | 2 |
| 8 | Method for the kept colour | Menu | Menu | Menu | Menu | Menu |
| 9 | Thread noticed | Yes | Yes | — | Yes | Yes |
| 10 | Armchair placed first try | Yes | Yes | Yes | Yes | Yes |
| 11 | Rotate found | Unprompted | Unprompted | Unprompted | Unprompted | Unprompted |
| 12 | Clock used · stops | Yes · 5 stops | — · 3 stops | Yes · 4 stops | Yes · 5 stops | Yes · 3 stops |
| 13 | Lamp found | Prompted | Unprompted | Prompted | Unprompted | Unprompted |
| 14 | Menu opened · furniture | Unprompted · Added furniture | Unprompted · Added furniture | Unprompted | Unprompted | Unprompted |
| 15 | Preset switched | Yes | Yes | Yes | Yes | Yes |
| 16 | Confidence 1–5 | 5 | 4 | 4 | 4 | 5 |
| 17 | Helpful/restrictive (a) · why menu (b) | — | — | (b) Easier to use | (b) More intuitive | (b) Easy setup |
| 18 | Rejected colour, why | Light colour , personal preference | — | Light colour by personal preference | Black colour personal | — |
| 19 | Trusted more, why | From the room, she misses more physical interaction | — | Menu, easier to use | Menu, more intuitive | — |
| 20 | Time of day changed a decision | No | — | No | — | — |
| 21 | One thing that confused them | The drag from the furnitures | — | Everything was pretty smooth | — | — |
| — | Observer notes | different  iteraction with the lamp (fisical interaction)   Gaming interaction like ballon trough in the wall and change the colour | Other types of room | — | Too sensitive the fortitude moving  Swap for the menu | — |

## Counts against the assumptions (raw, before analysis)

| Assumption | What was logged | Count |
|---|---|---|
| **A1′** first contact gets people to pull | Pulled a sample in free look without a prompt (row 3) | **4 / 5** "Unprompted" (P4 left blank). IP1 baseline: 0/5 |
| A1′ | Tab or glow noticed (row 2) | 5 / 5 |
| A1′ | Twist-to-tune found without help (row 6) | 3 / 5 (P1, P2: no) |
| **A2** constrained options help | Helpful/restrictive question (17a) | **not asked of anyone**: every kept colour came from the menu (row 8), so the branch never fired |
| A2 | Harmonised options tried before first commit (row 7) | 2, 2, 2, 1, 2 |
| **A3** which method, which trusted | Method for the kept colour (row 8) | **Menu 5 / 5** |
| A3 | Why the menu (17b) | "easier to use" (P3), "more intuitive" (P4), "easy setup" (P5); P1, P2 blank |
| A3 | Trusted more (row 19) | Menu: P3, P4 · From the room: P1 ("misses more physical interaction") · P2, P5 blank |
| A2 / A3 | Confidence in the kept colour (row 16) | 5, 4, 4, 4, 5 (mean 4.4) |
| **A4** time of day used | Clock used · stops visited (row 12) | used by 5 / 5 (P2's yes/no blank, 3 stops logged); stops 5, 3, 4, 5, 3 |
| A4 | Time of day changed a decision (row 20) | No (P1, P3); others blank |
| Secondary | Armchair placed first try · rotate found (10, 11) | 5 / 5 · 5 / 5 unprompted |
| Secondary | Lamp found (13) | unprompted 3 (P2, P4, P5) · prompted 2 (P1, P3) |
| Secondary | Menu opened (14) · preset switched (15) | 5 / 5 unprompted · 5 / 5 |

## Data quality — read before quoting any of this

- **Prompt order and phase are not reliable.** Every prompt is logged as "task 1" (the phase comes from whichever logger timer was running, and the timers were not used), and several were entered in a burst after the fact (P5's five prompts within 1.5 s; P2's four within 12 s). Which prompts were used is usable; *when* and *in what order* is not.
- **Row 5 times are whole minutes (1, 2, 3, 4, 4)**, typed by hand, not stopwatch readings. P4 and P5 at 4 min exceed the 2-minute task, so either the task ran long or "4" means "not within the task". Treat as coarse.
- **Blanks are blanks**, not "no": P4 row 3, P3 row 9, P2 row 12 yes/no, several of rows 17–21.
- **2 of 5 had seen the project before** (P4, P5).
- **No recordings**: all five declined recording consent (the "Recording consent" row), and none were made.
- **Headset, not desktop**: A1′ on the Quest means reaching with a real controller, so the 4/5 is not directly comparable with IP1's 0/5, which was simulator.

## Device (Quest 3S)

- 72 fps held in the app throughout the checks (logcat `VrApi FPS=72/72`). The headset keeps only its last few minutes of log, so `device/app-events-2026-09-25.txt` covers 15:55–15:56 only; earlier observations are from the live session (`../../ip2a/quest-sideload.md`).
- Pauses ("Resume / Quit" screen), matched to logger timestamps (headset clock ≈ 2 min ahead of the Mac): **Meta button pressed twice ≈ 15:19, during P3's session** (P3 prompts logged 15:21–15:23) — system-reserved, can't be blocked; **headset off 15:31–15:37, between P3 and P4** (proximity sleep; disabled from 15:38, so P4–P5 ran without it).
- Session times from the logger: P1 ≈ 14:37, P2 ≈ 14:59, P3 ≈ 15:21, P4 ≈ 15:40, P5 ≈ 15:50. P1 and P2's records were created on Thu 24 Sep (dry-run entries reused), filled in on the day.
- Fixed on the day, before the sessions: lamp, clock and preset frames only responded to physical touch on the headset; point + trigger now presses them (`edd1d07`).
