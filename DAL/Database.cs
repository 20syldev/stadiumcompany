using Npgsql;

namespace StadiumCompany.DAL;

public static class Database
{
    private const string ConnectionString = "Host=localhost;Database=stadiumcompany;Username=stadiumcompany;Password=stadiumcompany";

    public static NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(ConnectionString);
    }

    public static bool TestConnection()
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
