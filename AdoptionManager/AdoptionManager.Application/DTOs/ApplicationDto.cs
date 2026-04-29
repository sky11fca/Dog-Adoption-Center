using System;
using AdoptionManager.Domain.Enums;

namespace AdoptionManager.Application.DTOs;

public class ApplicationDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public Guid UserId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}