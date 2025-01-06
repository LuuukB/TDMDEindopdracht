using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Map = Microsoft.Maui.Controls.Maps.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.Maui.Devices.Sensors;
using System.Timers;
using TDMDEindopdracht.Domain.Model;
using CommunityToolkit.Maui.Alerts;
using System.Diagnostics.Metrics;
using Shiny.Notifications;
using Shiny;


namespace TDMDEindopdracht.Domain.Services
{
    public partial class MapViewModel : ObservableObject
    {
        //[ObservableProperty]
        //private Map mappy;
        [ObservableProperty] private bool _isStartEnabled = true;
        [ObservableProperty] private bool _isStopEnabled = false;
        [ObservableProperty] private string _entryText;

        private Location? _home;


        private readonly IGeolocation _geolocation;
        private RouteHandler _routeHandler;
        private System.Timers.Timer? _locationTimer;

        //private readonly List<Location> _routeCoordinates = new();

        [ObservableProperty] private ObservableCollection<MapElement> _mapElements = new();

        private List<Location> _locationCache = [];


        [ObservableProperty] public MapSpan _currentMapSpan;

        //[ObservableProperty]
        //public string currentLocation;

        public MapViewModel(IGeolocation geolocation, RouteHandler routeHandler)
        {
            _geolocation = geolocation;
            //todo: beter gezegd de currentmapspan moet naar user toe op het moment dat de map word gemaakt.
            InitializeMap();
            _routeHandler = routeHandler;
        }

        private async void InitializeMap()
        {
            try
            {
                var location = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));

                if (location is not null)
                {
                    CurrentMapSpan = MapSpan.FromCenterAndRadius(location, Microsoft.Maui.Maps.Distance.FromMeters(10));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fout bij ophalen locatie: {ex.Message}");
            }
        }

        [RelayCommand]
        public void RouteStarting()
        {
            Debug.WriteLine("starting route/timer");
            _locationTimer = new System.Timers.Timer(5000);
            _locationTimer.Elapsed += OnTimedEvent;
            _locationTimer.AutoReset = true;
            _locationTimer.Start();
            IsStartEnabled = false;
            IsStopEnabled = true;

            _routeHandler.CreateRoute();
        }
        
        [RelayCommand]
        public void RouteStop()
        {

            if (EntryText.Length == 0)
            { var noName = Toast.Make("U need to fill in a name", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
                return; }

            if (EntryText.StartsWith(" "))
            { var startsWithSpace = Toast.Make("The name of the runs starts with a space", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
                return; }

            // geeft voor nu als afstand standaard 1000 mee. dit moet nog even aangepast worden naar de daadwerkelijk gelopen afstand
            _routeHandler.StopRoute(1000, EntryText);

            Debug.WriteLine("stopping route/timer");
         
            if (_locationTimer != null)
            {
                _locationTimer.Stop();
                _locationTimer.Dispose();
                _locationTimer = null;
                IsStartEnabled = true;
                IsStopEnabled = false;
                MapElements.Clear();
                //todo eerst route opslaan voordat je de lijnen weg haalt idk wat we nog echt willen gaan doen.
            }
        }

        private void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            Task.Run(OnTimedEventAsync);
        }

        private async Task OnTimedEventAsync()
        {
            try
            {
                Debug.WriteLine("Running {0} at {1}", nameof(OnTimedEventAsync), DateTime.Now.ToShortTimeString());

                var location =
                    await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));

                if (location is not null)
                {
                    Debug.WriteLine("Location: {0}", location);
                     CurrentMapSpan = MapSpan.FromCenterAndRadius(location, Microsoft.Maui.Maps.Distance.FromMeters(10));

                    UpdateRoute(location);
                    CheckHome(location);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fout bij ophalen locatie: {ex.Message}");
            }
        }

        private void UpdateRoute(Location location)
        {
            Debug.WriteLine("Running {0} at {1}.", nameof(UpdateRoute), DateTime.Now.ToShortTimeString());

            ProcessNewLocation(location);

            // Alleen tekenen als er meer dan 2 punten zijn. Anders heb je natuurlijk geen lijn!
            if (_locationCache.Count >= 2)
            {
                MapElements = [CreatePolyLineOfLocations(_locationCache)];
            }

            Debug.WriteLine("Added to {0}.", args: nameof(MapElements));

            Debug.WriteLine($"Route bijgewerkt: {location.Latitude}, {location.Longitude}");
        }

        private void ProcessNewLocation(Location location)
        {
            // TODO: Niet toevoegen als hij te dichtbij is bij de vorige locatie! En misschien andere logica toevoegen.
            const double minDistance = 0.01;
            if (_locationCache.Count == 0 || location.CalculateDistance(_locationCache.Last(), DistanceUnits.Kilometers) >= minDistance)
            {
                _locationCache.Add(location);
            }
            else 
            {
                Debug.WriteLine($"Locatie te dicht bij de vorige locatie: {location.Latitude}, {location.Longitude}");
            }
           
        }

        private Polyline CreatePolyLineOfLocations(IEnumerable<Location> locations)
        {
            Debug.WriteLine("Constructing {0}", args: nameof(Polyline));
            Polyline polyline = new Polyline
            {
                StrokeColor = Colors.Red,
                StrokeWidth = 12,
            };

            Debug.WriteLine("Adding to {0}.", args: nameof(polyline.Geopath));
            foreach (var loc in locations)
            {
                polyline.Geopath.Add(loc);
            }

            return polyline;
        }

        public async Task SetHome() 
        {
            try
            {

               _home = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));

                if (_home is not null)
                {
                    Debug.WriteLine("Location: {home}", _home); 
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fout bij ophalen locatie: {ex.Message}");
            }


        }

        public async void CheckHome(Location location)
        {

            if (_home is null)
                return;

            var notificationManager = await ShinyHost.Resolve<INotificationManager>();
            var access = await notificationManager.RequestAccess();

            if (access != AccessState.Available)
            {
                Debug.WriteLine("Notificaties zijn niet toegestaan. Geen notificatie verzonden.");
                return;
            }

            double distance = Location.CalculateDistance(_home, location, DistanceUnits.Kilometers);
            if (distance < 0.025)
            {
               
            }
        }

        [RelayCommand]
        public async Task GoToMainPage()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}