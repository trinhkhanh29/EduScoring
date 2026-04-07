# ĐỀ TÀI NGHIÊN CỨU KHOA HỌC 2026 (EduScoring_AI)
<b>HỆ THỐNG QUẢN LÝ VÀ CHẤM ĐIỂM TỰ ĐỘNG ỨNG DỤNG AI & OCR</b>
<i>Giải pháp chuyển đổi số trong giáo dục, tự động hóa quy trình chấm bài thi tự luận.</i>

<img src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet" alt="Dotnet 10"><img src="https://img.shields.io/badge/PostgreSQL-Supabase-336791?logo=postgresql" alt="Supabase"><img src="https://img.shields.io/badge/Cloudinary-Image_Storage-F5AF23?logo=cloudinary" alt="Cloudinary">
</p>

<p align="center">
  <b>SƠ ĐỒ TỔNG QUAN</b><br>
</p>

## 📌 Giới thiệu

**EduScoring** là hệ thống quản lý học tập tích hợp trí tuệ nhân tạo (AI) giúp hỗ trợ giảng viên trong việc số hóa và chấm điểm bài thi tự luận. Thay vì chấm tay hàng trăm bài thi, hệ thống cho phép sinh viên nộp ảnh bài làm qua Web hoặc Telegram, sau đó sử dụng công nghệ OCR và LLM để phân tích nội dung, đối chiếu đáp án (Rubrics) và đưa ra điểm số cùng nhận xét chi tiết.

Dự án được xây dựng trên kiến trúc **Vertical Slice Architecture (VSA)** hiện đại, giúp hệ thống dễ dàng mở rộng và bảo trì.

## ✨ Chức năng nổi bật

  - **Quản lý đa người dùng**: Phân quyền Admin (Hệ thống), Teacher (Quản lý đề/chấm điểm), Student (Nộp bài/Xem kết quả).
  - **Nộp bài đa nền tảng**: Hỗ trợ upload ảnh trực tiếp từ Web App hoặc thông qua Telegram Bot.
  - **Số hóa bài thi (OCR)**: Tự động trích xuất nội dung từ ảnh chụp bài làm của sinh viên (Sử dụng LMM/OCR).
  - **Chấm điểm thông minh**: Đối chiếu nội dung bài làm với bảng tiêu chí (Rubrics) để đưa ra điểm số khách quan.
  - **Bảo mật JWT**: Hệ thống đăng nhập và phân quyền sử dụng JSON Web Token bảo mật chuẩn Enterprise.
  - **Lưu trữ đám mây**: Tích hợp Cloudinary để quản lý kho ảnh bài thi và Supabase cho cơ sở dữ liệu PostgreSQL.

## 🏗 Kiến trúc hệ thống (Vertical Slice)

Hệ thống được tổ chức theo các lát cắt nghiệp vụ (Slices):

  - **Auth Slice**: Đăng ký, đăng nhập, cấp phát Token và băm mật khẩu BCrypt.
  - **Exams Slice**: Giảng viên tạo đề thi, quản lý bảng tiêu chí chấm điểm (Rubrics).
  - **Submissions Slice**: Sinh viên nộp ảnh bài làm, quản lý trạng thái chấm điểm.
  - **Scoring Slice**: (Phase 2) Xử lý logic AI, OCR và tính toán điểm số.
  - **Common & Data**: Cấu hình dùng chung, Middleware và Entity Framework Core.

## 🛠 Công nghệ sử dụng

  - **Backend**: .NET 10 (C\#), ASP.NET Core Minimal API.
  - **Database**: PostgreSQL (Hosted on **Supabase**).
  - **Storage**: **Cloudinary** (Image Management).
  - **Security**: JWT Authentication, BCrypt Password Hashing.
  - **AI/ML**: OCR Engines, LLM (Gemini/GPT) cho việc phân tích ngữ nghĩa bài làm.
  - **Architecture**: Vertical Slice Architecture, Entity Framework Core.

## 🚀 Hướng dẫn chạy dự án

### 1\. Clone dự án

```bash
git clone https://github.com/trinhkhanh29/EduScoring.git
cd EduScoring
```

### 2\. Cấu hình môi trường

Mở file `appsettings.Development.json` và điền các thông tin sau:

  - **ConnectionStrings**: Link kết nối Supabase của bạn.
  - **JwtSettings**: Secret key (độ dài \> 32 ký tự), Issuer và Audience.
  - **CloudinarySettings**: CloudName, ApiKey và ApiSecret.

### 3\. Cập nhật Cơ sở dữ liệu

```powershell
dotnet ef database update
```

### 4\. Chạy Backend

```powershell
dotnet run --project EduScoring
```

Truy cập `http://localhost:5261/scalar/v1` để xem tài liệu API chi tiết qua giao diện Scalar.

## 📸 Demo luồng hoạt động

\<p align="center"\>
\<b\>Giao diện API (Scalar Docs)\</b\><br>
\<i\>Hệ thống Endpoint được tổ chức theo từng Feature nghiệp vụ.\</i\>
\</p\>

\<p align="center"\>
\<b\>Luồng nộp bài (Postman Test)\</b\><br>
\<i\>Sinh viên đẩy ảnh lên và nhận kết quả upload Real-time.\</i\>
\</p\>

## 👨‍💻 Tác giả & Nhóm nghiên cứu

  * **Tác giả:** Trịnh Quốc Khánh
  * **GitHub:** [trinhkhanh29](https://github.com/trinhkhanh29)
