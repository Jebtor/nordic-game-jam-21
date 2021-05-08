using CommandTerminal;
using MLAPI;
using MLAPI.Transports.UNET;

public static class TerminalCommands
{
    static void FrontCommandConnect(CommandArg[] args)
    {
        var address = args.Length > 0 ? args[0].String : "127.0.0.1";
        int port = args.Length > 1 ? args[1].Int : 7777;

        if (Terminal.IssuedError)
            return;

        CommandConnect(address, port);
    }

    [RegisterCommand(Help = "Connect to server", MinArgCount = 0, MaxArgCount = 2)]
    static void CommandConnect(string address = "127.0.0.1", int port = 7777)
    {
        var connectionSettings = NetworkManager.Singleton.GetComponent<UNetTransport>();
        connectionSettings.ConnectAddress = address;
        connectionSettings.ConnectPort = port;

        NetworkManager.Singleton.StartClient();
    }

    static void FrontCommandDisconnect(CommandArg[] args)
    {
        if (Terminal.IssuedError)
            return;

        CommandDisconnect();
    }

    [RegisterCommand(Help = "Disconnect from server", MaxArgCount = 0)]
    static void CommandDisconnect()
    {
        if (NetworkManager.Singleton.IsHost)
            NetworkManager.Singleton.StopHost();
        else if(NetworkManager.Singleton.IsClient)
            NetworkManager.Singleton.StopClient();
    }

    static void FrontCommandHost(CommandArg[] args)
    {
        if (Terminal.IssuedError)
            return;

        CommandHost();
    }

    [RegisterCommand(Help ="Start hosting", MaxArgCount = 0)]
    static void CommandHost()
    {
        NetworkManager.Singleton.StartHost();
    }
}
