using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils.IPC
{
    /// <summary>
    /// Demonstrates a working PipeServer and client system
    /// </summary>
    internal static class PipeDemo
    {
        private static NamedPipeServerStream demoServer;

        public static void Run()
        {
            Task.Run(sendByteAndReceiveResponseContinuous);
            Task.Run(receiveByteAndRespondContinuous);
        }

        private static void sendByteAndReceiveResponseContinuous()
        {
            demoServer = new NamedPipeServerStream("test-pipe");

            string[] testValues = ["test", "yes", "x"];

            var testEnumerator = testValues.GetEnumerator();

            UtilityLogger.Log("Server waiting for a connection...");

            bool connected = false;
            while (!connected)
            {
                try
                {
                    demoServer.WaitForConnection();
                    connected = true;
                }
                catch (Exception ex)
                {
                    UtilityLogger.LogError("Server error", ex);
                }
            }
            UtilityLogger.Log("A client has connected, send a byte from the server: ");

            testEnumerator.MoveNext();
            string b = (string)testEnumerator.Current;

            UtilityLogger.Log(string.Format("About to send byte {0} to client.", b));

            demoServer.WriteByte(Encoding.UTF8.GetBytes(b).First());

            UtilityLogger.Log("Byte sent, waiting for response from client...");
            int byteFromClient = demoServer.ReadByte();

            UtilityLogger.Log(string.Format("Received byte response from client: {0}", byteFromClient));
            while (byteFromClient != 120)
            {
                UtilityLogger.Log("Send a byte response: ");

                testEnumerator.MoveNext();
                b = (string)testEnumerator.Current;

                UtilityLogger.Log(string.Format("About to send byte {0} to client.", b));
                demoServer.WriteByte(Encoding.UTF8.GetBytes(b).First());

                UtilityLogger.Log("Byte sent, waiting for response from client...");
                byteFromClient = demoServer.ReadByte();

                UtilityLogger.Log(string.Format("Received byte response from client: {0}", byteFromClient));
            }
            UtilityLogger.Log("Server exiting, client sent an 'x'...");
        }

        private static void receiveByteAndRespondContinuous()
        {
            string[] testValues = ["no", "fear", "x"];

            var testEnumerator = testValues.GetEnumerator();

            using (NamedPipeClientStream demoClient = new NamedPipeClientStream("test-pipe"))
            {
                bool connected = false;
                while (!connected)
                {
                    try
                    {
                        demoClient.Connect();
                        connected = true;
                    }
                    catch (Exception ex)
                    {
                        UtilityLogger.LogError("Client error", ex);
                    }
                }

                UtilityLogger.Log("Client connected to the named pipe server. Waiting for server to send first byte...");
                UtilityLogger.Log(string.Format("The server sent a single byte to the client: {0}", demoClient.ReadByte()));
                UtilityLogger.Log("Provide a byte response from client: ");

                testEnumerator.MoveNext();
                string b = (string)testEnumerator.Current;

                UtilityLogger.Log(string.Format("About to send byte {0} to server.", b));
                demoClient.WriteByte(Encoding.UTF8.GetBytes(b).First());

                while (b != "x")
                {
                    UtilityLogger.Log(string.Format("The server sent a single byte to the client: {0}", demoClient.ReadByte()));
                    UtilityLogger.Log("Provide a byte response from client: ");

                    testEnumerator.MoveNext();
                    b = (string)testEnumerator.Current;

                    UtilityLogger.Log(string.Format("About to send byte {0} to server.", b));
                    demoClient.WriteByte(Encoding.UTF8.GetBytes(b).First());
                }

                UtilityLogger.Log("Client chose to disconnect...");
            }
        }
    }
}
