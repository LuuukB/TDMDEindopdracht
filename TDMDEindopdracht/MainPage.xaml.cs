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

        public async Task<PermissionStatus> CheckAndRequestNotificationPermission()
        {
    
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

            if (status == PermissionStatus.Granted)
                return status;

      
            if (status == PermissionStatus.Denied && Permissions.ShouldShowRationale<Permissions.PostNotifications>())
            {
                bool shouldContinue = await DisplayAlert(
                    "Notificaties vereist",
                    "De app heeft toestemming nodig om notificaties te kunnen tonen.",
                    "Toestaan",
                    "Annuleren"
                );

                if (!shouldContinue)
                {
                    return PermissionStatus.Denied;
                }
            }

            status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            return status;
        }

  
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var status = await CheckAndRequestNotificationPermission();

            if (status == PermissionStatus.Denied)
            {
                await DisplayAlert("Notificatie permissie vereist", "De app heeft toegang tot notificaties nodig om goed te functioneren.", "Ok");

                var shouldOpenSettings = await DisplayAlert(
                    "Notificatie permissie vereist",
                    "De app heeft toegang tot notificaties nodig om goed te functioneren. Ga naar de instellingen om deze in te schakelen.",
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

}



