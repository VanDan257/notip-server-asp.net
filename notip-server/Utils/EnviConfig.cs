namespace notip_server.Utils
{
    public class EnviConfig
    {
        public static string DbConnectionString { get; private set; }
        public static string BlobConnectionString { get; private set; }
        public static string SecretKey { get; private set; }
        public static int ExpirationInMinutes { get; private set; }
        public static string DailyToken { get; private set; }

        public static void Config(IConfiguration configuration)
        {
            DbConnectionString = configuration.GetConnectionString("DbConnection");
            BlobConnectionString = configuration.GetConnectionString("BlobConnectionString");
            SecretKey = configuration["JwtConfig:SecretKey"];
            ExpirationInMinutes = Convert.ToInt32(configuration["JwtConfig:ExpirationInMinutes"]);
            DailyToken = configuration["DailyToken"];
        }
    }
}
