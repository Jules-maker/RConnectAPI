using Microsoft.AspNetCore.Mvc;
using RconnectAPI.Models;
using RconnectAPI.Services;

namespace RconnectAPI.Controllers;

[Controller]
[Route("api/[controller]")]
public class HobbyController: Controller {
    
    private readonly HobbyService _hobbyService;

    public HobbyController(HobbyService hobbyService) {
        _hobbyService = hobbyService;
    }

    [HttpGet]
    public async Task<ResponseData<Hobby>> Get(string fields = "", int limit = 10, int page = 1)
    {
        var data = await _hobbyService.GetAsync(fields, limit, page);
        var count = await _hobbyService.GetCountAsync();
        return new ResponseData<Hobby>(data, count);
    }
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Hobby>> Get(string id, string fields = "")
    {
        var hobby = await _hobbyService.GetAsync(id, fields);

        if (hobby is null)
        {
            return NotFound();
        }

        return hobby;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Hobby newHobby)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _hobbyService.CreateAsync(newHobby);

            return CreatedAtAction(nameof(Get), newHobby);
        }
        catch
        {
            // Log the exception
            return StatusCode(500, "An error occurred while creating the hobby.");
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Hobby updatedHobby)
    {
        var hobby = await _hobbyService.GetAsync(id, "");

        if (hobby is null)
        {
            return NotFound();
        }

        updatedHobby.Id = hobby.Id;

        await _hobbyService.UpdateAsync(id, updatedHobby);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var hobby = await _hobbyService.GetAsync(id, "");

        if (hobby is null)
        {
            return NotFound();
        }

        await _hobbyService.RemoveAsync(id);

        return NoContent();
    }

}