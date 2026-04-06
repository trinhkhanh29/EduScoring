# EduScoring
Automated essay scoring and feedback system for students using Large Language Models (LLMs)

## Overview

EduScoring is an **ASP.NET Core Web API** built with **Clean Architecture** that integrates Large Language Models (LLMs) such as OpenAI GPT or Anthropic Claude to provide automated, rubric-based grading and constructive feedback on student essays.

---

## Architecture

The solution follows Clean Architecture with four distinct layers:

```
EduScoring/
├── src/
│   ├── EduScoring.Domain/          # Core domain entities and value objects
│   ├── EduScoring.Application/     # Use cases, interfaces, and DTOs
│   ├── EduScoring.Infrastructure/  # LLM service, repositories, DI wiring
│   └── EduScoring.API/             # ASP.NET Core Web API (controllers, Swagger)
└── EduScoring.slnx
```

### Layer Responsibilities

| Layer | Responsibility |
|-------|---------------|
| **Domain** | Domain entities (`StudentEssaySubmission`, `GradingCriteria`, `EvaluationResult`) and base types. No external dependencies. |
| **Application** | Defines use-case orchestrators (`EvaluateEssayUseCase`, `ManageGradingCriteriaUseCase`), repository and LLM service interfaces, and DTOs. Depends only on Domain. |
| **Infrastructure** | Implements `ILLMEvaluationService` (OpenAI-compatible API), in-memory repository implementations, and DI registration helpers. |
| **API** | HTTP controllers, Swagger/OpenAPI documentation, and application entry point. |

---

## Domain Entities

### `StudentEssaySubmission`
Represents a student's essay submitted for automated grading.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Unique submission identifier |
| `StudentId` | `string` | Identifier of the submitting student |
| `StudentName` | `string` | Full name of the student |
| `Title` | `string` | Title of the essay |
| `EssayContent` | `string` | Full essay text |
| `GradingCriteriaId` | `Guid` | Reference to the scoring rubric |
| `Status` | `enum` | `Pending` → `Processing` → `Completed` / `Failed` |

### `GradingCriteria`
Holds a standardized scoring rubric with multiple weighted dimensions.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Unique criteria identifier |
| `Name` | `string` | Rubric name (e.g., "Academic Essay Rubric") |
| `Description` | `string` | Purpose and scope of the rubric |
| `Dimensions` | `ScoringDimension[]` | Weighted evaluation dimensions |
| `MaxScore` | `double` | Sum of all dimension max points |
| `PromptTemplate` | `string` | LLM prompt template (supports `{EssayContent}`, `{Dimensions}`, etc.) |

### `EvaluationResult`
Stores the automated score and constructive feedback returned by the LLM.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Unique result identifier |
| `SubmissionId` | `Guid` | Reference to the evaluated submission |
| `TotalScore` | `double` | Aggregated score across all dimensions |
| `MaxPossibleScore` | `double` | Maximum achievable score |
| `ScorePercentage` | `double` | Derived percentage score |
| `DimensionScores` | `DimensionScore[]` | Per-dimension scores and feedback |
| `OverallFeedback` | `string` | Holistic feedback from the LLM |
| `StrengthsSummary` | `string` | Key strengths identified in the essay |
| `ImprovementSuggestions` | `string` | Actionable improvement guidance |
| `LlmModel` | `string` | Model used for evaluation |

---

## Application Interface

### `ILLMEvaluationService`

Defined in `EduScoring.Application/Interfaces/ILLMEvaluationService.cs`, this contract decouples the use cases from any specific LLM provider:

```csharp
public interface ILLMEvaluationService
{
    Task<EvaluationResult> EvaluateAsync(
        StudentEssaySubmission submission,
        GradingCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<string> AnalyzeTextContextAsync(
        string textContent,
        string? context = null,
        CancellationToken cancellationToken = default);
}
```

The default implementation (`LLMEvaluationService`) calls the **OpenAI Chat Completions API** and parses the structured JSON response back into domain objects.

---

## API Endpoints

Base URL: `https://localhost:{port}/api`

### Grading Criteria

#### `POST /api/gradingcriteria`
Creates a new scoring rubric used to evaluate essays.

**Request Body:**
```json
{
  "name": "Academic Essay Rubric",
  "description": "Standard rubric for evaluating university essays",
  "dimensions": [
    { "name": "Content", "description": "Depth and relevance of ideas", "maxPoints": 30 },
    { "name": "Structure", "description": "Organization and flow", "maxPoints": 20 },
    { "name": "Grammar", "description": "Language accuracy", "maxPoints": 20 },
    { "name": "Coherence", "description": "Logical consistency", "maxPoints": 20 },
    { "name": "Originality", "description": "Unique perspective or argument", "maxPoints": 10 }
  ],
  "promptTemplate": "You are an academic essay grader. Evaluate the following essay titled '{EssayTitle}' by {StudentName} using the rubric below.\n\nDimensions:\n{Dimensions}\n\nEssay:\n{EssayContent}"
}
```

**Response: `201 Created`**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Academic Essay Rubric",
  "description": "...",
  "maxScore": 100,
  "dimensions": [...]
}
```

---

#### `GET /api/gradingcriteria`
Returns all available grading criteria.

**Response: `200 OK`** — Array of grading criteria objects.

---

#### `GET /api/gradingcriteria/{id}`
Returns a single grading criteria by its identifier.

**Response: `200 OK`** | `404 Not Found`

---

### Essay Submissions

#### `POST /api/essaysubmissions`
Submits a student essay for evaluation. The essay is stored with `Pending` status.

**Request Body:**
```json
{
  "studentId": "student-001",
  "studentName": "Jane Doe",
  "title": "The Impact of Climate Change",
  "essayContent": "Climate change is one of the defining challenges...",
  "gradingCriteriaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response: `201 Created`**
```json
{
  "submissionId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "studentId": "student-001",
  "studentName": "Jane Doe",
  "title": "The Impact of Climate Change",
  "status": "Pending",
  "submittedAt": "2024-01-15T10:30:00Z"
}
```

---

#### `POST /api/essaysubmissions/{submissionId}/evaluate`
Triggers LLM evaluation for a submitted essay. The status transitions `Pending → Processing → Completed`.

**Response: `200 OK`**
```json
{
  "evaluationId": "a1b2c3d4-...",
  "submissionId": "7c9e6679-...",
  "totalScore": 82.5,
  "maxPossibleScore": 100,
  "scorePercentage": 82.5,
  "dimensionScores": [
    {
      "dimensionName": "Content",
      "score": 25,
      "maxPoints": 30,
      "feedback": "Strong central argument with relevant examples..."
    }
  ],
  "overallFeedback": "This is a well-structured essay that demonstrates...",
  "strengthsSummary": "Clear thesis, good use of evidence, logical flow.",
  "improvementSuggestions": "Consider expanding the counterargument section...",
  "llmModel": "gpt-4o",
  "evaluatedAt": "2024-01-15T10:31:05Z"
}
```

**Error Responses:**
- `404 Not Found` — Submission does not exist.
- `502 Bad Gateway` — LLM provider is unavailable.

---

#### `GET /api/essaysubmissions/{submissionId}/result`
Retrieves the evaluation result for a previously evaluated submission.

**Response: `200 OK`** — Same shape as the evaluate response above.  
**Response: `404 Not Found`** — Essay has not been evaluated yet.

---

## Workflow

```
1. Create Grading Criteria   POST /api/gradingcriteria
          ↓
2. Submit Essay              POST /api/essaysubmissions
          ↓
3. Trigger Evaluation        POST /api/essaysubmissions/{id}/evaluate
          ↓
4. Retrieve Result           GET  /api/essaysubmissions/{id}/result
```

---

## Configuration

Set the LLM provider details in `appsettings.json` (or via environment variables / user secrets):

```json
{
  "LLM": {
    "BaseUrl": "https://api.openai.com/v1",
    "ApiKey": "<your-api-key>",
    "Model": "gpt-4o",
    "MaxTokens": 2000,
    "Temperature": 0.3
  }
}
```

> **Security:** Never commit your API key to source control. Use environment variables (`LLM__ApiKey=...`) or .NET User Secrets in development.

### Supported Providers

| Provider | BaseUrl | Example Model |
|----------|---------|---------------|
| OpenAI | `https://api.openai.com/v1` | `gpt-4o`, `gpt-4-turbo` |
| Anthropic (via proxy) | Use an OpenAI-compatible proxy | `claude-3-5-sonnet-20241022` |
| Azure OpenAI | `https://<resource>.openai.azure.com/openai` | deployment name |

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- An API key for OpenAI or a compatible LLM provider

### Run the API

```bash
cd src/EduScoring.API
dotnet user-secrets set "LLM:ApiKey" "<your-openai-api-key>"
dotnet run
```

Open the Swagger UI at: `https://localhost:<port>/swagger`
