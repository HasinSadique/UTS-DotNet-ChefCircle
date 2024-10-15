// using System.Net.Http;
using Microsoft.AspNetCore.Http;
using myfirstapi.Controller;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//Create DB connection
string Host="chef-circle-dotnet-project-chefcircle.f.aivencloud.com";
string username="avnadmin";
string Password="AVNS_O7_liIuxjEB0i2ZhTMf";
string Database="defaultdb";

string connectionString = $"Host={Host};Username={username};Password={Password};Database={Database}";

var dbConnection = new DBConnection(connectionString);



try
    {
        using (var conn = dbConnection.GetConnection())
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                Console.WriteLine("My ChefCircle DB is connected and ready for query execution.");
                // await context.Response.WriteAsync("Database connection established successfully.");
            }
            else
            {
                Console.WriteLine("My ChefCircle Database connection could not be established.");
                // await context.Response.WriteAsync("Database connection could not be established.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error in DB connection.\n");
        Console.WriteLine(ex);
    }






if(dbConnection!=null){
    Console.WriteLine("ChefCircle DB online.");
}

app.MapGet("/",()=>{
    // Console.WriteLine("Chef circle server is running.");
    return "Welcome to the backend server of ChefCircle.";
});

app.MapGet("/hello",()=>{
    return "Hello world from laptop.";
});

app.MapPost("/signup",async(HttpContext context)=>{
    var user=await context.Request.ReadFromJsonAsync<User>();

});

app.Run();
