# EduScoring — AI & Backend Coding Instructions

Welcome! You are an AI assistant helping to build the `EduScoring` project.
This project is a modern ASP.NET Core Web API built strictly following
**Vertical Slice Architecture (VSA)**, **CQRS**, and **LLM Pipeline** principles.

---

## 1. Architectural Pattern: Vertical Slice Architecture (VSA)

- Group code strictly by **Business Features** under the `Features/` directory
  (e.g., `Features/Exams/Features/CreateExam/`).
- Every Slice must contain: `[UseCase]Command/Query`, `[UseCase]Handler`, and `[UseCase]Endpoint`.
- Handlers handle business logic; Endpoints strictly map HTTP to Handlers using **Minimal APIs**.

---

## 2. CQRS & Database

- **`AppDbContext`** — Write operations (Commands). Connects to the **Leader** node.
- **`AppReadDbContext`** — Read operations (Queries). Connects to **Follower** nodes, always `NoTracking`.

```csharp
// Commands → Write DB (Leader)
public class CreateExamCommandHandler(AppDbContext db) { ... }

// Queries → Read DB (Follower)
public class GetExamDetailQueryHandler(AppReadDbContext db) { ... }
```

---

## 3. Asynchronous & Event-Driven Workflows

- Heavy tasks (OCR, LLM Evaluation) **MUST NOT** block the HTTP thread.
- Use **RabbitMQ** to publish events (e.g., `SubmissionCreatedEvent`).
- Workers consume these events and trigger the AI Pipeline asynchronously.

```
HTTP Request → Handler → SaveChanges → Publish Event → Return 202
                                              ↓
                                         RabbitMQ
                                              ↓
                                    Worker: TriggerAiEvaluationHandler
```

---

## 4. AI Scoring Pipeline (CRITICAL)

> Do NOT treat the LLM as a magic endpoint.
> The LLM is just **one component** in a strict Evaluation Pipeline.

### 4.1 Abstraction & Interfaces

Never inject `HttpClient` directly for LLM calls inside business handlers.
Always use `IAiScoringService`:

```csharp
public interface IAiScoringService
{
    Task<AiEvaluationResult> EvaluateAsync(string ocrText, List<Rubric> rubrics);
}
```

### 4.2 LLM Output Schema (Strict JSON)

The LLM must be instructed to return a strictly typed JSON object.
Prompt responses must map to this exact C# record:

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
  "overallFeedback": "Solid logical flow, needs minor grammatical improvements.",
  "confidenceScore": 0.95
}
```

### 4.3 Prompt Engineering & Guardrails

When generating prompts or integrating LLM SDKs (Semantic Kernel / OpenAI SDK),
enforce these rules in the **System Prompt**:

1. **Identity:** `"You are a strict, highly accurate, and objective academic examiner."`
2. **Grounding (Anti-Hallucination):** `"Score based ONLY on the provided Student Answer and the Rubric. Do NOT hallucinate information. If the student answer is unreadable or completely off-topic, assign a score of 0."`
3. **Consistency over Creativity:** Set `Temperature = 0.0` or `0.1` to ensure deterministic and consistent scoring.

### 4.4 Pipeline Execution Steps

When writing worker logic (e.g., `TriggerAiEvaluationHandler`), follow this exact pipeline:

1. **Pre-process** — Clean OCR raw text: remove gibberish, normalize spacing.
2. **Compile Prompt** — Dynamically inject the Exam Rubric + cleaned OCR text into the Prompt Template.
3. **LLM Execution** — Call the LLM via `IAiScoringService`.
4. **Post-process & Validate** — Parse JSON. Verify `totalScore <= MaxExamScore`. Verify all `criteriaScores` match the exact Rubric. If validation fails → retry or flag for manual review.
5. **Persist** — Save `AiEvaluation` entity including LLM reasoning for audit and Human vs. AI comparison (Mean Absolute Error tracking).

---

## 5. Console Logging Convention

Every endpoint and handler uses a consistent tag prefix for easy grep:

```
[FeatureName | EntityId=N]  THÀNH CÔNG — ...
[FeatureName | EntityId=N]  THẤT BẠI [404] — ...
```

Example:
```csharp
var tag = $"[CreateExam]";
Console.WriteLine($"{tag} THÀNH CÔNG — ExamId: {newExam.Id}");
```

---

## Summary

When implementing any feature, follow this checklist:

- [ ] Slice nằm đúng thư mục `Features/<Module>/Features/<UseCase>/`
- [ ] Command/Query là plain `record`, không chứa logic
- [ ] Handler dùng đúng `AppDbContext` (write) hoặc `AppReadDbContext` (read)
- [ ] Endpoint không chứa business logic — chỉ parse JWT, gọi Handler, trả response
- [ ] Event-driven cho mọi tác vụ nặng (AI, email, notification)
- [ ] AI calls đi qua `IAiScoringService`, không inject HttpClient trực tiếp
- [ ] LLM output được validate trước khi persist
- [ ] Console logging dùng đúng tag convention