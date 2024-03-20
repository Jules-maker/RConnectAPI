using System.Net;
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
    public async Task<IActionResult> Get(string fields = "", int limit = 10, int page = 1)
    {
        try
        {
            var data = await _hobbyService.GetAsync(fields, limit, page);
            var count = await _hobbyService.GetCountAsync();
            return Ok(new ListResponseData<Hobby>(data, count));
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
        
    }
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Hobby>> Get(string id, string fields = "")
    {
        var hobby = await _hobbyService.GetAsync(id, fields);

        if (hobby is null)
        {
            return StatusCode(404);
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
            var response = new ResponseData<Hobby>(newHobby);
            return Ok(response);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, [FromBody] Hobby updatedHobby)
    {
        var hobby = await _hobbyService.GetAsync(id, "");

        if (hobby is null)
        {
            return StatusCode(404);
        }

        updatedHobby.Id = hobby.Id;

        await _hobbyService.UpdateAsync(id, updatedHobby);

        return Ok(new ResponseData<Hobby>(updatedHobby));
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var hobby = await _hobbyService.GetAsync(id, "");

        if (hobby is null)
        {
            return StatusCode(404);
        }

        await _hobbyService.RemoveAsync(id);

        return Ok(new ResponseData<string>(id + " deleted"));
    }

}