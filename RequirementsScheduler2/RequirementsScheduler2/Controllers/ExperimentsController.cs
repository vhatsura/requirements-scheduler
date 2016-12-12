using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler2.Extensions;
using RequirementsScheduler2.Models;
using RequirementsScheduler2.Repository;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RequirementsScheduler2.Controllers
{
    [Route("api/[controller]")]
    public class ExperimentsController : Controller
    {
        private readonly ExperimentsRepository Repository = new ExperimentsRepository();

        // GET: api/values
        [HttpGet]
        [Authorize]
        public IEnumerable<Experiment> Get()
        {
            return Repository.Get();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [Authorize]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        [Authorize]
        public ActionResult Post([FromBody]Experiment value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = $"Experiment isn't valid: {ModelState.ErrorsToString()}" });
            }

            Repository.Add(value);

            return Ok(new { Message = "Experiment added successfully" });
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
