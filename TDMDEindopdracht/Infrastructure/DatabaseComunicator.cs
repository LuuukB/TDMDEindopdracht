using SQLite;
using TDMDEindopdracht.Domain.Model;

namespace TDMDEindopdracht.Infrastructure
{
    public class DatabaseComunicator
    {
        string _dbPath;
        private SQLiteConnection connection;

        public DatabaseComunicator(string dbPath)
        {
            _dbPath = dbPath;
        }

        public void Init() 
        {
            connection = new(_dbPath);
            connection.CreateTable<Route>();
        }

        public List<Route> GetAllRoutes()
        {
            Init();
            return connection.Table<Route>().ToList();
        }

        public void AddRoute(Route route) 
        {
            connection = new(_dbPath);
            connection.Insert(route);
        }

        public void Delete(int id)
        {
            connection = new(_dbPath);
            connection.Delete(new { Id = id });
        }
    }
}
