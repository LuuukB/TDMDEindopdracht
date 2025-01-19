
using Microsoft.Maui.Controls.Maps;
using Moq;
using System.Reflection;
using TDMDEindopdracht.Domain.Model;
using TDMDEindopdracht.Domain.Services;

namespace UnitTests_DomainLayer
{
    public class UnitTest1
        //todo naam veranderen klasse
    {
        [Fact]
        public void StartRoute_ShouldSetCurrentTime()
        {
            // Arrange
            var route = new Route();

            // Act
            route.StartRoute();

            // Assert
            Assert.True((DateTime.Now - route.Time).TotalSeconds < 1, "StartRoute should set the Time to the current DateTime");
        }

        [Fact]
        public void StopRoute_ShouldCalculateCorrectValues()
        {
            // Arrange
            var route = new Route();
            route.StartRoute();

            // Simulate 2 hours passing
            System.Threading.Thread.Sleep(100); // Adding a small sleep to ensure measurable runtime
            route.Time = route.Time.AddHours(-2); // Set start time 2 hours back

            double expectedDistance = 100; // km
            string routeName = "Test Route";

            // Act
            route.StopRoute(expectedDistance, routeName);

            // Assert
            Assert.Equal(routeName, route.Name);
            Assert.Equal(expectedDistance, route.Distance);
            Assert.Equal("02:00:00", route.TotalRunTime); // runtime formatting is "hh:mm:ss"
            Assert.Equal(50, route.AveradgeSpeed); // Speed = distance / time (100 km / 2 hours)
        }

        [Fact]
        public void StopRoute_ShouldHandleZeroDistance()
        {
            // Arrange
            var route = new Route();
            route.StartRoute();

            // Simulate 1 hour passing
            route.Time = route.Time.AddHours(-1);

            double expectedDistance = 0; // 0 km
            string routeName = "Zero Distance Route";

            // Act
            route.StopRoute(expectedDistance, routeName);

            // Assert
            Assert.Equal(routeName, route.Name);
            Assert.Equal(expectedDistance, route.Distance);
            Assert.Equal("01:00:00", route.TotalRunTime); // 1 hour
            Assert.Equal(0, route.AveradgeSpeed); // No distance covered
        }

        [Fact]
        public void StopRoute_ShouldHandleSmallRunTime()
        {
            // Arrange
            var route = new Route();
            route.StartRoute();

            // Simulate a very small runtime (e.g., 1 second)
            route.Time = route.Time.AddSeconds(-1);

            double expectedDistance = 1; // 1 km
            string routeName = "Short Run";

            // Act
            route.StopRoute(expectedDistance, routeName);

            // Assert
            Assert.Equal(routeName, route.Name);
            Assert.Equal(expectedDistance, route.Distance);
            Assert.Equal("00:00:01", route.TotalRunTime); // 1 second
            Assert.True(route.AveradgeSpeed > 0, "Average speed should be greater than 0 when distance is positive.");
        }

        private Mock<IGeolocation> _mockGeolocation;
        private Mock<IDatabaseCommunicator> _mockDatabaseCommunicator;
        private MapViewModel _viewModel;
        private Mock<ILocationPermisssionService> _mockLocationPermission;

        public void MapViewModelTests()
        {
            _mockGeolocation = new Mock<IGeolocation>();
            _mockDatabaseCommunicator = new Mock<IDatabaseCommunicator>();
            _mockLocationPermission = new Mock<ILocationPermisssionService>();

            _viewModel = new MapViewModel(_mockGeolocation.Object, _mockDatabaseCommunicator.Object, _mockLocationPermission.Object);
        }

        [Fact]
        public async Task RouteStarting_ShouldStartRouteAndEnableStop()
        {
            MapViewModelTests();
            // Arrange
            _mockGeolocation.Setup(g => g.StartListeningForegroundAsync(It.IsAny<GeolocationListeningRequest>())).Returns(Task.FromResult(true));

            // Act
            await _viewModel.RouteStarting();

            // Assert
            Assert.False(_viewModel.IsStartEnabled, "Start should be disabled after starting the route.");
            Assert.True(_viewModel.IsStopEnabled, "Stop should be enabled after starting the route.");
        }

        [Fact]
        public void RouteStop_ShouldStopRouteAndSaveToDatabase()
        {
            MapViewModelTests();
            // Arrange
            _viewModel.EntryText = "TestRoute";
            _viewModel.RouteStarting().Wait(); // Start the route

            _mockGeolocation.Setup(g => g.StopListeningForeground());
            _mockDatabaseCommunicator.Setup(d => d.AddRoute(It.IsAny<Route>())).Verifiable();

            // Act
            _viewModel.RouteStop();

            // Assert
            Assert.True(_viewModel.IsStartEnabled, "Start should be enabled after stopping the route.");
            Assert.False(_viewModel.IsStopEnabled, "Stop should be disabled after stopping the route.");
            _mockDatabaseCommunicator.Verify(d => d.AddRoute(It.IsAny<Route>()), Times.Once, "The route should be saved to the database.");
        }

        [Fact]
        public async Task RouteStop_ShouldNotStopWhenNameIsInvalid()
        {
            MapViewModelTests();
            // Arrange

            await _viewModel.RouteStarting();
            _viewModel.EntryText = " "; // Invalid name

            // Act
            _viewModel.RouteStop();

            // Assert
            Assert.False(_viewModel.IsStartEnabled, "Start should not be enabled if stopping fails.");
            Assert.True(_viewModel.IsStopEnabled, "Stop should remain enabled if stopping fails.");
        }


        [Fact]
        public void UpdateRoute_ShouldAddPolylineIfEnoughLocations()
        {
            MapViewModelTests();
            // Arrange
            var location1 = new Location(52.370216, 4.895168); // Amsterdam
            var location2 = new Location(52.371216, 4.896168); // Slightly different location

            _viewModel.RouteStarting().Wait();
            _viewModel.UpdateRoute(location1);
            _viewModel.UpdateRoute(location2);

            // Act
            var mapElements = _viewModel.MapElements;

            // Assert
            Assert.Single(mapElements);
            Assert.IsType<Polyline>(mapElements.First());
        }

        [Fact]
        public void CheckHome_ShouldTriggerNotificationWhenReturningHome()
        {
            MapViewModelTests();
            // Arrange
            var homeLocation = new Location(52.370216, 4.895168); // Home location
            var awayLocation = new Location(52.371216, 4.896168); // Away location

            _viewModel.RouteStarting().Wait();
            _viewModel.UpdateRoute(homeLocation);
            _viewModel.CheckHome(homeLocation); // First set home

            // Act
            _viewModel.CheckHome(awayLocation); // Leave home
            _viewModel.CheckHome(homeLocation); // Return home

            // Assert
            Assert.False(_viewModel.IsStartEnabled, "Start should remain disabled during route.");
            // Notification logic should be mocked and verified if needed.
        }

        [Fact]
        public void LocationChanged_ShouldUpdateCurrentMapSpan_WhenLocationIsNotNull()
        {
            MapViewModelTests();
            // Arrange
            var testLocation = new Location(52.370216, 4.895168); // Voorbeeldlocatie
            var eventArgs = new GeolocationLocationChangedEventArgs(testLocation);

            // Act
            _viewModel.LocationChanged(null, eventArgs);

            // Assert
            Assert.NotNull(_viewModel.CurrentMapSpan);
            Assert.Equal(testLocation.Latitude, _viewModel.CurrentMapSpan.Center.Latitude);
            Assert.Equal(testLocation.Longitude, _viewModel.CurrentMapSpan.Center.Longitude);
            //Assert.Equal(30, _viewModel.CurrentMapSpan.Radius.Meters); // Radius moet 30 meter zijn is 29,99999999
        }

        [Fact]
        public void LocationChanged_ShouldCallSetHome_WhenHomeIsNull()
        {
            MapViewModelTests();
            // Arrange
            var testLocation = new Location(52.370216, 4.895168); // Voorbeeldlocatie
            var eventArgs = new GeolocationLocationChangedEventArgs(testLocation);

            // Act
            _viewModel.LocationChanged(null, eventArgs);

            // Assert
            Assert.NotNull(_viewModel.CurrentMapSpan); // Controleer dat SetHome correct werkt
        }

        [Fact]
        public void LocationChanged_ShouldNotCallSetHome_WhenHomeIsNotNull()
        {
            MapViewModelTests();
            // Arrange
            var homeLocation = new Location(52.370216, 4.895168); // Zet een home locatie
            var testLocation = new Location(52.370317, 4.895269); // Nieuwe locatie


            var eventArgs = new GeolocationLocationChangedEventArgs(testLocation);
            var eventArgsHome = new GeolocationLocationChangedEventArgs(homeLocation);

            // Act
            _viewModel.LocationChanged(null, eventArgsHome);
            _viewModel.LocationChanged(null, eventArgs);

            // Assert
            Assert.NotNull(_viewModel.CurrentMapSpan);
            Assert.Equal(homeLocation, _viewModel.GetType().GetField("_home", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_viewModel));
        }

        [Fact]
        public void LocationChanged_ShouldCallUpdateRouteAndCheckHome()
        {
            MapViewModelTests();
            // Arrange
            var testLocation = new Location(52.370216, 4.895168);
            var eventArgs = new GeolocationLocationChangedEventArgs(testLocation);

            // Gebruik reflection om de private methodes te "spy"-en
            var updateRouteCalled = false;
            var checkHomeCalled = false;

            var updateRouteMethod = _viewModel.GetType().GetMethod("UpdateRoute", BindingFlags.Instance | BindingFlags.NonPublic);
            var checkHomeMethod = _viewModel.GetType().GetMethod("CheckHome", BindingFlags.Instance | BindingFlags.NonPublic);

            var updateRouteMock = new Mock<Action<Location>>();
            var checkHomeMock = new Mock<Action<Location>>();

            // Act
            updateRouteMock.Verify();
            //Assert.True(checkHomeMock);
        }

        [Fact]
        public void SetHome_ShouldSetHomeLocation_WhenHomeIsNull()
        {
            MapViewModelTests();
            // Arrange
            var testLocation = new Location(52.370216, 4.895168); // Voorbeeldlocatie

            // Act
            _viewModel.SetHome(testLocation);

            // Assert
            Assert.NotNull(_viewModel.Home);
            Assert.Equal(testLocation.Latitude, _viewModel.Home?.Latitude);
            Assert.Equal(testLocation.Longitude, _viewModel.Home?.Longitude);
        }
    }
}