namespace MCT.Repositories;

public interface IPersonRepository
{
    Task<List<Person>> GetPersons();
}

public class PersonRepository : IPersonRepository
{
    private readonly CosmosClient _cosmosClient;

    public PersonRepository(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<List<Person>> GetPersons()
    {
        // 'persondb' is the database, 'persons' the container
        var container = _cosmosClient.GetContainer("persondb", "persons");

        var query = container.GetItemQueryIterator<Person>();
        List<Person> results = new List<Person>();

        // results come back in pages - keep reading until there are none left
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results;
    }
}