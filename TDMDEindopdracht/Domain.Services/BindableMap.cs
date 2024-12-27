using Microsoft.Maui.Controls.Maps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;


namespace TDMDEindopdracht.Domain.Services
{
    public partial class BindableMap : Microsoft.Maui.Controls.Maps.Map
    {
        public static readonly BindableProperty MapElementsProperty =
            BindableProperty.Create(
                nameof(MapElements),
                typeof(ObservableCollection<MapElement>),
                typeof(BindableMap),
                default(ObservableCollection<MapElement>),
                propertyChanged: OnMapElementsChanged);

        public static readonly BindableProperty VisibleRegionProperty =
          BindableProperty.Create(
              nameof(VisibleRegion),
              typeof(MapSpan),
              typeof(BindableMap),
              default(MapSpan),
              propertyChanged: OnVisibleRegionChanged);


        public ObservableCollection<MapElement> MapElements
        {
            get => (ObservableCollection<MapElement>)GetValue(MapElementsProperty);
            set => SetValue(MapElementsProperty, value);
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

        private static void OnMapElementsChanged(BindableObject bindable, Object oldValue, Object newValue) 
        {
            if (bindable is BindableMap map && newValue is ObservableCollection<MapElement> newElements)
            {
                map.MapElements.Clear();
                foreach (var element in newElements)
                {
                    map.MapElements.Add(element);
                }

                newElements.CollectionChanged += (s, e) =>
                {
                    if (e.NewItems != null)
                    {
                        foreach (MapElement item in e.NewItems)
                        {
                            map.MapElements.Add(item);
                        }
                    }

                    if (e.OldItems != null)
                    {
                        foreach (MapElement item in e.OldItems)
                        {
                            map.MapElements.Remove(item);
                        }
                    }
                };
            }
        }
    }




}
    

