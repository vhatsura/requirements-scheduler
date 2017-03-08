using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.Core.Service;
using RequirementsScheduler2.Extensions;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RequirementsScheduler2.Controllers
{
    [Route("api/[controller]")]
    public class ExperimentsController : Controller
    {
        private IExperimentsService Service { get; }

        public ExperimentsController(IExperimentsService service)
        {
            Service = service;
        }

        // GET: api/values
        [HttpGet]
        [Authorize]
        public IEnumerable<Experiment> Get()
        {
            var username = UserName;
            return string.IsNullOrWhiteSpace(username) ?
                Enumerable.Empty<Experiment>() :
                Service.GetAll(username);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [Authorize]
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet("[action]/{status}")]
        [Authorize]
        public IActionResult GetByStatus(string status)
        {
            var username = UserName;
            if (string.IsNullOrWhiteSpace(username))
            {
                return new ObjectResult(Enumerable.Empty<Experiment>());
            }

            if (Enum.TryParse(status, true, out ExperimentStatus experimentStatus))
            {
                return new ObjectResult(Service.GetByStatus(experimentStatus, username));
            }

            return BadRequest(new { Message = "Invalid status of experiment" });
        }

        // POST api/values
        [HttpPost]
        [Authorize]
        public ActionResult Post([FromBody]Experiment value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = $"Experiment isn't valid: { ModelState.ErrorsToString() }" });
            }

            var username = UserName;
            if (string.IsNullOrWhiteSpace(username))
            {
                return Forbid();
            }

            var experiment = Service.AddExperiment(value, username);

            return Ok(experiment);
        }

        private string UserName
        {
            get
            {
                var identity = User.Identity as ClaimsIdentity;
                return identity?.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            }
        }
    }
}
