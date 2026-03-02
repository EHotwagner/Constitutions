<!--
Sync Impact Report
- Version change: (template) → 1.0.0
- Modified principles:
  - N/A (initial constitution from template)
- Added sections:
  - Principle I: Spec-First Delivery
  - Principle II: Compiler-Enforced Structural Contracts
  - Principle III: Test Evidence Is Mandatory
  - Principle IV: Observability and Safe Failure Handling
  - Principle V: Scripting Accessibility
  - Principle VI: Comprehensive Documentation
  - Engineering Constraints (F#/.NET stack, FSI contracts, NuGet local
    packaging, scripting infrastructure, multi-language components)
  - Workflow and Quality Gates
  - Governance
- Removed sections:
  - None (initial population)
- Templates requiring updates:
  - ✅ reviewed: .specify/templates/plan-template.md (Constitution Check
    gate present; no changes needed — gate is dynamically filled)
  - ✅ reviewed: .specify/templates/spec-template.md (user-story structure
    compatible; no mandatory section additions required)
  - ✅ reviewed: .specify/templates/tasks-template.md (task categorization
    generic; FSI/baseline/scripting tasks will be added per-feature by
    /speckit.tasks at generation time)
- Follow-up TODOs:
  - None
-->

# Constitutions Project Constitution

## Core Principles

### I. Spec-First Delivery
Every non-trivial change MUST map to a current feature spec and
implementation plan before coding starts. Work items MUST remain traceable
from requirement to task to code. Implementation-only changes without
documented user value, acceptance criteria, and scope boundaries are
non-compliant.

Rationale: Spec-first execution reduces rework, keeps scope explicit, and
ensures decisions are reviewable.

### II. Compiler-Enforced Structural Contracts
Every public F# module MUST have a corresponding `.fsi` signature file that
declares its public API surface. The `.fsi` file serves as a structural
contract: the compiler MUST verify that the implementation (`.fs`) conforms
to its signature before the build succeeds. Any symbol omitted from the
`.fsi` file becomes module-private by design.

Surface area baselines MUST be maintained for public API modules. A
serialized snapshot of the public API surface MUST be stored as a baseline
file and validated by automated tests. Any divergence between actual API
surface and baseline MUST fail the build until the change is explicitly
reviewed and the baseline updated.

The verification model for this repository has three layers:
1. **Spec compliance** — human/AI review of requirements traceability
   (spec ↔ plan).
2. **Structural compliance** — compiler-enforced `.fsi` ↔ `.fs`
   conformance plus surface-area baseline tests.
3. **Behavioral compliance** — automated tests that exercise runtime
   behavior.

All three layers MUST pass before a behavior-changing PR is merged.

Rationale: F# signature files extend the spec-first philosophy into the
type system. The compiler becomes a structural quality gate that prevents
undocumented API drift, enforces encapsulation by default, and provides an
implicit implementation to-do list. Surface-area baselines catch accidental
public API changes the same way regression tests catch behavioral changes.

### III. Test Evidence Is Mandatory
Behavior-changing code MUST include automated tests that fail before the
fix/feature and pass after implementation. At minimum, each user story MUST
define independent verification criteria and corresponding test coverage
(unit, integration, or contract, as applicable). Unverified behavior
changes MUST NOT be merged.

Rationale: Mandatory test evidence prevents regressions and keeps delivery
confidence high across incremental feature work.

### IV. Observability and Safe Failure Handling
Operationally significant events (startup, subsystem failure, asset/IO
failure, recovery paths) MUST emit structured diagnostics with actionable
context. Errors MUST fail fast or degrade explicitly; silent failure and
swallowed exceptions are prohibited in critical paths.

Rationale: High-signal diagnostics and explicit failure semantics minimize
mean time to detect and recover from defects.

### V. Scripting Accessibility
F# projects that expose a public API MUST also be usable from F# Interactive
(FSI) scripting. Each such project MUST provide a prelude module
(`scripts/prelude.fsx`) that loads the compiled library and exposes
ergonomic helper functions for interactive use. The prelude MUST be loadable
with a single `#load` directive. Numbered example scripts
(`scripts/examples/NN-<topic>.fsx`) MUST accompany the prelude and cover
core API scenarios end-to-end. Example scripts MUST remain runnable against
the latest packed build; broken examples are treated as build defects.

Rationale: F# has first-class scripting support via FSI. Ensuring scripting
accessibility lowers the barrier to experimentation, enables rapid
prototyping, and serves as living documentation that is continuously
validated.

### VI. Comprehensive Documentation
API documentation MUST be generated from `.fsi` signature files using the
`/doc` skill. The `.fsi` files are the single source of truth for public
API documentation. XML doc comments (`///`) MUST be authored in the `.fsi`
file, not the `.fs` implementation file; the compiler emits these comments
into the assembly's XML documentation output, and documentation tooling
MUST consume that compiled output to produce developer-facing docs. Any
workflow that produces API documentation from implementation files or
hand-maintained documents instead of the `.fsi`-originated XML output is
non-compliant.

The `/doc` skill MUST be invoked to generate comprehensive documentation
whenever public API surface changes. Generated documentation MUST cover:
- Module-level summaries
- Type and member signatures with descriptions
- Usage examples drawn from scripting preludes and example scripts
- Cross-references between related modules

Rationale: Centralizing documentation authorship in `.fsi` files and
automating generation via `/doc` ensures docs stay synchronized with the
actual API, eliminates documentation drift, and provides a single
repeatable workflow for documentation updates.

## Engineering Constraints

- Primary Stack: F# on .NET is the default and required baseline for this
  repository.
- Multi-language components (e.g., HTML/JS/CSS for dashboards or
  visualization layers) are permitted when justified in the feature spec.
  Such components MUST:
  - Follow established best practices for their respective language/framework
    (semantic HTML, accessible markup, modern JS/TS patterns, CSS
    conventions).
  - Be isolated in clearly named directories (e.g., `dashboard/`,
    `web/`, `ui/`).
  - Include their own build/bundle configuration and linting rules.
  - Define a clear integration boundary with the F# backend (e.g., REST
    API, WebSocket protocol, or file-based data exchange).
  - Include basic tests appropriate to the technology (e.g., DOM tests for
    JS components).
- Any non-F# or non-.NET production component MUST be justified in the
  feature spec, approved during planning, and include interoperability and
  maintenance rationale.
- Every public `.fs` module MUST have a curated `.fsi` signature file
  checked into source control alongside it. Initial `.fsi` files MAY be
  generated via the `--sig` compiler flag and then curated to expose only
  the intended public surface.
- Surface-area baseline files MUST exist for each public module and be
  validated in CI. Changes to baselines MUST be reviewed as part of the PR
  that introduces them.
- Public API changes MUST document compatibility impact and migration
  guidance.
- Documentation MUST be generated from `.fsi` signature files using the
  `/doc` skill. XML doc comments (`///`) MUST be authored in the `.fsi`
  file. The `/doc` skill MUST be run after any public API surface change
  to regenerate documentation.
- Dependencies MUST be minimized; each new dependency requires a stated
  need, version pinning strategy, and maintenance owner.
- Every dotnet project that produces a library MUST be packable via
  `dotnet pack`. The resulting `.nupkg` MUST be output to the local NuGet
  store (`~/.local/share/nuget-local/`) so that other local projects and
  scripts can resolve the package without publishing to a remote feed. The
  `PackageOutputPath` property MUST be set in the project file or a
  `Directory.Build.props` to automate this. CI builds MUST also produce
  packages to this local store before running cross-project integration
  tests.
- F# projects with a public API MUST include a `scripts/prelude.fsx` file
  and at least one numbered example script under `scripts/examples/`. The
  prelude MUST load the compiled library via `#r` directives referencing
  the packed or built output. Example scripts MUST be validated as part of
  the CI pipeline or manual pre-merge checklist.

## Workflow and Quality Gates

1. `/speckit.specify` produces the feature spec with testable user stories.
2. `/speckit.plan` MUST pass Constitution Check gates before implementation
   begins.
3. `/speckit.plan` MUST define `.fsi` signature contracts for new or changed
   public modules as part of the structural design output.
4. `/speckit.tasks` MUST produce story-grouped tasks including required
   verification, `.fsi` creation/update tasks, surface-area baseline tasks,
   and scripting prelude/example tasks where applicable.
5. `/speckit.analyze` SHOULD be used before `/speckit.implement` for
   cross-artifact consistency checks.
6. `/doc` MUST be invoked after any public API surface change to regenerate
   comprehensive documentation from `.fsi` files.
7. Pull requests MUST include: linked spec/plan/tasks, test evidence, and
   updated `.fsi`/surface-area baselines when public API surface changes.
8. Multi-language components (HTML/JS dashboards) MUST pass their own
   linting and test gates before merge.

## Governance

This constitution is the authoritative governance source for engineering
and delivery workflow in this repository.

Amendment procedure:
- Propose changes via PR updating `.specify/memory/constitution.md` and
  any affected templates.
- Include rationale, migration impact, and a Sync Impact Report update.
- Approval requires maintainer review and explicit acknowledgment of
  downstream impact.

Versioning policy:
- MAJOR for incompatible governance changes or principle removals/
  redefinitions.
- MINOR for new principle/section additions or materially expanded
  obligations.
- PATCH for clarifications, wording refinements, and typo-level edits.

Compliance review expectations:
- Every plan MUST complete Constitution Check gates before implementation.
- Every PR review MUST verify applicable constitutional requirements and
  record exceptions explicitly.
- Periodic audits SHOULD verify templates and agent prompts remain aligned.

**Version**: 1.0.0 | **Ratified**: 2026-03-02 | **Last Amended**: 2026-03-02
