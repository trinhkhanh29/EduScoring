# EduScoring — Vertical Slice Architecture

> Hệ thống chấm điểm tự động bài thi tự luận, được thiết kế theo **Vertical Slice Architecture** kết hợp với AI evaluation pipeline.

---

## Tại sao Vertical Slice Architecture?

Kiến trúc truyền thống (Layered Architecture) chia code theo **chiều ngang** — Controllers / Services / Repositories. Mọi tính năng mới đều phải chạm vào nhiều tầng cùng lúc, dễ gây coupling và khó maintain khi hệ thống lớn dần.

**Vertical Slice Architecture** chia code theo **chiều dọc** — mỗi tính năng (feature) là một "lát cắt" độc lập, chứa đủ mọi thứ từ HTTP endpoint xuống tới database. Mỗi lát cắt có thể đọc, sửa, test và deploy độc lập mà không ảnh hưởng tính năng khác.

```
Layered (cũ)                    Vertical Slice (hiện tại)
─────────────────────           ─────────────────────────────────────
Controllers  ←──────────────    [CreateExam]   [DeleteExam]   [Login]
Services     ←──────────────     Endpoint       Endpoint       Endpoint
Repositories ←──────────────     Handler        Handler        Handler
Database     ←──────────────     Command        Command        Command
                                 DB access      DB access      DB access
```

---

## Cấu trúc thư mục

```
EduScoring/
│
├── Common/                          # Dùng chung toàn hệ thống
│   ├── Authentication/              # JWT helpers, AppRoles constants
│   ├── Extensions/                  # IServiceCollection extensions
│   ├── Messaging/                   # Event bus interface (RabbitMQ)
│   ├── Middleware/                  # Global error handling, logging
│   └── Storage/                     # File upload (Azure Blob / S3)
│
├── Infrastructure/                  # Kỹ thuật nền tảng
│   ├── AppDbContext.cs              # EF Core DbContext duy nhất (Monolith)
│   └── Migrations/                  # Database schema versioning
│
└── Features/                        # ← TRÁI TIM CỦA KIẾN TRÚC
    ├── Auth/
    ├── Exams/
    ├── Submissions/
    ├── Users/
    └── System/
```

---

## Cấu trúc một Feature (Lát cắt)

Mỗi feature lớn (Auth, Exams, Submissions...) chứa nhiều **lát cắt nhỏ**. Mỗi lát cắt nhỏ đại diện cho **một use case cụ thể** và có cấu trúc thống nhất:

```
Features/Exams/Features/CreateExam/
│
├── CreateExamEndpoint.cs       # Định nghĩa HTTP route, parse request, trả response
├── CreateExamCommand.cs        # Input DTO (record) + Output DTO (response)
├── CreateExamCommandHandler.cs # Toàn bộ business logic + DB access
├── Events/
│   └── ExamCreatedEvent.cs     # Domain event phát ra sau khi xử lý thành công
└── EventHandlers/
    └── NotifyStudentsHandler.cs # Side effect chạy ngầm (gửi noti, ghi log...)
```

### Trách nhiệm từng file

| File | Trách nhiệm |
|---|---|
| `*Endpoint.cs` | Nhận HTTP request, xác thực JWT, gọi Handler, trả HTTP response |
| `*Command.cs` | Plain record — input không có logic, output là DTO thuần túy |
| `*CommandHandler.cs` | Toàn bộ logic nghiệp vụ: validate, query DB, xử lý, lưu |
| `*Event.cs` | Signal "việc đã xảy ra" — immutable, không chứa logic |
| `*EventHandler.cs` | Phản ứng với event: gửi mail, push notification, ghi audit log... |

---

## Sơ đồ luồng xử lý một request

```
HTTP Request
     │
     ▼
*Endpoint.cs
  ├─ Parse JWT → lấy UserId, Role
  ├─ Validate input cơ bản (field rỗng, format...)
  └─ Gọi Handler(Command)
          │
          ▼
  *CommandHandler.cs
    ├─ Query DB (EF Core)
    ├─ Kiểm tra business rules (quyền, trạng thái...)
    ├─ Thực thi thay đổi
    ├─ SaveChangesAsync()
    └─ Publish Event (tùy chọn)
              │
              ▼
      *EventHandler.cs          (chạy ngầm, không block response)
        └─ Side effects: email, noti, audit log, RabbitMQ...
          │
          ▼
     HTTP Response
```

---

## Features hiện có

### Auth — Xác thực người dùng

| Lát cắt | Method | Endpoint | Mô tả |
|---|---|---|---|
| `Login` | POST | `/api/auth/login` | Đăng nhập, trả JWT token |
| `Register` | POST | `/api/auth/register` | Đăng ký tài khoản mới |

### Exams — Quản lý đề thi

| Lát cắt | Method | Endpoint | Quyền | Mô tả |
|---|---|---|---|---|
| `CreateExam` | POST | `/api/exams` | Admin, Teacher | Tạo đề thi mới |
| `GetExamDetail` | GET | `/api/exams/{id}` | Authenticated | Xem chi tiết đề thi |
| `UpdateExam` | PUT | `/api/exams/{id}` | Admin, Teacher | Cập nhật đề thi |
| `DeleteExam` | DELETE | `/api/exams/{id}` | Admin, Teacher | Xóa mềm đề thi |
| `RestoreExam` | POST | `/api/exams/{id}/restore` | Admin, Teacher | Phục hồi đề thi đã xóa |

### Submissions — Nộp & Chấm bài *(đang phát triển)*

| Lát cắt | Mô tả |
|---|---|
| `SubmitExam` | Học sinh nộp bài thi kèm ảnh |
| `GetSubmissionDetail` | Xem kết quả chấm chi tiết |
| `GetMySubmissions` | Lịch sử nộp bài của học sinh |
| `CreateAppeal` | Khiếu nại kết quả chấm |

### Users — Quản lý người dùng *(đang phát triển)*

| Lát cắt | Mô tả |
|---|---|
| `GetUsers` | Admin xem danh sách người dùng |

---

## Nguyên tắc thiết kế áp dụng

**1. Mỗi lát cắt chỉ làm một việc**
`CreateExam` chỉ biết tạo exam. Không biết gì về `DeleteExam` hay `Login`.

**2. Handler không gọi Handler**
Handlers giao tiếp với nhau qua **Events**, không gọi trực tiếp. Điều này giữ cho các lát cắt độc lập hoàn toàn.

**3. Một DbContext duy nhất (Monolith)**
`AppDbContext` dùng chung toàn hệ thống. Khi cần tách Microservices, mỗi feature đã sẵn sàng vì code đã được cô lập theo feature.

**4. Soft Delete nhất quán**
Mọi entity hỗ trợ xóa đều có `IsDeleted`, `DeletedAt`, `RestoredAt`, `RestoredBy`. Query dùng `IgnoreQueryFilters()` khi cần truy cập bản ghi đã xóa.

**5. Authorization tại Endpoint, không phải Handler**
Handler nhận `IsAdmin` như một tham số — không tự đọc HTTP context. Điều này giúp Handler dễ test độc lập không cần mock HTTP.

---

## Logging convention

Mọi endpoint và handler đều dùng tag prefix nhất quán để dễ grep log:

```
[CreateExam | ExamId=42]    THÀNH CÔNG — ...
[DeleteExam | ExamId=7]     THẤT BẠI [403] — ...
[RestoreExam | ExamId=3]    THẤT BẠI [404] — ...
```

Pattern: `[FeatureName | EntityId=N]` → `STATUS [code] — chi tiết`

---

## Lợi ích thực tế khi làm việc với codebase này

- **Tìm bug nhanh**: lỗi ở `DeleteExam` → vào thư mục `DeleteExam/`, không cần đọc toàn bộ service layer
- **Thêm feature không sợ**: tạo thư mục mới, không đụng code cũ
- **Onboarding dễ**: đọc một lát cắt là hiểu toàn bộ flow của feature đó
- **Sẵn sàng tách Microservice**: mỗi feature đã là một đơn vị độc lập

---

## Tech stack

| Thành phần | Công nghệ |
|---|---|
| Framework | ASP.NET Core 8 — Minimal APIs |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Token |
| Password hashing | BCrypt.Net |
| Database | PostgreSQL |
| AI Evaluation | RabbitMQ → AI Worker service |
| File storage | Azure Blob Storage |

---

## Tham khảo

- [Vertical Slice Architecture — Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)
- [CQRS Pattern — Microsoft Docs](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)
