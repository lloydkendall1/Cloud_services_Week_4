namespace Howest.Functions.EX2;
public class Person
{
    [JsonProperty("id")]
    public string Id { get; set; } = "";

    [JsonProperty("firstName")]
    public string FirstName { get; set; } = "";

    [JsonProperty("lastName")]
    public string LastName { get; set; } = "";

    [JsonProperty("eMail")]
    public string Email { get; set; } = "";

    [JsonProperty("age")]
    public int Age { get; set; }

    [JsonProperty("locations")]
    public List<Location> Locations { get; set; } = new();
}

public class Location
{
    [JsonProperty("city")]
    public string City { get; set; } = "";

    [JsonProperty("street")]
    public string Street { get; set; } = "";
}