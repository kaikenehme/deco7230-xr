# IP2b change — Empty room replaces the MR showcase

**20 Oct 2026 · branch `ip2b` · test Fri 23 Oct (Studio, Quest 3S, hurdle)**
Amends `2026-10-17-ip2b-design.md` (N7 MR showcase is withdrawn). Everything else in that spec stands.

## Why

MR cannot be tried before Friday: there is no headset until the test, and the editor has no passthrough. On 19 Oct Kaike's desktop lap found that switching to MR hides the room shell (floor included) while the rig's `GravityProvider` keeps running. The rig falls (y = −11 981 m after a minute), and switching back to VR leaves it below the room. On the Quest the "Your room" frame would stay where it was put while the participant fell away from it, so they could not get back without restarting the app. Kaike's call: scanning a real room with no chance to test it is too risky on a hurdle. Replace it with an **empty room**.

**Intent (Kaike):** the empty room works like the other rooms, but bare, so participants are "as free as possible to create something they are imagining".

## Goals

| # | Goal |
|---|---|
| G1 | A third room, **Empty**: bare walls and floor, nothing else to look at; same 5.5 × 5 m shell |
| G2 | Reached like the other rooms: a third **preset frame** on Wall_N ("Empty room"), and `F3` for the facilitator (desktop only) |
| G3 | Same mechanics as Living and Bedroom: pull → fan → hold-up / toss, floor tint, floor material swap, curtains, clock, pendant, menu, schemes |
| G4 | Colours come from the **menu catalogue**. Once a wall is painted, pulling from it opens the fan deck as usual |
| G5 | **MR removed completely**: no "Your room" frame, no AR session / camera / plane managers, no `USE_SCENE` permission request at launch, no `M` key, Meta MR OpenXR features off |
| G6 | Docs follow: testing plan, research paragraph, iPad logger, CLAUDE.md, AI log |
| G7 | EditMode + PlayMode green; snapshots of the empty room checked by eye |
| G8 | **One headless Android build, no install** (Kaike's go, 20 Oct), after G1–G7. Confirms the branch builds for Android and the merged manifest has no `USE_SCENE` permission. The APK is not installed; Friday morning still builds fresh from the tagged commit |

## Non-goals

- Building a new way to add furniture. **Correction (final review, 20 Oct):** the floor menu's Add tab (from IP2a) already adds furniture and works in the Empty room. It stays available, which fits "as free as possible", and Phase 4 logs whether it was used.
- Any new interaction.
- The arc lamp in the empty room (light control there = curtains, clock, pendant).

## Anti-goals (a failure even if it works)

- **Any behaviour change in Living or Bedroom.** They are what Friday's Phases 1–3 run on. Their look, layout, sources and tests stay identical.
- Uninstalling packages or changing XR settings beyond switching the Meta MR features off. AR Foundation 6.1.1 + Unity OpenXR: Meta 2.1.1 stay installed, unused. The VR config must match what ran on the Quest for IP2a (OpenXR, Meta Quest feature, Touch controller profile).
- Anything that makes the empty room look broken or unfinished. "Bare" means a clean, lit, plain room, not a grey box.

## Decided (Kaike, 19–20 Oct)

| Topic | Decision |
|---|---|
| What's in it | Bare walls (plain white) and floor (plain light boards). **No** furniture, rug, prints, arc lamp |
| What stays | Window + garden view, curtains + rail, wall clock, ceiling pendant (+ its switch), door, preset frames |
| Colour source | Menu catalogue; fan deck from any painted surface |
| Entry | Third preset frame + `F3`. Start room stays Living (preset 0) |
| MR | Removed, including the launch permission prompt |
| Plan | Phase 4 (MR) → empty-room phase; B6 (MR) → new exploratory assumption about free creation |
| Research | Swap the MR sentence and its 2 MR references (Milgram & Kishino 1994; de Souza & Tartz 2024) for sources on open-ended / creative tools; **keep ≥ 6 references** (rubric minimum is 5 → 0 if under) |

## Left to the builder

- **How room-level dressing hides per room.** Rug, the two prints and the arc lamp are built by `SceneBuilder` as part of the room, not by presets, so `PresetApplier` has to show/hide them per preset (e.g. a flag or list on `RoomPreset`). A hidden rug must drop out of `Surface.All`, so it can't be pulled from. Living/Bedroom must show them exactly as now.
- **The ceiling beam:** keep (architecture, not decoration). *Claude's call.*
- **Light on entering Empty:** Empty opens in **daylight** (default time of day; curtains already reopen on every room switch). Done with a per-preset setting that only Empty uses, so switching between Living and Bedroom keeps today's behaviour (time of day carries over). Otherwise Phase 4 would open in Phase 3's evening light, with no arc lamp. *Claude's call.*
- **Pulling from an unpainted wall:** stays allowed, as everywhere today. A white wall gives its fan (two whites + greys from `LightnessLadder`). Blocking it would mean a per-room check in the pull code that Living and Bedroom share (`SamplePuller`, `PullAffordance`, `HandGlow`), which breaks the first anti-goal. The logger records the colour source (menu vs fan) per commit, so B6 can still be read. *Claude's call.*
- **Pendant height:** unchanged (shade bottom ≈ 1.75 m). In Empty nothing stops a tall participant from walking under it. Moving it for Empty only is new per-room code for a cosmetic clip. Noted in the plan's Known limits; the facilitator can steer people round it. *Claude's call.*
- **Frame layout for three frames on Wall_N:** keep clear of the door (x ≥ 1.5) and the NW monstera in Living; existing clearance tests (`SceneWiringTests`) are the guard.
- **Exact white and floor boards:** pick from the catalogue (a plain white paint; the lightest plain wood floor). Card for the frame via `PresetCard.Render` like the others.
- **MR removal mechanics:** delete `Scripts/MR/` (+ tests), `MrWiringTests`, `MrPlane` prefab, `BuildMr` and the `KeepOutOfShell`/`RoomShell` parenting only if nothing else needs them; drop the `Renovation.MR` reference from the PlayMode test asmdef; replace `ProjectConfigurator.EnableMetaMr` with switching those three features off. `PresetApplier.BeforeApply` may stay (harmless) or go.
- **Passthrough graphics settings** (HDR off, intermediate texture Auto, set for MR on 17 Oct): leave as they are. HDR off is the usual Quest setting and is what Kaike has been reviewing since 17 Oct. *Claude's call.*
- **Tests:** a test per goal where it's logic (preset count = 3, empty preset spawns no furniture, dressing hidden in Empty and shown in Living/Bedroom, no MR components in the built scene, Meta MR features off). Snapshot views of the empty room added to `SnapshotTests`.
- **Docs (drafts for Kaike's voice pass):**
  - Testing plan Phase 4 becomes **"Empty room (1 min)"**: *"Touch the frame that says 'Empty room'. Make the room you're imagining."* B6 *(exploratory)* becomes *"In a bare room, the same tools let people make the room they imagine"*, measured by *"How close is this to the room you had in mind?"* 1–5 + why, plus colour source (menu vs fan). Total stays about 7 min. Remove MR lines from Setup, Known limits and the five-principles table.
  - Logger: B6 field and Phase 4 label follow the plan.
  - Research: new sources verified the same way as the other 7 (Crossref / publisher page).

## Out of scope this week

XR Simulation check, the plan PDF, merge to main (separate step after this lands).
