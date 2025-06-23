using System.Data.SqlClient;

public class JobFilterRepository
{
    public void Save(int userId, string role, string location, string skills)
    {
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        conn.Open();

        string query = @"INSERT INTO UserJobFilters (UserId, Role, Location, Skills)
                         VALUES (@UserId, @Role, @Location, @Skills)";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Role", role);
        cmd.Parameters.AddWithValue("@Location", location);
        cmd.Parameters.AddWithValue("@Skills", skills);

        cmd.ExecuteNonQuery();
    }

    public List<UserJobFilter> GetByUserId(int userId)
    {
        var list = new List<UserJobFilter>();
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        conn.Open();

        string query = "SELECT * FROM UserJobFilters WHERE UserId = @UserId";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new UserJobFilter
            {
                Id = (int)reader["Id"],
                UserId = (int)reader["UserId"],
                Role = reader["Role"].ToString(),
                Location = reader["Location"].ToString(),
                Skills = reader["Skills"].ToString()
            });
        }

        return list;
    }
}
