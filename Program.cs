// using System.Net.Http;
using Microsoft.AspNetCore.Http;
using myfirstapi.Controller;
using Npgsql;



using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data;

// string Host="chef-circle-dotnet-project-chefcircle.f.aivencloud.com";
// string username="avnadmin";
// string Password="AVNS_O7_liIuxjEB0i2ZhTMf";
// string Database="ChefCircle";


var builder = WebApplication.CreateBuilder(args);
// var conN=null;
// var c=builder.Services.AddScoped<IDbConnection>((sp)=>{
//     var connectionStr="Host=chef-circle-dotnet-project-chefcircle.f.aivencloud.com;Port=15473;Username=avnadmin;Password=AVNS_O7_liIuxjEB0i2ZhTMf;Database=ChefCircle;SSL Mode=Require;Trust Server Certificate=true";
//     var conN=new NpgsqlConnection(connectionStr);
//     conN.Open();
//     Console.WriteLine("Database Connected!!");

//     return conN;
// });


var app = builder.Build();

app.UseHttpsRedirection();


// string connectionString = ConfigurationHelper.GetConnectionString("DefaultConnection");
// Console.WriteLine("The string is : "+connectionString);
// await using var conn = new NpgsqlConnection(connectionString);

// await conn.OpenAsync();

// Console.WriteLine($"The PostgreSQL version: {conn.PostgreSqlVersion}");

// //Create DB connection
// string Host="chef-circle-dotnet-project-chefcircle.f.aivencloud.com";
// string username="avnadmin";
// string Password="AVNS_O7_liIuxjEB0i2ZhTMf";
// string Database="ChefCircle";

// // string connectionString = $"Host={Host};Username={username};Password={Password};Database={Database}";
// string connectionString = $"Server={Host};Username={username};Password={Password};Database={Database}";

// var dbConnection = new DBConnection(connectionString);



// try
//     {
//         using (var conn = dbConnection.GetConnection())
//         {
//             if (conn.State == System.Data.ConnectionState.Open)
//             {
//                 Console.WriteLine("My ChefCircle DB is connected and ready for query execution.");
//                 // await context.Response.WriteAsync("Database connection established successfully.");
//             }
//             else
//             {
//                 Console.WriteLine("My ChefCircle Database connection could not be established.");
//                 // await context.Response.WriteAsync("Database connection could not be established.");
//             }
//         }
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine("Error in DB connection.\n");
//         Console.WriteLine(ex);
//     }






// if(dbConnection!=null){
//     Console.WriteLine("ChefCircle DB online.");
// }




// *************************************** Backend APIs Working *************************************** 

app.MapGet("/", async (IConfiguration configuration) =>
{
    // Get the connection string
    // var connectionString = configuration.GetConnectionString("DefaultConnection");
    var connectionString = "Host=chef-circle-dotnet-project-chefcircle.f.aivencloud.com;Port=15473;Username=avnadmin;Password=AVNS_O7_liIuxjEB0i2ZhTMf;Database=ChefCircle;SSL Mode=Require;Trust Server Certificate=true";
    
    // Create a connection to your PostgreSQL database
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("DB Connection Done.");

    // Create a command to execute your query
    String query = "SELECT * FROM Users";
    await using var command = new NpgsqlCommand(query, connection);
    
    // Execute the command and read the result
    using var reader = await command.ExecuteReaderAsync();
    // Console.WriteLine("The result: " + reader.GetInt32(0));
    // Assuming the result is a single row with a single column
    if (await reader.ReadAsync())
    {
        // Return the fetched result
        return Results.Ok(reader.GetInt32(0)); // or reader[0]
    }

    return Results.NotFound();
});

// app.MapGet("/",async (IDbConnection dbConnection)=>{
//     const string query="SELECT * FROM Users";
    
//     // var users=await dbConnection.QueryAsync<User>(query);

//     // Console.WriteLine("Chef circle server is running.");
//     return "Welcome to the backend server of ChefCircle. \n" +query;
// });

app.MapGet("/hello",()=>{
    return "Hello world from laptop.";
});

app.MapPost("/signup",async(HttpContext context)=>{
    var user=await context.Request.ReadFromJsonAsync<User>();

});

app.Run();
