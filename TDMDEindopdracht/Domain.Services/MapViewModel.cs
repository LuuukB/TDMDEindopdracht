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

namespace TDMDEindopdracht.Domain.Services
{
    public partial class MapViewModel : ObservableObject
    {
       
        [ObservableProperty] private bool _isStartEnabled = true;
        [ObservableProperty] private bool _isStopEnabled = false;
        [ObservableProperty] private string _entryText;
        

        private readonly IGeolocation _geolocation;
        private System.Timers.Timer? _locationTimer;
        private Route route;
        private IDatabaseCommunicator _communicator;


        [ObservableProperty] private ObservableCollection<MapElement> _mapElements = new();

        private List<Location> _locationCache = [];
        private ILocationPermisssionService _locationPermisssion;

        [ObservableProperty] public MapSpan _currentMapSpan;

     

        public MapViewModel(IGeolocation geolocation, IDatabaseCommunicator databaseCommunicator,ILocationPermisssionService locationPermission )
        {
            _geolocation = geolocation;
            _communicator = databaseCommunicator;
            _locationPermisssion = locationPermission;
            
            InitializeMapAsync();
        }
        private async void InitializeMapAsync()
        {
            // Wacht op het resultaat van de permissiecontrole
            var status = await _locationPermisssion.CheckAndRequestLocationPermissionAsync();

            if (status == PermissionStatus.Granted)
            {
                try
                {
                    var location = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));

                    if (location is not null)
                    {
                        CurrentMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromMeters(10));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fout bij ophalen locatie: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine("Locatiepermissie is niet verleend.");
                await _locationPermisssion.ShowSettingsIfPermissionDeniedAsync();
            }
        }
        //public async Task CheckAndRequestLocationPermission()
        //{
        //    var status = await _locationPermisssion.CheckAndRequestLocationPermissionAsync();

        //    if (status == PermissionStatus.Denied)
        //    {
        //        await _locationPermisssion.ShowSettingsIfPermissionDeniedAsync();
        //    }
        //}
        //private async void InitializeMap()
        //{
        //    await CheckAndRequestLocationPermission();
        //    try
        //    {
        //        var location = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));

        //        if (location is not null)
        //        {
        //            CurrentMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromMeters(10));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Fout bij ophalen locatie: {ex.Message}");
        //    }
        //}

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
            { var startsWithSpace = Toast.Make("The name of the runs starts with a space", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
                return; }

            // geeft voor nu als afstand standaard 1000 mee. dit moet nog even aangepast worden naar de daadwerkelijk gelopen afstand
            route.StopRoute(1000, EntryText);
            _communicator.AddRoute(route);


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
                     CurrentMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromMeters(10));

                    UpdateRoute(location);
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

        [RelayCommand]
        public async Task GoToMainPage()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}