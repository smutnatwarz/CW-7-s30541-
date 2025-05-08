namespace TripCw7.Models.DTOs;

public class ClientTripCreateDTO
{
    public int IdClient { get; set; }
    public int IdTrip{ get; set; }
    public int RegisteredAt { get; set; }
    public int  PaymentDate  { get; set; }
}