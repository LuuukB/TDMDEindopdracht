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

namespace TDMDEindopdracht.Domain.Services
{
    public partial class MapViewModel : ObservableObject
    {

        [ObservableProperty]
        private Map mappy;
        private IGeolocation _geolocation;

        private readonly List<Location> _routeCoordinates = new();



        [ObservableProperty]
        public MapSpan currentMapSpan;

        [ObservableProperty]
        public string currentLocation;

        public MapViewModel(Map map, IGeolocation geolocation)
        {
            Mappy = map;
            _geolocation = geolocation;

            _geolocation.LocationChanged += OnLocationChanged;
        }


        private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            if (e?.Location != null)
            {
                Debug.WriteLine("in LocaionCHANGEEDddddddddddddddddddddddddddddddddd");
                CurrentLocation = $"{e.Location.Latitude}, {e.Location.Longitude}";


                var newLocation = new Location(e.Location.Latitude, e.Location.Longitude);
                _routeCoordinates.Add(newLocation);


                currentMapSpan = MapSpan.FromCenterAndRadius(newLocation, Distance.FromMeters(50));
                Mappy.MoveToRegion(currentMapSpan);

                AddPolylinesToMap();
            }

        }

        [RelayCommand]
        public void locatieToeveogen() {
            StartLocationUpdates();
        
        }
        private async void StartLocationUpdates()   
        {
            try
            {
                var location = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));
                if (location != null)
                {
                    Debug.WriteLine($"Current location222: {location.Latitude}, {location.Longitude}");
                    _routeCoordinates.Add(location);

                    currentMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromMeters(50));
                    Mappy.MoveToRegion(currentMapSpan);

                    AddPolylinesToMap();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Geolocation failed: {ex.Message}");
            }
        }


        private void AddPolylinesToMap()
        {
           
            Polyline polyline = new Polyline
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 5
                
            };

            foreach (var line in _routeCoordinates) {
                Debug.WriteLine("LINES" + line);
                polyline.Geopath.Add(line);
            }
            Mappy.MapElements.Clear();
            Mappy.MapElements.Add(polyline);
            //todo: het heeft de lines wel alleen maakt er geen polyline van op de map. maar de rest is er
            MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Mappy.MapElements.Add(polyline);
                });
        }

        [RelayCommand]
        public async Task GoToMainPage()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }



     

    }
}
