using CSharpFunctionalExtensions;
using Dapper;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders
{
    public class GetCreatedAndAssignedOrdersHandler : IRequestHandler<GetCreatedAndAssignedOrdersQuery, GetCreatedAndAssignedOrdersModel>
    {
        private readonly string _connectionString;

        public GetCreatedAndAssignedOrdersHandler(string connectionString)
        {
            _connectionString = !string.IsNullOrWhiteSpace(connectionString)
                                ? connectionString
                                : throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<GetCreatedAndAssignedOrdersModel> Handle(GetCreatedAndAssignedOrdersQuery request, CancellationToken cancellationToken)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var result = await connection.QueryAsync<dynamic>(
                @"SELECT o.id as order_id, o.location_x, o.location_y
                  FROM public.orders as o
                  WHERE o.status IN (@created, @assigned);",
                new { created = OrderStatus.Created.Name, assigned = OrderStatus.Assigned.Name });

            if (result.AsList().Count == 0)
                return null;

            return new GetCreatedAndAssignedOrdersModel([.. result.Select(MapOrder)]);
        }

        private Order MapOrder(dynamic order)
        {
            var location = new Location
            {
                X = order.location_x,
                Y = order.location_y,
            };

            var o = new Order
            {
                Id = order.order_id,
                Location = location
            };

            return o;
        }
    }
}
