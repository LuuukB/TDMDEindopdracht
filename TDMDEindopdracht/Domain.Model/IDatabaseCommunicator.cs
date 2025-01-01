using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMDEindopdracht.Domain.Model
{
    public interface IDatabaseCommunicator
    {
        void Init();
        List<Route> GetAllRoutes();
        void AddRoute(Route route);
        void Delete(int id);
    }
}
