using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterReading.Infrastructure.Validation
{
    using System.Text;

    namespace MeterReading.Infrastructure.Logging
    {
        /// <summary>
        /// Simple exception logger that formats exceptions nicely and outputs to console
        /// Can be adapted for proper logging
        /// </summary>
        public static class ExceptionLogger
        {
            /// <summary>
            /// Logs an exception with full details including inner exceptions and stack trace
            /// </summary>
            public static void LogException(Exception exception, string? context = null)
            {
                if (exception == null) return;

                var message = FormatException(exception, context);
                Console.WriteLine(message);
            }

            /// <summary>
            /// Formats an exception with all details in a readable format
            /// </summary>
            public static string FormatException(Exception exception, string? context = null)
            {
                var sb = new StringBuilder();

                sb.AppendLine($"EXCEPTION: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
      
                if (!string.IsNullOrWhiteSpace(context))
                {
                    sb.AppendLine($"CONTEXT: {context}");
                    sb.AppendLine();
                }

                var currentException = exception;

                while (currentException != null)
                {
                    sb.AppendLine($"Type: {currentException.GetType().Name}");
                    sb.AppendLine($"Message: {currentException.Message}");

                    if (!string.IsNullOrWhiteSpace(currentException.StackTrace))
                    {
                        sb.AppendLine(currentException.StackTrace);                        
                    }

                    sb.AppendLine();
                    currentException = currentException.InnerException;
                }
                return sb.ToString();
            }
        }
    }
}
