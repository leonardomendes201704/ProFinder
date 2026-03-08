namespace ProFinder.Application.DTOs.Geography;

public class StateOptionDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public class CityOptionDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class DistrictOptionDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class ZipCodeLookupResultDto
{
    public string ZipCode { get; set; } = string.Empty;

    public string StateCode { get; set; } = string.Empty;

    public string StateName { get; set; } = string.Empty;

    public string CityName { get; set; } = string.Empty;

    public int? CityId { get; set; }

    public string? DistrictName { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}
