using Dapper;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetCouriers
{
    public class GetCouriersHandler : IRequestHandler<GetCouriersQuery, GetCouriersModel>
    {
        private readonly string _connectionString;

        public GetCouriersHandler(string connectionString)
        {
            _connectionString = !string.IsNullOrWhiteSpace(connectionString)
                                ? connectionString
                                : throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<GetCouriersModel> Handle(GetCouriersQuery request, CancellationToken cancellationToken)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var result = await connection.QueryAsync<dynamic>(
                @"SELECT id, name, location_x, location_y FROM public.couriers",
                new { });

            if (result.AsList().Count == 0)
                return null;

            return new GetCouriersModel([.. result.Select(MapCourier)]);
        }

        private Courier MapCourier(dynamic courier)
        {
            var location = new Location
            {
                X = courier.location_x,
                Y = courier.location_y,
            };

            var c = new Courier
            {
                Id = courier.id,
                Name = courier.name,
                Location = location
            };

            return c;
        }
    }
}
