namespace MCT.Repositories;

public interface IPersonRepository
{
    Task<List<Person>> GetPersons();
    Task<Person> AddPerson(Person person);
    Task<Person?> GetPerson(string id);
    Task<Person?> UpdatePerson(string id, Person person);
    Task<bool> DeletePerson(string id);
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




public async Task<Person> AddPerson(Person person)
{
    var container = _cosmosClient.GetContainer("persondb", "persons");

    // every document needs a unique id
    person.Id = Guid.NewGuid().ToString();

    var response = await container.CreateItemAsync(person);

    // response.Resource is the stored document, including anything the service added
    return response.Resource;
}

    private Container PersonsContainer => _cosmosClient.GetContainer("persondb", "persons");

    // Looks the document up by id across all partitions and reads its partition key value
    // from the container's configured partition key path, so any partition key works.
    private async Task<PartitionKey?> FindPartitionKey(string id)
    {
        var container = PersonsContainer;
        var path = (await container.ReadContainerAsync()).Resource.PartitionKeyPath;

        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter("@id", id);
        var iterator = container.GetItemQueryIterator<Newtonsoft.Json.Linq.JObject>(query);
        while (iterator.HasMoreResults)
        {
            foreach (var doc in await iterator.ReadNextAsync())
            {
                return ToPartitionKey(doc.SelectToken(path.TrimStart('/').Replace('/', '.')));
            }
        }
        return null;
    }

    private static PartitionKey ToPartitionKey(Newtonsoft.Json.Linq.JToken? token) => token?.Type switch
    {
        Newtonsoft.Json.Linq.JTokenType.String => new PartitionKey(token.ToObject<string>()),
        Newtonsoft.Json.Linq.JTokenType.Integer or Newtonsoft.Json.Linq.JTokenType.Float => new PartitionKey(token.ToObject<double>()),
        Newtonsoft.Json.Linq.JTokenType.Boolean => new PartitionKey(token.ToObject<bool>()),
        _ => PartitionKey.None
    };

    public async Task<Person?> GetPerson(string id)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter("@id", id);
        var iterator = PersonsContainer.GetItemQueryIterator<Person>(query);
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();
            var person = page.FirstOrDefault();
            if (person is not null) return person;
        }
        return null;
    }

    public async Task<Person?> UpdatePerson(string id, Person person)
    {
        var oldKey = await FindPartitionKey(id);
        if (oldKey is null) return null;

        person.Id = id;
        var container = PersonsContainer;
        var path = (await container.ReadContainerAsync()).Resource.PartitionKeyPath;
        var newKey = ToPartitionKey(Newtonsoft.Json.Linq.JObject.FromObject(person).SelectToken(path.TrimStart('/').Replace('/', '.')));

        if (newKey.Equals(oldKey.Value))
        {
            return (await container.ReplaceItemAsync(person, id, newKey)).Resource;
        }

        // the partition key value changed, so the document has to move to another partition
        var created = (await container.CreateItemAsync(person, newKey)).Resource;
        await container.DeleteItemAsync<Person>(id, oldKey.Value);
        return created;
    }

    public async Task<bool> DeletePerson(string id)
    {
        var key = await FindPartitionKey(id);
        if (key is null) return false;

        await PersonsContainer.DeleteItemAsync<Person>(id, key.Value);
        return true;
    }
}
