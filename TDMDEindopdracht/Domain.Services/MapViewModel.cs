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

namespace TDMDEindopdracht.Domain.Services
{
    public partial class MapViewModel : ObservableObject
    {

        private readonly Map _map;
        private IGeolocation _geolocation;

        private List<Location> _routeCoordinates = new List<Location>();

        [ObservableProperty]
        public MapSpan currentMapSpan;

        [ObservableProperty]
        public string currentLocation;

        public MapViewModel(Map map, IGeolocation geolocation)
        {
            _map = map;
            _geolocation = geolocation;

            _geolocation.LocationChanged += LocationChanged;
        }

       
        private async void LocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            if (e?.Location != null)
            {

                CurrentLocation = e.Location.ToString();


                _routeCoordinates.Add(new Location(e.Location.Latitude, e.Location.Longitude));


                CurrentMapSpan = MapSpan.FromCenterAndRadius(e.Location, Distance.FromMeters(500));
                


                AddPolylineToMap();
            }
            
        }

        private void AddPolylineToMap()
        {
            var polyline = new Polyline
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 5
            };

          
            foreach (var coordinate in _routeCoordinates)
            {
                polyline.Geopath.Add(coordinate);
            }

            
            _map.MapElements.Clear(); 
            _map.MapElements.Add(polyline); 
        }


        [RelayCommand]
        public async Task GoToMainPage()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }



     

    }
}
