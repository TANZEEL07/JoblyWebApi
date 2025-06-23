using System.Data.SqlClient;

namespace JoblyWebApi.Repositories
{
   public class AppliedJobRepository
{
    public void Save(AppliedJob job)
    {
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        conn.Open();

        string query = @"INSERT INTO AppliedJobs (UserId, JobTitle, Company, Location, AppliedAt)
                         VALUES (@UserId, @JobTitle, @Company, @Location, @AppliedAt)";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", job.UserId);
        cmd.Parameters.AddWithValue("@JobTitle", job.JobTitle);
        cmd.Parameters.AddWithValue("@Company", job.Company);
        cmd.Parameters.AddWithValue("@Location", job.Location);
        cmd.Parameters.AddWithValue("@AppliedAt", job.AppliedAt);

        cmd.ExecuteNonQuery();
    }
}

    public class AppliedJob
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string JobTitle { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
