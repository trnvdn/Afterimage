# ADR-0006: Identity and authentication

- **Status:** Accepted
- **Date:** 2026-08-02

## Context
The API needs authenticated users: endpoints must be protected, and every photo
needs an owner (`photos.owner_id`, see [ADR-0002](0002-photo-persistence-model.md)).
This ADR defines where the identity concern lives, how users register and sign
in, and how tokens are issued and validated across services.

## Decision

### 1. Auth as a separate service
Authentication and user management live in a dedicated **Identity microservice**
with its **own database**. No other service shares the `users` table. As a
result `owner_id` is a cross-service *logical* reference, **not** a database
foreign key (see Consequences; supersedes the FK note in ADR-0002).

### 2. User identity
Users use **`Guid` keys** (`IdentityUser<Guid>`). The `Guid` is what lands in
tokens and what other services persist as `owner_id`.

### 3. Registration, login, ban
- **Registration** with email confirmation.
- **Login** only for confirmed, non-banned users; issues an access token and a
  refresh token.
- **Ban** is a flag on the user, checked at **login and refresh** — not on every
  request. A ban therefore takes effect within at most the access-token
  lifetime.

### 4. Tokens
- **Access token:** JWT, ~15 minutes.
- **Refresh token:** ~1 month, **sliding** — every refresh issues a new access
  token *and* a new refresh token (valid another month) and invalidates the
  previous refresh token (rotation).
- Refresh tokens are stored **hashed** in a dedicated `refresh_tokens` table
  (`user_id`, `token_hash`, `expires_at`, `created_at`, `revoked_at`,
  `replaced_by`). This enables rotation lineage, **reuse detection** (a replayed
  revoked token revokes the whole chain), multi-device sessions, and per-device
  logout.
- The access token carries the user's `Guid` as `sub`. Services derive
  `owner_id` **from the token, never from the request body** (prevents IDOR).

### 5. Signing & validation
- Tokens are signed with an **asymmetric key (RS256)**: the Identity service
  holds the private key and signs; every other service validates with the
  **public key published via JWKS** (`/.well-known/jwks.json`), discovered via
  the OIDC `Authority`. Key rotation is handled through the token's `kid` header.
- **Validation is local to each service** (signature + expiry against the cached
  JWKS) — no per-request call to the Identity service.
- On an expired access token a service returns `401`; **the client** calls the
  Identity refresh endpoint and retries. The Worker does not authenticate — it
  consumes a queue, not HTTP.
- This model is OIDC-shaped, so an external provider (**Google OAuth**) can be
  added later with the Identity service acting as the broker, transparent to the
  other services.

### 6. Secrets & password hashing
- **Password hashing:** ASP.NET Core Identity default (PBKDF2).
- **Secrets** (signing keys, etc.) via `.env` / secrets store, never committed.

## Constraints

### Password policy
Minimum length 6; at least one uppercase letter, one lowercase letter, and one
special character. _Marked for revisit: a length-first policy (≥ 8–12) plus a
breach check (HaveIBeenPwned) is a likely later improvement._

## Consequences
- **No cross-service FK for `owner_id`.** Integrity is enforced at write time
  (`owner_id` comes from a signed token → the user existed when it was issued)
  and, on user deletion, via a future `user.deleted` event the Photo service
  reacts to (cascade / anonymise). Until then orphaned photos are tolerated.
  This supersedes ADR-0002's "add the FK later".
- **Fourth own service.** The system now has four own services (Api, Worker, ai,
  Identity), not three — update the structure notes in README / CLAUDE.md. The
  Identity service needs its own database (a separate DB in the compose Postgres,
  or its own instance).
- **Email confirmation needs a mail sender** (SMTP) plus a dev mail-catcher
  (e.g. Mailhog / Papercut) in compose — added when the Identity service is built.
- **Ban / logout latency** equals the access-token lifetime (~15 min), since
  validation is local and stateless — accepted, in exchange for not calling
  Identity on every request.

## Alternatives considered
- **Symmetric signing (HS256)** — rejected: the shared secret would have to be
  handed to every validating service, letting any of them mint tokens. RS256
  keeps the private key inside Identity.
- **Central token introspection (opaque tokens)** — rejected: a per-request
  dependency on Identity and a bottleneck; local JWKS validation scales and
  matches ban-at-refresh.
- **Foreign key on `owner_id` (modular monolith)** — rejected: a real DB FK
  means not splitting auth into its own service; the distributed setup (and the
  learning it brings) was the explicit goal.
- **Long-lived access tokens, no refresh** — rejected: a stolen token would stay
  valid too long with no revocation path.
