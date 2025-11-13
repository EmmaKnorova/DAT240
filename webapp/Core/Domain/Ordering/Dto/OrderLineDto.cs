
namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Dto;

public record OrderLineDto
(
    int FoodItemId,
    string FoodItemName,
    int Amount,
    decimal Price
);