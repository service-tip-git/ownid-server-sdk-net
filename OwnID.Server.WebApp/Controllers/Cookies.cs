using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OwnID.Server.WebApp.Controllers
{
   
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class Cookies : ControllerBase
    {
        private readonly WebAppOptions _webAppOptions;

        public Cookies(IOptions<WebAppOptions> webAppOptions)
        {
            _webAppOptions = webAppOptions.Value;
        }
        
        /// <summary>
        ///     Add new HttpOnly cookie
        /// </summary>
        /// <param name="name">cookie name</param>
        /// <param name="value">cookie value</param>
        /// <returns></returns>
        /// <response code="204">Cookie has been set successfully</response>
        /// <response code="415">Cookie value is missed or in the wrong format</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        [HttpPut]
        [Route("{name}")]
        public IActionResult Put(string name, [FromBody] string value)
        {
            if (Request.Cookies.ContainsKey(name))
                Response.Cookies.Delete(name);

            Response.Cookies.Append(name, value, new CookieOptions
            {
                Expires = DateTime.Now.AddDays(_webAppOptions.CookieExpiration),
                Secure = !_webAppOptions.IsDevEnvironment,
                Path = "/cookies",
                HttpOnly = true,
                SameSite = SameSiteMode.Strict
            });

            return new NoContentResult();
        }

        /// <summary>
        ///     Get cookie
        /// </summary>
        /// <param name="name">cookie name</param>
        /// <returns>cookie value if any has been found, otherwise 404</returns>
        /// <response code="200">Returns cookie value</response>
        /// <response code="404">Cookie not found</response>            
        [HttpGet]
        [Route("{name}")]
        public IActionResult Get(string name)
        {
            if (!Request.Cookies.ContainsKey(name))
                return NotFound();

            return new JsonResult(Request.Cookies[name]);
        }
    }
}