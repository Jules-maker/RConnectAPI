using Microsoft.AspNetCore.Mvc;
using RconnectAPI.Models;
using RconnectAPI.Services;
using Host = RconnectAPI.Models.Host;

namespace RconnectAPI.Controllers;

[Controller]
[Route("api/[controller]")]
public class HostController : Controller
{
    private readonly HostService _hostService;

    public HostController(HostService hostService)
    {
        _hostService = hostService;
    }

    [HttpGet]
    public async Task<ResponseData<Host>> Get(string fields = "", int limit = 10, int page = 1, string searchValue = "")
    {
        var data = await _hostService.GetAsync(fields, limit, page, searchValue);
        var count = await _hostService.GetCountAsync(searchValue);
        return new ResponseData<Host>(data, count);
    }
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Host>> Get(string id, string fields = "")
    {
        var host = await _hostService.GetAsync(id, fields);

        if (host is null)
        {
            return NotFound();
        }

        return host;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Host newHost)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _hostService.CreateAsync(newHost);

            return CreatedAtAction(nameof(Get), newHost);
        }
        catch
        {
            // Log the exception
            return StatusCode(500, "An error occurred while creating the host.");
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Host updatedHost)
    {
        var host = await _hostService.GetAsync(id, "");

        if (host is null)
        {
            return NotFound();
        }

        updatedHost.Id = host.Id;

        await _hostService.UpdateAsync(id, updatedHost);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var host = await _hostService.GetAsync(id, "");

        if (host is null)
        {
            return NotFound();
        }

        await _hostService.RemoveAsync(id);

        return NoContent();
    }

}