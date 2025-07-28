namespace MeterReading.Domain
{
    public record ProcessingResult(
    int SuccessfulReadings,
    int FailedReadings,
    string[] Errors);
}
