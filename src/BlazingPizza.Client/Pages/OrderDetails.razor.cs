using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BlazingPizza.Client.Pages
{
    public sealed partial class OrderDetails : IAsyncDisposable
    {
        private HubConnection _hubConnection;
        private OrderWithStatus _orderWithStatus;
        private bool _invalidOrder;

        [Parameter] public int OrderId { get; set; }

        [Inject] public NavigationManager Nav { get; set; }

        [Inject] public OrdersClient OrdersClient { get; set; }

        [Inject] public IAccessTokenProvider AccessTokenProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(
                    Nav.ToAbsoluteUri("/orderstatus"),
                    options => options.AccessTokenProvider = GetAccessTokenValueAsync)
                .WithAutomaticReconnect()
                .AddMessagePackProtocol()
                .Build();

            _hubConnection.On<OrderWithStatus>(
                OrderStatusHubConsts.EventNames.OrderStatusChanged, OnOrderStatusChangedAsync);

            await _hubConnection.StartAsync();
        }

        private async Task<string> GetAccessTokenValueAsync()
        {
            var result = await AccessTokenProvider.RequestAccessToken();
            return result.TryGetToken(out var accessToken)
                ? accessToken.Value
                : null;
        }

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                _orderWithStatus = await OrdersClient.GetOrder(OrderId);

                StateHasChanged();

                if (_orderWithStatus.IsDelivered)
                {
                    await _hubConnection.InvokeAsync(
                        OrderStatusHubConsts.MethodNames.StopTrackingOrder, _orderWithStatus.Order);
                    await _hubConnection.StopAsync();
                }
                else
                {
                    await _hubConnection.InvokeAsync(
                        OrderStatusHubConsts.MethodNames.StartTrackingOrder, _orderWithStatus.Order);
                }
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
            }
            catch (Exception ex)
            {
                _invalidOrder = true;
                Console.Error.WriteLine(ex);
                StateHasChanged();
            }
        }

        private Task OnOrderStatusChangedAsync(OrderWithStatus orderWithStatus) =>
            InvokeAsync(() =>
            {
                _orderWithStatus = orderWithStatus;
                StateHasChanged();
            });

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                if (_orderWithStatus is not null)
                {
                    await _hubConnection.InvokeAsync(
                        OrderStatusHubConsts.MethodNames.StopTrackingOrder, _orderWithStatus.Order);
                }

                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }
}
