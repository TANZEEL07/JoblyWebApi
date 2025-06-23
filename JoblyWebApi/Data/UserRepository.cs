using System.Data.SqlClient;

public class UserRepository
{
    public User GetByEmail(string email)
    {
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        conn.Open();

        // ✅ Fixed: [User] instead of User
        string query = "SELECT * FROM [Users] WHERE Email = @Email";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Email", email ?? (object)DBNull.Value);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader["Id"] != DBNull.Value ? (int)reader["Id"] : 0,
                Name = reader["Name"]?.ToString() ?? "",
                Email = reader["Email"]?.ToString() ?? "",
                PasswordHash = reader["PasswordHash"]?.ToString() ?? ""
            };
        }

        return null;
    }

    public void Register(User user)
    {
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        conn.Open();

        // ✅ Fixed: [User] instead of User
        string query = "INSERT INTO [Users] (Name, Email, PasswordHash) VALUES (@Name, @Email, @PasswordHash)";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Name", user.Name ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value);

        cmd.ExecuteNonQuery();
    }
}
