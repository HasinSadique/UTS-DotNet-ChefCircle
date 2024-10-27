using Microsoft.Extensions.Options;
// using myfirstapi.Controller;
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
//  async Task<User> ValidateUser(string Email, string Password)
 
 async Task<List<Dictionary<string,object>>> ValidateUser(string Email, string Password)
{
    string query = $"SELECT * FROM public.users WHERE \"Email\" = @Email AND \"Password\" = @Password";
    await using var command = new NpgsqlCommand(query, connection);
    command.Parameters.AddWithValue("@Email", Email);
    command.Parameters.AddWithValue("@Password", Password);
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

    return results;
}

async Task<bool> validateByRID(int? RID)
{
    if (RID == null)
        return false;

    // Perform a query to check if the UID exists in the users table
    string query = "SELECT COUNT(1) FROM public.recipes WHERE \"RID\" = @RID";
    await using var command = new NpgsqlCommand(query, connection);
    command.Parameters.AddWithValue("@RID", RID);

    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
    return count == 1;
}

async Task<bool> ValidateUserByUID(int? UID)
{
    if (UID == null)
        return false;

    // Perform a query to check if the UID exists in the users table
    string query = "SELECT COUNT(1) FROM public.users WHERE \"UID\" = @UID";
    await using var command = new NpgsqlCommand(query, connection);
    command.Parameters.AddWithValue("@UID", UID);

    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
    return count == 1;
}

// *************************************** Backend APIs Working *************************************** 
// Gets all recipes from DB and sends to client.


try
{

    app.MapPost("/addRecipe", async (Recipe recipe) =>
    {
        try
        {
            Console.WriteLine("recipe: " + recipe);
            int? UID = recipe.UID;
            string? Title = recipe.Title;
            string? Description = recipe.Description;
            int? Likes = 0;
            bool? IsVerified = false;
            bool user = await ValidateUserByUID(UID);
            Console.WriteLine(recipe);

            bool success = false;
            int status = 401; // Default to 400, assuming failure
            string msg = "Login first.";
            int result = 0;

            if (!user)
            {
                var response = new
                {
                    success = success,
                    status = status,
                    result = result,
                    msg = msg
                };
                return Results.Json(response);


            }

            string query = @"INSERT INTO public.recipes( ""UID"",""Title"", ""Description"", ""Likes"",""IsVerified"") VALUES ( @UID,@Title, @Description,@Likes,@IsVerified) RETURNING ""RID""";
            await using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@UID", UID ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Title", Title ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Description", Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Likes", Likes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsVerified", IsVerified ?? (object)DBNull.Value);


            var RID = await command.ExecuteScalarAsync();

            Console.WriteLine(RID);
            if (RID != null)
            {
                return Results.Created($"/recipes/{RID}", new { RID = RID, Message = "Recipe added successfully." });
            }
            else
            {
                return Results.Problem("An error occurred while adding the recipe.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Results.Problem("An error occurred while adding the recipe.");
        }

    });
    app.MapGet("/getallrecipes", async()=>
    {
        // Create a new connection for this request
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Create a command to execute your query
        // String query = "select * FROM recipes WHERE \"IsVerified\" = TRUE";
        String query = @"select * FROM recipes ORDER BY ""RID"" DESC";
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
        // Console.WriteLine("Done");
        return Results.Json(results, new JsonSerializerOptions { WriteIndented = true });
    });

    app.MapPut("/approverecipe", async (Recipe recipe) =>
    {
        try
        {
            int? RID = recipe.RID;
            // string? Title = recipe.Title;
            // string? Description = recipe.Description;


            bool success = false;
            int status = 401;
            string msg = "add recipe first.";
            int result = 0;

            bool validateRecipe = await validateByRID(RID);

            if (!validateRecipe)
            {
                var response = new
                {
                    success = success,
                    status = status,
                    result = result,
                    msg = msg
                };
                return Results.Json(response);
            }
            string query = @"UPDATE public.recipes SET ""IsVerified"" = true WHERE ""RID"" = ""RID""";
            await using var command = new NpgsqlCommand(query, connection);

            // command.Parameters.AddWithValue("@Title", Title);
            // command.Parameters.AddWithValue("@Description", Description);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                success = true;
                status = 200;
                msg = "recipe updated suceffully.";
                result = 0;
            }
            var response1 = new
            {
                success = success,
                status = status,
                result = result,
                msg = msg
            };
            return Results.Json(response1);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Results.Problem("An error occurred while updating the recipe.");
        }

    });


    app.MapPut("/promoteuser", async (User user) =>
    {
        try
        {
            Console.WriteLine("user ID: "+user.UID);
            int? UID = user.UID;
            bool success = false;
            int status = 401;
            string msg = "";
            int result = 0;

            bool validateUser = await ValidateUserByUID(UID);

            if (!validateUser)
            {
                var response = new
                {
                    success = false,
                    status = 404,
                    // result = result,
                    msg = "User not found"
                };
                return Results.Json(response);
            }
            // string query = @"UPDATE public.users SET ""Role"" = Masterchef WHERE ""UID"" = ""UID""";
            string query = @"UPDATE public.users SET ""Role"" = @Role WHERE ""UID"" = @UID";
            await using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@UID", UID);
            command.Parameters.AddWithValue("@Role", "Masterchef");

            int rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                success = true;
                status = 200;
                msg = "User promoted suceffully.";
                result = 0;
            }
            var response1 = new
            {
                success = success,
                status = status,
                result = result,
                msg = msg
            };
            return Results.Json(response1);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Results.Problem("An error occurred while promoting the user.");
        }

    });

    app.MapGet("/getallverifiedrecipes", async()=>
    {
        // Create a new connection for this request
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Create a command to execute your query
        String query = "select * FROM recipes WHERE \"IsVerified\" = TRUE";
        // String query = "select * FROM recipes";
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
        // Console.WriteLine("Done");
        return Results.Json(results, new JsonSerializerOptions { WriteIndented = true });
    });
    // Gets user details of specific user using ID.
    app.MapGet("/getuserbyid/{id:int}",async(int id)=>
    {
        // Create a new connection for this request
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

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

    app.MapPost("/api/addComment", async (COMMENT comment) =>
    {
        try
        {
            int? RID = comment.RID;  // Recipe ID for which the comment is being added
            int? UID = comment.UID;  // User ID of the person adding the comment
            string? Comm = comment.Comment;  // Comment 
            Console.WriteLine("Comment", Comm);
            // Set up the response variables
            bool success = false;
            int status = 401;  // Default to 401 Unauthorized, assuming failure
            string msg = "Please provide a valid recipe to comment on.";
            int result = 0;

            // Validate that the recipe exists
            bool validateRecipe = await validateByRID(RID);

            if (!validateRecipe)
            {
                // Return response if the recipe ID is invalid
                var response = new
                {
                    success = success,
                    status = status,
                    result = result,
                    msg = msg
                };
                return Results.Json(response);
            }

            // SQL query to insert a new comment
            string query = @"INSERT INTO public.comments (""RID"", ""UID"", ""Comment"") VALUES (@RID, @UID, @Comment)";
            await using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@RID", RID );
            command.Parameters.AddWithValue("@UID", UID );
            command.Parameters.AddWithValue("@Comment", Comm );

            int rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                success = true;
                status = 201;  // HTTP Created
                msg = "Comment added successfully.";
                result = 0;  // Optionally, set result to the comment ID or other relevant data
            }

            // Return the JSON response
            var response1 = new
            {
                success = success,
                status = status,
                result = result,
                msg = msg
            };
            return Results.Json(response1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Results.Problem("An error occurred while adding the comment.");
        }
    });

    app.MapGet("/getallcomments/{id:int}", async(int id) =>
    {
        Console.WriteLine(id);
        try
        {
            // if (id == null)
            // {
            //     // Return an error if RID is missing
            //     return Results.BadRequest(new { Message = "Recipe ID (RID) is required." });
            // }
            Console.WriteLine("RID: >> ",id);
            // Create a new connection for this request
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // SQL query to retrieve comments for the specified recipe
            // string query = "SELECT \"CID\", \"UID\", \"Comment\" FROM public.comments WHERE \"RID\" = @RID";
            var query="";
            if(id>0){
                query = $"SELECT * FROM public.comments WHERE \"RID\" = '{id}'";
            }
            await using var command = new NpgsqlCommand(query, connection);
            // command.Parameters.AddWithValue("RID", id);

            // List to hold comments
            var comments = new List<Dictionary<string, object>>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var comment = new Dictionary<string, object>
                {
                    ["CID"] = reader.GetInt32(0),     // Comment ID
                    ["UID"] = reader.GetInt32(1),     // User ID of the commenter
                    ["RID"] = reader.GetInt32(2),     // Recipe ID of the comment
                    ["Comment"] = reader.GetString(3) // The actual comment text
                };
                comments.Add(comment);
            }

            if (comments.Count == 0)
            {
                return Results.NotFound(new { Message = "No comments found for this recipe." });
            }

            return Results.Ok(new
            {
                success = true,
                status = 200,
                result = comments,
                msg = "Comments retrieved successfully."
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Results.Problem("An error occurred while retrieving comments.");
        }
    });

    // app.MapDelete("/deletepost", async (int rid) =>
    // {
    //     // int rid=recipe.RID;
    //     Console.WriteLine("My RID is >>> "+rid);

    //     try
    //     {
    //         Console.WriteLine("My RID >> "+rid);
    //         bool success = false;
    //         int status = 404;  // Default to not found
    //         string msg = "Recipe not found";
    //         int result = 0;

    //         // Check if recipe exists
    //         string validateQuery = @"SELECT COUNT(*) FROM public.recipes WHERE ""RID"" = @RID";
    //         await using var validateCmd = new NpgsqlCommand(validateQuery, connection);
    //         validateCmd.Parameters.AddWithValue("@RID", rid);

    //         var count = await validateCmd.ExecuteScalarAsync();
    //         bool recipeExists = Convert.ToInt32(count) > 0;

    //         if (!recipeExists)
    //         {
    //             return Results.NotFound(new
    //             {
    //                 success = success,
    //                 status = status,
    //                 result = result,
    //                 msg = msg
    //             });
    //         }

    //         // Delete the recipe
    //         string deleteQuery = @"DELETE FROM public.recipes WHERE ""RID"" = @RID";
    //         await using var deleteCmd = new NpgsqlCommand(deleteQuery, connection);
    //         deleteCmd.Parameters.AddWithValue("@RID", rid);

    //         int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();

    //         if (rowsAffected > 0)
    //         {
    //             success = true;
    //             status = 200;
    //             msg = "Recipe deleted successfully";
    //             result = rowsAffected;
    //         }

    //         return Results.Ok(new
    //         {
    //             success = success,
    //             status = status,
    //             result = result,
    //             msg = msg
    //         });
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error deleting recipe: {ex.Message}");
    //         return Results.Problem(
    //             title: "Internal Server Error",
    //             detail: "An error occurred while deleting the recipe",
    //             statusCode: 500
    //         );
    //     }
    // });


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

    });

    
    app.MapPost("/signup",async (User user)=>
    {
        string? Fullname = user.Fullname;
        string? Email = user.Email;
        string? Phone = user.Phone;
        string? Password = user.Password;
        string? Address = user.Address;
        string? Role = user.Role;

        bool success = false;
        int status = 400; // Default to 400, assuming failure
        string msg = "An error occurred.";
        int result = 0;

        Console.WriteLine(user.Phone);
        try
        {
            // Check if the user already exists by email
            string checkQuery = "SELECT COUNT(*) FROM public.users WHERE \"Email\" = @Email";
            await using var checkCommand = new NpgsqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@Email", Email);

            var userExists = (long)await checkCommand.ExecuteScalarAsync() > 0;

            if (userExists)
            {
                status = 409; // Conflict status code
                success=false;
                msg = "Email already exists. Use another email address.";
            }
            else
            {
                // Insert the new user into the database
                string query = @"INSERT INTO public.users (""Fullname"", ""Email"", ""Phone"", ""Password"", ""Role"", ""Address"") 
                                VALUES (@Fullname, @Email, @Phone, @Password, @Role, @Address) 
                                RETURNING ""UID""";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@Fullname", Fullname ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Phone", Phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Password", Password ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Role", Role ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Address", Address ?? (object)DBNull.Value);

                var UID = await command.ExecuteScalarAsync(); // Get the UID from the inserted row
                success = true;
                status = 200;
                result = (int)(UID ?? 0); // Set result to the returned UID
                msg = $"User added successfully with ID: {UID}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            success=false;
            status=500; //Internal server error
            msg = "Internal Server error. An error occurred while processing your request. Contact backend developer.";
        }

        // Return a structured response
        var response = new
        {
            success = success,
            status = status,
            result = result,
            msg = msg
        };
        return Results.Json(response);   
    });

    app.MapPost("/signin", async (User user) => {
        // User u= await ValidateUser(user.Email,user.Password);
        string responseJson="";
        if(user.Email!=null && user.Password!=null){
            Task<List<Dictionary<string,object>>> results = ValidateUser(user.Email,user.Password);
            Console.WriteLine("result >>>> " + results.Result.Count());
            if(results.Result.Count()>0)
            {
                var response = new
                {
                    success = true,
                    status = 200,
                    result = results,
                    msg = "These are the users."
                };
                responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            }else{
                var response = new
            {
                success = false,
                status = 200,
                msg = "These are no users with this email and password."
            };
             responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            }
             
        }else{
             var response = new
        {
            success = true,
            status = 200,
            msg = "Email and password empty."
        };
         responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        Console.WriteLine(responseJson);
        return responseJson;
    });

    app.MapPut("/api/addlike", async (Recipe recipe) =>
    {
        int RID=recipe.RID;
        Console.WriteLine("RID: "+RID);
        // Check if the RID is provided
        if (RID <= 0)
        {
            return Results.BadRequest(new { Message = "Invalid Recipe ID (RID). It must be greater than 0." });
        }

        try
        {
            // SQL query to increment the like count for the specified recipe
            string query = "UPDATE public.recipes SET \"Likes\" = \"Likes\" + 1 WHERE \"RID\" = @RID RETURNING \"Likes\"";

            // Create and prepare the command
            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("RID", RID);

            // Execute the command and get the updated like count
            var updatedLikeCount = await command.ExecuteScalarAsync();

            // Check if the recipe was found and updated
            if (updatedLikeCount != null)
            {
                return Results.Ok(new
                {
                    success = true,
                    status = 200,
                    result = new { RID = RID, UpdatedLikes = (int)updatedLikeCount },
                    msg = "Like count updated successfully."
                });
            }
            else
            {
                return Results.NotFound(new { Message = "Recipe not found." });
            }
        }
        catch (Exception ex)
        {
            // Log the exception and return an error response
            Console.WriteLine($"Error: {ex.Message}");
            return Results.Problem("An error occurred while updating likes.");
        }
        });

}
catch(Exception e)
{
    Console.WriteLine("\n\n\nException is \n"+e);
}


app.Run();