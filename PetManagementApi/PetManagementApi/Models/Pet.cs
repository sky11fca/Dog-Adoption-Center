namespace PetManagementApi.Models;

public class Pet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int AgeYears { get; set; }
    public string Temperament { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PetStatus Status { get; set; } = PetStatus.Available;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PetPhoto> Photos { get; set; } = [];
}
