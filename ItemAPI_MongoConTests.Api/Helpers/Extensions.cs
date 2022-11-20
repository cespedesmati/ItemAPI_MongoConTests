using ItemAPI_MongoConTests.Api.DTOs;
using ItemAPI_MongoConTests.Api.Models;

namespace ItemAPI_MongoConTests.Api.Helpers;

public static class Extensions
{
    public static ItemDto AsDto(this Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Price = item.Price,
            CreatedDate = item.CreatedDate
        };
    }
}
