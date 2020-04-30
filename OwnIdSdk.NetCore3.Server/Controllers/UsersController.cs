using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace OwnIdSdk.NetCore3.Server.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public UsersController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        
        // GET
        public async Task<IActionResult> Index()
        {
            var a = new SampleChallengeHandler(_httpClientFactory, _configuration);
            try
            {
                await a.OnSuccessLoginAsync("did:idw:89e7fc58-be21-48ea-a06e-66bc5aa6d0e1", Response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            return Ok();
        }
    }
}