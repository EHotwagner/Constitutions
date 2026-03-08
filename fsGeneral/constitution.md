# Project Constitution

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
Documentation MUST be produced using the five specialized documentation
skills, each targeting a distinct concern:

1. **`fsdocs-setup`** — Initialize FSharp.Formatting for the project.
2. **`fsdocs-api-doc`** — Maintain XML doc comments (`///`) in `.fsi` signature files.
3. **`fsdocs-examples`** — Create literate F# scripts (`.fsx`) in `docs/`.
4. **`fsdocs-technical`** — Create Markdown files in `docs/` for architecture
   overviews, design decision records, and migration guides.
5. **`fsdocs-build`** — Build the documentation site and diagnose issues.

Documentation MUST cover:
- Module-level summaries and namespace documentation (via `fsdocs-api-doc`)
- Type and member signatures with descriptions (via `fsdocs-api-doc`)
- Executable usage examples organized by feature (via `fsdocs-examples`)
- Architecture overviews and design decision records (via `fsdocs-technical`)
- Cross-references between related modules and documentation pages

### VII. Inter-Project Communication
Projects governed by this constitution are purely F# on .NET. Other
languages MUST live in their own separate spec-kit projects. Communication
between projects MUST follow well-defined, contract-first protocols:

1. **Real-time communication** — MUST use gRPC. Service definitions MUST
   be implemented using the `fsgrpc-*` agent skills (`fsgrpc-setup`,
   `fsgrpc-proto`, `fsgrpc-server`, `fsgrpc-client`, `fsgrpc-codefirst`).
   Proto files or code-first contracts define the canonical interface.
2. **Non-real-time communication** — MUST use OpenAPI. An OpenAPI
   specification MUST be authored and versioned alongside the service.
   F# server implementations MUST generate or validate against the OpenAPI
   spec. Consumers MUST use generated clients derived from the spec.

Cross-project contracts (`.proto` files, OpenAPI specs) MUST be versioned,
reviewed, and treated as first-class artifacts subject to the same spec-first
workflow as code changes. Breaking changes to inter-project contracts MUST
include migration guidance and coordinated rollout plans.

Rationale: Strict language separation with protocol-based boundaries
enforces clean architecture, eliminates polyglot complexity within a single
project, and makes inter-project contracts explicit and testable.

## Engineering Constraints

- **F# on .NET is the exclusive stack.** No other languages are permitted
  within projects governed by this constitution. Multi-language needs MUST
  be addressed by separate projects communicating via gRPC or OpenAPI.
- Every public `.fs` module MUST have a curated `.fsi` signature file.
- Surface-area baseline files MUST exist for each public module.
- Public API changes MUST document compatibility impact and migration guidance.
- Dependencies MUST be minimized; each new dependency requires a stated
  need, version pinning strategy, and maintenance owner.
- Every dotnet project that produces a library MUST be packable via
  `dotnet pack`. The resulting `.nupkg` MUST be output to the local NuGet
  store (`~/.local/share/nuget-local/`).
- gRPC services MUST be set up using the `fsgrpc-setup` skill and
  implemented with `fsgrpc-server`/`fsgrpc-client`. Proto definitions
  MUST use `fsgrpc-proto` or `fsgrpc-codefirst` as appropriate.
- OpenAPI specs MUST be stored in the repository and validated in CI.
  Server endpoints MUST conform to the spec; clients MUST be generated
  from the spec.

## Workflow and Quality Gates

1. Specify — produce the feature spec with testable user stories.
2. Plan — MUST pass Constitution Check gates before implementation begins.
3. Plan MUST define `.fsi` signature contracts for new or changed public modules.
4. Tasks MUST produce story-grouped tasks including verification and `.fsi` tasks.
5. Analyze SHOULD be used before implementation for consistency checks.
6. Implement — execute tasks phase-by-phase.
7. After implementation completes, run documentation updates using
   `fsdocs-api-doc`, `fsdocs-examples`, `fsdocs-technical`, and `fsdocs-build`
   as applicable for any public API or behavioral changes.
8. Pull requests MUST include: linked spec/plan/tasks, test evidence, and
   updated `.fsi`/surface-area baselines when public API surface changes.

## Governance

This constitution is the authoritative governance source for engineering
and delivery workflow in projects that adopt it.

Amendment procedure:
- Propose changes via PR.
- Include rationale, migration impact.
- Approval requires maintainer review.

Versioning policy:
- MAJOR for incompatible governance changes or principle removals.
- MINOR for new principle/section additions or expanded obligations.
- PATCH for clarifications and wording refinements.

**Version**: 2.0.0
