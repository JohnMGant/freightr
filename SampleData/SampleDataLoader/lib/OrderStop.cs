public record OrderStop
{
    public string? OrderId { get; init; }
    public int StopSequenceNumber { get; init; }
    public string? StopType { get; init; }
    public string? CustomerId { get; init; }
    public string? LocationId { get; init; }
    public DateTime? MinExpectedArrivalTime { get; init; }
    public DateTime? MaxExpectedArrivalTime { get; init; }
    public DateTime? ActualArrivalTime { get; init; }
    public DateTime? DepartureTime { get; init; }
    public decimal? Distance { get; init; }
    public decimal? ChargePerDistanceUnit { get; init; }
}