using Hall_rent.Dto;

namespace Hall_rent.Helpers;

public static class FavorCalculator
{
    public static decimal Calculate(decimal startPrice, List<FavorDto> favors)
    {
        decimal price = startPrice;

        foreach (FavorDto favorDto in favors)
        {
            price += favorDto.Price;
        }

        return price;
    }
}