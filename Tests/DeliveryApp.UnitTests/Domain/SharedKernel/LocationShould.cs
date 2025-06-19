using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.SharedKernel
{
    public class LocationShould
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(3, 5)]
        [InlineData(10, 10)]
        public void CreateCorrectLocationObject(int x, int y)
        {
            // Arrange

            // Act
            var location = Location.Create(x, y);

            // Assert
            location.Value.X.Should().Be(x);
            location.Value.Y.Should().Be(y);
        }

        [Fact]
        public void CreateCorrectRandomLocationObject()
        {
            // Arrange

            // Act
            var location = Location.CreateRandom();

            location.X.Should().BeGreaterThanOrEqualTo(1);
            location.X.Should().BeLessThanOrEqualTo(10);
            location.Y.Should().BeGreaterThanOrEqualTo(1);
            location.Y.Should().BeLessThanOrEqualTo(10);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(11)]
        [InlineData(12)]
        public void ReturnErrorResultWhenXOutsideTheRange(int x)
        {
            // Arrange
            const int y = 1;

            // Act
            var location = Location.Create(x, y);

            // Assert
            location.Error.Should().NotBeNull();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(11)]
        [InlineData(12)]
        public void ReturnErrorResultWhenYOutsideTheRange(int y)
        {
            // Arrange
            const int x = 1;

            // Act
            var location = Location.Create(x, y);

            // Assert
            location.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeEqualToLocationWithSameParameters()
        {
            // Arrange
            var x = 1;
            var y = 2;
            var l1 = Location.Create(x, y).Value;
            var l2 = Location.Create(x, y).Value;

            // Act
            var result = l1 == l2;

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(3, 1)]
        [InlineData(1, 1)]
        public void NotBeEqualToLocationWithDifferentParameters(int x, int y)
        {
            // Arrange
            var l1 = Location.Create(8, 9).Value;
            var l2 = Location.Create(x, y).Value;

            // Act
            var result = l1 == l2;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CalculateDistanceWithSameCoordinates()
        {
            // Arrange
            var l1 = Location.Create(1, 1).Value;
            var l2 = Location.Create(1, 1).Value;

            // Act
            var distance = l1.DistanceTo(l2);

            // Assert
            distance.Value.Should().Be(0);
        }

        [Fact]
        public void CaclulateDistanceWithDifferentCoordinates()
        {
            // Arrange
            var l1 = Location.Create(1, 4).Value;
            var l2 = Location.Create(2, 1).Value;

            // Act
            var distance = l1.DistanceTo(l2);

            // Assert
            distance.Value.Should().Be(4);
        }

        [Fact]
        public void ReturnErrorResultWhenLocationIsNull()
        {
            // Arrange
            var l1 = Location.Create(1, 1).Value;
            Location l2 = null;

            // Act
            var distance = l1.DistanceTo(l2);

            // Assert
            distance.Error.Should().NotBeNull();
        }
    }
}
