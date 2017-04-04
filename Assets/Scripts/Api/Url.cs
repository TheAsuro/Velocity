namespace Api
{
    public static class Url
    {
        private const string BASE_URL = "http://api.theasuro.de/velocity/";

        public static string LOGIN
        {
            get { return BASE_URL + "login"; }
        }

        public static string HIGHSCORES
        {
            get { return BASE_URL + "highscores"; }
        }

        public static string DEMOS
        {
            get { return BASE_URL + "demos"; }
        }
    }
}