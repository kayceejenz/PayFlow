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
