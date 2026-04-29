using System;
using AdoptionManager.Application.DTOs;
using MediatR;

namespace AdoptionManager.Application.Queries;

public record GetApplicationQuery(Guid Id) : IRequest<ApplicationDto?>;