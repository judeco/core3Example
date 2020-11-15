using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Api.Controllers
{
    public interface ITestController
    {
        string LoggerTest();
    }

    [Route("[controller]")]
    [ApiController]
    public class TestController : ITestController
    {
        private readonly ILogger _logger;

        public TestController(ILogger logger)
        {
            _logger = logger;
        }

        public string LoggerTest()
        {
            _logger.Debug("Logger test" );
            return "done";
        }



    }
}
