using Courier.Service.Interfaces;
using Courier.Service.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Courier.Service.Controllers
{
    [Route("api/100/[controller]")]
    public class CourierController : ControllerBase
    {
        private readonly IEventBus<CourierRequest> bus;

        public CourierController(IEventBus<CourierRequest> bus)
        {
            this.bus = bus;
        }

        [HttpPost("Pickup")]
        public async Task<IActionResult> Pickup([FromBody] CourierRequest request)
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest("Courier Pickup request could not be parsed");

            await bus.Process(request);

            return Ok();
        }
    }
}
