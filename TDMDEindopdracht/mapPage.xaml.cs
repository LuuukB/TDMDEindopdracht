namespace TDMDEindopdracht;
using  Map = Microsoft.Maui.Controls.Maps.Map;
public partial class mapPage : ContentPage
{
	public mapPage()
	{
		InitializeComponent();
		Map map = new Map();
		Content = map;
	}
}