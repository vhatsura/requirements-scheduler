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
using RequirementsScheduler.Core.Worker;
using RequirementsScheduler.Library.Extensions;
using RequirementsScheduler.Library.Worker;
using RequirementsScheduler2.Extensions;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RequirementsScheduler.Controllers
{
    [Route("api/[controller]")]
    public class ExperimentsController : Controller
    {
        private IExperimentsService Service { get; }
        private IUserService UserService { get; }
        private IExperimentTestResultService ResultService { get; }
        private IServiceProvider Container { get; }

        public ExperimentsController(
            IExperimentsService service,
            IUserService userService,
            IServiceProvider container,
            IExperimentTestResultService resultService)
        {
            Service = service;
            UserService = userService;
            ResultService = resultService;

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
        public IActionResult Get(int id)
        {
            //var username = UserName;
            
            return Ok("value");
        }

        [HttpGet("{id}/result")]
        [Authorize]
        public async Task<IActionResult> Result(Guid id)
        {
            var username = UserName;

            var experiment = Service.Get(id, username);
            if (experiment == null)
            {
                return BadRequest();
            }

            var infos = new List<ExperimentInfo>();

            for (int i = 1; i <= experiment.TestsAmount; i++)
            {
                var experimentInfo = await ResultService.GetExperimentTestResult(id, i);
                infos.Add(experimentInfo);
            }

            return Ok(infos);
        }

        [HttpGet("{id}/result/{testNumber}")]
        [Authorize]
        public async Task<IActionResult> Result(Guid id, int number)
        {
            //var username = UserName;
            var experimentInfo =  await ResultService.GetExperimentTestResult(id, number);

            return Ok(experimentInfo);
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

            var detailTimes = Enumerable.Empty<double>()
                .Concat(experimentInfo.J1.Select(detail => detail.Time.A))
                .Concat(experimentInfo.J2.Select(detail => detail.Time.A))
                .Concat(experimentInfo.J12.Select(detail => detail.OnFirst.Time.A))
                .Concat(experimentInfo.J12.Select(detail => detail.OnSecond.Time.A))
                .Concat(experimentInfo.J21.Select(detail => detail.OnFirst.Time.A))
                .Concat(experimentInfo.J21.Select(detail => detail.OnSecond.Time.A))
                .ToList();

            var minBoundaryRange = detailTimes.Min();
            var maxBoundaryRange = detailTimes.Max();

            var detailTimesPercentage = Enumerable.Empty<double>()
                .Concat(experimentInfo.J1.Select(detail => (detail.Time.B - detail.Time.A) / detail.Time.A))
                .Concat(experimentInfo.J2.Select(detail => (detail.Time.B - detail.Time.A) / detail.Time.A))
                .Concat(
                    experimentInfo.J12.Select(
                        detail => (detail.OnFirst.Time.B - detail.OnFirst.Time.A) / detail.OnFirst.Time.A))
                .Concat(
                    experimentInfo.J12.Select(
                        detail => (detail.OnSecond.Time.B - detail.OnFirst.Time.A) / detail.OnSecond.Time.A))
                .Concat(
                    experimentInfo.J21.Select(
                        detail => (detail.OnFirst.Time.B - detail.OnFirst.Time.A) / detail.OnFirst.Time.A))
                .Concat(
                    experimentInfo.J21.Select(
                        detail => (detail.OnSecond.Time.B - detail.OnSecond.Time.A) / detail.OnFirst.Time.A))
                .ToList();

            var minPercentageFromA = detailTimesPercentage.Min();
            var maxPercentageFromA = detailTimesPercentage.Max();

            var experiment = new Experiment()
            {
                TestsAmount = 1,
                RequirementsAmount = number,
                N1 = experimentInfo.J1.Count == 0 ? 0 : (int) Math.Ceiling(((double) experimentInfo.J1.Count / number) * 100),
                N2 = experimentInfo.J2.Count == 0 ? 0 : (int)Math.Ceiling(((double)experimentInfo.J2.Count / number) * 100),
                N12 = experimentInfo.J12.Count == 0 ? 0 : (int)Math.Ceiling(((double)experimentInfo.J12.Count / number) * 100),
                N21 = experimentInfo.J21.Count == 0 ? 0 : (int)Math.Ceiling(((double)experimentInfo.J21.Count / number) * 100),
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
