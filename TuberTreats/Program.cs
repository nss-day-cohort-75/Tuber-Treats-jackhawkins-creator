using TuberTreats.Models;
using TuberTreats.Models.DTOs;

List<Customer> customers = new List<Customer>
{
    new Customer { Id = 1, Name = "The Spudmeister", Address = "123 Potato Jct" },
    new Customer { Id = 2, Name = "Yammy Tammy", Address = "456 Sweet Ave" },
    new Customer { Id = 3, Name = "Russet Rocket", Address = "789 Idaho Rd" },
    new Customer { Id = 4, Name = "Tot King", Address = "101 Tater Ln" },
    new Customer { Id = 5, Name = "Mashley Simpson", Address = "202 Golden St" }
};

List<Topping> toppings = new List<Topping>
{
    new Topping { Id = 1, Name = "Soy Sauce" },
    new Topping { Id = 2, Name = "Cheddar Cheese" },
    new Topping { Id = 3, Name = "Sour Cream" },
    new Topping { Id = 4, Name = "Chives" },
    new Topping { Id = 5, Name = "Bacon Bits" }
};

List<TuberDriver> tuberDrivers = new List<TuberDriver>
{
    new TuberDriver { Id = 1, Name = "Chowda" },
    new TuberDriver { Id = 2, Name = "Crinkle Fry" },
    new TuberDriver { Id = 3, Name = "Hashbrown Harry" }
};

List<TuberOrder> tuberOrders = new List<TuberOrder>
{
    new TuberOrder
    {
        Id = 1,
        OrderPlacedOnDate = DateTime.Now.AddHours(-2),
        CustomerId = 1,
        TuberDriverId = 2,
        DeliveredOnDate = DateTime.Now.AddMinutes(-30)
    },
    new TuberOrder
    {
        Id = 2,
        OrderPlacedOnDate = DateTime.Now.AddDays(-1),
        CustomerId = 3,
        TuberDriverId = 1,
        DeliveredOnDate = DateTime.Now.AddDays(-1).AddHours(2)
    },
    new TuberOrder
    {
        Id = 3,
        OrderPlacedOnDate = DateTime.Now.AddMinutes(-90),
        CustomerId = 5,
        TuberDriverId = null,
        DeliveredOnDate = null
    }
};

List<TuberTopping> tuberToppings = new List<TuberTopping>
{
    new TuberTopping { Id = 1, TuberOrderId = 1, ToppingId = 2 }, // Cheddar
    new TuberTopping { Id = 2, TuberOrderId = 1, ToppingId = 3 }, // Sour Cream
    new TuberTopping { Id = 3, TuberOrderId = 2, ToppingId = 1 }, // Soy Sauce
    new TuberTopping { Id = 4, TuberOrderId = 2, ToppingId = 4 }, // Chives
    new TuberTopping { Id = 5, TuberOrderId = 3, ToppingId = 5 }  // Bacon Bits
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//add endpoints here

//tuberorders (all, order by id w/ customer 'n' optional driver/toppings, new order, put to assign drivers, complete order)
app.MapGet("/api/tuberorders", () =>
{
    return tuberOrders.Select(to => new TuberOrderDTO
    {
        Id = to.Id,
        OrderPlacedOnDate = to.OrderPlacedOnDate,
        CustomerId = to.CustomerId,
        TuberDriverId = to.TuberDriverId,
        DeliveredOnDate = to.DeliveredOnDate
    });
});

app.MapGet("/api/tuberorders/{id}", (int id) =>
{
    TuberOrder order = tuberOrders.FirstOrDefault(to => to.Id == id);

    if (order == null)
    {
        return Results.NotFound();
    }

    Customer customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);

    TuberDriver driver = tuberDrivers.FirstOrDefault(d => d.Id == order.TuberDriverId);

    //get all tt's (i.e., join table) instances w/ matching order id
    List<TuberTopping> orderToppings = tuberToppings.Where(tt => tt.TuberOrderId == order.Id).ToList();
    //all topping instances based on toppingids from join table
    List<Topping> toppingList = toppings.Where(t => orderToppings.Any(ot => ot.ToppingId == t.Id)).ToList();

    return Results.Ok(
        new TuberOrderDTO
        {
            Id = order.Id,
            OrderPlacedOnDate = order.OrderPlacedOnDate,
            CustomerId = order.CustomerId,
            TuberDriverId = order.TuberDriverId,
            DeliveredOnDate = order.DeliveredOnDate,
            Customer = new CustomerDTO
            {
                Id = customer.Id,
                Name = customer.Name,
                Address = customer.Address
            },
            TuberDriver = driver != null ? new TuberDriverDTO
            {
                Id = driver.Id,
                Name = driver.Name
            } : null,
            Toppings = toppingList.Select(t => new ToppingDTO
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        });
});

app.MapPost("/api/tuberorders", (TuberOrder tuberOrder) =>
{
    tuberOrder.Id = tuberOrders.Max(to => to.Id) + 1;

    tuberOrder.OrderPlacedOnDate = DateTime.Now;

    tuberOrders.Add(tuberOrder);

    return Results.Created($"/tuberorders/{tuberOrder.Id}", new TuberOrderDTO
    {
        Id = tuberOrder.Id,
        OrderPlacedOnDate = tuberOrder.OrderPlacedOnDate,
        CustomerId = tuberOrder.CustomerId,
        TuberDriverId = tuberOrder.TuberDriverId,
        DeliveredOnDate = tuberOrder.DeliveredOnDate
    });
});

app.MapPut("/api/tuberorders/{id}", (int id, TuberOrder tuberOrder) =>
{
    TuberOrder orderToUpdate = tuberOrders.FirstOrDefault(to => to.Id == id);

    orderToUpdate.TuberDriverId = tuberOrder.TuberDriverId;

    return Results.NoContent();
});

app.MapPost("/api/tuberorders/{id}/complete", (int id) =>
{
    TuberOrder orderToComplete = tuberOrders.FirstOrDefault(to => to.Id == id);
    orderToComplete.DeliveredOnDate = DateTime.Today;
});

//toppings (all & by id)
app.MapGet("/api/toppings", () =>
{
    return toppings.Select(t => new ToppingDTO
    {
        Id = t.Id,
        Name = t.Name
    });
});

app.MapGet("/api/toppings/{id}", (int id) =>
{
    Topping topping = toppings.FirstOrDefault(t => t.Id == id);

    if (topping == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new ToppingDTO
    {
        Id = topping.Id,
        Name = topping.Name
    });
});


//tubertoppings (all, add topping to TuberOrder, delete topping) 
app.MapGet("/api/tubertoppings", () =>
{
    return tuberToppings.Select(tt => new TuberToppingDTO
    {
        Id = tt.Id,
        TuberOrderId = tt.TuberOrderId,
        ToppingId = tt.ToppingId
    });
});

app.MapPost("/api/tubertoppings", (TuberTopping tuberTopping) =>
{
    tuberTopping.Id = tuberToppings.Max(tt => tt.Id) + 1;

    tuberToppings.Add(tuberTopping);

    return Results.Created($"/api/tubertoppings/{tuberTopping.Id}", new TuberToppingDTO
    {
        Id = tuberTopping.Id,
        TuberOrderId = tuberTopping.TuberOrderId,
        ToppingId = tuberTopping.ToppingId
    });
});

app.MapDelete("/api/tubertoppings/{id}", (int id) =>
{
    TuberTopping tuberToppingToDelete = tuberToppings.FirstOrDefault(tt => tt.Id == id);

    if (tuberToppingToDelete == null)
    {
        return Results.NotFound();
    }

    tuberToppings.Remove(tuberToppingToDelete);

    return Results.NoContent();
});

//customers (all, customer ids w/ orders, add customer, delete customer)
app.MapGet("/api/customers", () =>
{
    return customers.Select(c => new CustomerDTO
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address
    });
});

app.MapGet("/api/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);

    if (customer == null)
    {
        return Results.NotFound();
    }

    List<TuberOrder> customerOrders = tuberOrders.Where(order => order.CustomerId == id).ToList();

    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        TuberOrders = customerOrders.Select(order => new TuberOrderDTO
        {
            Id = order.Id,
            OrderPlacedOnDate = order.OrderPlacedOnDate,
            CustomerId = order.CustomerId,
            TuberDriverId = order.TuberDriverId,
            DeliveredOnDate = order.DeliveredOnDate
        }).ToList()
    });
});

app.MapPost("/api/customers", (Customer customer) =>
{

    // Generate a new ID
    customer.Id = customers.Max(c => c.Id) + 1;

    // Add to list
    customers.Add(customer);

    return Results.Created($"/api/customers/{customer.Id}", new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address
    });
});

app.MapDelete("/api/customers/{id}", (int id) =>
{
    // Find the customer
    Customer customerToDelete = customers.FirstOrDefault(c => c.Id == id);

    if (customerToDelete == null)
    {
        return Results.NotFound();
    }

    // Remove the customer
    customers.Remove(customerToDelete);

    return Results.NoContent();
});



//tuberdrivers (all & by id w/ deliveries)
app.MapGet("/api/tuberdrivers", () =>
{
    return tuberDrivers.Select(td => new TuberDriverDTO
    {
        Id = td.Id,
        Name = td.Name
    });
});

app.MapGet("api/tuberdrivers/{id}", (int id) =>
{
    TuberDriver tuberDriver = tuberDrivers.FirstOrDefault(td => td.Id == id);

    if (tuberDriver == null)
    {
        return Results.NotFound();
    }

    List<TuberOrder> deliveries = tuberOrders.Where(order => order.TuberDriverId == id).ToList();

    return Results.Ok(new TuberDriverDTO
    {
        Id = tuberDriver.Id,
        Name = tuberDriver.Name,
        TuberDeliveries = deliveries.Select(d => new TuberOrderDTO
        {
            Id = d.Id,
            OrderPlacedOnDate = d.OrderPlacedOnDate,
            CustomerId = d.CustomerId,
            TuberDriverId = d.TuberDriverId,
            DeliveredOnDate = d.DeliveredOnDate
        }).ToList()
    });

});

app.Run();
//don't touch or move this!
public partial class Program { }