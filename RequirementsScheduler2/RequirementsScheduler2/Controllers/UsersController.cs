using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler2.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RequirementsScheduler2.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private static readonly BlockingCollection<User> UsersCollection = new BlockingCollection<User>()
        {
            new User() { Id = 1, Username = "admin", Password = "admin" }
        };

        // GET: api/values
        [HttpGet]
        public IEnumerable<User> Get()
        {
            return UsersCollection;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public User Get(int id)
        {
            return UsersCollection.FirstOrDefault(user => user.Id == id);
        }

        // POST api/authenticate
        [HttpPost("authenticate")]
        public ActionResult Authenticate([FromBody]User user)
        {
            var loginUser = UsersCollection.FirstOrDefault(u => u.Username == user.Username);
            if (loginUser == null)
            {
                return Ok();
            }
            if (loginUser.Password == user.Password)
            {
                var tokenResponse = new { Token = "fake-jwt-token" };
                return Ok(tokenResponse);
            }
            return Ok();
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]User value)
        {
            if (!ModelState.IsValid) return;

            value.Id = UsersCollection.Max(user => user.Id) + 1;
            UsersCollection.Add(value);
        }

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
