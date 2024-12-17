using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui;

namespace TDMDEindopdracht
{
    public partial class MainPage : ContentPage
    {
        

        public MainPage()
        {
            InitializeComponent();
        }


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

                    bool shouldContinue = await Application.Current.MainPage.DisplayAlert(
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

                var shouldOpenSettings = await Application.Current.MainPage.DisplayAlert(
                    "Locatie permissie vereist",
                    "De app heeft toegang tot je locatie nodig om goed te functioneren. Ga naar de instellingen om deze in te schakelen.",
                    "Naar Instellingen",
                    "Annuleren"
                );
                if (shouldOpenSettings)
                {

                    //var intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                    //var uri = Android.Net.Uri.FromParts("package", Android.App.Application.Context.PackageName, null);
                    //intent.SetData(uri);
                    //Android.App.Application.Context.StartActivity(intent);
                    //todo fiks intent probleem
                }
                else
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            }
        }



    }



}


