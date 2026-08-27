# AI Acknowledgement — Interactive Prototype 1

**Name:** Kaike Nehme · **Student number:** [student number]
**Course:** DECO7230 — Digital Prototyping and Extended Reality
**Assignment:** Interactive Prototype 1 (Fri 28 Aug 2026, Week 5 Studio)

**AI use:** ☑ Yes, AI tools have been used to generate or edit material in this assessment.

All AI use was Claude Code (Anthropic, model Claude Fable 5), driven interactively by
Kaike Nehme in supervised terminal sessions. Prompts were conversational; the "Prompt(s)"
column summarises the direction given. The raw running log kept during development is
`ip1/ai-use-log.md` (committed with full history). The design — concept, interaction loop,
constraint model, no-menus rule, phasing, testing objectives and protocol — is Kaike's own
work, documented with revision history in `concept/2026-08-07-xr-renovation-concept-design.md`
and predating all rows below (see `ip1/statement-of-originality.md`).

| Tool | Use | Prompt(s) | Section | Date |
|---|---|---|---|---|
| Claude Code (Claude Fable 5) | Planned: turned my concept spec v1.1 into a task-by-task Unity implementation plan | "Write an implementation plan for IP1 from the concept doc" | `docs/superpowers/plans/2026-08-21-ip1-unity-build.md` | 21 Aug 2026 |
| Claude Code (Claude Fable 5) | Generated: scaffolded the Unity 6 project from the CLI, authored the package manifest (URP, XRI 3.1.2, OpenXR, Input System), configured URP/XR/Android build settings via editor scripts | "Set up the Unity project for Quest per the plan" | `ip1/RenovationPreviewer/` project + `ProjectSettings/` | 21 Aug 2026 |
| Claude Code (Claude Fable 5) | Generated: implemented the C# runtime scripts to my §8 architecture (Surface, Sample, HarmonyPalette, HarmonyTuner, SchemeManager, LampController, MarkTool, SamplePuller, HoldUpPreviewer, SchemeCycler), plus EditMode/PlayMode test suites and editor automation (ProjectConfigurator, SceneBuilder, BuildScript) | "Implement the architecture in concept §8 with tests" | `Assets/Scripts/**`, `Assets/Editor/**` | 21 Aug 2026 |
| Claude Code (Claude Fable 5) | Edited: reformatted my §10 testing protocol into the brief's p.3 template; drafted the data-collection sheet | "Draft the testing plan and data sheet from concept §10" | `ip1/2026-08-28-ip1-testing-plan.md`, `ip1/data-collection-sheet.md` | 21 Aug 2026 |
| Claude Code (Claude Fable 5) | Drafted: statement-of-originality scaffold, flagged for my personal rewrite before submission | "Scaffold the statement of originality" | `ip1/statement-of-originality.md` | 21 Aug 2026 |
| Claude Code (Claude Fable 5) | Tooling: installed editor bridges so Claude Code could drive the open Unity Editor (MCP for Unity v10; Unity CLI skill; `com.unity.pipeline`) — editor-only, not shipped in the build | "Install the Unity bridge / CLI tooling" | `Packages/manifest.json` | 21–25 Aug 2026 |
| Claude Code (Claude Fable 5) | Researched: licence-clean asset and colour sources (Poly Haven CC0, ambientCG CC0, Dulux public colour data; IKEA rejected on ToS grounds) | "Find licence-safe furniture, materials and paint colours" | `Assets/Catalogue/**` | 25 Aug 2026 |
| Claude Code (Claude Fable 5) | Planned + generated: design spec and implementation for the controller-menu pivot (menu additive to the hold-up loop): Catalogue, Surface material preview/commit, FurnitureSlot, ControllerMenu/SwatchButton/MenuSelectRelay, CatalogueImporter, SceneBuilder wiring, tests | "Add a controller menu without breaking the diegetic loop" | `docs/superpowers/specs/2026-08-25-ip1-controller-menu-design.md`, `Assets/Scripts/**` | 25 Aug 2026 |
| Claude Code (Claude Fable 5) | Debugged + generated: diagnosed "walls not changing" (missing aim feedback, not a logic bug); rebuilt menu look (UiKit v2), added ray reticle + hover glow, furniture thumbnails, simulator auto-spawn; fixed the select ray hitting the rig's own CharacterController | "The walls don't change when I click — fix it"; "make the menu readable in VR" | `Assets/Scripts/Runtime/{UiKit,SwatchButton,ControllerMenu,RayFeedback,RayUtil}.cs` | 25 Aug 2026 |
| Claude Code (Claude Fable 5) | Generated: 5×4 m parametrised room (RoomSpec), floor teleport target, start pose facing the sofa; PlayMode test | "Make the room bigger and easier to move around" | `Assets/Scripts/Runtime/RoomSpec.cs`, `Assets/Editor/SceneBuilder.cs` | 25 Aug 2026 |
| Claude Code (Claude Fable 5) | Edited: testing plan v2 (menu + assumption A3), data sheet rows 13–15 + prompt 5, results scaffold | "Fold the menu comparison into the testing plan" | `ip1/2026-08-28-ip1-testing-plan.md`, `ip1/data-collection-sheet.md`, `testing-data/ip1/` | 25 Aug 2026 |
| Claude Code (Claude Fable 5) | Debugged + generated: fixed menu clicks selecting the wall behind the panel (RayUtil MenuPanel exemption); hardened reticle rotation against degenerate normals (Invalid-AABB Editor spam); unit tests for both | "Clicking a chip changes the wall behind the menu" | `Assets/Scripts/Runtime/{RayUtil,RayFeedback}.cs`, tests | 25 Aug 2026 |
| Claude Code (Claude Fable 5) | Verified + built: ran the full headless test pipeline (EditMode 61/61, PlayMode 8/8), regenerated the scene, built the final Android APK; added an editor-only simulator cursor-lock script; confirmed furniture grab existed and made its discoverability a test measure (plan v3, sheet row 16); consolidated this acknowledgement table from the running log and produced its PDF | "Get everything ready for tomorrow's IP1"; "contain the mouse in the simulation"; "grab and drag the furniture" | whole `ip1/` tree, this file | 27 Aug 2026 |

**Not AI:** the concept and all design decisions (Weeks 1–3), the cardboard low-fi prototype,
the Week 2 studio testing, all in-class testing tomorrow, and the analysis of its results.
