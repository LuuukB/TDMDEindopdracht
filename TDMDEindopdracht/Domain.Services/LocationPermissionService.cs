using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMDEindopdracht.Domain.Model;

namespace TDMDEindopdracht.Domain.Services
{
    public class LocationPermissionService : ILocationPermisssionService
    {


        public async Task<PermissionStatus> CheckAndRequestLocationPermissionAsync()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status == PermissionStatus.Granted)
                return status;
            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
                return status;
            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.Android)
            {
                if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
                {
                    
                    bool shouldContinue = await Application.Current.MainPage.DisplayAlert(
                        "Locatie vereist",
                        "De app heeft je locatie nodig anders kan de app niet goed werken.",
                        "Toestaan",
                        "Annuleren"
                    );
                    if (!shouldContinue)
                        return PermissionStatus.Denied;
                }
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            return status;
        }
        //voor wanneer settings verwijzing nodig is
        public async Task ShowSettingsIfPermissionDeniedAsync()
        {
            var currentPage = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
            if (currentPage == null)
            {
                currentPage = Application.Current.MainPage;
            }
            var shouldOpenSettings = await currentPage.DisplayAlert(
                "Locatie permissie vereist",
                "De app heeft toegang tot je locatie nodig om goed te functioneren. Ga naar de instellingen om deze in te schakelen.",
                "Naar Instellingen",
                "Annuleren"
            );
            if (shouldOpenSettings)
            {
#if ANDROID
            var context = Android.App.Application.Context;
            var intent = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
            intent.AddFlags(Android.Content.ActivityFlags.NewTask);
            var uri = Android.Net.Uri.FromParts("package", context.PackageName, null);
            intent.SetData(uri);
            context.StartActivity(intent);
#endif
                
            }
            
        }
    }
}
