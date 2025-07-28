namespace MeterReading.API.DTOs
{
    public record MeterReadingResponse(
       int Validated,
       int Failed,
       int Committed,
       string[] Errors);
}
