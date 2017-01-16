using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler.Core;
using RequirementsScheduler.Core.Model;
using RequirementsScheduler.Core.Service;
using RequirementsScheduler2.Extensions;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RequirementsScheduler2.Controllers
{
    [Route("api/[controller]")]
    public class ExperimentsController : Controller
    {
        private readonly IExperimentsService Service = new ExperimentsService();

        // GET: api/values
        [HttpGet]
        [Authorize]
        public IEnumerable<Experiment> Get()
        {
            return Service.GetAll();
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

            Service.AddExperiment(value);

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
