using System;

namespace Models
{
    public class Config
    {
        public string GameCode { get; private set; }
        public int Duration { get; private set; }

        private static Config _instance;
        
        private Config() { }
        
        public static readonly object _lock = new object();

        public static Config GetConfig()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Config();
                    }
                } 
            }

            return _instance;
        }

        public void SetupConfig(string code, int duration)
        {
            GameCode = code;
            Duration = duration;
        }

        public void Reset()
        {
            _instance = null;
        }
    }
}