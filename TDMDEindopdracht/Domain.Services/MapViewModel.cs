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

namespace TDMDEindopdracht.Domain.Services
{
    public partial class MapViewModel : ObservableObject
    {
        //[ObservableProperty]
        //private Map mappy;
        [ObservableProperty] private bool _isStartEnabled = true;
        [ObservableProperty] private bool _isStopEnabled = false;

        private readonly IGeolocation _geolocation;


        private System.Timers.Timer? _locationTimer;

        //private readonly List<Location> _routeCoordinates = new();

        [ObservableProperty] private ObservableCollection<MapElement> _mapElements = new();


        [ObservableProperty] public MapSpan _currentMapSpan;

        //[ObservableProperty]
        //public string currentLocation;

        public MapViewModel(IGeolocation geolocation)
        {
            _geolocation = geolocation;
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
        }

        [RelayCommand]
        public void RouteStop()
        {
            Debug.WriteLine("stopping route/timer");

            if (_locationTimer != null)
            {
                _locationTimer.Stop();
                _locationTimer.Dispose();
                _locationTimer = null;
                IsStartEnabled = true;
                IsStopEnabled = false;
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
                    // CurrentMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromMeters(10));

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
            Debug.WriteLine("Constructing {0}", args: nameof(Polyline));
            Polyline polyline = new Polyline
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 5
            };

            // Test geopath.
            Location[] locations = [new(0D, 0D), new(60, 60), new(50D, 50D), new(10, 10)];
            Debug.WriteLine("Adding to {0}.", args: nameof(polyline.Geopath));
            foreach (var loc in locations) polyline.Geopath.Add(loc);

            Debug.WriteLine("Adding to {0}.", args: nameof(MapElements));
            MainThread.InvokeOnMainThreadAsync(() => MapElements.Add(polyline));

            Debug.WriteLine($"Route bijgewerkt: {location.Latitude}, {location.Longitude}");
        }

        [RelayCommand]
        public async Task GoToMainPage()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}