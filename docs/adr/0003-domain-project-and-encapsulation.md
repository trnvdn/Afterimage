# ADR-0003: Domain project and persistence encapsulation

- **Status:** Proposed
- **Date:** 2026-07-28

## Context
Both `Afterimage.Api` and `Afterimage.Worker` operate on the same persistence
types (the `Photo` entity, the `DbContext`). Those types need one home that
both can depend on, without duplicating the model or leaking EF configuration
into every consumer.

## Decision
A separate class library **`Afterimage.Domain`** encapsulates persistence.
`Afterimage.Api` and `Afterimage.Worker` depend on it. (This is why it is a
dedicated project rather than living in one service: it is *shared* by API and
Worker.)

Access modifiers are strict:
- **Entities (`Photo`):** `public` class, `public get` / `private set` (or
  `init`) properties. State mutations happen only through domain methods.
- **DbContext:** `public`, injected directly — no custom repository wrappers
  (EF `DbContext`/`DbSet` already is the unit-of-work + repository).
- **EF configurations (`IEntityTypeConfiguration`):** `internal`, applied via
  assembly scanning.
- **DI registration:** a `public static` extension method on
  `IServiceCollection` inside the Domain project.

## Consequences
- The Worker takes an EF Core dependency (it must update state / metadata) —
  accepted, it genuinely needs DB access.
- `Afterimage.Domain` is a new project not yet listed in the repo structure
  (README / CLAUDE.md) — update those to include it, and state the boundary
  vs `Afterimage.Shared` (Shared = cross-service message contracts, Domain =
  persistence model + data access).

## Alternatives considered
- **Custom repository pattern over EF** — rejected; adds a wrapper over an
  abstraction that is already a repository/unit-of-work.
- **Model duplicated per service** — rejected; drift between Api and Worker.
