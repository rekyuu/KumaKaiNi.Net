using System;

namespace KumaKaiNi.Core
{
    public static class Config
    {
        public static string DanbooruUser => Environment.GetEnvironmentVariable("DANBOORU_USER");
        
        public static string DanbooruApiKey => Environment.GetEnvironmentVariable("DANBOORU_API_KEY");
        
        public static string ImgurClientId => Environment.GetEnvironmentVariable("IMGUR_CLIENT_ID");
        
        public static string PostgresHost => Environment.GetEnvironmentVariable("POSTGRES_HOST");
        
        public static string PostgresUsername => Environment.GetEnvironmentVariable("POSTGRES_USERNAME");
        
        public static string PostgresPassword => Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        
        public static string PostgresDatabase => Environment.GetEnvironmentVariable("POSTGRES_DATABASE");
    }
}