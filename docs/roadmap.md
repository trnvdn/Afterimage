# Roadmap

> Week-by-week plan. This is a stub — fill in details as the project grows.

## Phase 1 — Docker Compose
Everything runs locally via `docker compose`.

- **Week 1** — skeleton: Docker Compose (Postgres, MinIO, RabbitMQ), EF Core
  models, JWT auth.
- **Week 2** — _TBD_
- ...

Target release: `v0.1.0` — basic CRUD + upload works.

## Phase 2 — Kubernetes
k3d + Helm locally, then k3s on real hardware, ArgoCD for GitOps.

Target release: `v1.0.0` — full stack in k8s with observability.

## Phase 3 — Bare metal
Run on real hardware, harden, observe.

---

_See [`adr/`](adr/) for the reasoning behind individual technical decisions._
