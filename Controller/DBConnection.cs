using Npgsql;
using System;

public class DBConnection
{
    private readonly string _connectionString;
    
    public DBConnection(string connectionString)
    {
        // _connectionString = $"Host={Host};Username={username};Password={Password};Database={Database};SSL Mode=require;Trust Server Certificate=true";
        _connectionString = connectionString;
    }

    public NpgsqlConnection GetConnection()
    {
        try
        {
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            Console.WriteLine("DB Connected!!");
            return connection;
        }
        catch (Exception ex)
        {
            throw new Exception("Could not open database connection", ex);
        }
    }
}
