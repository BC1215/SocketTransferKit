using SocketTransferKit.Data;
using SuperSocket.SocketBase;

namespace SocketTransferKit.Server
{
    public delegate void OnCommandArrivedEventHandler(SocketSession clientSession, ICommand command);
}
