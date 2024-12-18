using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMDEindopdracht.Domain.Model
{
    public partial class ViewModel : ObservableObject
    {
        public ViewModel() { }

        [RelayCommand]
        private async Task ViewMap()
        {
            await Shell.Current.GoToAsync("//mapPage");
        }
    }
}
