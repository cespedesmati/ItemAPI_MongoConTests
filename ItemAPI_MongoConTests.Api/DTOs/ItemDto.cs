namespace ItemAPI_MongoConTests.Api.DTOs
{
    public class ItemDto
    {
        public Guid Id { get; init; }
        public string? Name { get; init; }
        public decimal Price { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
    }
}
