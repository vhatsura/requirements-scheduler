using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler2.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RequirementsScheduler2.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        // POST api/values
        [HttpPost]
        public void Post([FromBody]User value)
        {
        }
    }
}
