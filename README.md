# Afterimage

> What stays after you look away.

Self-hosted photo archive with semantic search. A pet project built to grow
toward senior-level engineering: AI/RAG, Kubernetes, observability, CI/CD.

Upload a photo and find it later by describing it — "dog on a beach",
"birthday cake", "red car at night" — instead of remembering a filename or a date.

## How it works

```
upload → MinIO (raw bucket) + Postgres row (Uploaded)
       → publish photo.uploaded to RabbitMQ
       → Worker: EXIF → thumbnail → afterimage-ai /embed-image
       → vector into Qdrant → status Processed → SignalR notifies the client
```

On failure a photo is marked `Failed` and retried via a dead-letter queue.

## Stack

| Area | Technology |
|---|---|
| API | ASP.NET Core 9, EF Core, JWT (ASP.NET Identity), SignalR |
| Worker | .NET Worker Service, RabbitMQ consumer |
| AI | Python, FastAPI, CLIP embeddings, Ollama |
| Storage | PostgreSQL (metadata), MinIO (files, S3-compatible), Qdrant (vectors) |
| Infra | RabbitMQ, OpenTelemetry, Serilog, Prometheus + Grafana |
| Deploy | Docker Compose → k3d + Helm → k3s on bare metal, ArgoCD |

## Repository layout

```
src/                     application services
  Afterimage.Api/        Web API
  Afterimage.Worker/     background processing
  afterimage-ai/         Python, ML (embeddings)
  Afterimage.Domain/     persistence model + data access (shared by Api & Worker)
  Afterimage.Shared/     cross-service message contracts
deploy/                  docker-compose, Helm charts, k8s manifests
tests/                   unit tests + evals/ (RAG evaluation)
docs/adr/                Architecture Decision Records
```

## Status

**Phase 1 — Docker Compose.** Week 1: project skeleton (Postgres, MinIO,
RabbitMQ via Compose), EF Core models, JWT auth. See
[`docs/roadmap.md`](docs/roadmap.md).

## License

See [`LICENSE`](LICENSE).
