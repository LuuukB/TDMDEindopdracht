
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMDEindopdracht.Domain.Model
{
    public partial class RouteHandler
    {
        private Route route;
        DateTime time;
        private IDatabaseCommunicator _communicator;

        public RouteHandler(IDatabaseCommunicator databaseCommunicator) { _communicator = databaseCommunicator; }


        public void CreateRoute() { 
        
            route = new ();
            StartRoute();
        }

        public void StartRoute() {
            time = DateTime.Now;
        }

        public void StopRoute(double distance, string name) { 
            var currentTime = DateTime.Now - time;
            route.TotalRunTime = currentTime.ToString();
            route.Distance = distance;
            var averadgeMeterPerSec = distance / currentTime.TotalSeconds;
            route.AveradgeSpeed = averadgeMeterPerSec;
            route.Name = name;

            _communicator.AddRoute(route);
        }
    }
}
