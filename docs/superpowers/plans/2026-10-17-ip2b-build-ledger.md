# SDD ledger — plan: docs/superpowers/plans/2026-10-17-ip2b-build.md
Setup: Ruling: work on branch `ip2b` in place, not a git worktree — a worktree needs a fresh 8.8 GB Library reimport with ~37 GB disk free; Kaike said "build it yourself, go" — cost if wrong: branch can be merged/rebased later, no loss.
Pre-flight: T1 produces ids → T2 (curtain/pendant/lamp instances) + T3 placements: consistent.
Pre-flight: T3 Step 6 defers ApplyPreset_ResetsCurtainsAndFan to T8: consistent (T8 owns it).
Pre-flight: T4 produces Surface.IsPaintTarget → T5? no; T6/T10 HoldUpPreviewer/TossedChip use IsPaintTarget: consistent.
Pre-flight: T5 FanDeck consumed by T8 test (FindObjectsByType<FanDeck>): consistent.
Pre-flight: T8 PresetApplier.Apply resets; T9 adds BeforeApply event on PresetApplier: both modify Apply — T9 must keep T8's reset. Noted.
Setup: Ruling: room_mock_v2.blend (120 MB) kept local + gitignored — over GitHub's 100 MB file limit — cost if wrong: mock-up only lives on this Mac; renders are committed.
Task 0: complete (commits 6bf493c..4b17056, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh EditMode → EditMode total=171 passed=171 failed=0)
Task 1: Ruling: CREDITS.md lines written by CatalogueImporter (ImportLocalFurniture), not hand-appended — Import() regenerates CREDITS.md from scratch — cost if wrong: none.
Task 1: Ruling: woven_pendant cord cut, shade scaled to 0.41 m; Unity draws the cord (T2) — Sketchfab model was 0.18 m shade on 1.2 m cord, would hang to the floor at real scale — cost if wrong: T2 must add a cord.
Task 1: Ruling: added LocalFurniture_RealWorldSize test (not in plan) — guards the scale the plan specifies — cost if wrong: none.
Task 1: Ruling: menu thumbnails show piece backs (thumb camera on -Z, pieces face +Z as presets expect) — pre-existing IP2a behaviour, not touched — cost if wrong: menu chips show backs.
Task 1: complete (commits 4b17056..fdb7fed, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh EditMode → EditMode total=175 passed=175 failed=0)
Task 2: Ruling: window view = Poly Haven ballawley_park tonemapped JPG, crop of the sunny lawn third, 2048x1024 — plan said "sunny garden HDRI" — cost if wrong: swap one jpg.
Task 2: Ruling: clock moved x -1.6 → -2.15 (plan said stays at -1.6) — Curtain_L spans x -1.69..-0.71 and would cover it — cost if wrong: none.
Task 2: Ruling: rig start (0.9, 0, 1.3) yaw 235 (SceneBuilder.StartYaw) facing the sofa corner — plan did not pin the start pose for the new room — cost if wrong: one constant.
Task 2: Ruling: added IP2b snapshot views 20–25 to SnapshotTests now (plan had them in T11) — needed to check each task by eye — cost if wrong: none.
Task 2: Ruling: Presets_Layout_NoOverlap fails until T3 (old 9x7 presets in the 5.5x5 room) — T3 rewrites presets — cost if wrong: none if T3 lands.
Task 2: complete (commits fdb7fed..fef7568, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh PlayMode 'SceneWiringTests.Room_IsLivingRoomSized;SceneWiringTests.Window_HasGardenView_AndTwoCurtains;SceneWiringTests.Shell_HasBeamArtAndPendant;SceneWiringTests.Lamp_IsArcLamp_InSouthWestCorner;SceneWiringTests.PresetFrames_OnWallN_Wired;SceneWiringTests.Room_IsRoomWByRoomD_AndFloorTeleports;SceneWiringTests.Lamp_IsBulbOnly_StartsWarm_WithPullCord_AndSchemeCyclerStillWired' → PlayMode total=7 passed=7 failed=0)
Task 3: Ruling: tv_unit = TV joined onto the slatted stand as one catalogue piece — FurnitureSlot spawns on the floor, a separate TV can't sit on the unit — cost if wrong: none (slatted_tv_stand + flat_tv stay in the catalogue).
Task 3: Ruling: rug is a shell Surface (Keep, kind None), not a preset piece — a slot under the table fails the no-overlap layout test — cost if wrong: same rug position in both rooms.
Task 3: Ruling: bedroom table lamp merged onto one nightstand (bed_nightstand_lamp) — same floor-spawn reason — cost if wrong: none.
Task 3: Ruling: Bedroom licences: skipped evermore3d "Nightstand" (CC-BY-NC) for Melonpolygons (CC-BY) — spec allows CC-BY/Free Standard only — cost if wrong: slightly less mid-century nightstand.
Task 3: Ruling: paints — "Sandy Day" walls (Living), teal Wall_W via new PresetAuthoring.PaintHex("#2F6B6A") — Natural White / Vivid White / Teal Waters not in the 24-colour catalogue — cost if wrong: none.
Task 3: Ruling: updated IP2a assertions "floor kept" → floor Change, and "shell kind set" exempts Keep sources (rug) — spec N3 / N1 — cost if wrong: none.
Task 3: Ruling: bedroom render shown to Kaike (opened ip2b/art/bedroom_v1.png); building on with these picks per spec §4.2 — cost if wrong: swap pieces later.
Task 3: complete (commits fef7568..d1cee13, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh EditMode → EditMode total=180 passed=180 failed=0)
Task 4: Ruling: onboarding hero = largest furniture footprint (OnboardingSequence.Hero) instead of "first slot in preset order" — FurnitureSlot doesn't store preset index; largest = sofa/bed in both rooms — cost if wrong: a different piece pulses.
Task 4: Ruling: removed IP2a tests Tick_ChangeSurface_AlwaysHidden and "not kept → not a source"; replaced by N1 tests — they asserted the retired kept/change rule — cost if wrong: none.
Task 4: Ruling: pull tab now also shows on walls/floor near either hand (incl. the hand holding a sample) — spec N1 says any surface; suppression not specified — cost if wrong: extra tab flicker during hold-up; revisit on the desktop lap (T11).
Task 4: complete (commits d1cee13..437bfa1, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh EditMode → EditMode total=181 passed=181 failed=0)
Task 5: Ruling: chips use ITouchable (other hand's controller cue via OnTriggerEnter, right-hand ray via MenuSelectRelay, desktop click) instead of an XRSimpleInteractable — matches lamp/clock/frames, avoids a second XRI select path fighting the sample's grab — cost if wrong: left-hand ray can't pick chips (left hand can still touch them).
Task 5: Ruling: Radius 0.12/Spread 100 → 0.16/120 — 5 cm cards overlapped badly in the snapshot — cost if wrong: one constant.
Task 5: Ruling: Sample's default colour = Palette[2] (first non-white) and FanDeck.Build picks 2 — plan says so — cost if wrong: none.
Task 5: Ruling: desktop scroll no longer twists anything (twist removed); no replacement binding — cost if wrong: facilitator uses clicks on chips.
Task 5: complete (commits 437bfa1..f7221e7, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh EditMode → EditMode total=207 passed=207 failed=0)
Task 6: Ruling: floor tests check the oak texture survives a commit (not CommittedMaterial) — presets set the floor's base override, CommittedMaterial stays null until a menu pick — cost if wrong: none; production code unchanged (tint rule already worked).
Task 6: complete (commits f7221e7..ad3a394, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh PlayMode 'SceneWiringTests.Floor_TakesTintOverMaterial;SceneWiringTests.Floor_MaterialSwapKeepsTint;SceneWiringTests.HoldUp_NearFloor_PreviewsFloor' → PlayMode total=3 passed=3 failed=0)
Task 7: Ruling: IP2a FurnitureGrabTests re-aim check now waits 0.6 s instead of 5 frames — the piece eases toward the target by design (Follow) — cost if wrong: none.
Task 7: complete (commits ad3a394..e02caa0, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh EditMode FurnitureSlotTests → EditMode total=22 passed=22 failed=0)
Task 8: Ruling: curtains toggle on touch (cue/ray/click) and animate the draw, rather than grab-and-drag along the rail — plan pinned touch; desktop mode can't drag along a rail — cost if wrong: less physical draw.
Task 8: Ruling: closed panels widen ×1.2 so two 0.97 m drapes cover the 2.2 m window — cost if wrong: none.
Task 8: complete (commits e02caa0..fdf9f98, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh PlayMode 'SceneWiringTests.Curtains_TouchClosesAndDimsDaylight_ThenOpens;SceneWiringTests.ClosedCurtains_SurviveTimeOfDayChange;SceneWiringTests.ApplyPreset_ResetsCurtainsAndFan' → PlayMode total=3 passed=3 failed=0)
Task 9: Ruling: AR Foundation 6.1.1 + meta-openxr 2.1.1 (not 6.0.x/2.0.x) — 2.1.1 is the newest that matches our OpenXR 1.14.x without an OpenXR upgrade — cost if wrong: package swap.
Task 9: Ruling: plan step 3 (XR Simulation run in the editor) deferred to the T11 lap / Friday device check — needs the GUI editor + a simulation environment; headless tests cover wiring and toggling — cost if wrong: first real MR run is Friday morning (already the plan's stated risk; VR fallback intact).
Task 9: Ruling: MR frame floats 0.9 m in front of the head when MR is on (the Wall_N it hangs on is virtual) — cost if wrong: frame position in the real room.
Task 9: Ruling: headless package resolve wedged twice (orphaned bee_backend/netcorerun from a killed run); fixed by killing all Unity build processes and clearing Library/Bee — cost if wrong: none; note for future sessions.
Task 9: Ruling: MrWiringTests sets LogAssert.ignoreFailingMessages while MR is on in the editor (no AR provider loaded) — cost if wrong: an AR error in the editor would not fail that one test.
Task 9: complete (commits fdf9f98..d01f660, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh EditMode MrModeTests → EditMode total=10 passed=10 failed=0)
Task 10: Ruling: added Velocity_UsesWholeWindow / Velocity_TooFewSamples / Threshold_IsInclusive tests (not in plan) — pin the window average and edge cases — cost if wrong: none.
Task 10: complete (commits d01f660..6f83c2a, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh PlayMode TossTests → PlayMode total=2 passed=2 failed=0)
Task 11: Ruling: desktop lap (step 4) left to Kaike — needs a human at the GUI editor; all automated checks run (full suites, snapshots by eye) — cost if wrong: feel issues found Mon/Tue instead of now.
Task 12: Ruling: logger saved as ip2b/ipad-logger.html but not yet published as a claude.ai artifact — publishing creates a shareable URL; Kaike should decide — cost if wrong: one publish call.
Task 11: complete (commits 6f83c2a..4a079fc, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh PlayMode → PlayMode total=42 passed=42 failed=0)
Task 12: complete (commits 6f83c2a..4a079fc, tests: .superpowers/sdd/2026-10-17-ip2b-build/ut.sh EditMode → EditMode total=234 passed=234 failed=0)
Final review: opus code-reviewer, range 0a3df2a..4a079fc (code-only diff). 3 Critical, 4 Important, 6 Minor.
Final: fixed C1 no USE_SCENE permission request — ScenePermission_IsMetaUseScene RED→GREEN (request at Start + on MR on; planes enable once granted), suite 242/42
Final: fixed C2 MR plane MeshCollider ClosestPoint broken — PlaneBox_ClosestPointLiesOnThePlane + PlanePrefab_UsesBox_NotMeshCollider RED→GREEN (thin BoxCollider fitted on boundaryChanged), suite 242/42
Final: fixed C3 passthrough black with HDR + intermediate texture — Urp_HdrOff_ForPassthrough + Urp_IntermediateTexture_Auto_ForPassthrough RED→GREEN (ProjectConfigurator.SetupPassthroughGraphics), suite 242/42
Final: fixed I1 cancelled select committed/tossed old sample — Canceled_AlwaysDiscards RED→GREEN (Decide(..., canceled) + Revert), suite 242/42
Final: fixed I2 pull swallowed ray chip-picks/prop touches — Trigger_RayOnTouchable_DoesNotPull RED→GREEN (SamplePuller.ShouldPull, MenuSelectRelay.PullTakesTrigger), suite 242/42
Final: fixed I3 MR frame faced away — Toggle_RestoresFramePose (+X faces user) RED→GREEN, suite 242/42
Final: fixed I4 curtains flipped while holding a chip by the window — Curtain_IgnoresHandHoldingASample RED→GREEN, suite 242/42
Final: minor (deferred): preset switch with curtains closed snaps daylight 4%→100% (PresetApplier SetOpen instant) — fade instead
Final: minor (deferred): DynamicGI.UpdateEnvironment every frame of the 1 s curtain fade — update env only at fade end
Final: minor (deferred): per-frame/per-pull material instances (MrPlaneSurface rend.material, FanDeck chips, TossedChip) never destroyed
Final: minor (deferred): toss trail includes the 0.15 s attach ease-in — quick let-go right after a pull can read as a toss
Final: minor (deferred): SceneBuilder (Editor) compiles against AR Foundation/MR — deleting MR/ breaks the editor build; isolate behind #if or an editor asmdef
Final: minor (deferred): hiding a held FurnitureSlot when MR turns on leaves LocomotionLock acquired
Final: Ruling: declined-to-judge items (XRI forced select, white pull from MR planes, chip cue gap, ClampToFloor centre, licences, LogAssert) — accepted as the reviewer reasoned; white MR pull is real but harmless after C2 (only within 0.35 m of a real wall) — cost if wrong: a white sample when pulling from a real wall.
