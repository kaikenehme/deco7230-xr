#!/usr/bin/env python3
"""Build testing-data/ip2b/README.md from the IP2b session logger export.

Inputs (next to this script):
  logger-export/participants/p*.json   one record per participant (iPad logger, db collection `participants`)
  device/P*_events.log                 the app's own log per participant (record-session.sh), optional

Usage:  python3 testing-data/ip2b/build-results.py [folder]
Counts are raw. Interpretation belongs to Evaluation 3, not here.
"""
import json, re, sys
from datetime import datetime
from pathlib import Path

BASE = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).resolve().parent
LOGGER = "https://claude.ai/artifact/MSM5HKKZdxh7xLpB1MV3RS"

# Logger rows in session order: (key, row label, measure). Mirrors ip2b/ipad-logger.html.
ROWS = [
    ("platform", "—", "Platform"),
    ("seen", "—", "Seen this project before"),
    ("consent", "—", "Recording consent"),
    ("r1", "1", "Reached for first"),
    ("r2", "2", "Noticed the sofa/bed pulse"),
    ("r3", "3", "Pulled a sample (free look)"),
    ("r3b", "3b", "Pulled from"),
    ("prompts", "4", "Prompts used (phase)"),
    ("r5ms", "5", "Time to first commit"),
    ("r6", "6", "Opened a fan deck"),
    ("r7", "7", "Cards picked before first commit"),
    ("rh", "8a", "Commits by hold-up (logger tally)"),
    ("rt", "8b", "Commits by toss (logger tally)"),
    ("dev_h", "8a′", "Commits by hold-up (app log)"),
    ("dev_t", "8b′", "Commits by toss (app log)"),
    ("r8", "8", "Kept wall colour came from"),
    ("r9", "9", "Floor changed by"),
    ("r10", "10", "Closed the curtains"),
    ("r11", "11", "Clock used · stops"),
    ("r12", "12", "Used the lamp"),
    ("r13", "13", "A light change changed a colour"),
    ("r14", "14", "Furniture placed first try"),
    ("r15", "15", "Switched to the Bedroom"),
    ("r16", "16", "Entered the Empty room"),
    ("r17", "17", "Empty room: painted with"),
    ("r17b", "17b", "Empty room: added furniture"),
    ("r18", "18", "“How close to the room you had in mind?” 1–5"),
    ("r18b", "18b", "…why"),
    ("r19", "19", "Fan: helpful or restrictive, why"),
    ("r20", "20", "Hold it up or throw it, why"),
    ("r21", "21", "“A room I could make mine” 1–5"),
    ("r21w", "21b", "…why"),
    ("r22", "22", "One thing that confused them"),
    ("notes", "—", "Observer notes (incl. B3 step-back / overview answers)"),
]
UEQ = [f"u{i}" for i in range(1, 9)]
VRSQ = [f"v{i}" for i in range(1, 10)]


def load():
    parts = sorted((BASE / "logger-export" / "participants").glob("p*.json"),
                   key=lambda p: int(re.sub(r"\D", "", p.stem) or 0))
    recs = [json.loads(p.read_text()) for p in parts]
    # Leftover test records (opened, never filled) are not participants.
    answers = [k for k, _, _ in ROWS if k not in ("platform", "seen", "consent", "dev_h", "dev_t")] + UEQ + VRSQ
    skipped = [r for r in recs if all(blank(r.get(k)) or r.get(k) == 0 for k in answers)]
    if skipped:
        print("Skipped empty records: " + ", ".join(f"p{r.get('n')}" for r in skipped))
    recs = [r for r in recs if r not in skipped]
    for r in recs:
        r.setdefault("n", int(re.sub(r"\D", "", str(r.get("n", 0))) or 0))
        ev = sorted((BASE / "device").glob(f"P{r['n']}_*_events.log"))
        if ev:
            text = "".join(e.read_text(errors="replace") for e in ev)
            r["dev_h"] = text.count("commit holdup")
            r["dev_t"] = text.count("commit toss")
            r["dev_err"] = len(re.findall(r"Exception|E/Unity", text))
    return recs


def blank(v):
    return v is None or v == "" or v == [] or (isinstance(v, str) and not v.strip())


def cell(r, k):
    v = r.get(k)
    if k == "prompts":
        return " → ".join(f"{p.get('n')} ({p.get('phase', '?')})" for p in v) if v else "none"
    if k == "r5ms":
        if blank(v): return "—"
        s = round(v / 1000); return f"{s // 60}:{s % 60:02d}"
    if k == "r11":
        stops = r.get("r11stops")
        return f"{v or '—'} · {stops} stops" if stops else (v or "—")
    if k in ("dev_h", "dev_t"):
        return "—" if v is None else str(v)
    if blank(v): return "—"
    return " ".join(str(v).split()).replace("|", "/")


def num(v):
    try: return float(str(v).split()[0].replace("+", ""))
    except (ValueError, IndexError): return None


def mean(xs):
    xs = [x for x in xs if x is not None]
    return round(sum(xs) / len(xs), 2) if xs else None


def ueq(r):
    vals = [num(r.get(k)) for k in UEQ]
    if any(v is None for v in vals): return None
    return mean(vals[:4]), mean(vals[4:]), mean(vals)


def vrsq(r):
    vals = [num(r.get(k)) for k in VRSQ]
    if any(v is None for v in vals): return None
    o = sum(vals[:4]) / 12 * 100          # Kim et al. (2018): oculomotor = items 1–4
    d = sum(vals[4:]) / 15 * 100          # disorientation = items 5–9
    return round(o, 1), round(d, 1), round((o + d) / 2, 1)


def tally(recs, k, value):
    hits = [f"P{r['n']}" for r in recs if r.get(k) == value]
    return f"{len(hits)} / {len(recs)}" + (f" ({', '.join(hits)})" if hits and len(hits) < len(recs) else "")


def answered(recs, k):
    got = [r for r in recs if not blank(r.get(k))]
    return f"{len(got)} / {len(recs)} answered"


def main():
    recs = load()
    if not recs:
        sys.exit(f"No records in {BASE / 'logger-export/participants'} — export the logger first.")
    n = len(recs)
    heads = " | ".join(f"P{r['n']}" for r in recs)
    L = []
    L += [f"# IP2b testing data — Fri 23 Oct 2026, Week 12 studio", "",
          f"**Status:** raw results from the iPad session logger ({LOGGER}), generated by `build-results.py` on "
          f"{datetime.now():%d %b %Y %H:%M}. Not analysed — that is Evaluation 3 (due Wed 28 Oct).", "",
          f"**n = {n}.** Platform per participant is in the first row. "
          "Plan: `../../ip2b/2026-10-23-ip2b-testing-plan.md` · Raw records: `logger-export/participants/` · "
          "App event logs (hold-up / toss commits, timestamped): `device/`. Videos are participant data and stay off GitHub.", "",
          "## Results as logged", "",
          f"| Row | Measure | {heads} |", "|---|---|" + "---|" * n]
    for k, row, label in ROWS:
        if k.startswith("dev_") and all(r.get(k) is None for r in recs):
            continue
        L.append(f"| {row} | {label} | " + " | ".join(cell(r, k) for r in recs) + " |")

    # Questionnaires
    L += ["", "## Questionnaires", "",
          f"| Scale | {heads} | Mean |", "|---|" + "---|" * n + "---|"]
    u = [ueq(r) for r in recs]
    v = [vrsq(r) for r in recs]
    for i, name in enumerate(["UEQ-S pragmatic (−3…+3)", "UEQ-S hedonic (−3…+3)", "UEQ-S overall"]):
        vals = [x[i] if x else None for x in u]
        L.append(f"| {name} | " + " | ".join("—" if x is None else f"{x:+.2f}" for x in vals) + f" | {'—' if mean(vals) is None else f'{mean(vals):+.2f}'} |")
    for i, name in enumerate(["VRSQ oculomotor (0–100)", "VRSQ disorientation (0–100)", "VRSQ total (0–100)"]):
        vals = [x[i] if x else None for x in v]
        L.append(f"| {name} | " + " | ".join("—" if x is None else f"{x:.1f}" for x in vals) + f" | {'—' if mean(vals) is None else f'{mean(vals):.1f}'} |")
    L += ["", "UEQ-S: pragmatic = items 1–4, hedonic = items 5–8 (Schrepp et al., 2017). "
          "VRSQ: oculomotor = items 1–4 ÷ 12 × 100, disorientation = items 5–9 ÷ 15 × 100, total = their mean (Kim et al., 2018). "
          "A participant with any item missing gets no score."]

    # Counts against the assumptions
    fan_openers = [r for r in recs if r.get("r6") not in (None, "", "No")]
    hold = sum(int(r.get("rh") or 0) for r in recs); toss = sum(int(r.get("rt") or 0) for r in recs)
    dev = [r for r in recs if r.get("dev_h") is not None]
    dev_line = (f" · app log: hold-up {sum(r['dev_h'] for r in dev)}, toss {sum(r['dev_t'] for r in dev)} (P{', P'.join(str(r['n']) for r in dev)})"
                if dev else "")
    L += ["", "## Counts against the assumptions (raw, before analysis)", "",
          "| Assumption | What was logged | Count |", "|---|---|---|",
          f"| **B1** pull unprompted | Pulled in free look, no prompt (row 3) | **{tally(recs, 'r3', 'Unprompted')}** · IP2a 4/5 · IP1 0/5 |",
          f"| B1 | Noticed the pulse (row 2) | {tally(recs, 'r2', 'Yes')} |",
          f"| **B2** choose from the fan | Kept wall colour from the fan deck (row 8) | **{tally(recs, 'r8', 'Fan deck')}** · menu {tally(recs, 'r8', 'Menu')} · both {tally(recs, 'r8', 'Both')} |",
          f"| **B2** | Helpful / restrictive asked of everyone who opened a fan (row 19) | {sum(1 for r in fan_openers if not blank(r.get('r19')))} of {len(fan_openers)} fan openers answered |",
          f"| B2 | Opened a fan (row 6) | unprompted {tally(recs, 'r6', 'Unprompted')} · after a prompt {tally(recs, 'r6', 'After a prompt')} |",
          f"| **B3** hold-up vs toss | Commits (rows 8a/8b) | logger: hold-up **{hold}**, toss **{toss}**{dev_line} |",
          f"| **B3** | Preference + why (row 20) | {answered(recs, 'r20')} — verbatim in the table |",
          f"| B3 | Step-back / whole-room overview (observer notes, added 23 Oct) | see observer notes row |",
          f"| **B4** a room I could make mine | Rating 1–5 (row 21) | {', '.join(cell(r, 'r21') for r in recs)} |",
          f"| **B5** light changes a decision | Curtains closed unprompted (row 10) | {tally(recs, 'r10', 'Unprompted')} |",
          f"| **B5** | A light change changed a colour (row 13) | **{tally(recs, 'r13', 'Yes')}** |",
          f"| B6 (exploratory) | Entered the Empty room (row 16) | {tally(recs, 'r16', 'Yes')} |",
          f"| B6 | “How close to the room you had in mind?” 1–5 (row 18) | {', '.join(cell(r, 'r18') for r in recs)} |",
          f"| B6 | Painted with (row 17) | fan {tally(recs, 'r17', 'Fan deck')} · menu {tally(recs, 'r17', 'Menu')} · both {tally(recs, 'r17', 'Both')} |"]

    # Five design principles (the brief asks for this alongside the user testing)
    v_cols = list(zip(*[x for x in v if x])) if any(v) else None
    eyestrain = [cell(r, "v3") for r in recs]
    L += ["", "## Five design principles — evidence from the session", "",
          "The brief asks for an evaluation against the five principles (Week 6), including any that do not apply. "
          "Evidence columns are filled from the logger; the verdict is written after the session from the observer notes.", "",
          "| Principle | Evidence logged | Verdict |", "|---|---|---|",
          "| Text readability | Observer notes (squinting, leaning in to read the frame labels) | |",
          "| Ergonomics | Observer notes (arm held up > 10 s, crouching for the floor) · furniture placed first try (row 14): "
          f"{tally(recs, 'r14', 'Yes')} | |",
          f"| Brightness changes | VRSQ eyestrain (item 3): {', '.join(eyestrain)} · observer notes (flinch at curtains) | |",
          "| Avoiding simulator sickness | VRSQ totals above" + (f" (mean {mean(list(v_cols[2]))})" if v_cols else "") + " · teleport only, no smooth locomotion | |",
          "| Button placement | Observer notes (wrong button, \"which button?\", stuck holding a sample: let go = grip) | |",
          "", "## Data quality — read before quoting any of this", "",
          "- Write here: phase timers used or not (prompt phases depend on them), anyone who skipped a phase, platform changes, "
          "logger rows filled in after the fact, and any mismatch between the logger tally and the app log for hold-up / toss.", ""]
    out = BASE / "README.md"
    out.write_text("\n".join(L))
    print(f"Wrote {out} — n = {n}")


if __name__ == "__main__":
    main()
