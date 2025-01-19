using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMDEindopdracht.Domain.Model
{
    public interface ILocationPermisssionService
    {
        Task<PermissionStatus> CheckAndRequestLocationPermissionAsync();
        Task ShowSettingsIfPermissionDeniedAsync();
    }

}
