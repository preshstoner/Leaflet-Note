namespace Leaflet;

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

public static class SingleInstanceManager
{
    private static Mutex? _mutex;
    private const string MutexName = "Leaflet_Editor_SingleInstance_Mutex"; 
    private const string PipeName = "Leaflet_Editor_NamedPipe"; 

    public static bool Initialize(string[] args, Action<string[]> onArgsReceived)
    {
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            // Another instance is already running; send arguments to it and exit

            SendArgsToPrimaryInstance(args);
            return false; 
        }

        // Start background server to listen for subsequent launch arguments
        Task.Run(() => ListenForArguments(onArgsReceived));
        return true;
    }

    private static void SendArgsToPrimaryInstance(string[] args)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(1000); // 1-second timeout
            using var writer = new StreamWriter(client);
            writer.WriteLine(string.Join("|", args));
        }
        catch
        {
            // Fallback if pipe fails
        }
    }

    private static async Task ListenForArguments(Action<string[]> onArgsReceived)
    {
        while (true)
        {
            try
            {
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.In);
                await server.WaitForConnectionAsync();
                using var reader = new StreamReader(server);
                string? message = await reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(message))
                {
                    var args = message.Split('|');
                    Dispatcher.UIThread.Post(() => onArgsReceived(args));
                }
            }
            catch
            {
                // Prevent background loop crash on pipe errors
                await Task.Delay(500);
            }
        }
    }
}