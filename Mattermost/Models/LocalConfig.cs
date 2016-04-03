namespace Mattermost.Models
{
    public class LocalConfig
    {
        public int LocalConfigId { get; set; }
        public string Server { get; set; }
        public string Team { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public string UserID { get; set; }
        public string TeamID { get; set; }

        public LocalConfig()
        { }
    }
}
