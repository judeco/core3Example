using Serilog;

namespace Services
{
    public interface IDiagnosticsService
    {
        string Ping();
    }

    public class DiagnosticsService : IDiagnosticsService
    {
        private readonly ILogger _logger;

        public DiagnosticsService(ILogger logger)
        {
            _logger = logger;
        }

        public string Ping()
        {
            _logger.Debug("I was pinged");
            return "pong";
        }
    }
}