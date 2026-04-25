# EduScoring — AI & Backend Coding Instructions

Welcome! You are an AI assistant helping to build the `EduScoring` project.
This project is a modern ASP.NET Core Web API built strictly following:

* **Vertical Slice Architecture (VSA)**
* **CQRS (Command Query Responsibility Segregation)**
* **Event-Driven Processing (RabbitMQ)**
* **LLM Evaluation Pipeline**
* **Read/Write Database Separation**

Your job is to make safe, minimal, production-grade backend changes without breaking existing architecture.

---

# 1. Architectural Pattern: Vertical Slice Architecture (VSA)

## Core Rule

Code must be grouped strictly by **Business Features** under `Features/`.

Example:

```text
Features/
 └── Exams/
     └── Features/
         └── CreateExam/
             ├── CreateExamCommand.cs
             ├── CreateExamCommandHandler.cs
             └── CreateExamEndpoint.cs
```

## Every Slice MUST contain

* `[UseCase]Command.cs` OR `[UseCase]Query.cs`
* `[UseCase]CommandHandler.cs` OR `[UseCase]QueryHandler.cs`
* `[UseCase]Endpoint.cs`

## Responsibilities

### Endpoint

ONLY:

* receive HTTP request
* parse JWT / claims
* validate request shape
* call Handler
* return response

NEVER:

* business logic
* database logic
* AI logic
* direct HttpClient calls

---

### Handler

ONLY:

* business logic
* database operations
* publish events
* orchestration

NEVER:

* HTTP logic
* endpoint mapping

---

### Command / Query

Must be:

* plain `record`
* no methods
* no business logic

---

# 2. CQRS + DATABASE RULES (CRITICAL)

---

# 2.1 DbContexts

## AppDbContext

Used for:

* Commands (WRITE)
* EventHandlers with DB writes
* RabbitMQ workers writing data
* SaveChangesAsync()

This is the **Leader DB**.

---

## AppReadDbContext

Used for:

* Queries (READ ONLY)

This is the **Follower / Replica DB**.

Must:

* inherit from `AppDbContext`
* use `QueryTrackingBehavior.NoTracking` by default

Example:

```csharp
public class AppReadDbContext : AppDbContext
{
    public AppReadDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }
}
```

---

# 2.2 Handler Rules

## Commands → Write DB

```csharp
public class CreateExamCommandHandler
{
    private readonly AppDbContext _db;
}
```

---

## Queries → Read DB

```csharp
public class GetExamDetailQueryHandler
{
    private readonly AppReadDbContext _db;
}
```

---

## IMPORTANT

Do NOT use redundant:

```csharp
.AsNoTracking()
```

if handler already uses:

```csharp
AppReadDbContext
```

because NoTracking is already global.

---

# 2.3 Dependency Injection

Must register BOTH:

```csharp
services.AddDbContext<AppDbContext>();
services.AddDbContext<AppReadDbContext>();
```

Use:

```json
ConnectionStrings:
  WriteDb
  ReadDb
```

If using Supabase:

```json
WriteDb == ReadDb
```

is acceptable temporarily.

Architecture first.
Infrastructure scaling later.

---

# 3. EVENT-DRIVEN WORKFLOWS

Heavy tasks MUST NOT block HTTP request threads.

Use RabbitMQ.

---

## Correct Flow

```text
HTTP Request
→ Handler
→ SaveChanges
→ Publish Event
→ Return 202 Accepted
                ↓
             RabbitMQ
                ↓
        Background Worker
                ↓
        AI Evaluation / Email / Notification
```

---

## MUST use events for

* OCR processing
* LLM evaluation
* email sending
* notifications
* heavy reports
* expensive calculations

---

# 4. AI SCORING PIPELINE (VERY CRITICAL)

LLM is NOT a magic endpoint.

It is ONE component inside a strict evaluation pipeline.

---

# 4.1 Abstraction Layer

Never inject `HttpClient` directly inside business handlers.

Always use:

```csharp
public interface IAiScoringService
{
    Task<AiEvaluationResult> EvaluateAsync(
        string ocrText,
        List<Rubric> rubrics);
}
```

---

# 4.2 Strict Output Schema

LLM MUST return strict JSON only.

Expected:

```json
{
  "totalScore": 8.5,
  "criteriaScores": [
    {
      "criteriaName": "Grammar",
      "score": 4.0,
      "maxScore": 5.0,
      "reasoning": "Good, but missed a comma."
    }
  ],
  "overallFeedback": "Solid logical flow.",
  "confidenceScore": 0.95
}
```

No free-text output.

---

# 4.3 Prompt Guardrails

System prompt MUST enforce:

## Identity

```text
You are a strict, highly accurate, and objective academic examiner.
```

## Anti-Hallucination

```text
Score based ONLY on the provided Student Answer and Rubric.
Do NOT hallucinate.
If unreadable or off-topic → assign 0.
```

## Consistency

Use:

```text
Temperature = 0.0 or 0.1
```

Never high temperature.

---

# 4.4 Pipeline Execution Steps

Worker logic MUST follow:

## Step 1 — Pre-process

Clean OCR raw text.

* remove gibberish
* normalize spacing
* remove invalid OCR artifacts

---

## Step 2 — Compile Prompt

Inject:

* Exam Rubric
* Cleaned OCR text

into strict prompt template.

---

## Step 3 — LLM Execution

Call only through:

```csharp
IAiScoringService
```

---

## Step 4 — Post-process + Validation

Validate:

* `totalScore <= MaxExamScore`
* all criteria exist
* criteria names match exact rubric
* scores valid

If invalid:

* retry
* or flag for manual review

---

## Step 5 — Persist

Save:

* AI score
* reasoning
* confidence
* audit information

for Human vs AI comparison.

---

# 5. CONSOLE LOGGING CONVENTION

Use strict grep-friendly logs.

Format:

```text
[FeatureName | EntityId=N] THÀNH CÔNG — ...
[FeatureName | EntityId=N] THẤT BẠI [404] — ...
```

Example:

```csharp
var tag = "[CreateExam]";
Console.WriteLine($"{tag} THÀNH CÔNG — ExamId={exam.Id}");
```

No random logs.
No unclear logs.

---

# 6. COPILOT SAFE REFACTOR RULES (EXTREMELY IMPORTANT)

Minimal safe refactor only.

Never rewrite the whole project.

---

# 6.1 STOP CONDITIONS (CRITICAL)

Immediately STOP if:

1. Any Endpoint file needs modification
2. Any DTO model needs modification
3. Any Entity model needs modification
4. Any EF Migration is generated
5. Build starts failing
6. Constructor dependency becomes ambiguous
7. MediatR registration breaks
8. RabbitMQ worker behavior changes
9. Authentication flow changes
10. Existing business logic changes

If any happen:

STOP
Explain the issue
Do NOT continue automatically

---

# 6.2 STEP ZERO (MANDATORY)

Before changing code:

1. Scan target folder
2. List all QueryHandlers found
3. List all CommandHandlers found
4. Show files to be modified
5. Show files NOT touched

Wait for confirmation before editing.

Never auto-refactor blindly.

---

# 6.3 SCOPE LIMIT

Refactor ONLY one folder at a time.

Example:

```text
Features/Exams
```

Do NOT scan the whole solution.

After build passes:

STOP
Wait for next folder.

---

# 6.4 EXAMPLE BEFORE → AFTER

## Before

```csharp
public class GetExamDetailQueryHandler
{
    private readonly AppDbContext _db;

    public GetExamDetailQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle()
    {
        return await _db.Exams
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}
```

---

## After

```csharp
public class GetExamDetailQueryHandler
{
    private readonly AppReadDbContext _db;

    public GetExamDetailQueryHandler(AppReadDbContext db)
    {
        _db = db;
    }

    public async Task Handle()
    {
        return await _db.Exams
            .FirstOrDefaultAsync();
    }
}
```

Business logic unchanged.

Only DbContext changed.

---

# 6.5 GIT STRATEGY

Never auto-commit.

Commit per folder only.

Example:

```bash
git commit -m "refactor(query): move Exams queries to AppReadDbContext"
```

Always:

```bash
dotnet build
```

after each folder.

---

# 7. FINAL CHECKLIST

Before finishing any feature:

* [ ] Slice đúng thư mục `Features/<Module>/Features/<UseCase>/`
* [ ] Command/Query là plain record
* [ ] Query inject `AppReadDbContext`
* [ ] Command inject `AppDbContext`
* [ ] Endpoint không chứa business logic
* [ ] Heavy task dùng RabbitMQ
* [ ] AI calls đi qua `IAiScoringService`
* [ ] LLM output được validate
* [ ] Logging đúng convention
* [ ] No Endpoint modification
* [ ] No DTO modification
* [ ] No Entity modification
* [ ] No accidental migration
* [ ] Build passes

---

# FINAL PRINCIPLE

Refactor like surgery.

Not like demolition.

Safe.
Minimal.
Production-first.
Architecture-consistent.

That is the standard for EduScoring.
