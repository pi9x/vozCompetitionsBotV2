using System.Configuration;

namespace vozCompetitionsLibrary
{
    public class Configuration
    {
        public static string API()
        {
            return ConfigurationManager.ConnectionStrings["BotAPI"].ConnectionString;
        }

        public static string MongoDB()
        {
            return ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
        }
    }
}
