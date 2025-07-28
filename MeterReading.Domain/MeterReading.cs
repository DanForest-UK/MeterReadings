namespace MeterReading.Domain;

public record MeterReading(
    MeterReadingId Id,
    AccountId AccountId,
    DateTime MeterReadingDateTime,
    MeterReadValue MeterReadValue);