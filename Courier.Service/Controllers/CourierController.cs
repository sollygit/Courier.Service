using Courier.Service.Interfaces;
using Courier.Service.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Courier.Service.Controllers
{
    [Route("api/[controller]")]
    public class CourierController : ControllerBase
    {
        private readonly IEventBusService<CourierRequest> service;

        public CourierController(IEventBusService<CourierRequest> service)
        {
            this.service = service;
        }

        [HttpPost("Pickup")]
        public async Task<IActionResult> Pickup([FromBody] CourierRequest request)
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest("Courier Pickup request could not be parsed");

            await service.Process(request);

            return Ok();
        }
    }
}
