using Microsoft.AspNetCore.Http.HttpResults;
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
    public async Task<IActionResult> Get(string fields = "", int limit = 10, int page = 1, string searchValue = "")
    {
        var data = await _hostService.GetAsync(fields, limit, page, searchValue);
        var count = await _hostService.GetCountAsync(searchValue);
        return Ok(new ListResponseData<Host>(data, count));
    }
    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> Get(string id, string fields = "")
    {
        var host = await _hostService.GetOneAsync(id, fields);

        if (host is null)
        {
            return StatusCode(404);
        }

        var response = new ResponseData<Host>(host);
        return Ok(response);
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

            var response = new ResponseData<Host>(newHost);
            return Ok(response);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Host updatedHost)
    {
        var host = await _hostService.GetOneAsync(id, "");

        if (host is null)
        {
            return StatusCode(404);
        }

        updatedHost.Id = host.Id;

        await _hostService.UpdateAsync(id, updatedHost);
        
        var response = new ResponseData<Host>(updatedHost);
        return Ok(response);
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var host = await _hostService.GetOneAsync(id, "");

        if (host is null)
        {
            return StatusCode(404);
        }

        await _hostService.RemoveAsync(id);

        var response = new ResponseData<string>(id + " deleted");
        return Ok(response);
    }

}