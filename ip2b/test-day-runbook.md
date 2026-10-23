# IP2b test day — runbook (Fri 23 Oct 2026, Studio)

Plan: `2026-10-23-ip2b-testing-plan.md` · Logger (iPad): https://claude.ai/artifact/MSM5HKKZdxh7xLpB1MV3RS · Device notes: `../ip2a/quest-sideload.md`

## Friday morning, before INFS at 10:00 (moved from Thursday night)

Already done Thu 22 Oct: tests 241/43 green · `ip2b` merged → `main`, tagged `ip2b-test`, pushed · APK built from the tag (20:33).

**Fri 23 Oct morning:** desktop lap passed (0 exceptions). Lap fix — no glass reflection, grass outside the window — tests 241/43 green, **re-tagged `ip2b-test` (`6917ef9`) and APK rebuilt 10:31** (`Builds/ip2b.apk`). Old tag `ip2b-test-pre-window`; old APK kept as `Builds/ip2b-fallback.apk`.

| # | Do | Done when |
|---|---|---|
| 1 | Desktop lap in the editor: pull → fan → hold-up / toss → floor → curtains → furniture → clock / lamp / pendant → Bedroom → **Empty room** → back to Living | Nothing broken |
| 2 | Voice pass: research paragraph + plan wording, in your own words | — |
| 3 | `ai-acknowledgement.md`: student number, research-row `[TODO]` | `python3 ip2b/build-pdfs.py` exits 0 (no TODO warnings) |
| 4 | Commit + push the rebuilt PDFs and doc edits (ask Claude). A **code** fix from step 1 also means re-tag + rebuild the APK (~20 min with tests) — finish by 13:30 | `git status` clean, pushed |
| 5 | iPad: open the logger, tap **+ New**, check the pill says **Saved**, then **Clear** | Pill green |
| 6 | Charge Mac + iPad. Pack USB-C data cable (not charge-only) + wall charger | — |

## Friday timeline

| Time | Where | What |
|---|---|---|
| before 10:00 | home | Morning checklist above (lap, voice pass, TODOs, PDFs, iPad) |
| 10:00–11:50 | Forgan Smith 01-W458 | INFS7203 applied class (nothing due) |
| 11:50–13:30 | anywhere | Only if the lap needed a code fix: re-tag + rebuild with Claude |
| 13:00 | | DECO7381 Team Project Final due (other course, 100 % late penalty) |
| 14:00 | **78-208** (studio) | Quest in hand → steps 2–3, quick lap, first participant by ~14:15 |
| 15:50 | | Studio ends — results pushed + Blackboard submitted before leaving |

8 min per participant + ~1 min reset ≈ 6 participants in 55 min, alongside testing peers' prototypes.

## Friday, before the first participant

```bash
cd "$HOME/Desktop/University of Queensland/Semester 4/Digital_Prototype"
UNITY=/Applications/Unity/Hub/Editor/6000.0.80f1/Unity.app/Contents/MacOS/Unity
ADB=/Applications/Unity/Hub/Editor/6000.0.80f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb
PKG=com.kaikenehme.renovationpreviewer

git status --short && git describe --tags             # clean tree, ip2b-test

# 0. iCloud check: the Desktop syncs to iCloud, and evicted cache files hang the compiler (17 + 22 Oct)
find ip2b/RenovationPreviewer -flags +dataless | wc -l  # must be 0. If not:
#   rm -rf ip2b/RenovationPreviewer/{Library/Bee,Library/ScriptAssemblies,Library/PackageCache,Library/PackageManager,.utmp}
#   (PackageCache and PackageManager go together, or packages never come back. Needs internet; adds ~5 min.)

# 1. Build — SKIP: built from ip2b-test on Thu 22 Oct. Only rerun after a code fix + re-tag (~7 min, editor closed).
"$UNITY" -batchmode -quit -projectPath "$PWD/ip2b/RenovationPreviewer" -buildTarget Android \
  -executeMethod BuildScript.BuildAndroid -logFile "$PWD/ip2b/apk-build.log"
grep "Build result" ip2b/apk-build.log                # Succeeded, errors 0

# 2. Headset: boundary set, worn, USB in, accept "Allow USB debugging" + tick "Always allow"
"$ADB" devices                                        # <serial>  device
"$ADB" install -r ip2b/RenovationPreviewer/Builds/ip2b.apk
"$ADB" shell am broadcast -a com.oculus.vrpowermanager.prox_close   # no sleep when the headset comes off
"$ADB" shell monkey -p $PKG 1                         # launch; "controller required" → Continue

# 3. Mirror (live view only, for the lap)
ip2b/record-session.sh
```

Quick lap on the headset (2 min): pull from the sofa → fan → hold-up on a wall → **toss with a real arm swing** (desktop could only drop it) → floor → curtains → walk up to the **window** (grass, no reflection) → **Empty room frame** → Living frame. Then restart the app.

**Let go = squeeze the side grip and release** (the sample stays in the hand without holding anything). Prompt for a stuck participant: "Squeeze the side button and let go."

## Each participant

| When | Do |
|---|---|
| Position | Participant on the start spot, facing the sofa → **hold the Meta button ~2 s** (recenter). A tap only opens the pause menu |
| Before | Restart app: `"$ADB" shell am force-stop $PKG && "$ADB" shell monkey -p $PKG 1` · logger **+ New** (first participant: tap **+ New** too — the logger holds two empty test records, so numbering may start at P3; that's fine) |
| Numbering | **Use the P-number the logger shows** for the recording too (`record-session.sh P3` if the logger says P3), so the video, the event log and the logger record match. Empty records are skipped in the results |
| Consent yes | `ip2b/record-session.sh P1` (P2, P3…) — close the window to save. Also saves `P1_…_events.log` (the app's hold-up / toss commits, timestamped) and prints the counts: cross-check the logger's B3 tally |
| Phases | **Start each logger timer** as the phase starts: Free look → Task 1 → Task 2 → Empty room → Interview (IP2a didn't, so prompt phases were unusable) |
| Prompt 5 | Say it as: "Hold the card near a wall, then **squeeze and release the side grip**." (the logger text only says "let go") |
| Interview | After B3 (row 20) ask: "When you held it up, could you tell how the colour looked in the whole room?" → Observer notes. Also note if they stepped back after a hold-up |
| Between | Headset on the wall charger if battery < 40 % (`"$ADB" shell dumpsys battery \| grep level`) |

## If something goes wrong

| Symptom | Fix |
|---|---|
| `adb devices` → `unauthorized` | Headset on, accept the dialog, tick **Always allow**; `"$ADB" kill-server && "$ADB" devices` |
| "Resume / Quit" mid-session | Meta button pressed, or headset slept → Resume; re-run the `prox_close` line |
| Spawned **outside the room** | Hold the Meta button ~2 s to recenter; else restart the app standing mid-boundary; else redraw the boundary |
| `unauthorized` that never clears, no Revoke option in Developer | UQ-managed headset — swap it (23 Oct: `…G6M009N` locked; `…G6L04R5` works) |
| "Controller required" won't accept a click | Controllers belong to another headset — use this headset's pair (serials in `dumpsys OVRRemoteService`) |
| Another student's app opens instead | `"$ADB" shell am force-stop <their.package>`, then launch ours |
| Participant stuck in another room | Touch the Living frame (Wall_N, behind the start position) |
| Build stuck at "Compiling Scripts", Unity at 0 % CPU | iCloud eviction. `kill -9` every Unity / `bee_backend` / `netcorerun` process, run step 0's `rm -rf`, rebuild |
| Build fails | Read the end of `ip2b/apk-build.log`. Fallback: `Builds/ip2b-fallback.apk` (22 Oct, before the window fix — only the window differs) |
| New APK misbehaves on the headset | `"$ADB" install -r ip2b/RenovationPreviewer/Builds/ip2b-fallback.apk` — same features, old window. Note it in the results |

## After the last participant

1. `"$ADB" shell am broadcast -a com.oculus.vrpowermanager.automation_disable` (undo `prox_close`), unplug, return the Quest.
2. Ask Claude: **"export the IP2b logger"**. Claude reads the logger database into `testing-data/ip2b/logger-export/participants/`, copies the event logs (`cp testing-data/ip2b/recordings/*_events.log testing-data/ip2b/device/`), runs `python3 testing-data/ip2b/build-results.py` → `testing-data/ip2b/README.md` (raw table, B1–B6 counts, UEQ-S + VRSQ scores, five-principles evidence).
3. Five minutes with Claude: write the **five-principles verdicts** and the **data-quality notes** into the README from your observer notes (the brief asks for the principles evaluation, including any that don't apply).
4. Commit `testing-data/ip2b/` (videos stay out — git-ignored) and push, **before leaving the room**.
5. **Blackboard:** `ip2b/IP2b-Testing-Plan-Kaike-Nehme.pdf` (has the research summary) + `ip2b/IP2b-AI-Acknowledgement-Kaike-Nehme.pdf`.
