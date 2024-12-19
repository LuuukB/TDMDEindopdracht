using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMDEindopdracht.Domain.Services
{
    public partial class ViewModel : ObservableObject
    {
        IGeolocation geolocation;
        public ViewModel(IGeolocation geolocation)
        {
            this.geolocation = geolocation;

        }


        [RelayCommand]
        public async Task GetCurrentLocation()
        {

        }

        [RelayCommand]
        private async Task ViewMap()
        {
            await Shell.Current.GoToAsync("//mapPage");
        }

    }

}
