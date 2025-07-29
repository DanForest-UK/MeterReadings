namespace MeterReading.Domain;

/// <summary>
/// Meter reading type
/// </summary>
public record MeterReading(
    MeterReadingId Id,
    AccountId AccountId,
    DateTime MeterReadingDateTime,
    MeterReadValue MeterReadValue)
{
    // Parameterless constructor for EF 
    private MeterReading() : this(new MeterReadingId(0), new AccountId(0), DateTime.MinValue, new MeterReadValue(0)) { }
};