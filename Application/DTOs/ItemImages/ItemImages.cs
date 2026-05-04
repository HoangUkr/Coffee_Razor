namespace Application.DTOs.ItemImages
{
    public record ItemImageResponse
    {
        public int Id { get; init; }
        public string Url { get; init; } = string.Empty;
        public int ItemId { get; init; }
        public string ItemName { get; init; } = string.Empty;
        public bool IsDefault { get; init; }
        public bool IsActive { get; init; }
    }
}
