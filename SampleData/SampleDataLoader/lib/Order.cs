public record Order
{
    public string? OrderId { get; init; }
    public string? BillOfLadingNumber { get; init; }
    public string? PayerCustomerId { get; init; }
    public string? CustomerReferenceNumber { get; init; }
    public string? CarrierId { get; init; }
    public DateTime? OrderPlacedDate { get; init; }
    public string? OrderStatus { get; init; }
    public DateTime? OrderClosedDate { get; init; }
    public string? LoadType { get; init; }
}