namespace MeterReading.Domain
{
    /// <summary>
    /// Meter reading processing result type
    /// </summary>
    public record ProcessingResult(
       int Validated,
       int Failed,
       int Committed,
       string[] Errors);
}
