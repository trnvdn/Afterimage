# ADR-0004: DTOs and object mapping

- **Status:** Proposed
- **Date:** 2026-07-28

## Context
Domain entities (`Photo`) must not be exposed directly over the API. They need
mapping to transport types, and the mapping approach should be safe and cheap.

## Decision
- **DTOs:** `public record` types in the API layer.
- **Mapping:** **Mapperly** (source generator). Mapping code is generated at
  compile time — no runtime reflection, and mapping mismatches surface as
  compile errors. Mapper definitions are `internal` in the API layer.
- Data access stays direct on `DbContext` / `DbSet`; no repository wrappers
  (see [ADR-0003](0003-domain-project-and-encapsulation.md)).

## Consequences
- Compile-time-checked, allocation-light mapping; mismatched members fail the
  build instead of silently at runtime.

## Alternatives considered
- **AutoMapper** — rejected: moved to a commercial license (2025) and maps via
  runtime reflection, which is heavier and only fails at runtime. Mapperly is a
  better fit for a portfolio codebase demonstrating current practice.
- **Hand-written mapping** — viable and dependency-free, but Mapperly removes
  the boilerplate while keeping the compile-time safety.
