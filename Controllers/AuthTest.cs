using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Newtonsoft.Json;

namespace JwtAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthTestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var json = JsonConvert.SerializeObject(new
            {
                is_authenticated = true,
                user_id = Utils.GetUserId(this.Request.HttpContext)
            });
            return Content(json, "application/json");
        }
    }
}
