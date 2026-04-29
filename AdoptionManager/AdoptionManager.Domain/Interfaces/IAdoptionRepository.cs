using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdoptionManager.Domain.Entities;

namespace AdoptionManager.Domain.Interfaces;

public interface IAdoptionRepository
{
    Task<AdoptionApplication?> GetByIdAsync(Guid id);
    Task<IEnumerable<AdoptionApplication>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<AdoptionApplication>> GetByPetIdAsync(Guid petId);
    Task AddAsync(AdoptionApplication application);
    Task UpdateAsync(AdoptionApplication application);
}