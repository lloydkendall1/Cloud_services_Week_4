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
}
