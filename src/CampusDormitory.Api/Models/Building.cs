namespace CampusDormitory.Api.Models;

public class Building
{
    public long Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
