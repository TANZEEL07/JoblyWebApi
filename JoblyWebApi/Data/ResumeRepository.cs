using System.Data.SqlClient;

public class ResumeRepository
{
    public void Save(int userId, string fileName)
    {
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        conn.Open();
        string query = "INSERT INTO UserResume (UserId, FileName, UploadedAt) VALUES (@UserId, @FileName, @UploadedAt)";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@FileName", fileName);
        cmd.Parameters.AddWithValue("@UploadedAt", DateTime.Now);
        cmd.ExecuteNonQuery();
    }
}
