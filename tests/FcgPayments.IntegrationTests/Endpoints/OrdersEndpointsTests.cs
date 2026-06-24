using System.Net;
using System.Net.Http.Json;
using FcgPayments.Application.Commands.Orders;
using FcgPayments.Application.Responses.Orders;
using FcgPayments.Domain.Entities;
using FcgPayments.Domain.ValueObjects;
using FcgPayments.IntegrationTests.Fixtures;
using FcgPayments.IntegrationTests.TestHelpers;
using FcgPayments.SharedKernel.Responses;
using Shouldly;

namespace FcgPayments.IntegrationTests.Endpoints;

[Collection("IntegrationTests")]
public class OrdersIntegrationTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private const string OrdersEndpoint = "/api/orders";

    private readonly IntegrationTestFixture _fixture = fixture;

    public async ValueTask InitializeAsync()
        => await _fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync()
        => ValueTask.CompletedTask;

    // ── POST /orders ─────────────────────────────
    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_CriarPedido_Entao_Retorna201ComId()
    {
        // Arrange
        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);
        var request = CreateValidOrderCommand();

        // Act
        var response = await client.PostAsJsonAsync(
            OrdersEndpoint,
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var result = await response.ReadContentAsync<CreateOrderResponse>(TestContext.Current.CancellationToken);
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Dado_UsuarioNaoAutenticado_Quando_CriarPedido_Entao_Retorna401()
    {
        // Arrange
        var client = TestAuthHelper.CreateAnonymousClient(_fixture);
        var request = CreateValidOrderCommand();

        // Act
        var response = await client.PostAsJsonAsync(
            OrdersEndpoint,
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var problem = await response.ReadProblemDetailsAsync(TestContext.Current.CancellationToken);
        problem.Status.ShouldBe(401);
        problem.Title.ShouldBe("Unauthorized");
    }

    [Fact]
    public async Task Dado_RequestInvalido_Quando_CriarPedido_Entao_Retorna400()
    {
        // Arrange
        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        var request = CreateOrderCommand(
            customer: string.Empty,
            totalAmount: 0,
            street: string.Empty,
            city: string.Empty,
            state: string.Empty,
            cep: string.Empty);

        // Act
        var response = await client.PostAsJsonAsync(
            OrdersEndpoint,
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var errors = await response.ReadValidationErrorsAsync(TestContext.Current.CancellationToken);
        errors.Count.ShouldBe(6);
        errors.ShouldContainKey("Customer");
        errors.ShouldContainKey("TotalAmount");
        errors.ShouldContainKey("Street");
        errors.ShouldContainKey("City");
        errors.ShouldContainKey("State");
        errors.ShouldContainKey("Cep");
    }

    [Fact]
    public async Task Dado_UsuarioSemRoleCustomer_Quando_CriarPedido_Entao_Retorna403()
    {
        // Arrange
        var client = await TestAuthHelper.CreateInvalidRoleClientAsync(_fixture);
        var request = CreateValidOrderCommand();

        // Act
        var response = await client.PostAsJsonAsync(
            OrdersEndpoint,
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var problem = await response.ReadProblemDetailsAsync(TestContext.Current.CancellationToken);
        problem.Status.ShouldBe(403);
        problem.Title.ShouldBe("Forbidden");
    }

    // ── GET /orders ─────────────────────────────
    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_ListarPedidos_Entao_Retorna200()
    {
        // Arrange
        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        // Act
        var response = await client.GetAsync(
            $"{OrdersEndpoint}?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.ReadContentAsync<PagedResponse<GetOrdersResponse>>(TestContext.Current.CancellationToken);
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Dado_UsuarioNaoAutenticado_Quando_ListarPedidos_Entao_Retorna401()
    {
        // Arrange
        var client = TestAuthHelper.CreateAnonymousClient(_fixture);

        // Act
        var response = await client.GetAsync(
            $"{OrdersEndpoint}?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var problem = await response.ReadProblemDetailsAsync(TestContext.Current.CancellationToken);
        problem.Status.ShouldBe(401);
        problem.Title.ShouldBe("Unauthorized");
    }

    [Fact]
    public async Task Dado_UsuarioSemRoleCustomer_Quando_ListarPedidos_Entao_Retorna403()
    {
        // Arrange
        var client = await TestAuthHelper.CreateInvalidRoleClientAsync(_fixture);

        // Act
        var response = await client.GetAsync(
            $"{OrdersEndpoint}?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var problem = await response.ReadProblemDetailsAsync(TestContext.Current.CancellationToken);
        problem.Status.ShouldBe(403);
        problem.Title.ShouldBe("Forbidden");
    }

    [Fact]
    public async Task Dado_PaginacaoInvalida_Quando_ListarPedidos_Entao_Retorna400()
    {
        // Arrange
        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        // Act
        var response = await client.GetAsync(
            $"{OrdersEndpoint}?page=0&pageSize=101",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var errors = await response.ReadValidationErrorsAsync(TestContext.Current.CancellationToken);
        errors.Count.ShouldBe(2);
        errors.ShouldContainKey("Page");
        errors.ShouldContainKey("PageSize");
    }

    [Fact]
    public async Task Dado_NenhumPedido_Quando_ListarPedidos_Entao_RetornaListaVazia()
    {
        // Arrange
        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        // Act
        var response = await client.GetAsync(
            $"{OrdersEndpoint}?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.ReadContentAsync<PagedResponse<GetOrdersResponse>>(TestContext.Current.CancellationToken);
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Dado_PedidosExistentes_Quando_ListarPedidos_Entao_RetornaPaginado()
    {
        // Arrange
        await _fixture.ExecuteDbContextAsync(async db =>
        {
            db.Orders.Add(CreateOrder(TestAuthHelper.CustomerEmail));
            db.Orders.Add(CreateOrder(TestAuthHelper.CustomerEmail));
            return await db.SaveChangesAsync();
        });

        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        // Act
        var response = await client.GetAsync(
            $"{OrdersEndpoint}?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.ReadContentAsync<PagedResponse<GetOrdersResponse>>(TestContext.Current.CancellationToken);
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBe(2);
        result.Items.Count().ShouldBe(2);
    }

    // ── GET /orders/{id} ─────────────────────────────

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_BuscaPedidoExistente_Entao_Retorna200()
    {
        // Arrange
        var order = CreateOrder(TestAuthHelper.CustomerEmail);

        await _fixture.ExecuteDbContextAsync(async db =>
        {
            db.Orders.Add(order);
            return await db.SaveChangesAsync();
        });

        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        // Act
        var response = await client.GetAsync(
            $"{OrdersEndpoint}/{order.Id}",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.ReadContentAsync<GetOrderByIdResponse>(TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(order.Id);
        result.Customer.ShouldBe(TestAuthHelper.CustomerEmail);
        result.TotalAmount.ShouldBe(150.00m);
        result.City.ShouldBe("Lages");
    }

    [Fact]
    public async Task Dado_PedidoInexistente_Quando_BuscaPorId_Entao_Retorna404()
    {
        // Arrange
        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        // Act
        var response = await client.GetAsync(
            $"{OrdersEndpoint}/{Guid.NewGuid()}",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problem = await response.ReadProblemDetailsAsync(TestContext.Current.CancellationToken);
        problem.Status.ShouldBe(404);
        problem.Title.ShouldBe("Not Found");
        problem.Detail!.Contains("Pedido não encontrado.");
    }

    [Fact]
    public async Task Dado_PedidoDeOutroCliente_Quando_BuscaPorId_Entao_Retorna404()
    {
        // Arrange
        var order = CreateOrder("outrocliente@teste.com");

        await _fixture.ExecuteDbContextAsync(async db =>
        {
            db.Orders.Add(order);
            return await db.SaveChangesAsync();
        });

        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        // Act
        var response = await client.GetAsync(
            $"{OrdersEndpoint}/{order.Id}",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problem = await response.ReadProblemDetailsAsync(TestContext.Current.CancellationToken);
        problem.Status.ShouldBe(404);
        problem.Title.ShouldBe("Not Found");
        problem.Detail!.Contains("Pedido não encontrado.");
    }

    // ── PUT /orders/{id} ─────────────────────────────

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_AtualizarEndereco_Entao_Retorna200()
    {
        // Arrange
        var order = CreateOrder(TestAuthHelper.CustomerEmail);

        await _fixture.ExecuteDbContextAsync(async db =>
        {
            db.Orders.Add(order);
            return await db.SaveChangesAsync();
        });

        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        var request = new UpdateDeliveryAddressCommand
        {
            Street = "Rua Nova",
            City = "Florianópolis",
            State = "SC",
            Cep = "88000000"
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"{OrdersEndpoint}/{order.Id}",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.ReadContentAsync<UpdateDeliveryAddressResponse>(TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(order.Id);
        result.Street.ShouldBe("Rua Nova");
        result.City.ShouldBe("Florianópolis");
        result.State.ShouldBe("SC");
        result.Cep.ShouldBe("88000000");
    }

    [Fact]
    public async Task Dado_UsuarioSemRoleCustomer_Quando_AtualizarEndereco_Entao_Retorna403()
    {
        // Arrange
        var client = await TestAuthHelper.CreateInvalidRoleClientAsync(_fixture);

        var request = new UpdateDeliveryAddressCommand
        {
            Street = "Rua Nova",
            City = "Florianópolis",
            State = "SC",
            Cep = "88000000"
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"{OrdersEndpoint}/{Guid.NewGuid()}",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var problem = await response.ReadProblemDetailsAsync(TestContext.Current.CancellationToken);
        problem.Status.ShouldBe(403);
        problem.Title.ShouldBe("Forbidden");
        problem.Detail.ShouldBe("Você não tem permissão para acessar este recurso.");
    }

    [Fact]
    public async Task Dado_RequestInvalido_Quando_AtualizarEndereco_Entao_Retorna400()
    {
        // Arrange
        var order = CreateOrder(TestAuthHelper.CustomerEmail);

        await _fixture.ExecuteDbContextAsync(async db =>
        {
            db.Orders.Add(order);
            return await db.SaveChangesAsync();
        });

        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        var request = new UpdateDeliveryAddressCommand
        {
            Street = string.Empty,
            City = string.Empty,
            State = string.Empty,
            Cep = string.Empty
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"{OrdersEndpoint}/{order.Id}",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var errors = await response.ReadValidationErrorsAsync(TestContext.Current.CancellationToken);
        errors.Count.ShouldBe(4);
        errors.ShouldContainKey("Street");
        errors.ShouldContainKey("City");
        errors.ShouldContainKey("State");
        errors.ShouldContainKey("Cep");
    }

    [Fact]
    public async Task Dado_PedidoInexistente_Quando_AtualizarEndereco_Entao_Retorna404()
    {
        // Arrange
        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        var request = new UpdateDeliveryAddressCommand
        {
            Street = "Rua Nova",
            City = "Florianópolis",
            State = "SC",
            Cep = "88000000"
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"{OrdersEndpoint}/{Guid.NewGuid()}",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problem = await response.ReadProblemDetailsAsync(TestContext.Current.CancellationToken);
        problem.Status.ShouldBe(404);
        problem.Title.ShouldBe("Not Found");
        problem.Detail!.Contains("Pedido não encontrado.");
    }

    [Fact]
    public async Task Dado_PedidoDeOutroUsuario_Quando_AtualizarEndereco_Entao_Retorna404()
    {
        // Arrange
        var order = CreateOrder("outro@usuario.com");

        await _fixture.ExecuteDbContextAsync(async db =>
        {
            db.Orders.Add(order);
            return await db.SaveChangesAsync();
        });

        var client = await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        var request = new UpdateDeliveryAddressCommand
        {
            Street = "Rua Nova",
            City = "Florianópolis",
            State = "SC",
            Cep = "88000000"
        };

        // Act
        var response = await client.PutAsJsonAsync(
            $"{OrdersEndpoint}/{order.Id}",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problem = await response.ReadProblemDetailsAsync(TestContext.Current.CancellationToken);
        problem.Status.ShouldBe(404);
        problem.Title.ShouldBe("Not Found");
        problem.Detail!.Contains("Pedido não encontrado.");
    }

    // ── PATCH /orders/{id}/force-status ─────────────────────────────
    [Fact]
    public async Task Dado_Admin_Quando_ForcarStatus_Entao_Retorna200()
    {
        // Arrange
        var order = CreateOrder(TestAuthHelper.CustomerEmail);

        await _fixture.ExecuteDbContextAsync(async db =>
        {
            db.Orders.Add(order);
            return await db.SaveChangesAsync();
        });

        var client = await TestAuthHelper.CreateAdminClientAsync(_fixture);

        var request = new ForceOrderStatusCommand
        {
            NewStatus = "Approved"
        };

        // Act
        var response = await client.PatchAsJsonAsync(
            $"{OrdersEndpoint}/{order.Id}/force-status",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.ReadContentAsync<ForceOrderStatusResponse>(TestContext.Current.CancellationToken);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(order.Id);
        result.Status.ShouldBe("Approved");
    }

    [Fact]
    public async Task Dado_StatusInvalido_Quando_ForcarStatus_Entao_Retorna400()
    {
        // Arrange
        var client = await TestAuthHelper.CreateAdminClientAsync(_fixture);

        var request = new ForceOrderStatusCommand
        {
            NewStatus = "StatusInexistente"
        };

        // Act
        var response = await client.PatchAsJsonAsync(
            $"{OrdersEndpoint}/{Guid.NewGuid()}/force-status",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var errors = await response.ReadValidationErrorsAsync(TestContext.Current.CancellationToken);
        errors.Count.ShouldBe(1);
        errors.ShouldContainKey("NewStatus");
    }

    [Fact]
    public async Task Dado_PedidoInexistente_Quando_ForcarStatus_Entao_Retorna404()
    {
        // Arrange
        var client = await TestAuthHelper.CreateAdminClientAsync(_fixture);

        var request = new ForceOrderStatusCommand
        {
            NewStatus = "Approved"
        };

        // Act
        var response = await client.PatchAsJsonAsync(
            $"{OrdersEndpoint}/{Guid.NewGuid()}/force-status",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problem = await response.ReadProblemDetailsAsync(TestContext.Current.CancellationToken);
        problem.Status.ShouldBe(404);
        problem.Title.ShouldBe("Not Found");
        problem.Detail!.Contains("Pedido não encontrado.");
    }

    private static CreateOrderCommand CreateValidOrderCommand()
        => CreateOrderCommand();

    private static CreateOrderCommand CreateOrderCommand(
        string customer = "Cliente Teste",
        decimal totalAmount = 150.00m,
        string street = "Rua Teste",
        string city = "Lages",
        string state = "SC",
        string cep = "88500000")
        => new(
            Customer: customer,
            TotalAmount: totalAmount,
            Street: street,
            City: city,
            State: state,
            Cep: cep);

    private static Order CreateOrder(
        string customer = "Cliente Teste",
        decimal totalAmount = 150.00m,
        string street = "Rua Teste",
        string city = "Lages",
        string state = "SC",
        string cep = "88500000")
        => new(
                customer,
                totalAmount,
                new Address(
                    street,
                    city,
                    state,
                    cep));
}
