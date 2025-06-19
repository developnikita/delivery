using CSharpFunctionalExtensions;
using Primitives;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.SharedKernel
{
    public class Location : ValueObject
    {
        [ExcludeFromCodeCoverage]
        private Location() { }

        private Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }

        public Result<int, Error> DistanceTo(Location other)
        {
            if (other == null)
                return GeneralErrors.ValueIsRequired(nameof(other));
            if (this == other)
                return 0;

            var xDistance = Math.Abs(X - other.X);
            var yDistance = Math.Abs(Y - other.Y);

            return xDistance + yDistance;
        }

        [ExcludeFromCodeCoverage]
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return X;
            yield return Y;
        }

        public static Result<Location, Error> Create(int x, int y)
        {
            if (x > MaxLocation.X || x < MinLocation.X)
                return GeneralErrors.ValueIsInvalid(nameof(x));
            if (y > MaxLocation.Y || y < MinLocation.Y)
                return GeneralErrors.ValueIsInvalid(nameof(x));

            return new Location(x, y);
        }

        public static Location CreateRandom()
        {
            var random = new Random();
            var randomX = random.Next(1, 11);
            var randomY = random.Next(1, 11);

            return new(randomX, randomY);
        }

        public static Location MinLocation = new(1, 1);
        public static Location MaxLocation = new(10, 10);
    }
}
