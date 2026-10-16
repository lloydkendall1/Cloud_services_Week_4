namespace Howest.Functions.EX2;

public interface IPersonService
{
    Task<List<Person>> GetPersons();

    Task<Person> AddPerson(Person person);
    Task<Person?> GetPerson(string id);
    Task<Person?> UpdatePerson(string id, Person person);
    Task<bool> DeletePerson(string id);
}

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;

    public PersonService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<List<Person>> GetPersons()
    {
           return await _personRepository.GetPersons();

    }

    public async Task<Person> AddPerson(Person person)
    {
        return await _personRepository.AddPerson(person);
    }

    public Task<Person?> GetPerson(string id) => _personRepository.GetPerson(id);

    public Task<Person?> UpdatePerson(string id, Person person) => _personRepository.UpdatePerson(id, person);

    public Task<bool> DeletePerson(string id) => _personRepository.DeletePerson(id);
}
