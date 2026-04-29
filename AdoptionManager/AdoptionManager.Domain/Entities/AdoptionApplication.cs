using System;
using AdoptionManager.Domain.Enums;
using Newtonsoft.Json;

namespace AdoptionManager.Domain.Entities;

public class AdoptionApplication
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public Guid UserId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}