using Game;
using Newtonsoft.Json;

namespace Api
{
    public class LeaderboardRequest : Request
    {
        public bool Done { get; private set; }

        public bool Error
        {
            get { return apiRequest.Error; }
        }

        public string ErrorText
        {
            get { return apiRequest.ErrorText; }
        }

        public LeaderboardEntry[] Result { get; private set; }

        private ApiRequest apiRequest;

        public LeaderboardRequest(ApiRequest apiRequest, MapData map, int offset)
        {
            this.apiRequest = apiRequest;
            this.apiRequest.OnDone += (sender, args) =>
            {
                if (!apiRequest.Error)
                {
                    Result = ParseEntries(apiRequest.Result, map.id, offset);
                }
                Done = true;
            };
            this.apiRequest.StartRequest();
        }

        private LeaderboardEntry[] ParseEntries(string entryJson, int mapId, int rankOffset = 0)
        {
            if (entryJson == "")
                return new LeaderboardEntry[0];

            LeaderboardResult[] results = JsonConvert.DeserializeObject<LeaderboardResult[]>(entryJson);
            LeaderboardEntry[] entries = new LeaderboardEntry[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                entries[i] = results[i];
            }

            for (int i = 0; i < entries.Length; i++)
            {
                entries[i].map = mapId.ToString();
                entries[i].rank = rankOffset + i + 1;
            }

            return entries;
        }
    }
}