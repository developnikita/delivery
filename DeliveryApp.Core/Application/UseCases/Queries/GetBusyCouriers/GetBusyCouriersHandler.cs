using Dapper;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers
{
    public class GetBusyCouriersHandler : IRequestHandler<GetBusyCouriersQuery, GetBusyCouriersModel>
    {
        private readonly string _connectionString;

        public GetBusyCouriersHandler(string connectionString)
        {
            _connectionString = !string.IsNullOrWhiteSpace(_connectionString)
                                ? connectionString
                                : throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<GetBusyCouriersModel> Handle(GetBusyCouriersQuery request, CancellationToken cancellationToken)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var result = await connection.QueryAsync<dynamic>(
                @"SELECT c.id as courier_id, c.name, c.location_x, c.location_y
                  FROM public.couriers as c
                  LEFT JOIN public.storage_places as sp on c.id = sp.courier_id
                  WHERE sp.order_id NOT NULL");

            if (result.AsList().Count == 0)
                return null;

            return new GetBusyCouriersModel([.. result.Select(MapCourier)]);
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
                Id = courier.courier_id,
                Name = courier.name,
                Location = location
            };

            return c;
        }
    }
}
