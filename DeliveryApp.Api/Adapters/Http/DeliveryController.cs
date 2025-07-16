using DeliveryApp.Core.Application.UseCases.Commands.CreateCourier;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Queries.GetCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;
using Location = OpenApi.Models.Location;

namespace DeliveryApp.Api.Adapters.Http
{
    public class DeliveryController : DefaultApiController
    {
        private readonly IMediator _mediator;

        public DeliveryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<IActionResult> CreateCourier([FromBody] NewCourier newCourier)
        {
            var createCourierCommand = new CreateCourierCommand(newCourier.Name, newCourier.Speed);
            var response = await _mediator.Send(createCourierCommand);
            if (response)
                return Ok();
            return Conflict();
        }

        public override async Task<IActionResult> CreateOrder()
        {
            var orderId = Guid.NewGuid();
            var street = "Айтишная";
            var createOrderCommand = new CreateOrderCommand(orderId, street, 2);
            var response = await _mediator.Send(createOrderCommand);
            if (response)
                return Ok();
            return Conflict();
        }

        public override async Task<IActionResult> GetCouriers()
        {
            // Вызываем Query
            var getAllCouriersQuery = new GetCouriersQuery();
            var response = await _mediator.Send(getAllCouriersQuery);

            // Мапим результат Query на Model
            if (response == null) return NotFound();
            var model = response.Couriers.Select(c => new OpenApi.Models.Courier
            {
                Id = c.Id,
                Name = c.Name,
                Location = new Location { X = c.Location.X, Y = c.Location.Y }
            });
            return Ok(model);

        }

        public override async Task<IActionResult> GetOrders()
        {
            // Вызываем Query
            var getActiveOrdersQuery = new GetCreatedAndAssignedOrdersQuery();
            var response = await _mediator.Send(getActiveOrdersQuery);

            // Мапим результат Query на Model
            if (response == null) return NotFound();
            var model = response.Orders.Select(o => new OpenApi.Models.Order
            {
                Id = o.Id,
                Location = new Location { X = o.Location.X, Y = o.Location.Y }
            });
            return Ok(model);
        }
    }
}
