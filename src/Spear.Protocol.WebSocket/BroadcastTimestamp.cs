using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Spear.Protocol.WebSocket
{
    internal class BroadcastTimestamp : IHostedService, IDisposable
    {
        private Timer _timer;

        public BroadcastTimestamp()
        { }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var interval = TimeSpan.FromSeconds(15);
            _timer = new Timer(QueueBroadcast, null, TimeSpan.Zero, interval);
            return Task.CompletedTask;
        }

        private void QueueBroadcast(object state)
        {
            WebSocketMiddleware.Broadcast($"Server time: {DateTimeOffset.Now:o}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
