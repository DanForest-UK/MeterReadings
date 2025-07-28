namespace MeterReading.Domain;

public record MeterReading(
    MeterReadingId Id,
    AccountId AccountId,
    DateTime MeterReadingDateTime,
    MeterReadValue MeterReadValue)
{
    // Parameterless constructor for EF Core
    private MeterReading() : this(new MeterReadingId(0), new AccountId(0), DateTime.MinValue, new MeterReadValue(0)) { }
};