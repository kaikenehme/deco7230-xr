# IP2b test day — runbook (Fri 23 Oct 2026, Studio)

Plan: `2026-10-23-ip2b-testing-plan.md` · Logger (iPad): https://claude.ai/artifact/MSM5HKKZdxh7xLpB1MV3RS · Device notes: `../ip2a/quest-sideload.md`

## Thursday night

| # | Do | Done when |
|---|---|---|
| 1 | Desktop lap in the editor: pull → fan → hold-up / toss → floor → curtains → furniture → clock / lamp / pendant → Bedroom → **Empty room** → back to Living | Nothing broken |
| 2 | Voice pass: research paragraph + plan wording, in your own words | — |
| 3 | `ai-acknowledgement.md`: student number, research-row `[TODO]` | `python3 ip2b/build-pdfs.py` exits 0 (no TODO warnings) |
| 4 | Commit · merge `ip2b` → `main` · tag `ip2b-test` · push `main` + tag | `git describe --tags` = `ip2b-test` |
| 5 | iPad: open the logger, tap **+ New**, check the pill says **Saved**, then **Clear** | Pill green |
| 6 | Charge Mac + iPad. Pack USB-C data cable (not charge-only) + wall charger | — |

## Friday timeline

| Time | Where | What |
|---|---|---|
| 10:00–11:50 | Forgan Smith 01-W458 | INFS7203 |
| by 13:30 | anywhere | APK built from the tag (step 1 below) — unless built Thursday night |
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

# 1. Build (~7 min; the Unity editor must be closed). Skip if ip2b.apk was built from the tag already.
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

Quick lap on the headset (2 min): pull from the sofa → fan → hold-up on a wall → toss → floor → curtains → **Empty room frame** → Living frame. Then restart the app.

## Each participant

| When | Do |
|---|---|
| Before | Restart app: `"$ADB" shell am force-stop $PKG && "$ADB" shell monkey -p $PKG 1` · logger **+ New** |
| Consent yes | `ip2b/record-session.sh P1` (P2, P3…) — close the window to save |
| Phases | Logger timers: Free look → Task 1 → Task 2 → Empty room → Interview |
| Between | Headset on the wall charger if battery < 40 % (`"$ADB" shell dumpsys battery \| grep level`) |

## If something goes wrong

| Symptom | Fix |
|---|---|
| `adb devices` → `unauthorized` | Headset on, accept the dialog, tick **Always allow**; `"$ADB" kill-server && "$ADB" devices` |
| "Resume / Quit" mid-session | Meta button pressed, or headset slept → Resume; re-run the `prox_close` line |
| Participant stuck in another room | Touch the Living frame (Wall_N, behind the start position) |
| Build stuck at "Compiling Scripts", Unity at 0 % CPU | iCloud eviction. `kill -9` every Unity / `bee_backend` / `netcorerun` process, run step 0's `rm -rf`, rebuild |
| Build fails | Read the end of `ip2b/apk-build.log`. The 20 Oct `ip2b.apk` predates the final fixes — last resort only, and say so in the results |

## After the last participant

1. `"$ADB" shell am broadcast -a com.oculus.vrpowermanager.automation_disable` (undo `prox_close`), unplug, return the Quest.
2. Ask Claude: "export the IP2b logger into `testing-data/ip2b/`" (JSON per participant + README table, as for IP2a).
3. **Blackboard:** `ip2b/IP2b-Testing-Plan-Kaike-Nehme.pdf` (has the research summary) + `ip2b/IP2b-AI-Acknowledgement-Kaike-Nehme.pdf`.
4. Commit `testing-data/ip2b/` (not recordings — git-ignored) and push, before leaving the room.
