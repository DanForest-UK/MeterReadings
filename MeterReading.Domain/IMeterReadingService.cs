using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterReading.Domain
{
    /// <summary>
    /// Interface for dependency injection for meter reading service
    /// </summary>
    public interface IMeterReadingService
    {
        Task<ProcessingResult> ProcessMeterReadingsAsync(Stream csvStream);
    }
}
