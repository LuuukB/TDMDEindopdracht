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
using Microsoft.Maui.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using Microsoft.Maui.Devices.Sensors;
using static System.Net.Mime.MediaTypeNames;
using CommunityToolkit.Maui.Alerts;

namespace TDMDEindopdracht.Domain.Services
{
    public partial class MapViewModel : ObservableObject
    {

        //[ObservableProperty]
        //private Map mappy;

        private IGeolocation _geolocation;
        private Location? _home;

        //private readonly List<Location> _routeCoordinates = new();

        [ObservableProperty]
        private ObservableCollection<MapElement> mapElements = new();


        [ObservableProperty]
        public MapSpan currentMapSpan;

        //[ObservableProperty]
        //public string currentLocation;

        public MapViewModel( IGeolocation geolocation)
        {
            
            _geolocation = geolocation;

            //_geolocation.LocationChanged += OnLocationChanged;
            StartLocationUpdates();

        }

        private async void StartLocationUpdates()
        {
            try
            {
                while (true)
                {
                    var location = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
                    if (location != null)
                    {
                        Debug.WriteLine($"Locatie: {location.Latitude}, {location.Longitude}");

                        CurrentMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromMeters(50));
                        //UpdateRoute(location);
                        CheckHome(location);
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fout bij ophalen locatie: {ex.Message}");
            }
        }
        private void UpdateRoute(Location location)
        {

            //MainThread.BeginInvokeOnMainThread(() =>
            //{

            //    if (MapElements.FirstOrDefault(e => e is Polyline) is Polyline polyline)
            //    {

            //        polyline.Geopath.Add(location);
            //    }
            //    else
            //    {

            //        var newPolyline = new Polyline
            //        {
            //            StrokeColor = Colors.Blue,
            //            StrokeWidth = 5
            //        };
            //        newPolyline.Geopath.Add(location);
            //        MapElements.Add(newPolyline);
            //    }
            //});

            var polyline = new Polyline
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 5
            };

            foreach (var coord in MapElements.OfType<Polyline>().SelectMany(p => p.Geopath))
            {
                polyline.Geopath.Add(coord);
            }

            polyline.Geopath.Add(location);
            //todo mainThread invoke zorgt voor de crash
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MapElements.Clear();
                MapElements.Add(polyline);
            });
        }
        //private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        //{

        //    //if (e?.Location != null)
        //    //{
        //    //    Debug.WriteLine("Location changed");
        //    //    UpdateLocation(e.Location);
        //    //}
        //    if (e?.Location != null)
        //    {
        //        Debug.WriteLine("in LocaionCHANGEEDddddddddddddddddddddddddddddddddd");
        //        CurrentLocation = $"{e.Location.Latitude}, {e.Location.Longitude}";


        //        var newLocation = new Location(e.Location.Latitude, e.Location.Longitude);
        //        _routeCoordinates.Add(newLocation);


        //        CurrentMapSpan = MapSpan.FromCenterAndRadius(newLocation, Distance.FromMeters(50));
        //        Mappy.MoveToRegion(CurrentMapSpan);

        //        AddPolylinesToMap();
        //    }

        //}
        //private void UpdateLocation(Location location)
        //{
        //    currentLocation = $"{location.Latitude}, {location.Longitude}";
        //    _routeCoordinates.Add(location);

        //    CurrentMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromMeters(50));

        //    AddPolylinesToMap();
        //}

        
        //private async void StartLocationUpdates()   
        //{
        //    try
        //    {
        //        var location = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));
        //        if (location != null)
        //        {
        //            Debug.WriteLine($"Current location222: {location.Latitude}, {location.Longitude}");
        //            _routeCoordinates.Add(location);
        //            CurrentMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromMeters(50));
        //            Mappy.MoveToRegion(CurrentMapSpan);

        //            AddPolylinesToMap();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Geolocation failed: {ex.Message}");
        //    }
        //}


        //private void AddPolylinesToMap()
        //{
        //    if (_routeCoordinates.Count < 2) {
        //        return;
        //    }
            

        //    Polyline polyline = new Polyline
        //    {
        //        StrokeColor = Colors.Blue,
        //        StrokeWidth = 5,
        //    };

        //    foreach (var coord in _routeCoordinates) {
        //        Debug.WriteLine("LINES" + coord);
        //        polyline.Geopath.Add(coord);
        //    }
           
        //    //Mappy.MapElements.Add(polyline);
        //    //todo: het heeft de lines wel alleen maakt er geen polyline van op de map. maar de rest is er  
        //    MainThread.InvokeOnMainThreadAsync(() =>
        //        {
        //            Mappy.MapElements.Add(polyline);
                
        //        });
        //}

        [RelayCommand]
        public async Task GoToMainPage()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        [RelayCommand]
        public async Task StartRoute()
        {
            try
            {
                 _home = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
                if (_home != null)
                {
                    Debug.WriteLine($"Locatie: {_home.Latitude}, {_home.Longitude}");

                    //CurrentMapSpan = MapSpan.FromCenterAndRadius(_home, Distance.FromMeters(50));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
            }
            

        }

        public void CheckHome(Location location)
        {
            double distance = Location.CalculateDistance(_home, location, DistanceUnits.Kilometers);
            if (distance < 0.025)
            {
                var toast = Toast.Make("u are almost home", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
            }
        }


     

    }
}
