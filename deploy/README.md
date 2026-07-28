# deploy

Deployment artifacts, ordered by the roadmap phases.

- `compose/` — Docker Compose for local development (Postgres, MinIO,
  RabbitMQ, Qdrant + the three own services).
- `helm/` — Helm charts for the three own services only. Infrastructure
  components use upstream community charts.
- `k8s/` — raw manifests / kustomize overlays for k3d and k3s.
