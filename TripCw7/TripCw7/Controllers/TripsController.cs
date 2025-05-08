using Microsoft.AspNetCore.Mvc;
using TripCw7.Models.DTOs;
using TripCw7.Services;

namespace TripCw7.Controllers;

[ApiController]
[Route("api")]
public class TripsController(ITripService service) : ControllerBase
{
    [HttpGet("trips")]
    public async Task<IActionResult> GetAllTripsAndCountriesAsync()
    {
        return Ok(await service.GetAllTripsAndCountriesAsync());
    }

    [HttpGet("clients/{id}/trips")]
    public async Task<IActionResult> GetAllClientsTripsAsync(int id)
    {
        //walidacja
        if (!await service.CheckIfClientExistsAsync(id))
        {
            return NotFound($"client with id {id} not found");
        }
        
        //zwrot
        return Ok(await service.GetClientTripsAsync(id));
        
        
    }

    [HttpPost("clients")]
    public async Task<IActionResult> AddClientAsync([FromBody] ClientCreateDTO clientCreateDto)
    { //zwrot
        var client = await service.CreateClientAsync(clientCreateDto);
        return Created("", client);
    }





    [HttpPut("clients/{id}/trips/{tripId}")]
    public async Task<IActionResult> PutClientToTripAsync(int id, int tripId)
    {   //walidacja
        if (!await service.CheckIfClientExistsAsync(id))
        {
            return NotFound($"Client with id {id} not found");
        }

        if (!await service.CheckIfTripExistsAsync(tripId))
        {
            return NotFound($"Trip with id {tripId} not found");
        }

        
        if (await service.CheckIfClientWithTripExistsAsync(id, tripId))
        {
            return Conflict($"client with  id {id} already sign in trip with id {tripId}");
        }

       
        if (!await service.CheckIfTripHaveFreeSpaceAsync(tripId))
        {
            return Conflict($"Trip with id {tripId} has not enough free space");
        }

        
        //zwrot
        await service.PutClientToTripAsync(id, tripId);
        return Created();

    }

    [HttpDelete("clients/{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteClientFromTripAsync(int id, int tripId)
    {
        //walidacja
        if (!await service.CheckIfClientExistsAsync(id))
        {
            return NotFound($"Client with id {id} not found");
        }

        if (!await service.CheckIfTripExistsAsync(tripId))
        {
            return NotFound($"Trip with id {tripId} not found");
        }

        if (!await service.CheckIfClientWithTripExistsAsync(id, tripId))
        {
            return NotFound($"client with id {id} is not  sign in trip with id {tripId}");
        }

        
        
        //zwrot
        await service.DeleteClientFromTripAsync(id, tripId);
        return NoContent();

    }
}