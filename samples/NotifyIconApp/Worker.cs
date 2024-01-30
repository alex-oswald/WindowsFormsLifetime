using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Hosting;

namespace NotifyIconApp;

class Worker : BackgroundService
{
    readonly IMessenger _messenger;

    public Worker(IMessenger messenger)
    {
        _messenger = messenger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _messenger.Send(new ShowBalloonTipMessage(
                "Worker",
                "Worker running at: " + DateTimeOffset.Now));

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
