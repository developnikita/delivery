namespace DeliveryApp.Core.Application.UseCases.Queries.GetCouriers
{
    public class GetCouriersModel
    {
        public GetCouriersModel(List<Courier> couriers)
        {
            Couriers.AddRange(couriers);
        }

        public List<Courier> Couriers { get; set; } = [];
    }

    public class Courier
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Location Location { get; set; }
    }

    public class Location
    {
        public int X { get; set; }

        public int Y { get; set; }
    }

}
