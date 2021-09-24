using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading;
using System.Security.Principal;

namespace TwitchVMCAuthClient
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AuthClient authClient = new AuthClient();
            MainAsync(args, authClient);
            Application.Run(authClient);
        }
        private static async Task MainAsync(string[] args, AuthClient authClient)
        {
            using (var pipeClient =
                new NamedPipeClientStream(".", "testpipe",
                        PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                try
                {
                    await pipeClient.ConnectAsync(1000000);
                    var ss = new StreamString(pipeClient);
                    await ss.WriteString("SYNC");
                    await authClient.signal.WaitAsync();
                    await ss.WriteString(authClient.authOut);
                }
                catch (IOException e)
                {
                    MessageBox.Show(e.Message, "Pipe Error: {0}");
                }
            }
            authClient.Close();
        }
    }
}
public class StreamString
{
    private Stream ioStream;
    private UnicodeEncoding streamEncoding;

    public StreamString(Stream ioStream)
    {
        this.ioStream = ioStream;
        streamEncoding = new UnicodeEncoding();
    }

    public async Task<string> ReadString()
    {
        int len = 0;

        len = ioStream.ReadByte() * 256;
        len += ioStream.ReadByte();
        byte[] inBuffer = new byte[len];
        await ioStream.ReadAsync(inBuffer, 0, len);

        return streamEncoding.GetString(inBuffer);
    }

    public async Task<int> WriteString(string outString)
    {
        byte[] outBuffer = streamEncoding.GetBytes(outString);
        int len = outBuffer.Length;
        if (len > UInt16.MaxValue)
        {
            len = (int)UInt16.MaxValue;
        }
        ioStream.WriteByte((byte)(len / 256));
        ioStream.WriteByte((byte)(len & 255));
        await ioStream.WriteAsync(outBuffer, 0, len);
        await ioStream.FlushAsync();

        return outBuffer.Length + 2;
    }
}