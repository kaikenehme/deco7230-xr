# SDD ledger — plan: docs/superpowers/plans/2026-10-20-empty-room.md
Setup: Ruling: work in place on branch ip2b (no worktree) — a worktree copies the Unity project and needs a full Library reimport (3–8 GB, disk was 98% full; ~1 h) — cost if wrong: none to main; ip2b is the feature branch, not main.
Spec: docs/superpowers/specs/2026-10-20-empty-room-design.md (read).
Pre-flight: T1 produces RoomPreset.bare / TimeOfDayController.StartIndex / PresetApplier.dressing,timeOfDay; T2 consumes PresetApplier (removes BeforeApply) and SceneBuilder (removes BuildMr) — same files, sequential edits, no signature conflict. T3 consumes T4's G8 result for CLAUDE.md — Ruling: write CLAUDE.md state after T4 (T3 step 4 finalised in T4 step 5) — cost if wrong: none.
Task 1: Ruling: logger Phase 4 colour source logged as one seg (Menu/Fan/Both/Nothing) not per-commit tallies — 1-minute phase, matches existing r8 pattern — cost if wrong: less granular B6 colour-source data.
Task 1: complete (commits 618af95..56c3379, tests: .superpowers/sdd/2026-10-20-empty-room/run.sh t1done edit play → play exit=0 total="45" passed="45" failed="0" )
Task 2: Ruling: QuestVrFeatures_StillOnForAndroid passed in RED (plan expected 3 fails, got 2) — it guards existing IP2a config (anti-goal), so passing before the change is correct; MetaMr + NoArFoundation failed as expected — cost if wrong: none.
Task 2: Ruling: DisableMetaMr also calls EditorUtility.SetDirty on each feature (EnableMetaMr didn't) so the flip persists in batchmode — cost if wrong: none.
Task 2: Ruling: OpenXRPackageSettings shows 4 Android flips, not 3 — the 4th is MetaOpenXR-OpenXRLifeCycle (hidden, nameUi empty), a dependency of the Meta AR features that Unity disables with them; IP2a ran without the meta-openxr package at all, so off = IP2a VR setup — cost if wrong: if Quest VR needed it, Friday's first device run shows it (Meta Quest feature + Touch profile verified still on by QuestVrFeatures_StillOnForAndroid).
Task 2: complete (commits 56c3379..fe3c995, tests: .superpowers/sdd/2026-10-20-empty-room/run.sh t2done edit play → play exit=0 total="43" passed="43" failed="0" )
Task 2: note: range 56c3379..fe3c995 includes the Task 3 docs commit fe3c995 (committed while task-done ran); Task 2's own commit is dab8673.
Task 3: complete (commits dab8673..fe3c995, tests: .superpowers/sdd/2026-10-20-empty-room/docs-check.sh → docs-check: refs=7 fail=0)
Task 4: complete (commits fe3c995..a8f317e, tests: .superpowers/sdd/2026-10-20-empty-room/t4-check.sh → apk: no USE_SCENE)
Final review: opus reviewer, verdict "With fixes", 0 Critical, 2 Important, 7 Minor.
Final: fixed #1 frame brush wipes current room — PresetFrame_TouchOnCurrentRoom_DoesNotWipeIt RED→GREEN, suite 241/241 + 43/43
Final: fixed #2 add-furniture exists in Empty room (spec premise wrong) — kept available, logged (logger r17b, plan B6/Phase 4, spec correction); docs-check pass
Final: Ruling: #4 plan/logger colour-source mismatch re-graded Minor→Important (facilitator gets contradicting instructions on hurdle day) — fixed in the same doc edit — cost if wrong: none
Final: Ruling: #8 research overclaim re-graded Minor→Important (research paragraph is its own rubric criterion; CLAUDE.md: don't overclaim) — reworded — cost if wrong: none
Final: Ruling: #1 fix changes Living/Bedroom too (touching the current room's frame no longer resets it) — accepted: it only removes an accidental wipe; nothing relies on it (app restarts between participants; F1–F3 call Apply directly) — cost if wrong: facilitator can't reset a room by touching its frame on device (restart app instead)
Final: minor (deferred): #3 menu can stay open on the hidden rug after switching to Empty (ControllerMenu.cs:137 checks null only)
Final: minor (deferred): #5 orphan Assets/Materials/YourRoomCard.mat left from BuildMr
Final: minor (deferred): #6 QuestVrFeatures_StillOnForAndroid checks two features on, not the exact enabled set
Final: minor (deferred): #7 PresetAuthoring.Preset() doesn't reset bare
Final: minor (deferred): #9 stale frame-range comment PresetAuthoring.cs:26; unused using SceneBuilder.cs:9
Final: Ruling: declined-to-judge lines (MR cut vs brief, HDR off kept, pendant height, pull from unpainted wall, frames moved, held furniture/toss during switch, reference aptness) all stand as spec/Kaike calls — cost if wrong: see spec.
