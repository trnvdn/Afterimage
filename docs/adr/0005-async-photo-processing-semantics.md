# ADR-0005: Asynchronous photo processing semantics

- **Status:** Proposed
- **Date:** 2026-07-28

## Context
Photo processing (EXIF, thumbnail, embedding) runs asynchronously in the Worker
off a RabbitMQ queue. We need well-defined lifecycle transitions, message
acknowledgement rules, and a story for the dual-write between the DB commit and
the message publish. Lifecycle state storage is defined in
[ADR-0002](0002-photo-persistence-model.md).

## Decision

### Lifecycle transitions
- **Uploaded:** initial state on the HTTP request. The API commits the row,
  then publishes a `photo.uploaded` event to RabbitMQ.
- **Processed:** the Worker processes the file, updates metadata, sets the state
  to `Processed`, and ACKs.
- **Failed:** the row is kept. On a bad file the Worker sets `Failed`, writes the
  cause into `failure_reason`, and ACKs (a poison message must not requeue).

### Acknowledgement
- **Bad input (poison):** ACK — do not requeue; mark `Failed`.
- **System error (e.g. MinIO down):** NACK + requeue. A Dead Letter Exchange
  (DLX) consumer handles exhausted retries by marking the row `Failed`.

### Idempotency
Processing must be **idempotent**: handling the same `photo.id` more than once
must not corrupt state or double-write derived artifacts. This is required
because the compensation sweeper (below) and requeue can both re-deliver a
message. The Worker checks/settles state on the `id` rather than assuming a
first-and-only delivery.

## Consequences
- **Dual-write compensation:** a background job sweeps rows stuck in `Uploaded`
  beyond a safe timeout and republishes them, protecting against an API crash
  between the DB commit and the publish. Because republishing can hit a photo
  the Worker is still processing, idempotency (above) is what makes the sweep
  safe — the timeout alone does not.

## Alternatives considered
- **Transactional Outbox for RabbitMQ events** — rejected for the added
  complexity. The `photos` table itself (filter by `uploaded` state + timestamp)
  acts as a sufficient outbox for this scale.
