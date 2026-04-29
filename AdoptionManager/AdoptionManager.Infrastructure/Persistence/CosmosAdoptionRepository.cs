using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdoptionManager.Domain.Entities;
using AdoptionManager.Domain.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace AdoptionManager.Infrastructure.Persistence;

public class CosmosAdoptionRepository : IAdoptionRepository
{
    private readonly Container _container;

    public CosmosAdoptionRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<AdoptionApplication?> GetByIdAsync(Guid id)
    {
        try
        {
            ItemResponse<AdoptionApplication> response = await _container.ReadItemAsync<AdoptionApplication>(
                id.ToString(), 
                new PartitionKey(id.ToString()));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<AdoptionApplication>> GetByUserIdAsync(Guid userId)
    {
        var query = _container.GetItemLinqQueryable<AdoptionApplication>()
            .Where(a => a.UserId == userId)
            .ToFeedIterator();

        var results = new List<AdoptionApplication>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }
        return results;
    }

    public async Task<IEnumerable<AdoptionApplication>> GetByPetIdAsync(Guid petId)
    {
        var query = _container.GetItemLinqQueryable<AdoptionApplication>()
            .Where(a => a.PetId == petId)
            .ToFeedIterator();

        var results = new List<AdoptionApplication>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }
        return results;
    }

    public async Task AddAsync(AdoptionApplication application)
    {
        // Need a string Id for Cosmos
        // Setting Newtonsoft.Json attributes would be ideal, but Cosmos V3 uses System.Text.Json by default with some options, 
        // actually Cosmos SDK V3 uses Newtonsoft.Json by default unless configured otherwise.
        // We will add an id property implicitly if it's named 'id' lowercase in json, or 'Id' works if properly mapped.
        await _container.CreateItemAsync(application, new PartitionKey(application.Id.ToString()));
    }

    public async Task UpdateAsync(AdoptionApplication application)
    {
        await _container.UpsertItemAsync(application, new PartitionKey(application.Id.ToString()));
    }
}