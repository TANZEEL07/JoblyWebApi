using System.Data;
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

    public static void SaveQuestionAnswer(int userId, string question, string answer)
    {
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand("InsertResumeQnA", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Question", question);
        cmd.Parameters.AddWithValue("@Answer", answer);
        cmd.Parameters.AddWithValue("@UserId", userId);

        cmd.ExecuteNonQuery();
    }

    public static string?  GetAnswerByQuestion(string question)
    {
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand("GetAnswerByQuestion", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Question", question);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return reader["Answer"]?.ToString();
        }

        return null; 
    }

}
