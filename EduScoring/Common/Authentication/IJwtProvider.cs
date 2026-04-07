using EduScoring.Data.Entities;

namespace EduScoring.Common.Authentication;

public interface IJwtProvider
{
    string GenerateToken(User user);
}