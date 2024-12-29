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
        [ObservableProperty]
        private bool _isStartEnabled = true;
        [ObservableProperty]
        private bool _isStopEnabled = false;

        private IGeolocation _geolocation;


        private System.Timers.Timer? _locationTimer;

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
            //StartLocationUpdates();
        }

        //private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        //{
        //    if (e.Location != null)
        //    {
        //        CurrentMapSpan = MapSpan.FromCenterAndRadius(e.Location, Distance.FromMeters(50));
        //        Debug.WriteLine($"LocatieTESTTTTTTTTTTTTT: {e.Location.Latitude}, {e.Location.Longitude}");

        //        MainThread.BeginInvokeOnMainThread(() =>
        //        {
        //            UpdateRoute(e.Location);
        //        });
        //    }
        //}

       
        [RelayCommand]
        public void RouteStarting() 
        {
            Debug.WriteLine("starting route/timer");
            _locationTimer = new System.Timers.Timer(1000); 
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
            Task.Run(async () =>
            {
                try
                {
                    var location = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
                    if (location != null)
                    { 
                        Debug.WriteLine($"LocatiE: { location}");
                        CurrentMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromMeters(10));

                        UpdateRoute(location);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fout bij ophalen locatie: {ex.Message}");
                }
            });
        }




       

        private void UpdateRoute(Location location)
        {
             Polyline polyline = new Polyline
             {
                StrokeColor = Colors.Blue,
                StrokeWidth = 5

             };

            MapElements.Add(polyline);
          
            polyline.Geopath.Add(location);
            
            

            Debug.WriteLine(MapElements + "mapelementen");
                Debug.WriteLine($"Route bijgewerkt: {location.Latitude}, {location.Longitude}");
           

            //if (MapElements.FirstOrDefault(e => e is Polyline) is not Polyline polyline)
            //{

            //    polyline = new Polyline
            //    {
            //        StrokeColor = Colors.Blue,
            //        StrokeWidth = 5
            //    };
            //    MapElements.Add(polyline);
            //}


            //polyline.Geopath.Add(location);
        }

        //private void UpdateRoute(Location location)
        //{

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

        //    var polyline = new Polyline
        //    {
        //        StrokeColor = Colors.Blue,
        //        StrokeWidth = 5
        //    };

        //    foreach (var coord in MapElements.OfType<Polyline>().SelectMany(p => p.Geopath))
        //    {
        //        polyline.Geopath.Add(coord);
        //    }

        //    polyline.Geopath.Add(location);
        //    //todo mainThread invoke zorgt voor de crash
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        MapElements.Clear();
        //        MapElements.Add(polyline);
        //    });
        //}
        


        
       
        [RelayCommand]
        public async Task GoToMainPage()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }



     

    }
}
