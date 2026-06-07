# Project Brief: PayFlow — A Real-Time Payments & Ledger Platform

## 1. Purpose

A backend system that demonstrates production-grade fintech engineering patterns — not a CRUD demo with a payments theme bolted on. The system should behave the way Stripe/Monzo/Revolut-style platforms actually behave: money movements are event-sourced, transactions are idempotent and saga-orchestrated, balances are strongly consistent, and everything is observable and auditable.

**Target reader for the finished repo:** a senior backend engineer or hiring manager skimming the README and code for 10 minutes. Every design decision should be explainable in one sentence and traceable to a real fintech problem.

---

## 2. Domain Scope (deliberately narrow)

**"Wallet + Payments" system** — users hold balances in a wallet, can top up, transfer to other users, and make payments to merchants. This is narrow enough to build in 1-2 weeks but touches every pattern that matters.

**In scope:**
- Wallet accounts (per user, single currency to start — GBP)
- Top-up (simulate external funding source, e.g. a fake card processor)
- Peer-to-peer transfer (wallet → wallet)
- Merchant payment (wallet → merchant, with a "hold then capture" flow like a real card auth)
- Transaction history / statement (read model)

**Explicitly out of scope (say so in the README — scoping judgment is itself a signal):**
- Multi-currency / FX
- Real KYC/AML integration
- Real card network integration
- Frontend (API + docs only, maybe a minimal Swagger/Postman collection)

---

## 3. Services (bounded contexts)

| Service | Responsibility |
|---|---|
| **LedgerService** | Source of truth. Append-only event store of double-entry ledger entries. Owns balance calculation. |
| **WalletService** | Wallet lifecycle (create, freeze), orchestrates top-up/transfer/payment commands, talks to LedgerService via events. |
| **PaymentService** | Merchant payment flow — authorize (hold) → capture/release. Models the Saga. |
| **FundingService** | Simulated external funding source (fake "card processor") — deliberately flaky (configurable failure rate) to force retry/idempotency handling. |
| **NotificationService** | Consumes ledger events, sends (simulated) notifications — demonstrates eventual consistency / fan-out. |
| **ApiGateway** | Single entry point, auth, rate limiting, request idempotency-key enforcement. |
| **StatementService (read model)** | CQRS read side — denormalized transaction history, built from the same event stream. |

---

## 4. Patterns to Demonstrate (and why each one is there)

| Pattern | Where | Why it's real, not decorative |
|---|---|---|
| **Event Sourcing** | LedgerService | Balances are derived by replaying events, never mutated directly — mirrors how Monzo's ledger works and makes the system audit-provable. |
| **Double-entry accounting model** | LedgerService domain | Every movement is a debit + credit pair that must balance to zero — this is the actual constraint real ledgers enforce. |
| **CQRS** | StatementService vs LedgerService | Write path (ledger) is strongly consistent; read path (statements) is eventually consistent, rebuilt from events. |
| **Saga (orchestrated)** | PaymentService: authorize → hold funds → capture/release | Multi-step transaction with explicit compensation (release hold on failure) — the actual hard part of "call 3 services" systems. |
| **Idempotency keys** | ApiGateway + FundingService | Every mutating request requires a client-supplied idempotency key; replayed requests return the original result, never double-execute. |
| **Outbox pattern** | WalletService, PaymentService | DB write + event publish happen atomically via an outbox table + relay — prevents ledger/event drift. |
| **Circuit breaker + retry w/ backoff (Polly)** | FundingService client in WalletService | FundingService is deliberately flaky; WalletService must degrade gracefully, not cascade-fail. |
| **Dead-letter queue** | RabbitMQ/MassTransit config | Events that repeatedly fail processing land in a DLQ with a documented recovery path. |
| **Eventual consistency / fan-out** | NotificationService | Demonstrates that not everything needs to be in the strongly-consistent path. |
| **Distributed tracing** | All services | OpenTelemetry + Jaeger — a single transfer should be traceable end-to-end across 3-4 services. |
| **Observability** | All services | Prometheus + Grafana dashboard showing transaction throughput, saga failure rate, funding failure rate. |
| **Immutable audit log** | LedgerService | Separate from operational reads — append-only, never updated/deleted. |

This table itself should basically become your README — it's the single most interview-useful artifact in the repo.

---

## 5. Non-Functional Requirements (what makes it "real" not "demo")

- **Idempotency is enforced, not just claimed** — write a test that fires the same request twice concurrently and asserts single execution.
- **Concurrency correctness** — two simultaneous transfers from the same wallet must not overdraw it (optimistic concurrency / row versioning on the ledger).
- **Chaos knob** — FundingService has a configurable failure rate (env var) so you can demo resilience live in an interview.
- **Every service has structured logs + trace correlation IDs.**
- **Docker Compose spins up the whole system in one command.**
- **A load test script (k6 or NBomber)** showing behavior under concurrent transfers — include the results in the README, not just the script.

---

## 6. Suggested 1-2 Week Build Order

**Days 1-2:** LedgerService (event store + double-entry model + balance projection). This is the foundation everything else depends on — get it right first.

**Days 3-4:** WalletService + outbox pattern + basic top-up flow (sync, no saga yet). Get one full happy path working end-to-end with tracing.

**Days 5-6:** FundingService (with configurable flakiness) + idempotency keys at the gateway + Polly resilience. This is where you demonstrate the "network is unreliable" story.

**Days 7-9:** PaymentService Saga (authorize/hold → capture/release) with compensation logic. This is the centerpiece — spend real time here.

**Days 10-11:** StatementService (CQRS read model) + NotificationService (fan-out consumer) + DLQ setup.

**Days 12-13:** Observability (OTel/Jaeger/Prometheus/Grafana dashboard), load test, concurrency tests.

**Day 14:** README, architecture diagram, "design decisions & tradeoffs" doc (this document is what recruiters/interviewers actually read).

---

## 7. Deliverables Checklist

- [ ] `docker-compose.yml` — full stack, one command
- [ ] README with architecture diagram (Eraser/Mermaid) and the pattern table above
- [ ] `DESIGN_DECISIONS.md` — tradeoffs you made and why (this is your interview script)
- [ ] Postman/Swagger collection for the API
- [ ] Grafana dashboard screenshot in README
- [ ] Load test results (throughput, p95 latency under concurrent load) in README
- [ ] Tests: idempotency double-fire test, concurrent-transfer overdraft test, saga compensation test

---

## 8. How to Talk About It in Interviews

Frame it as: *"I built a wallet/payments system that models the same correctness problems Stripe and Monzo solve — event-sourced ledger, saga-orchestrated payment authorization, idempotent APIs, and outbox-based event publishing — because I wanted to understand those tradeoffs hands-on rather than just read about them."*

Then be ready to whiteboard: what happens if the outbox relay crashes mid-publish? What happens if two capture requests for the same hold arrive concurrently? What breaks if you removed the idempotency key requirement? Those are the actual questions this project prepares you to answer well.
