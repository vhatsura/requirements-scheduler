using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        public UsersController(IUserService service)
        {
            Service = service;
        }

        private IUserService Service { get; }

        // GET: api/values
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IEnumerable<User> Get() => Service.GetAllUsers();

        // GET api/values/5
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public User Get(int id) => Service.GetUserById(id);

        // POST api/values
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public ActionResult Post([FromBody] User value)
        {
            if (!ModelState.IsValid)
                return BadRequest(new {Message = $"User isn't valid: {ModelState.ErrorsToString()}"});

            var result = Service.AddUser(value);
            if (result)
                return Ok(new {Message = "User added successfully"});
            return BadRequest(new {Message = "The user with the same username already exists"});
        }
    }
}