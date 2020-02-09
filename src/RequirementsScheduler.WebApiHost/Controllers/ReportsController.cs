using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler.WebApiHost.BLL.Model;
using RequirementsScheduler.WebApiHost.BLL.Service;

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