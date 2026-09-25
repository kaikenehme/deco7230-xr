# Quest sideload — the hardware track

**Why this file exists:** on Fri 28 Aug the borrowed Quest sat at `adb devices → unauthorized` all day; no USB-debugging dialog ever appeared in the headset, and IP1 ran on the simulator. IP2a *should* run on device; IP2b *must* (grade cap otherwise). Solve it in Week 7, not Week 11.

## Checklist for Fri 5 Sep studio (ask staff first)

1. **Is the headset managed?** UQ-managed Quests can have developer mode locked. Ask studio staff which account owns it and whether dev mode is enabled in the Meta Horizon app for that account.
2. **Developer mode** must be on for the *paired* account (Horizon app → Devices → headset → Developer Mode). Without it, no USB dialog.
3. **Revoke and retry:** in the headset, Settings → System → Developer → *Revoke USB debugging authorisations*; on the Mac `adb kill-server && adb devices`; put the headset on — the "Allow USB debugging?" dialog appears *inside* the headset, and only while it is worn and awake.
4. **Cable:** a data cable, not a charge-only one. Try a second cable before blaming anything else.
5. **Meta Quest Developer Hub** (MQDH) on the Mac shows the device state and can toggle dev mode if the account allows it.
6. Confirm: `adb devices` shows `device`, then:

```bash
ADB="/Applications/Unity/Hub/Editor/6000.0.80f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb"
"$ADB" devices
"$ADB" install -r ip2a/RenovationPreviewer/Builds/ip2a.apk
"$ADB" shell monkey -p com.kaikenehme.renovationpreviewer 1     # launch
"$ADB" logcat -s Unity VrApi | head -50                           # FPS + errors
```

## Build

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.0.80f1/Unity.app/Contents/MacOS/Unity"
PROJ="$PWD/ip2a/RenovationPreviewer"
"$UNITY" -batchmode -quit -projectPath "$PROJ" -buildTarget Android -executeMethod BuildScript.BuildAndroid -logFile /tmp/apk.log
```
~7 min. Only build for a device session; the simulator needs nothing.

## Perf to check on device (Week 8)

Soft shadows at 2048 / 15 m / 1 cascade, 24 furniture prefabs, transparent glass. Target 72 fps steady. Fallbacks in order: hard shadows → 1024 map → shadows off at the 19:00 and 22:00 stops.

## Log

| Date | What happened |
|---|---|
| 2026-08-28 | `unauthorized` all day, no in-headset dialog even with dev settings open. Suspect managed device. |
| 2026-09-25 | **Working.** Quest 3S (`340YC10G6L046X`) authorised over USB. It dropped back to `unauthorized` twice (headset asleep / adb restart) until "Always allow from this computer" was ticked. `BuildScript.BuildAndroid` headless: 72 MB APK in 3 min 30 s, 0 errors. First launch shows the headset's "controller required" dialog → Continue. On device: **72/72 fps**, no Unity errors in logcat. Mirror: `brew install scrcpy`, run with Unity's adb so the two adb versions don't fight: `ADB="$ADB" scrcpy --no-audio --max-fps=30 --video-bit-rate=8M --stay-awake`. (QuestLiveView DMG not used: unsigned, Gatekeeper rejects it.) |
| 2026-09-25 | "Resume / Quit" screen mid-session, two causes read from logcat: (1) the **Meta button** on the right controller (`SystemButtonHandler: setHomeButtonDownState` → app loses focus) — system-reserved, apps can't block it; warn participants. (2) **Headset taken off** (`sys.hmt.mounted=0`) → sleep → pause. Fix for (2): `adb shell am broadcast -a com.oculus.vrpowermanager.prox_close` (permanent until reboot; undo: `... automation_disable`). Battery fell 71 % → 28 % in ~1 h on the Mac's USB port while "AC powered": use a wall charger between participants. |
