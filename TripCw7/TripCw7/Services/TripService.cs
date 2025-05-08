using Microsoft.Data.SqlClient;
using TripCw7.Models.DTOs;

namespace TripCw7.Services;



public interface ITripService
{
    public Task<IEnumerable<TripsWithCountryGetDTO>> GetAllTripsAndCountriesAsync();

    public Task<IEnumerable<CustomerTripsGetDTO>> GetClientTripsAsync(int id);

    public Task<int> CreateClientAsync(ClientCreateDTO clientCreateDto);

    public Task PutClientToTripAsync(int clientId, int tripId);

    public Task DeleteClientFromTripAsync(int id, int tripId);
    
    
    

    //WALIDACYJNE
    public Task<bool> CheckIfClientExistsAsync(int id);

    public Task<bool> CheckIfTripExistsAsync(int id);
    
    public Task<bool> CheckIfClientWithTripExistsAsync(int clientId,int tripId);
    
    public Task<bool> CheckIfTripHaveFreeSpaceAsync(int tripId);
}

public class TripService(IConfiguration config) : ITripService
{
    
    
    //Zwrotna część
    public async Task<IEnumerable<TripsWithCountryGetDTO>> GetAllTripsAndCountriesAsync()
    {
        var tripDict = new Dictionary<int, TripsWithCountryGetDTO>();
        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);

        const string sql = """
                             SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name
                             FROM trip t
                             INNER JOIN country_trip ct ON t.IdTrip = ct.IdTrip
                             INNER JOIN country c ON ct.IdCountry = c.IdCountry;
                           """;

        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var tripId = reader.GetInt32(0);
            if (!tripDict.TryGetValue(tripId, out var dto))
            {
                dto = new TripsWithCountryGetDTO()
                {
                    IdTrip = tripId,
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    Countries = []
                };

                tripDict.Add(tripId, dto);
            }

            var trip = tripDict[tripId];
            trip.Countries.Add(reader.GetString(6));
        }

        return tripDict.Values;
    }


    public async Task<IEnumerable<CustomerTripsGetDTO>> GetClientTripsAsync(int id)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        var customerTrips = new List<CustomerTripsGetDTO>();

        const string sql = """
                           SELECT t.*,  ct.RegisteredAt , ct.PaymentDate
                           FROM trip t inner join client_trip ct on ct.IdTrip = t.IdTrip
                           where ct.IdClient = @IdClient
                           """;
        
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdClient", id);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

       

        while(await reader.ReadAsync())
        {
            //jak mamy pusty idtrip znaczy że klient nie ma wycieczki zwracamy pustą liste
            if (reader.IsDBNull(0))
            {
                continue;
            }

            var trip = new CustomerTripsGetDTO
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                RegisteredAt = reader.GetInt32(6),
                PaymentDate = reader.IsDBNull(7) ? null : reader.GetInt32(7)
            };
            customerTrips.Add(trip);
        } 

        return customerTrips;
    }


    public async Task<int> CreateClientAsync(ClientCreateDTO clientCreateDto)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        
        var sql =
            """
            INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
            OUTPUT inserted.IdClient
            VALUES  (@FirstName, @LastName, @Email, @Telephone, @Pesel);
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", clientCreateDto.FirstName);
        command.Parameters.AddWithValue("@LastName", clientCreateDto.LastName);
        command.Parameters.AddWithValue("@Email", clientCreateDto.Email);
        command.Parameters.AddWithValue("@Telephone", clientCreateDto.Telephone);
        command.Parameters.AddWithValue("@Pesel", clientCreateDto.Pesel);

        await connection.OpenAsync();
        var insertedId = Convert.ToInt32(await command.ExecuteScalarAsync());
        return insertedId;
    }

    
    
    
    public async Task PutClientToTripAsync(int clientId, int tripId)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        const string insert = """
                                 INSERT INTO client_trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
                                 VALUES (@IdClient, @IdTrip, @RegisteredAt, @PaymentDate)
                              """;
        await using var command = new SqlCommand(insert, connection);
        var pomocnicza = DateTime.Now.Year * 10000
                         + DateTime.Now.Month * 100
                         + DateTime.Now.Day;

        command.Parameters.AddWithValue("@IdClient", clientId);
        command.Parameters.AddWithValue("@IdTrip", tripId);
        command.Parameters.AddWithValue("@RegisteredAt", pomocnicza);
        command.Parameters.AddWithValue("@PaymentDate", pomocnicza);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteClientFromTripAsync(int id, int tripId)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        var sql = "Delete from client_trip where IdClient = @IdClient and IdTrip = @IdTrip";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdClient", id);
        command.Parameters.AddWithValue("@IdTrip", tripId);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    
    
    
    
    
    
    
    
    
    //WALIDACYJNE
    public async Task<bool> CheckIfClientExistsAsync(int id)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        const string sql = "SELECT Count(*) FROM Client WHERE IdClient = @IdClient";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdClient", id);

        await connection.OpenAsync();
        
        var count = (int)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }

    public async Task<bool> CheckIfTripExistsAsync(int id)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        const string sql = "SELECT Count(*) FROM Trip WHERE IdTrip = @IdTrip";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdTrip", id);

        await connection.OpenAsync();
        var count = (int)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }

   
    public async Task<bool> CheckIfClientWithTripExistsAsync(int clientId, int tripId)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        const string sql = """
                            select count(*) from  client_trip where IdClient = @IdClient and IdTrip = @IdTrip;
                           """;
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdClient", clientId);
        command.Parameters.AddWithValue("@IdTrip", tripId);
        await connection.OpenAsync();
        var count = (int)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }
    
    public async Task<bool> CheckIfTripHaveFreeSpaceAsync(int tripId)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        const string sql = """
                            select  count(*) from trip where MaxPeople>(select count(*) from client_trip ct
                               inner join Trip t on ct.IdTrip = t.IdTrip
                                    where ct.IdTrip=@IdTrip) and IdTrip=@IdTrip;
                           """;
        await using var command = new SqlCommand(sql, connection);
       command.Parameters.AddWithValue("@IdTrip", tripId);
        await connection.OpenAsync();
        var count = (int)(await command.ExecuteScalarAsync())!;
        return count > 0;
    }
    
}