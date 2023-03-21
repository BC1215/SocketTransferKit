using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketTransferKit.Data;

namespace SocketTransferKit.Client
{
    public delegate void ServerConnectedEventHandler(SocketClient client,ICommand serverWelcome);

    public delegate void ServerConnectionLostEventHandler(SocketClient client);

    public delegate void OnCommandArrivedEventHandler(SocketClient client, ICommand command);

    public delegate void OnDelayUpdateEventHandler(SocketClient client, int delay);
}
