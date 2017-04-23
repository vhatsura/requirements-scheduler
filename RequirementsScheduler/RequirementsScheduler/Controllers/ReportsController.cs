using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;

namespace RequirementsScheduler2.Controllers
{
    [Route("api/[controller]")]
    public class ReportsController : Controller
    {
        private IReportsService ReportsService { get; }

        public ReportsController(IReportsService reportsService)
        {
            ReportsService = reportsService;
        }

        [HttpGet]
        public IEnumerable<ExperimentReport> Get()
        {
            return ReportsService.GetAll();
        }
    }
}
