using System;
using AdoptionManager.Application.DTOs;
using MediatR;

namespace AdoptionManager.Application.Commands;

public record SubmitApplicationCommand(
    Guid PetId,
    Guid UserId,
    string ApplicantName,
    string ApplicantEmail,
    string Justification) : IRequest<ApplicationDto>;