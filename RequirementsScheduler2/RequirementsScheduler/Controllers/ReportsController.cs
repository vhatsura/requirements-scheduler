using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler2.Controllers
{
    [Route("api/[controller]")]
    public class ReportsController : Controller
    {
        [HttpGet]
        public IEnumerable<ExperimentReport> Get()
        {
            //todo get from service
            return new List<ExperimentReport>()
            {
                new ExperimentReport()
                {
                    Id = 1,
                    Stop1 = 26,
                    Stop2 = 14,
                    Stop3 = 37,
                    Stop4 = 23,
                    ConflictsAmount = 281,
                    ConflictsResolutionAmount = 193,
                    ExecutionTime = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(37)).TotalSeconds,
                    DeltaCmaxAverage = 36.5f,
                    DeltaCmaxMax = 51
                },
                new ExperimentReport()
                {
                    Id = 2,
                    Stop1 = 48,
                    Stop2 = 20,
                    Stop3 = 17,
                    Stop4 = 15,
                    ConflictsAmount = 83,
                    ConflictsResolutionAmount = 64,
                    ExecutionTime = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(52)).TotalSeconds,
                    DeltaCmaxAverage = 41.44f,
                    DeltaCmaxMax = 87
                }
            };
        }
    }
}
