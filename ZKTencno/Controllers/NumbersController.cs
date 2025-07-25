using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ZKTencno.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NumbersController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<int> Get()
        {
            // Returns numbers 1 to 10
            for (int i = 1; i <= 10; i++)
            {
                yield return i;
            }
        }
    }
}
