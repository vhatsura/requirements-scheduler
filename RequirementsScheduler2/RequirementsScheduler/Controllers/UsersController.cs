using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.Core.Service;
using RequirementsScheduler2.Extensions;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RequirementsScheduler.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private IUserService Service { get; }

        public UsersController(IUserService service)
        {
            Service = service;
        }

        // GET: api/values
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IEnumerable<User> Get()
        {
            return Service.GetAllUsers();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public User Get(int id)
        {
            return Service.GetUserById(id);
        }

        // POST api/values
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Post([FromBody]User value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message =  $"User isn't valid: {ModelState.ErrorsToString()}"});
            }

            var result = Service.AddUser(value);
            if (result)
                return Ok(new { Message = "User added successfully" });
            return BadRequest(new { Message = "The user with the same username already exists" });
        }
    }
}

