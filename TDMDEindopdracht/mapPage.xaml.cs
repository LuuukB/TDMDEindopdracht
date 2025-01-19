using Microsoft.Maui.Controls.Maps;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TDMDEindopdracht.Domain.Services;

namespace TDMDEindopdracht;

public partial class mapPage : ContentPage
{
	public mapPage(MapViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }

  

   
}