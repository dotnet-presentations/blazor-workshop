// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Client.Pages;

[Authorize]
[Route("/orders/{orderId:int}")]
public sealed partial class OrderDetails : IDisposable
{
    [Inject] public required OrdersClient Client { get; set; }

    [Parameter] public int OrderId { get; set; }

    private OrderWithStatus? _orderWithStatus;
    private bool _invalidOrder;
    private CancellationTokenSource? _pollingCancellationToken;

    protected override void OnParametersSet()
    {
        // If we were already polling for a different order, stop doing so
        _pollingCancellationToken?.Cancel();

        // Start a new poll loop
        _ = PollForUpdatesAsync();
    }

    private async Task PollForUpdatesAsync()
    {
        _invalidOrder = false;
        _pollingCancellationToken = new CancellationTokenSource();

        while (!_pollingCancellationToken.IsCancellationRequested)
        {
            try
            {
                _orderWithStatus = await Client.GetOrderAsync(OrderId);

                StateHasChanged();

                if (_orderWithStatus.IsDelivered)
                {
                    _pollingCancellationToken.Cancel();
                }
                else
                {
                    await Task.Delay(4_000);
                }
            }
            catch (AccessTokenNotAvailableException ex)
            {
                _pollingCancellationToken.Cancel();

                ex.Redirect();
            }
            catch (Exception ex)
            {
                _invalidOrder = true;

                _pollingCancellationToken.Cancel();

                Console.Error.WriteLine(ex);

                StateHasChanged();
            }
        }
    }

    void IDisposable.Dispose() => _pollingCancellationToken?.Cancel();
}

