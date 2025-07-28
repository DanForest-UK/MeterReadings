namespace MeterReading.API.DTOs
{
    public record ProcessingResult(
    int SuccessfulReadings,
    int FailedReadings,
    string[] Errors);
}
