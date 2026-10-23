#!/bin/zsh
# Live view of the Quest on this Mac, recorded to a video file for one participant.
#   ip2b/record-session.sh P1        -> testing-data/ip2b/recordings/P1_2026-10-23_1432.mp4 (+ _events.log)
#   ip2b/record-session.sh           -> live view only, nothing recorded
# Records the headset view and the headset microphone (think-aloud). Only start a recording
# after the participant has agreed to it. Close the window (or Ctrl+C) to stop; the file is saved on close.
# Recordings are participant data: git-ignored, never pushed.

ADB=/Applications/Unity/Hub/Editor/6000.0.80f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb
export ADB   # scrcpy must use Unity's adb, or the two adb versions fight and the Quest asks for authorisation again

if [[ "$("$ADB" devices | sed -n 2p | awk '{print $2}')" != "device" ]]; then
  echo "Quest not ready: $("$ADB" devices | sed -n 2p)"
  echo "Plug in the USB cable, put the headset on and allow USB debugging (tick 'Always allow')."
  exit 1
fi

args=(--no-audio-playback --max-fps=30 --video-bit-rate=8M --stay-awake --window-title "Quest 3S — live")

if [[ -n "$1" ]]; then
  dir="${0:A:h}/../testing-data/ip2b/recordings"
  mkdir -p "$dir"
  stamp=$(date +%Y-%m-%d_%H%M)
  file="$dir/${1}_${stamp}.mp4"
  events="$dir/${1}_${stamp}_events.log"
  echo "Recording $1 -> $file"
  # The app's own log, timestamped: every hold-up / toss commit (B3 tally) and any error.
  # Runs beside the video; if it fails, the video still records.
  "$ADB" logcat -c 2>/dev/null
  "$ADB" logcat -v time -s Unity:I > "$events" 2>/dev/null &
  logpid=$!
  scrcpy "${args[@]}" --audio-source=mic --record="$file"
  kill $logpid 2>/dev/null
  echo "Saved: $file ($(du -h "$file" | cut -f1))"
  echo "Events: $events  (hold-up $(grep -c 'commit holdup' "$events" 2>/dev/null) · toss $(grep -c 'commit toss' "$events" 2>/dev/null))"
else
  scrcpy "${args[@]}" --no-audio
fi
