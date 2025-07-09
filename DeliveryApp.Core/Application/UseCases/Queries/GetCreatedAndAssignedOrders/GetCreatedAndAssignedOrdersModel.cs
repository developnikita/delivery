namespace DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders
{
    public class GetCreatedAndAssignedOrdersModel
    {
        public GetCreatedAndAssignedOrdersModel(List<Order> orders)
        {
            Orders.AddRange(orders);
        }

        public List<Order> Orders { get; set; } = [];
    }

    public class Order
    {
        public Guid Id { get; set; }

        public Location Location { get; set; }
    }

    public class Location
    {
        public int X { get; set; }

        public int Y { get; set; }
    }
}
