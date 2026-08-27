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
| Claude Code (Claude Fable 5) | Planning: turned my concept spec and design decisions into implementation plans and a design spec for the controller-menu addition | "Write an implementation plan from the concept doc"; "add a controller menu without breaking the diegetic loop" | `docs/superpowers/` plans + specs | 21–25 Aug 2026 |
| Claude Code (Claude Fable 5) | Code generation: all C# in the Unity project — runtime scripts implementing the architecture from my concept §8, EditMode/PlayMode test suites, and the editor automation that configures the project and builds the scene | "Implement the architecture in concept §8 with tests" and follow-up feature requests | `ip1/RenovationPreviewer/Assets/` | 21–27 Aug 2026 |
| Claude Code (Claude Fable 5) | Debugging and iteration: diagnosed and fixed issues I found in simulator rehearsals (menu readability, ray feedback, menu click-through, room size, mouse capture) | Bug reports in my words, e.g. "the walls don't change when I click" | `Assets/Scripts/**` | 25–27 Aug 2026 |
| Claude Code (Claude Fable 5) | Documents: reformatted my testing protocol into the brief's template, drafted the data sheet, statement-of-originality scaffold (rewritten by me), and this coversheet with its PDF | "Draft the testing plan from concept §10"; "make the acknowledgement a PDF" | `ip1/*.md`, `ip1/*.pdf` | 21–27 Aug 2026 |
| Claude Code (Claude Fable 5) | Tooling and builds: Unity project setup for Quest, editor bridges for AI-assisted testing, headless test runs, Android APK builds | "Set up the Unity project"; "get everything ready for tomorrow" | project config, `Builds/` | 21–27 Aug 2026 |
| Claude Code (Claude Fable 5) | Research: licence-clean furniture, material and paint-colour sources (Poly Haven and ambientCG CC0, Dulux public colour data) | "Find licence-safe furniture, materials and paint colours" | `Assets/Catalogue/**` | 25 Aug 2026 |


**Not AI:** the concept and all design decisions (Weeks 1–3), the cardboard low-fi prototype,
the Week 2 studio testing, all in-class testing tomorrow, and the analysis of its results.
