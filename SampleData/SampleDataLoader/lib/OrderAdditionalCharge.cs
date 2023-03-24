public record OrderAdditionalCharge()
{
    public string? OrderId { get; init; }
    public int? AdditionalChargeSequenceNumber { get; init; }
    public string? ChargeDescription { get; init; }
    public decimal? ChargeAmount { get; init; }
}