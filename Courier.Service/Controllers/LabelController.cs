using Courier.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Courier.Service.Controllers
{
    [Route("api/[controller]")]
    public class LabelController : ControllerBase
    {
        private readonly IParcelLabelService labelService;

        public LabelController(IParcelLabelService labelService)
        {
            this.labelService = labelService;
        }

        [HttpGet("{consignmentId}/Download")]
        public async Task<IActionResult> Download([FromRoute] string consignmentId, [FromQuery] string username)
        {
            if (string.IsNullOrEmpty(consignmentId))
                return BadRequest("ConsignmentId is required");

            if (string.IsNullOrEmpty(username))
                return BadRequest("Username is required");

            var fileContents = await labelService.Download(consignmentId, username);

            return File(fileContents, "application/octet-stream", $"label_{consignmentId}.pdf");
        }
    }
}
