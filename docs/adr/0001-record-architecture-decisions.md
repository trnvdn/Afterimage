# ADR-0001: Record architecture decisions

- **Status:** Accepted
- **Date:** 2026-07-28

## Context
Afterimage is a learning project whose main value is the reasoning behind
each technical choice, not just the running code. Six months from now the
"why" behind a decision (Qdrant over pgvector, RabbitMQ over an outbox,
k3s over managed Kubernetes) is easy to forget, and a repository visitor
has no way to reconstruct it from the diff alone.

## Decision
We keep lightweight Architecture Decision Records in `docs/adr/`, one file
per decision, numbered sequentially (`NNNN-title.md`), using
[`template.md`](template.md). Every significant technical decision gets an
ADR. Superseding a decision means a new ADR that references the old one,
which is marked `Superseded by`.

## Consequences
- The reasoning behind the architecture is captured at the moment it is
  fresh, and stays greppable in the repo.
- A small amount of overhead per decision; trivial choices do not need an ADR.
- ADRs are immutable once accepted — we change direction by adding a new one,
  not by rewriting history.

## Alternatives considered
- **Wiki / external doc** — drifts from the code and is not versioned with it.
- **No records, rely on commit messages** — decisions get buried and are hard
  to find later.
