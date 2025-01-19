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
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;





namespace TDMDEindopdracht.Domain.Services
{
    public partial class MapViewModel : ObservableObject
    {
     
        [ObservableProperty] private bool _isStartEnabled = true;
        [ObservableProperty] private bool _isStopEnabled = false;
        [ObservableProperty] private string _entryText;

        private Location? _home;
        private bool _hasLeftHome = false;
        private const double HomeRadiusMeters = 15.0;



        private readonly IGeolocation _geolocation;
        private System.Timers.Timer? _locationTimer;
        private Route route;
        private IDatabaseCommunicator _communicator;


        [ObservableProperty] private ObservableCollection<MapElement> _mapElements = new();

        private List<Location> _locationCache = [];


        [ObservableProperty] public MapSpan _currentMapSpan;


        public MapViewModel(IGeolocation geolocation, IDatabaseCommunicator databaseCommunicator)
        {
            _geolocation = geolocation;
            _communicator = databaseCommunicator;
            //todo: beter gezegd de currentmapspan moet naar user toe op het moment dat de map word gemaakt.
            _geolocation.LocationChanged += LocationChanged;

            InitializeMap();
        }
        private void LocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            if (e?.Location != null)
            { 
                try
                {
                    Debug.WriteLine("Location: {0}", e.Location);
                    CurrentMapSpan = MapSpan.FromCenterAndRadius(e.Location, Distance.FromMeters(30));
                    SetHome(e.Location); 
                    Debug.WriteLine("Home: ", _home);
                    UpdateRoute(e.Location);
                    CheckHome(e.Location);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fout bij ophalen locatie: {ex.Message}");
                }
            }
        }

        private void SetHome(Location location)
        {
            if (_home == null)
            {
                _home = location;
            }
        }

        public async Task ListeningToLocation() 
        {
            await _geolocation.StartListeningForegroundAsync(new GeolocationListeningRequest
            {
                MinimumTime = TimeSpan.FromSeconds(5),
                DesiredAccuracy = GeolocationAccuracy.Best
            });
        
        }

        public void StopListeningToLocation() 
        {
            _geolocation.StopListeningForeground();
        }
        private async void InitializeMap()
        {
            try
            {
                var location = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));

                if (location is not null)
                {
                    CurrentMapSpan = MapSpan.FromCenterAndRadius(location, Microsoft.Maui.Maps.Distance.FromMeters(20));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fout bij ophalen locatie: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task RouteStarting()
        {
            Debug.WriteLine("starting route");
            IsStartEnabled = false;
            IsStopEnabled = true;
           await ListeningToLocation();

            route = new Route();
            route.StartRoute();
        }
        
        [RelayCommand]
        public void RouteStop()
        {
           
           

            if (string.IsNullOrEmpty(EntryText))
            { var noName = Toast.Make("U need to fill in a name", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
                return; }

            if (EntryText.StartsWith(" "))
            { var startsWithSpace = Toast.Make("The name of the run starts with a space", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
                return; } 
            Debug.WriteLine("stopping route");
            StopListeningToLocation();
            IsStartEnabled = true;
            IsStopEnabled = false;

            // geeft voor nu als afstand standaard 1000 mee. dit moet nog even aangepast worden naar de daadwerkelijk gelopen afstand
            route.StopRoute(1000, EntryText);
            _communicator.AddRoute(route);
            Debug.WriteLine("EntryText text: ", EntryText);
        
            var totalDistance = CalculateTotalDistanceRoute();
            _routeHandler.StopRoute(totalDistance, EntryText);


            _locationCache.Clear();
            MapElements = new ObservableCollection<MapElement>();
            _home = null;


        }

        private double CalculateTotalDistanceRoute() 
        {
            return _locationCache.Count < 2? 0: _locationCache.Zip(_locationCache.Skip(1), (a, b) => a.CalculateDistance(b, DistanceUnits.Kilometers))
                        .Sum() * 1000;
            //*1000 zorgt voor dat het meters zijn
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
            const double minDistance = 0.005;
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
        public async Task<bool> CheckPermissionNotification() 
        {
            var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (status == PermissionStatus.Granted)
            {
                return true; 
            }
            return false;
        }

        public async void CheckHome(Location location)
        {

            if (_home == null) return;

            double distanceInKm = Location.CalculateDistance(_home, location, DistanceUnits.Kilometers);
            double distanceInMeters = distanceInKm * 1000;

            if (distanceInMeters > HomeRadiusMeters)
            {
                _hasLeftHome = true; 
                Debug.WriteLine("You have left the home geofence.");
            }
            else if (distanceInMeters <= HomeRadiusMeters && _hasLeftHome)
            {
                _hasLeftHome = false;
                Debug.WriteLine("You have entered the home geofence!");
                bool hasPermission = await CheckPermissionNotification();
                if (hasPermission)
                {
                    var request = new NotificationRequest
                    {
                        NotificationId = 1,
                        Title = "Welcome Home",
                        Description = "You are close to your home!",
                        BadgeNumber = 1
                    };

                    await LocalNotificationCenter.Current.Show(request);
                }
            }
    
        }

        [RelayCommand]
        public async Task GoToMainPage()
        { 
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}