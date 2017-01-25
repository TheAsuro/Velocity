using Api;
using UnityEngine;

namespace Game
{
    public class SaveData
    {
        private const string PLAYER_PREFIX = "p_";

        private static bool PlayerExists(string name)
        {
            return PlayerPrefs.HasKey(PLAYER_PREFIX + name);
        }

        public static string SaveName(string name)
        {
            return PLAYER_PREFIX + name;
        }

        public string Name { get; private set; }
        public Account Account { get; private set; }

        //Creates a new instance with given data
        public SaveData(string name)
        {
            Name = name;
            Account = new Account(Name);

            if (!PlayerExists(Name))
                PlayerPrefs.SetString(SaveName(Name), "");
        }

        public bool SaveIfPersonalBest(decimal time, string mapName)
        {
            decimal pbTime = GetPersonalBest(mapName);
            if (pbTime <= 0 || time < pbTime)
            {
                PlayerPrefs.SetString(SaveName(Name) + "_" + mapName, time.ToString());
                return true;
            }
            return false;
        }

        public decimal GetPersonalBest(string mapName)
        {
            if (PlayerPrefs.HasKey(SaveName(Name) + "_" + mapName))
            {
                string s = PlayerPrefs.GetString(SaveName(Name) + "_" + mapName);
                if (!s.Equals(""))
                    return decimal.Parse(s);
                else
                    return -1;
            }
            else
                return -1;
        }

        public void DeleteData()
        {
            PlayerPrefs.DeleteKey(SaveName(Name));
            // TODO - delete map times
        }
    }
}