using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.Core.Service;
using RequirementsScheduler.Library.Worker;
using RequirementsScheduler2.Extensions;
using RequirementsScheduler.Core.Worker;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RequirementsScheduler2.Controllers
{
    [Route("api/[controller]")]
    public class ExperimentsController : Controller
    {
        private IExperimentsService Service { get; }
        private IUserService UserService { get; }
        private IServiceProvider Container { get; }


        public ExperimentsController(
            IExperimentsService service,
            IUserService userService,
            IServiceProvider container)
        {
            Service = service;
            UserService = userService;
            Container = container;
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

        // POST api/values
        [HttpPost("[action]")]
        [Authorize]
        public ActionResult Test([FromBody]TestExperiment value)
        {
            var username = UserName;
            if (string.IsNullOrWhiteSpace(username))
            {
                return Forbid();
            }

            var experimentInfo = new ExperimentInfo();
            int number = 0;
            if (value.J1.Any())
            {
                experimentInfo.J1.AddRange(value.J1.Select(time => new Detail(time.A, time.B, ++number)));
            }

            if (value.J2.Any())
            {
                experimentInfo.J2.AddRange(value.J1.Select(time => new Detail(time.A, time.B, ++number)));
            }

            if (value.J12 != null)
            {
                experimentInfo.J12
                    .AddRange(value.J12.OnFirst.Zip(
                        value.J12.OnSecond,
                        (onFirst, onSecond) => new LaboriousDetail(onFirst, onSecond, ++number)));
            }

            if (value.J21 != null)
            {
                experimentInfo.J21
                    .AddRange(value.J21.OnFirst.Zip(
                        value.J21.OnSecond,
                        (onFirst, onSecond) => new LaboriousDetail(onFirst, onSecond, ++number)));
            }

            var minBoundaryRange = Enumerable.Empty<double>()
                .Append(experimentInfo.J1.Min(detail => detail.Time.A))
                .Append(experimentInfo.J2.Min(detail => detail.Time.A))
                .Append(experimentInfo.J12.Min(detail => detail.OnFirst.Time.A))
                .Append(experimentInfo.J12.Min(detail => detail.OnSecond.Time.A))
                .Append(experimentInfo.J21.Min(detail => detail.OnFirst.Time.A))
                .Append(experimentInfo.J21.Min(detail => detail.OnSecond.Time.A))
                .Min();

            var maxBoundaryRange = Enumerable.Empty<double>()
                .Append(experimentInfo.J1.Max(detail => detail.Time.A))
                .Append(experimentInfo.J2.Max(detail => detail.Time.A))
                .Append(experimentInfo.J12.Max(detail => detail.OnFirst.Time.A))
                .Append(experimentInfo.J12.Max(detail => detail.OnSecond.Time.A))
                .Append(experimentInfo.J21.Max(detail => detail.OnFirst.Time.A))
                .Append(experimentInfo.J21.Max(detail => detail.OnSecond.Time.A))
                .Max();

            var minPercentageFromA = Enumerable.Empty<double>()
                .Append(experimentInfo.J1.Min(detail => (detail.Time.B - detail.Time.A) / detail.Time.A))
                .Append(experimentInfo.J2.Min(detail => (detail.Time.B - detail.Time.A) / detail.Time.A))
                .Append(experimentInfo.J12.Min(detail => (detail.OnFirst.Time.B - detail.OnFirst.Time.A) / detail.OnFirst.Time.A))
                .Append(experimentInfo.J12.Min(detail => (detail.OnSecond.Time.B - detail.OnFirst.Time.A) / detail.OnSecond.Time.A))
                .Append(experimentInfo.J21.Min(detail => (detail.OnFirst.Time.B - detail.OnFirst.Time.A) / detail.OnFirst.Time.A))
                .Append(experimentInfo.J21.Min(detail => (detail.OnSecond.Time.B - detail.OnSecond.Time.A) / detail.OnFirst.Time.A))
                .Min();

            var maxPercentageFromA = Enumerable.Empty<double>()
                .Append(experimentInfo.J1.Max(detail => (detail.Time.B - detail.Time.A) / detail.Time.A))
                .Append(experimentInfo.J2.Max(detail => (detail.Time.B - detail.Time.A) / detail.Time.A))
                .Append(experimentInfo.J12.Max(detail => (detail.OnFirst.Time.B - detail.OnFirst.Time.A) / detail.OnFirst.Time.A))
                .Append(experimentInfo.J12.Max(detail => (detail.OnSecond.Time.B - detail.OnFirst.Time.A) / detail.OnSecond.Time.A))
                .Append(experimentInfo.J21.Max(detail => (detail.OnFirst.Time.B - detail.OnFirst.Time.A) / detail.OnFirst.Time.A))
                .Append(experimentInfo.J21.Max(detail => (detail.OnSecond.Time.B - detail.OnSecond.Time.A) / detail.OnFirst.Time.A))
                .Max();

            var experiment = new Experiment()
            {
                TestsAmount = 1,
                RequirementsAmount = number,
                N1 = (int) Math.Ceiling(number / (double) experimentInfo.J1.Count),
                N2 = (int)Math.Ceiling(number / (double)experimentInfo.J2.Count),
                N12 = (int)Math.Ceiling(number / (double)experimentInfo.J12.Count),
                N21 = (int)Math.Ceiling(number / (double)experimentInfo.J21.Count),
                MinBoundaryRange = (int)Math.Floor(minBoundaryRange),
                MaxBoundaryRange = (int)Math.Ceiling(maxBoundaryRange),
                Status = ExperimentStatus.InProgress,
                MinPercentageFromA = (int) Math.Floor(minPercentageFromA * 100),
                MaxPercentageFromA = (int) Math.Ceiling(maxPercentageFromA * 100)
            };

            experiment = Service.AddExperiment(experiment, username);

            var generatorMock = new Mock<IExperimentGenerator>();

            generatorMock.Setup(g => g.GenerateDataForTest(It.Is<Experiment>(ex => ex.Id == experiment.Id)))
                .Returns(() => experimentInfo);

            var experimentPipeline = new ExperimentPipeline(
                generatorMock.Object,
                Container.GetService<IWorkerExperimentService>(),
                Container.GetService<IExperimentTestResultService>());

            Task.Factory.StartNew(
                async () => await experimentPipeline.Run(Enumerable.Empty<Experiment>().Append(experiment)));

            return Ok(value);
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
