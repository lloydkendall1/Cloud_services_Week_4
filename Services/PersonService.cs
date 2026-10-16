namespace Howest.Functions.EX2;

public interface IPersonService
{
    Task<List<Person>> GetPersons();
}

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;

    public PersonService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<List<Person>> GetPersons() => await _personRepository.GetPersons();
}