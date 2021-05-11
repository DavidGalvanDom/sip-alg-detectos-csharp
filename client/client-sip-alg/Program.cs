using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace client_sip_alg
{
    class Program
    {
        static void Main(string[] args)
        {
            //StartClient();

            UdpClient udpClient = null;
            TcpClient tcpClient = null;
            NetworkStream tcpStream = null;
            int port = 5060;
            ConsoleKeyInfo key;
            bool run = true;
            byte[] buffer;

            Console.WriteLine(string.Format("Starting TCP and UDP clients on port {0}...", port));

            try
            {
                udpClient = new UdpClient();
                udpClient.Connect(IPAddress.Loopback, port);

                tcpClient = new TcpClient();
                tcpClient.Connect(IPAddress.Loopback, port);

                while (run)
                {
                    Console.WriteLine("Press 'T' for TCP sending, 'U' for UDP sending or 'X' to exit.");
                    key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.X:
                            run = false;
                            break;

                        case ConsoleKey.U:
                            buffer = Encoding.ASCII.GetBytes(DateTime.Now.ToString("HH:mm:ss.fff"));
                            udpClient.Send(buffer, buffer.Length);
                            break;

                        case ConsoleKey.T:

                            //buffer = Encoding.ASCII.GetBytes(DateTime.Now.ToString("HH:mm:ss.fff"));
                            buffer = Encoding.ASCII.GetBytes(getRequest(IPAddress.Loopback.ToString(),port.ToString(), port.ToString(), IPAddress.Loopback.ToString(), "tcp"));
                            
                            if (tcpStream == null)
                                tcpStream = tcpClient.GetStream();

                            
                            tcpStream.Write(buffer, 0, buffer.Length);

                            var data = new Byte[256];

                            // String to store the response ASCII representation.
                            String responseData = String.Empty;

                            // Read the first batch of the TcpServer response bytes.
                            Int32 bytes = tcpStream.Read(data, 0, data.Length);


                            while (bytes > 0) {

                                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                                Console.WriteLine("Received: {0}", responseData);

                                if (bytes < 256)
                                    bytes = 0;
                                else
                                    bytes = tcpStream.Read(data, 0, data.Length);
                            }

                            

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Main exception: " + ex);
            }
            finally
            {
                if (udpClient != null)
                    udpClient.Close();

                if (tcpStream != null)
                    tcpStream.Close();

                if (tcpClient != null)
                    tcpClient.Close();
            }

            Console.WriteLine("Press <ENTER> to exit.");
            Console.ReadLine();
        }


        public static string getRequest (string ipLocal,string portLocal, 
                                         string portServer,string @ipServer, 
                                         string transport)
        {            
            string body = $"v={Utils.RandomString(1, "0123456789")}\r\n" +
                   $"o={Utils.RandomString(8)} {Utils.RandomString(8, "0123456789")} {Utils.RandomString(7, "0123456789")} IN IP4 {ipLocal}\r\n" +
                   $"s=-\r\n" +
                   $"c=IN IP4 {ipLocal}\r\n" +
                   $"t=0 0\r\n" +
                   $"m=audio {Utils.RandomString(4, "123456789")} RTP/AVP 8 0 3 101\r\n" +
                   $"a=rtpmap:8 PCMA/8000\r\n" +
                   $"a=rtpmap:0 PCMU/8000\r\n" +
                   $"a=rtpmap:3 GSM/8000\r\n" +
                   $"a=rtpmap:101 telephone-event/8000\r\n" +
                   $"a=fmtp:101 0-15\r\n" +
                   $"a=ptime:20\r\n";

            body = Regex.Replace(body, @"\s|\t", "");

            string headers = $"INVITE sip:sip-alg-detector-ttec@{ipServer}:{portServer} SIP/2.0\r\n" +
                    $"Via: SIP/2.0/{transport.ToUpper()} {ipLocal}:{portLocal};rport;branch={Utils.GenerateBranch()}\r\n" +
                    $"Max-Forwards: 5\r\n" +
                    $"To: <sip:sip-alg-detector-ttec@{ipServer}:{portServer}>\r\n" +
                    $"From: \"SIP ALG Detector\" <sip:sip-alg-detector-ttec@killing-alg-routers.war>;tag={Utils.GenerateTag()}\r\n" +
                    $"Call-ID: {Utils.GenerateCallid()}@{ipLocal}\r\n" +
                    $"CSeq: {Utils.GenerateCseq()} INVITE\r\n" +
                    $"Contact: <sip:0123@{ipLocal}:{portLocal};transport={transport}>\r\n" +
                    $"Allow: INVITE\r\n" +
                    $"Content-Type: application/sdp\r\n" +
                    $"Content-Length: {body.Length}\r\n";

            headers = Regex.Replace(headers, @"/^[\s\t]*/", "");

            return headers + "\r\n" + body;
        }

        public static void StartClient()
        {
            byte[] bytes = new byte[1024];

            try
            {
                // Connect to a Remote server  
                // Get Host IP Address that is used to establish a connection  
                // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
                // If a host has multiple addresses, you will get a list of addresses  
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 5060);

                // Create a TCP/IP  socket.    
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Udp);

                // Connect the socket to the remote endpoint. Catch any errors.    
                try
                {
                    // Connect to Remote EndPoint  
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.    
                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                    // Send the data through the socket.    
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.    
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // Release the socket.    
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
