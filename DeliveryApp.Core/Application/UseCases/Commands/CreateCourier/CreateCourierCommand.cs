using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateCourier
{
    public class CreateCourierCommand : IRequest<bool>
    {
        public CreateCourierCommand(string name, int speed)
        {
            if (speed <= 0)
                throw new ArgumentException(nameof(speed));
            Speed = speed;

            Name = !string.IsNullOrEmpty(name)
                   ? name
                   : throw new ArgumentException(nameof(name));
        }

        public string Name { get; }
        public int Speed { get; }
    }
}
