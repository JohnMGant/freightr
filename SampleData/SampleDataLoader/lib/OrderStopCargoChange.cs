public record OrderStopCargoChange
{
    public string? OrderId { get; init; }
    public int? StopSequenceNumber { get; init; }
    public int? CargoChangeSequenceNumber { get; init; }
    public string? CargoChangeType { get; init; }
    public string? GeneralDescription { get; init; }
    public double? Weight { get; init; }
    public string? TruckNumber { get; init; }
    public string? TrailerNumber { get; init; }
    public string? Driver1Name { get; init; }
    public string? Driver2Name { get; init; }
}