using Microsoft.Maui.Controls.Maps;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TDMDEindopdracht.Domain.Services;

namespace TDMDEindopdracht;

public partial class mapPage : ContentPage
{
	public mapPage(MapViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }

    //LET OP, als deze code in een andere klasse wordt gezet moet je op letten op sychronisatie. 
    public async Task<PermissionStatus> CheckAndRequestLocationPermission()
    {

        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
            return status;


        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            return status;
        }

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.Android)
        {

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {

                bool shouldContinue = await DisplayAlert(
                    "Locatie vereist",
                    "De app heeft je locatie nodig anders kan de app niet goed werken.",
                    "Toestaan",
                    "Annuleren"
                );

                if (shouldContinue)
                {

                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
                else
                {

                    return PermissionStatus.Denied;
                }
            }
            else
            {

                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
        }

        return status;
    }


    //overides voor bij het opstarten van de mainpage. 
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var status = await CheckAndRequestLocationPermission();


        if (status == PermissionStatus.Denied)
        {

            await DisplayAlert("Locatie permissie vereist", "De app heeft toegang tot je locatie nodig om goed te functioneren.", "Ok");

            var shouldOpenSettings = await DisplayAlert(
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
            else
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }

    private void MapView_BindingContextChanged(object sender, EventArgs e)
    {

    }
}