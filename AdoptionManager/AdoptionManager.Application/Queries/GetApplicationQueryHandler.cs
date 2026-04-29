using System;
using System.Threading;
using System.Threading.Tasks;
using AdoptionManager.Application.DTOs;
using AdoptionManager.Domain.Interfaces;
using MediatR;

namespace AdoptionManager.Application.Queries;

public class GetApplicationQueryHandler : IRequestHandler<GetApplicationQuery, ApplicationDto?>
{
    private readonly IAdoptionRepository _repository;

    public GetApplicationQueryHandler(IAdoptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationDto?> Handle(GetApplicationQuery request, CancellationToken cancellationToken)
    {
        var application = await _repository.GetByIdAsync(request.Id);
        if (application == null)
            return null;

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