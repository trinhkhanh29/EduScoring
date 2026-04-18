using System.Threading.Tasks;
using EduScoring.Features.Exams.Models;
using EduScoring.Features.Submissions.Models;

namespace EduScoring.Features.Submissions.Services;

public interface IOcrService
{
    Task<string> ExtractTextFromImageAsync(string imageUrl);
}