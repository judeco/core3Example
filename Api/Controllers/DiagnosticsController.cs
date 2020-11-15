using Microsoft.AspNetCore.Mvc;
using Serilog;
using Services;

namespace Api.Controllers
{
    [Route("[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        // We can use the nice Serilog type format instead of Microsoft ILogger<someclass> here
        // Serilog will support the ILogger<someclass> as well if you must
        private readonly ILogger _logger;

        private readonly IDiagnosticsService _diagnosticsService;

        public DiagnosticsController(ILogger logger, IDiagnosticsService diagnosticsService)
        {
            _logger = logger;
            _diagnosticsService = diagnosticsService;
        }

        // Sijmple AmI alive endpoint. Should have no access restrictions
        [HttpGet("[action]")]
        public ActionResult<string> Ping()
        {
            return Ok(_diagnosticsService.Ping());
        }

        // Simply check that the log output is working as expected
        [HttpGet("[action]")]
        public string TestLogging()
        {
            var msg = "Test log message for {Level} level";

            _logger.Fatal(msg, "fatal");
            _logger.Error(msg, "error");
            _logger.Warning(msg, "warning");
            _logger.Information(msg, "information");
            _logger.Debug(msg, "debug");
            _logger.Verbose(msg, "verbose");
            return "Test messages sent to logger. Check log output";
        }


    }
}