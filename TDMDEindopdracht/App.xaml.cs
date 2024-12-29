using TDMDEindopdracht.Infrastructure;

namespace TDMDEindopdracht
{
    public partial class App : Application
    {
        public static DatabaseComunicator RouteDatabase { get; private set; }
        public App(DatabaseComunicator routeDataBase)
        {
            InitializeComponent();

            MainPage = new AppShell();

            RouteDatabase = routeDataBase;
        }
    }
}
