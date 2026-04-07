namespace EduScoring.Common.Authentication;

// Dùng static class để có thể gọi ở mọi nơi: AppRoles.Admin
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
}