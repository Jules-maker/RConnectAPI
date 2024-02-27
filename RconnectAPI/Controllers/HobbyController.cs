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
    public async Task<List<Hobby>> Get(int limit = 10, int page = 1)
    {
        return await _hobbyService.GetAsync(limit, page);
    }
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Hobby>> Get(string id)
    {
        var user = await _hobbyService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return user;
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
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, "An error occurred while creating the hobby.");
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Hobby updatedBook)
    {
        var book = await _hobbyService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        updatedBook.Id = book.Id;

        await _hobbyService.UpdateAsync(id, updatedBook);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var book = await _hobbyService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        await _hobbyService.RemoveAsync(id);

        return NoContent();
    }

}