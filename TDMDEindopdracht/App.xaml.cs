using TDMDEindopdracht.Domain.Model;
using TDMDEindopdracht.Infrastructure;

namespace TDMDEindopdracht
{
    public partial class App : Application
    {
        public static IDatabaseCommunicator RouteDatabase { get; private set; }
        public App(IDatabaseCommunicator routeDataBase)
        {
            InitializeComponent();

            MainPage = new AppShell();

            RouteDatabase = routeDataBase;
        }
    }
}
