using Microsoft.Extensions.Options;
using myfirstapi.Controller;
using myfirstapi.Model;
using Npgsql;
using System.Runtime.InteropServices;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(Options =>{
    Options.AddDefaultPolicy(builder=> {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
builder.WebHost.UseUrls("http://*:5076");

var app = builder.Build();
app.UseCors();
app.UseHttpsRedirection();

// Get the connection string
var connectionString = "Host=chef-circle-dotnet-project-chefcircle.f.aivencloud.com;Port=15473;Username=avnadmin;Password=AVNS_O7_liIuxjEB0i2ZhTMf;Database=ChefCircle;SSL Mode=Require;Trust Server Certificate=true";
// Create a connection to your PostgreSQL database
using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();
Console.WriteLine("DB Connection Done.");

// *************************************** Methods/Functions *************************************** 
 



// *************************************** Backend APIs Working *************************************** 
// Gets all recipes from DB and sends to client.
app.MapGet("/getallrecipes", async()=>{
    // Create a command to execute your query
    String query = "select * FROM recipes";
    await using var command = new NpgsqlCommand(query, connection);
    // Execute the command and read the result
    using var reader = await command.ExecuteReaderAsync();
    // var ListOfRecipes = new List<Dictionary<string, object>>();
    // Prepare a list to hold the rows in a form of a dictionary
    var results = new List<Dictionary<string, object>>();
    while (await reader.ReadAsync())
    {
        var row = new Dictionary<string, object>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }
        results.Add(row);
    }
    return Results.Json(results, new JsonSerializerOptions { WriteIndented = true });
});

// Gets user details of specific user using ID.
app.MapGet("/getuserbyid/{id:int}",async(int id)=>{
    // Create a command to execute your query
    var query = $"SELECT * FROM public.users WHERE \"UID\" = '{id}'";
    await using var command = new NpgsqlCommand(query, connection);
    //add id as parameter for @id
    command.Parameters.AddWithValue("id", id);
    // Execute the command and read the result
    using var reader = await command.ExecuteReaderAsync();
    // Prepare a list to hold the rows in a form of a dictionary
    var results = new List<Dictionary<string, object>>();
    while (await reader.ReadAsync())
    {
        var row = new Dictionary<string, object>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }
        results.Add(row);
    }
    return Results.Json(results, new JsonSerializerOptions { WriteIndented = true });
});


app.MapGet("/",  async() =>
{
    // Create a command to execute your query
    String query = "select * FROM users";
    await using var command = new NpgsqlCommand(query, connection);
    
    // Execute the command and read the result
    using var reader = await command.ExecuteReaderAsync();
    Console.WriteLine("reader >>>> \n "+reader);

    // Prepare a list to hold the rows in a form of a dictionary
    var results = new List<Dictionary<string, object>>();
    while (await reader.ReadAsync())
    {
        var row = new Dictionary<string, object>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }
        results.Add(row);
    }
    Console.WriteLine("Results: "+results[0]);
    // Serialize the results to JSON format
    string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
    return json;



    // // Console.WriteLine("The result: " + reader.GetInt32(0));
    // // Assuming the result is a single row with a single column
    // if (await reader.ReadAsync())
    // {
    //     // Return the fetched result
    //     return Results.Ok(reader.GetInt32("ID")); // or reader[0]
    // }

    // return Results.NotFound();
});

app.MapGet("/hello",()=>{
    return "Hello world from laptop.";
});

app.MapPost("/signup",async (User user) =>
{
    Console.WriteLine($"Fullname: {user.Email}");
    // int id=registerUser(user);
    // return Results.Ok(new {ID = $"User ID : {id}"});
});

app.MapPost("/signin", async (User user)=>{
    // string Email=user.Email;
    // string Password =user.Password;

// query
// check if the email is int the table. If available 


// if everything mathes >>> Return the whole record of that user to the frontend

});

app.MapPut("/addlike",async (Recipe recipe)=>{
    // // var query = $"SELECT * FROM public.users WHERE \"UID\" = '{id}'";
    // var query = "UPDATE recipes SET Likes = @Likes WHERE RID = @RID";
    // await using var co`mmand = new NpgsqlCommand(query, connection);

    // command.Parameters.AddWithValue("@Likes", recipe.Likes);
    // command.Parameters.AddWithValue("@RID", recipe.RID);

});


app.Run();