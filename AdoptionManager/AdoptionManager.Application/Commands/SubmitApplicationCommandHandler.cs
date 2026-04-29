using System;
using System.Threading;
using System.Threading.Tasks;
using AdoptionManager.Application.DTOs;
using AdoptionManager.Domain.Entities;
using AdoptionManager.Domain.Enums;
using AdoptionManager.Domain.Interfaces;
using MediatR;

namespace AdoptionManager.Application.Commands;

public class SubmitApplicationCommandHandler : IRequestHandler<SubmitApplicationCommand, ApplicationDto>
{
    private readonly IAdoptionRepository _repository;

    public SubmitApplicationCommandHandler(IAdoptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationDto> Handle(SubmitApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = new AdoptionApplication
        {
            Id = Guid.NewGuid(),
            PetId = request.PetId,
            UserId = request.UserId,
            ApplicantName = request.ApplicantName,
            ApplicantEmail = request.ApplicantEmail,
            Justification = request.Justification,
            Status = ApplicationStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(application);

        return new ApplicationDto
        {
            Id = application.Id,
            PetId = application.PetId,
            UserId = application.UserId,
            ApplicantName = application.ApplicantName,
            ApplicantEmail = application.ApplicantEmail,
            Justification = application.Justification,
            Status = application.Status.ToString(),
            SubmittedAt = application.SubmittedAt
        };
    }
}