using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;



namespace TDMDEindopdracht.Domain.Services
{
    public partial class BindableMap : Microsoft.Maui.Controls.Maps.Map
    {
        public static readonly BindableProperty MvvmMapElementsProperty =
            BindableProperty.Create(
                nameof(MvvmMapElements),
                typeof(ICollection<MapElement>),
                typeof(BindableMap),
                null,

                // Runt alleen op '= new()', niet op Add() of Clear()!!!
                propertyChanged: (b, _, n) =>
                {
                    if (b is BindableMap map)
                    {
                        map.MapElements.Clear();
                        foreach (var element in (IEnumerable<MapElement>)n)
                        {
                            map.MapElements.Add(element);
                        }
                    }
                });

        public static readonly BindableProperty VisibleRegionProperty =
          BindableProperty.Create(
              nameof(VisibleRegion),
              typeof(MapSpan),
              typeof(BindableMap),
              default(MapSpan),
              propertyChanged: OnVisibleRegionChanged);


        public ICollection<MapElement> MvvmMapElements
        {
            get => (ICollection<MapElement>)GetValue(MvvmMapElementsProperty);
            set => SetValue(MvvmMapElementsProperty, value);
        }

        public MapSpan VisibleRegion
        {
            get => (MapSpan)GetValue(VisibleRegionProperty);
            set => SetValue(VisibleRegionProperty, value);
        }

        private static void OnVisibleRegionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BindableMap map && newValue is MapSpan newRegion)
            {
                map.MoveToRegion(newRegion);
            }
        }
    }
}
    

