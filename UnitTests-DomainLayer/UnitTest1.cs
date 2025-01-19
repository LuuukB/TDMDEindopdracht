
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
    }
}