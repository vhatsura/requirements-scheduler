using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;

namespace RequirementsScheduler2.Controllers
{
    [Route("api/[controller]")]
    public class ReportsController : Controller
    {
        public ReportsController(IReportsService reportsService)
        {
            ReportsService = reportsService;
        }

        private IReportsService ReportsService { get; }

        [HttpGet]
        public IEnumerable<ExperimentReport> Get() => ReportsService.GetAll();
    }
}