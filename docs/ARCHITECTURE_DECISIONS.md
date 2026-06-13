# Architecture Decisions

This file records significant architectural decisions made during the PayFlow project. Each entry includes context, options considered, the chosen approach, and tradeoffs.

## Decision Record

### 001: Feature-Folder Architecture over Strict Clean Architecture Layers

**Date:** 2026-07-29

**Context:**
Each service (LedgerService, WalletService, etc.) could be split into 4+ .csproj files (Domain, Application, Infrastructure, Api) to enforce dependency direction at the compiler level. This is the textbook "Clean Architecture" layout, resulting in ~30 projects for the full system.

**Options considered:**

1. **Strict Clean Architecture (4+ projects/service):** Compiler-enforced isolation via separate assemblies. ~30 projects total. Slower builds, more ceremony — developers jump between projects for a single feature. Build times degrade as the solution grows.

2. **Vertical Slices:** Feature-organized code with maximal cohesion (everything for "Transfer" in one place). Less familiar pattern in .NET. Can lead to infrastructure duplication across slices.

3. **Feature-Folder Architecture (chosen):** One project per service with Clean Architecture dependency rules enforced by namespace convention + automated architecture tests (NetArchTest). Domain code lives under `Domain/`, application logic under `Features/`, infrastructure under `Infrastructure/`.

**Why:**
Production fintech services rarely split into 4 assemblies per service — the cost in build time and navigation outweighs the benefit. A single project with feature folders achieves the same architectural boundaries when paired with automated architecture tests, while keeping the developer experience fast. The layering is preserved in the *code* (domain types don't reference EF), not in the *build system*.

**Tradeoff:**
Requires discipline during code review. Mitigated by NetArchTest rules that fail CI if infrastructure leaks into domain.

### 002: Async Event-Driven Inter-Service Communication via Outbox

**Date:** 2026-06-12

**Context:**
WalletService orchestrates top-up/transfer operations that ultimately record double-entry pairs in LedgerService. The project brief states WalletService "talks to LedgerService via events." Additionally, the outbox pattern is a listed deliverable for atomic DB + event publishing.

**Options considered:**

1. **Synchronous HTTP:** WalletService calls LedgerService's REST API (`POST /ledger/transactions`). Simple to build, but couples service availability and leaves the outbox pattern undemonstrated. Contradicts the "via events" design intent.

2. **MassTransit Request/Response:** Sends a command via RabbitMQ and awaits a reply. Decoupled, but ties HTTP lifecycle to a message round-trip with timeout/correlation complexity. No outbox benefit.

3. **MassTransit Fire-and-Forget + Outbox (chosen):** WalletService writes command data to an `outbox_messages` table in the same EF transaction as its own state change. A background `OutboxRelayService` publishes pending messages to RabbitMQ. LedgerService consumes the command and publishes a result event.

**Why:**
This decision checks three deliverables at once: async event-driven communication, the outbox pattern, and eventual consistency. The 202 Accepted + correlation ID + status endpoint pattern mirrors real fintech APIs (Stripe, Monzo). It cleanly decouples service lifecycles — if LedgerService is down, the outbox holds messages until it recovers. Adding new consumers (StatementService, NotificationService) requires zero changes to the publishing service.

**Tradeoff:**
Clients receive `202 Accepted` with a correlation ID and must poll for completion. Not suitable for synchronous low-latency flows, but acceptable for top-ups and transfers where clients expect an async confirmation. Adds infrastructure complexity (outbox table, background relay, eventual consistency guarantees).
