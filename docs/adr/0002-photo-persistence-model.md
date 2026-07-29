# ADR-0002: Photo persistence model

- **Status:** Proposed
- **Date:** 2026-07-28

## Context
When a user uploads a photo, the system must persist a record of it
immediately — in the `Uploaded` state, before any processing — so the Worker
can pick it up asynchronously and the API can list and track it. There is no
persistence layer yet. This ADR fixes the `photos` table shape and its
identifier and naming decisions.

Related: [ADR-0003](0003-domain-project-and-encapsulation.md) (where this lives),
[ADR-0005](0005-async-photo-processing-semantics.md) (lifecycle transitions and
messaging).

Pipeline reference (see CLAUDE.md):
`upload → Postgres row (Uploaded) → Worker processing → Processed | Failed`.

## Decision

### 1. Identifier strategy
`Id` is a **GUID generated application-side**, before the row is inserted — the
API needs it up front to build the MinIO object key and the RabbitMQ message.

The generator is a **time-ordered UUIDv7**, not random v4: random ids scatter
B-tree inserts across pages and fragment the primary key under a steady upload
stream, whereas v7 keeps inserts local and aligns with `created_at`-ordered
access. On **.NET 9** this is the built-in `Guid.CreateVersion7()` — no external
dependency. (This drives the target framework to .NET 9; update the stack notes
in README / CLAUDE.md accordingly.)

### 2. Owner reference
`owner_id` is a plain `GUID` for now — the user identifier is also a GUID.
No foreign key to `users` yet (the table does not exist). The FK is added once
auth lands (#4).

### 3. Implications for external systems
- **MinIO (S3):** the object path pattern is `{owner_id}/{id}/original` and
  `{owner_id}/{id}/thumbnail`, physically grouping files by user while staying
  unique. `original_storage_key` / `thumbnail_storage_key` are stored explicitly
  (not derived on read) so a future change to the path scheme does not break
  existing rows.
- **Qdrant (Vector DB):** Qdrant supports UUID point ids natively, giving a
  direct 1:1 mapping `photos.id` = point id — no surrogate or hashing.

### 4. Naming convention
**snake_case** in the database, via `EFCore.NamingConventions`
(`UseSnakeCaseNamingConvention()`) — entities stay PascalCase in C#, columns
become `owner_id`, `lifecycle_state`, etc. Idiomatic Postgres, no per-property
mapping, no quoting in raw SQL.

### 5. Lifecycle state (storage)
`lifecycle_state` is a C# enum (`Uploaded`, `Processed`, `Failed`) mapped to
**text** (`.HasConversion<string>()`) for human readability in the DB. The
transition rules and messaging semantics live in
[ADR-0005](0005-async-photo-processing-semantics.md).

### 6. Columns needed on the read/serve path
Because downloads are served **through the API** (not via a client-facing MinIO
URL), the response path needs these as first-class columns, not buried in
`metadata` jsonb:
- `content_type` — to set the `Content-Type` header
- `file_name` — original name, for `Content-Disposition`
- `size_bytes` — object size

Metadata discovered later during processing (dimensions, capture time, …) that
is *not* on a hot path stays in `metadata` jsonb (nullable).

### 7. Indexes
Access pattern "list one user's photos, filterable by state, newest first" →
composite B-tree index on `(owner_id, lifecycle_state, created_at DESC)`
(equality columns first, sort column last).

## Schema
Source of truth: [`docs/database-diagram.mmd`](../database-diagram.mmd)
(rendered: [`database-diagram.png`](../database-diagram.png)).

## Consequences
- **Eventual consistency:** clients do not get full metadata immediately on
  upload; the client polls or relies on a fallback state.
- Revisit later to add the real FK from `owner_id` to `users`.

## Alternatives considered
- **Validate photos via business logic in the API** — overengineering; heavy
  validation and metadata extraction belong in the async Worker.
- **Random UUIDv4 as PK** — simpler (`Guid.NewGuid()`), but fragments the PK
  index under load; rejected in favour of v7.
