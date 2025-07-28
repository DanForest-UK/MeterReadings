namespace MeterReading.Domain
{
    public record ProcessingResult(
       int Validated,
       int Failed,
       int Committed,
       string[] Errors);
}
