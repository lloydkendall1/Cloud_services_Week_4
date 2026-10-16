using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Howest.Functions.EX2;

public class PersonTrigger
{
    private readonly ILogger<PersonTrigger> _logger;
    private readonly IPersonService _personService;

    public PersonTrigger(ILogger<PersonTrigger> logger, IPersonService personService)
    {
        _logger = logger;
        _personService = personService;
    }

    [Function("GetPersons")]
    public async Task<IActionResult> GetPersons([HttpTrigger(AuthorizationLevel.Function, "get", Route = "persons")] HttpRequest req)
    {
        _logger.LogInformation("Fetching all persons from Cosmos DB.");
        var persons = await _personService.GetPersons();
        return new OkObjectResult(persons);
    }

    [Function("AddPerson")]
    public async Task<IActionResult> AddPerson([HttpTrigger(AuthorizationLevel.Function, "post", Route = "persons")] HttpRequest req)
    {
        using var reader = new StreamReader(req.Body);
        var body = await reader.ReadToEndAsync();

        Person? person;
        try
        {
            person = JsonConvert.DeserializeObject<Person>(body);
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Request body is not valid JSON.");
        }

        if (person is null || string.IsNullOrWhiteSpace(person.FirstName) || string.IsNullOrWhiteSpace(person.LastName))
        {
            return new BadRequestObjectResult("firstName and lastName are required.");
        }

        var created = await _personService.AddPerson(person);
        _logger.LogInformation("Added person {Id}.", created.Id);
        return new ObjectResult(created) { StatusCode = StatusCodes.Status201Created };
    }

    [Function("GetPerson")]
    public async Task<IActionResult> GetPerson([HttpTrigger(AuthorizationLevel.Function, "get", Route = "persons/{id}")] HttpRequest req, string id)
    {
        var person = await _personService.GetPerson(id);
        return person is null ? new NotFoundObjectResult($"Person {id} not found.") : new OkObjectResult(person);
    }

    [Function("UpdatePerson")]
    public async Task<IActionResult> UpdatePerson([HttpTrigger(AuthorizationLevel.Function, "put", Route = "persons/{id}")] HttpRequest req, string id)
    {
        using var reader = new StreamReader(req.Body);
        var body = await reader.ReadToEndAsync();

        Person? person;
        try
        {
            person = JsonConvert.DeserializeObject<Person>(body);
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Request body is not valid JSON.");
        }

        if (person is null || string.IsNullOrWhiteSpace(person.FirstName) || string.IsNullOrWhiteSpace(person.LastName))
        {
            return new BadRequestObjectResult("firstName and lastName are required.");
        }

        var updated = await _personService.UpdatePerson(id, person);
        return updated is null ? new NotFoundObjectResult($"Person {id} not found.") : new OkObjectResult(updated);
    }

    [Function("DeletePerson")]
    public async Task<IActionResult> DeletePerson([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "persons/{id}")] HttpRequest req, string id)
    {
        var deleted = await _personService.DeletePerson(id);
        return deleted ? new NoContentResult() : new NotFoundObjectResult($"Person {id} not found.");
    }
}
