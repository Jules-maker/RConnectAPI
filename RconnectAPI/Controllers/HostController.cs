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
    public async Task<List<Host>> Get(int limit = 10, int page = 1)
    {
        return await _hostService.GetAsync();
    }
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Host>> Get(string id)
    {
        var user = await _hostService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return user;
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
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, "An error occurred while creating the host.");
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Host updatedBook)
    {
        var book = await _hostService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        updatedBook.Id = book.Id;

        await _hostService.UpdateAsync(id, updatedBook);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var book = await _hostService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        await _hostService.RemoveAsync(id);

        return NoContent();
    }

}