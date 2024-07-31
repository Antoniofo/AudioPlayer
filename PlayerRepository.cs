using LiteDB;
using System;
using System.IO;


namespace AudioPlayer
{
    public class PlayerRepository : IDisposable
    {
        private readonly LiteDatabase _db;

        public PlayerRepository(string databasePath)
        {
            var directory = Path.GetDirectoryName(databasePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _db = new LiteDatabase(databasePath);

            var players = _db.GetCollection<PlayerDB>("t_players");
            players.EnsureIndex(x => x.UserId, true);
            
        }

        public PlayerDB GetPlayerByUserId(string userId)
        {
            var players = _db.GetCollection<PlayerDB>("t_players");
            var player = players.FindOne(x => x.UserId == userId);            
            return player;
        }

        public void InsertPlayer(PlayerDB player)
        {
            var players = _db.GetCollection<PlayerDB>("t_players");
            players.Insert(player);
        }

        public void UpdatePlayer(PlayerDB player)
        {
            var players = _db.GetCollection<PlayerDB>("t_players");
            players.Update(player);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }

    public class PlayerDB
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int Mute { get; set; }
    }

}
