using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler2.Extensions;
using RequirementsScheduler2.Models;
using RequirementsScheduler2.Repository;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RequirementsScheduler2.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly UsersRepository Repository = new UsersRepository();

        // GET: api/values
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IEnumerable<User> Get()
        {
            return Repository.Get();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public User Get(int id)
        {
            return Repository.Get(id);
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

            var existedUser = Repository.Get(user => user.Username == value.Username);
            if (existedUser != null)
                return BadRequest(new { Message = "The user with the same username already exists" } );

            Repository.Add(value);

            return Ok(new { Message = "User added successfully" });
        }
    }
}

