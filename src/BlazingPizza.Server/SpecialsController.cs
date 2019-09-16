using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingPizza.Server
{
    [Route("specials")]
    [ApiController]
    public class SpecialsController : Controller
    {
        private readonly PizzaStoreContext _db;

        public SpecialsController(PizzaStoreContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<PizzaSpecial>>> GetSpecials()
        {
            // See: https://github.com/dotnet-presentations/blazor-workshop/pull/143/commits/3f3ea64a25ae42040bd52a2338449a2dd776e385
            return await _db.Specials.OrderByDescending(s => (float)s.BasePrice).ToListAsync();
        }
    }
}
