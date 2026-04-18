using System;
using System.Collections.Generic;

namespace EduScoring.Features.Submissions.Features.UploadSubmissionImages;

public record UploadSubmissionImagesRequest(List<string> ImageUrls);

public record UploadSubmissionImagesCommand(
    Guid SubmissionId,
    List<string> ImageUrls,
    Guid UserId,
    bool IsAdmin);

public record UploadSubmissionImagesResponse(string Message);

public record SubmissionImagesUploadedEvent(Guid SubmissionId);