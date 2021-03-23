using Business.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionsController : ControllerBase
    {
        private readonly ICompetitionService _competitionService;
        public CompetitionsController(ICompetitionService competitionService)
        {
            _competitionService = competitionService;
        }
        [HttpGet("getcompetitionbylink")]
        //[HttpPost]
        //[Route("get")]
        public async Task<IActionResult> Get(string url)
        {
            if (string.IsNullOrEmpty(url)) return BadRequest();

            var competition = await this._competitionService.GetDetail(url);

            if (competition == null) return NotFound();

            return Ok(competition);
        }
        [HttpGet("getcompetitionsbyproductname")]
        public async Task<IActionResult> GetProducts(string productName)
        {
            if (string.IsNullOrEmpty(productName)) return BadRequest();

            var competitions = await _competitionService.GetProducts(productName.Trim());

            if (competitions == null) return NotFound();

            return Ok(competitions);
        }
    }
}
