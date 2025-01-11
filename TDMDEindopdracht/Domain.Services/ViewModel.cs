

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMDEindopdracht.Domain.Model;

namespace TDMDEindopdracht.Domain.Services
{
    public partial class ViewModel : ObservableObject
    {
        
        
        private IDatabaseCommunicator _communicator;
        public ViewModel(IDatabaseCommunicator communicator)
        {
            _communicator = communicator;
            AddRoutes();
        }
        [ObservableProperty]private ObservableCollection<Route> _routes = [];
        [ObservableProperty] private string _name;
        [ObservableProperty] private Route _selectedRoute;
        [ObservableProperty] private string _selectedName;
        [ObservableProperty] private string _selectedDistance;
        [ObservableProperty] private string _selectedTotalRunTime;
        [ObservableProperty] private string _selectedAveradgeSpeed;
        partial void OnSelectedRouteChanged(Route value)
        {
            SelectedName = value?.Name ?? string.Empty;
            SelectedDistance = value?.Distance.ToString() ?? string.Empty;
            SelectedTotalRunTime = value?.TotalRunTime ?? string.Empty;
            SelectedAveradgeSpeed = value?.AveradgeSpeed.ToString() ?? string.Empty;
        }


        [RelayCommand]
        private async Task ViewMap()
        {
            await Shell.Current.GoToAsync("//mapPage");
        }

        public void AddRoutes() { 
            var pastRoutes = _communicator.GetAllRoutes();
            Routes.Clear();

            foreach (var route in pastRoutes)
            {
                Routes.Add(route);
            }
        }

        [RelayCommand]
        public void RefreshRoutes()
        {
            //await Task.Run(() => { AddRoutes();   });
            MainThread.BeginInvokeOnMainThread(() => { AddRoutes(); });
        }

    }

}
