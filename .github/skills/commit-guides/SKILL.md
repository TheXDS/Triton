---
name: commit-guides
description: 'Generate and validate Conventional Commits messages for Triton. Use when: creating commit messages, reviewing commit history, validating commit format, suggesting conventional commit prefixes.'
argument-hint: 'Describe what you changed for a commit message'
---

# Triton Conventional Commits Guide

## When to Use
- Generating commit messages after code changes
- Validating existing commit messages follow Conventional Commits
- Determining the correct prefix for a set of changes

## Procedure

### Step 1: Classify the Change
Map the changes to the appropriate prefix:

| Prefix   | Use When | Example |
|----------|----------|---------|
| `feat:`  | New public API, new transport, new bundle | `feat: add SQLite transport support` |
| `fix:`   | Bug fix, correction, regression | `fix: EFCore context dispose error` |
| `docs:`  | Documentation only (README, CONTRIBUTING, AGENTS) | `docs: update contributing guidelines` |
| `test:`  | Adding or fixing tests (no production code) | `test: add EFCore roundtrip test` |
| `chore:` | Build, deps, CI, config, no code change visible to users | `chore: update Microsoft.EntityFrameworkCore to 8.0.0` |
| `refactor:` | Code change that neither fixes a bug nor adds a feature | `refactor: extract common connection string parsing` |
| `style:` | Formatting, whitespace, semicolons, no logic change | `style: fix indentation in EfContextBuilder` |
| `perf:`  | Performance improvement | `perf: optimize dynamic query builder` |
| `ci:`    | GitHub Actions, CI config | `ci: add codecov upload step` |
| `build:` | Build system, package management | `build: update target framework to net8.0` |

### Step 2: Write the Message
Follow this structure:

```
<type>: <short description>

<optional longer description>
```

Rules:
- **Short description:** imperative mood, no capital first letter, no period
- **Optional body:** explain *why* and *what*, not *how*
- **Reference issues:** `Fixes #42` or `Refs #17`

### Step 3: Validate
Check:
1. Prefix is one of the approved types above
2. Description starts with lowercase
3. Description is under 72 characters
4. No trailing period in the subject line
5. Body (if any) is blank-separated from subject

### Quick Reference
```
feat: add SQLite transport support
fix: correct EFCore connection dispose error
docs: update contributing guidelines
test: add EFCore roundtrip test
refactor: extract common header parsing
chore: update Microsoft.EntityFrameworkCore to 8.0.0
```

## Triton-Specific Notes
- New database transports (EFCore, Dapper, InMemory, etc.) → `feat:`
- Transport bug fixes → `fix:`
- Unit test additions → `test:` (changes without tests face strict scrutiny)
- Build prop/target updates → `build:`
- MSBuild configuration → `build:` or `chore:`
- Core library changes (no external dependencies) → appropriate prefix based on change type
- Bundle changes (may depend on 3rd-party packages) → appropriate prefix based on change type
