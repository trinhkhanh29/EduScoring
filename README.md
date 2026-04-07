<div align="center">

# SCIENTIFIC RESEARCH PROJECT 2026 (EduScoring_AI)

**AI & OCR-BASED AUTOMATED ESSAY GRADING SYSTEM**  
*A digital solution for automating essay assessment in education.*

<br />

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Supabase](https://img.shields.io/badge/PostgreSQL-Supabase-336791?logo=postgresql&logoColor=white)](https://supabase.com/)
[![Cloudinary](https://img.shields.io/badge/Cloudinary-Image_Storage-F5AF23?logo=cloudinary&logoColor=white)](https://cloudinary.com/)

</div>

---

<p align="center">
  <b>OVERVIEW</b><br>
</p>

## 📌 Introduction

**EduScoring** is an AI-powered system that helps teachers digitize and automatically grade essay exams.  
Students can submit their work via Web or Telegram, and the system uses OCR and LLM to analyze answers, compare them with rubrics, and generate scores with feedback.

Built with **Vertical Slice Architecture (VSA)** for scalability and maintainability.

## ✨ Key Features

- **Multi-role system**: Admin, Teacher, Student  
- **Multi-platform submission**: Web & Telegram  
- **OCR processing**: Extract text from images  
- **Smart grading**: AI-based scoring with rubrics  
- **Secure authentication**: JWT & BCrypt  
- **Cloud storage**: Cloudinary (images), Supabase (PostgreSQL)

## 🏗 Architecture

- **Auth**: Login, register, JWT, password hashing  
- **Exams**: Manage exams & grading rubrics  
- **Submissions**: Upload and track submissions  
- **Scoring**: (Phase 2) AI + OCR grading logic  
- **Common & Data**: Shared configs, middleware, EF Core  

## 🛠 Tech Stack

- **Backend**: .NET 10, ASP.NET Core Minimal API  
- **Database**: PostgreSQL (Supabase)  
- **Storage**: Cloudinary  
- **Security**: JWT, BCrypt  
- **AI/ML**: OCR + LLM (Gemini/GPT)  
- **Architecture**: Vertical Slice Architecture  

## 🚀 Getting Started

### 1. Clone repository

```bash
git clone https://github.com/trinhkhanh29/EduScoring.git
cd EduScoring
````

### 2. Configure environment

Update `appsettings.Development.json`:

* ConnectionStrings (Supabase)
* JwtSettings (Secret, Issuer, Audience)
* CloudinarySettings

### 3. Update database

```powershell
dotnet ef database update
```

### 4. Run project

```powershell
dotnet run --project EduScoring
```

Access API docs at:
`http://localhost:5261/scalar/v1`

## 👨‍💻 Author

* **Trinh Quoc Khanh**
* GitHub: [https://github.com/trinhkhanh29](https://github.com/trinhkhanh29)

```
