namespace MeterReading.API.DTOs
{
    public record MeterReadingResponse(
    int SuccessfulReadings,
    int FailedReadings,
    string Errors);
}
