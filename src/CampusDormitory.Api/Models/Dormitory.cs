namespace CampusDormitory.Api.Models;

public class Dormitory
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Старое поле временно сохраняется на этапе Expand.
    // После завершения перехода его можно удалить в Contract-релизе.
    public string? Address { get; set; }

    public long? BuildingId { get; set; }
    public Building? Building { get; set; }
}
