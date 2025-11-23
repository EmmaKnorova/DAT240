
namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.DTOs;

public record OrderLineDto
(
    int FoodItemId,
    string FoodItemName,
    int Amount,
    decimal Price
);