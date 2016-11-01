using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RequirementsScheduler.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RequirementsScheduler.API
{
    [Route("api/[controller]")]
    public class ReportsController : Controller
    {
        // GET: api/values
        [HttpGet]
        public IEnumerable<Report> Get()
        {
            return new List<Report>
            {
                new Report() { Id = 1, Title = "Report 1"},
                new Report() { Id = 2, Title = "Report 2" },
                new Report() { Id = 3, Title = "Report 3"}
            };
        }
    }
}
