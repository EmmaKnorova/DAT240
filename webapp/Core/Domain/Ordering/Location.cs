using Microsoft.EntityFrameworkCore;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;

[Owned]
public class Location
{
    public string Building { get; init; }
    public string RoomNumber { get; init; }
    public string? Notes { get; init; }
}