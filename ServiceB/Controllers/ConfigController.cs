using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ServiceB.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private ConfigOptions _configOptions;
        public ConfigController(IConfiguration configuration, IOptions<ConfigOptions> configOptions)
        {
            _configuration = configuration;
            _configOptions = configOptions.Value;
        }

        /// <summary>
        /// ConsulOption:ConsulAddress
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("{key}")]
        public IActionResult GetValueForKey(string key)
        {
            return Ok(_configuration.GetSection(key));
        }
        [HttpGet("GetConfigOptions")]
        public ConfigOptions GetConfigOptions()
        {
            return _configOptions;
        }
    }
}