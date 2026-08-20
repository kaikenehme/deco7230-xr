# IP1 — Running AI-use log

**Purpose:** IP1 is a secure assessment — every AI use must be declared (UQ Library
acknowledgement table format). This file is the raw running log, kept as work happens.
It gets consolidated into the final acknowledgement table + Statement of Originality
by Thu 27 Aug (see plan Task 15). Under-declaring is the risk; log everything.

**Tool used:** Claude Code (Anthropic, model Fable 5), driven interactively by Kaike Nehme.

| Date | What AI did | Files touched | Human role |
|---|---|---|---|
| 2026-08-21 | Wrote IP1 implementation plan from concept spec v1.1 | `docs/superpowers/plans/2026-08-21-ip1-unity-build.md` | Concept, architecture (§8), scope and all design decisions predate this and are Kaike's (see concept doc revision history) |
| 2026-08-21 | Scaffolded Unity project via CLI (`-createProject`), authored `Packages/manifest.json` package list | `ip1/RenovationPreviewer/` | Directed the work; verified environment |
| 2026-08-21 | Implemented all IP1 C# scripts from concept §8 architecture (Surface, Sample, HarmonyPalette, HarmonyTuner, SchemeManager, LampController, MarkTool, SamplePuller, HoldUpPreviewer, SchemeCycler), EditMode + PlayMode test suites, ProjectConfigurator + SceneBuilder + BuildScript editor automation | `ip1/RenovationPreviewer/Assets/` | Architecture, signatures and behaviours specified in Kaike's concept doc §8–9; AI translated to Unity C#; Kaike to review and play-test |
| 2026-08-21 | Drafted IP1 testing plan + data collection sheet from concept §10 protocol | `ip1/2026-08-28-ip1-testing-plan.md`, `ip1/data-collection-sheet.md` | Protocol and measures authored by Kaike in concept v1.1; AI reformatted to brief p.3 template |
| 2026-08-21 | Drafted statement-of-originality scaffold (flagged for Kaike's personal rewrite) | `ip1/statement-of-originality.md` | Must be personally reviewed, edited, signed by Kaike before submission |
