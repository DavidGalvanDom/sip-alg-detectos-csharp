using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Serilog;
using Serilog.Core;

namespace client_sip_alg
{
    class Program
    {
        static void Main(string[] args)
        {           
            ConsoleKeyInfo key;
            bool run = true;

            Logger log = new LoggerConfiguration()
                       .WriteTo.Console()
                       .WriteTo.File("Sip-alg-log.txt")
                       .CreateLogger();

            log.Information(string.Format("Starting TCP and UDP clients on port {0}...", General.PORT_NUMBER));

            try
            {            
                while (run)
                {
                    log.Information("Press 'T' for TCP sending, 'U' for UDP sending or 'X' to exit.");
                    key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.X:
                            run = false;
                            break;

                        case ConsoleKey.U:
                            CreateUDPRequest();                            
                            break;

                        case ConsoleKey.T:
                            CreateTCPRequest();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex,"Main exception: ");
            }


            log.Information("Press <ENTER> to exit.");
            Console.ReadLine();
        }

        public static void CreateUDPRequest()
        {
            byte[] buffer;
            
            UdpClient udpClient = null;
            
            try
            {
                IPEndPoint ep1 = new IPEndPoint(IPAddress.Parse(General.SERVER), General.PORT_NUMBER);
                udpClient = new UdpClient();
                //udpClient.Connect(General.SERVER, General.PORT_NUMBER);

                buffer = Encoding.ASCII.GetBytes(DateTime.Now.ToString("HH:mm:ss.fff"));
                udpClient.Send(buffer, buffer.Length, ep1);
                var dataResponse = udpClient.Receive(ref ep1);

                Console.WriteLine(System.Text.Encoding.ASCII.GetString(dataResponse, 0, dataResponse.Length));
            }
            finally
            {
                if (udpClient != null)
                    udpClient.Close();
            }

        }


        public static void CreateTCPRequest()
        {
            TcpClient tcpClient = null;
            NetworkStream tcpStream = null;
            StringBuilder responseData;

            string mirrorRequest = "";
            int minSize = 1850;
            int sizeBytes = 250;
            int offset = 0;
            Int32 bytes;
            byte[] buffer;
            byte[] dataResponse;

            try
            {
                string request = GetStringRequest(IPAddress.Loopback.ToString(),
                                                    General.PORT_NUMBER.ToString(),
                                                    General.PORT_NUMBER.ToString(),
                                                    General.SERVER, 
                                                    "tcp");

                //Console.WriteLine("Cliente request");
                Console.WriteLine(request);
                
                buffer = Encoding.ASCII.GetBytes(request);

                if (tcpClient == null)
                {
                    tcpClient = new TcpClient();
                    
                    tcpClient.Connect(General.SERVER, General.PORT_NUMBER);
                    tcpStream = tcpClient.GetStream();
                }

                tcpStream.Write(buffer, 0, buffer.Length);

                //Console.WriteLine("Information Sent.");

                dataResponse = new Byte[sizeBytes];
                responseData = new StringBuilder();

                //Console.WriteLine("Waiting for response.");
                // Read the first batch of the TcpServer response bytes.

                // Set a 10 millisecond timeout for reading.
                tcpStream.ReadTimeout = 10000;

                bytes = tcpStream.Read(dataResponse, 0, dataResponse.Length);
               // Thread.Sleep(1000);
                offset += bytes;

                while (bytes > 0)
                {
                    responseData.Append(System.Text.Encoding.ASCII.GetString(dataResponse, 0, bytes));

                    // bytes = tcpStream.Read(dataResponse, offset, dataResponse.Length - offset);
                    //Console.WriteLine($"Exec {bytes}");
                    //Console.WriteLine($"offset {offset}");
            
                    if (bytes < sizeBytes && 
                        offset > minSize)
                    {
                        bytes = 0;
                        tcpClient.Close();
                        tcpClient.Dispose();
                        tcpClient = null;
                        //Console.WriteLine("Response Server:");
                        //Console.WriteLine(responseData.ToString());
                        mirrorRequest = GetMirrorRequest(responseData.ToString());
                        responseData.Clear();
                    }
                    else
                    {
                        bytes = tcpStream.Read(dataResponse, 0, dataResponse.Length);
                    }

                    offset += bytes;
                }

                //Validate an error
                if(responseData.Length > 0) {
                    throw new ApplicationException(responseData.ToString());
                }

                bool tcpAlgTest = false;
                if (mirrorRequest.Trim() != string.Empty) {
                    tcpAlgTest = CompareRequestAndMirror(request, mirrorRequest);
                } else
                {
                    Console.WriteLine("\n__________________________________________________________________");
                    Console.WriteLine("         ERROR:      Server no data response");
                    Console.WriteLine("\n__________________________________________________________________");
                    return;
                }

                Console.WriteLine("\n__________________________________________________________________");
                Console.WriteLine($"INFO: SIP TCP ALG test result: {tcpAlgTest}");
                if (tcpAlgTest)
                    Console.WriteLine("INFO: It seems that your router is performing ALG for SIP TCP");
                else
                    Console.WriteLine("INFO: It seems that your router is not performing ALG for SIP TCP");
                Console.WriteLine("__________________________________________________________________");
            }
            catch (ArgumentNullException anex)
            {
                Console.WriteLine($"ERROR: CreateTCPRequest ArgumentNullException : {anex}");
            }
            catch (SocketException soex)
            {
                Console.WriteLine($"ERROR: CreateTCPRequest SocketException : {soex}");
            }
            catch (Exception exp)
            {
                Console.WriteLine("\n_____________________________ERROR_____________________________________");
                Console.WriteLine($" {Environment.NewLine}{exp.Message}");
                Console.WriteLine("\n__________________________________________________________________");
            }
            finally
            {
                if (tcpStream != null)
                    tcpStream.Close();

                if (tcpClient != null)
                    tcpClient.Close();
            }        
        }

        public static string GetStringRequest (string ipLocal,string portLocal, 
                                         string portServer,string ipServer, 
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
            
            string headerName = "ttec";            
            if (ipServer == "199.180.223.109")
                headerName = "daemon";

            string headers = $"INVITE sip:sip-alg-detector-{headerName}@{ipServer}:{portServer} SIP/2.0\r\n" +
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


        public static string GetMirrorRequestNoSIP(string mirrorRequest)
        {
            string[] stringNewLine = new string[] { "\r\n" };
            string origenMirrorRequest;
            var arrMirrorRequest = mirrorRequest.Split(stringNewLine, StringSplitOptions.None);

            //// 100: Request first line and headers
            //string firstLine = arrMirrorRequest[1];
            //if (!firstLine.Contains("Server: SipAlgDetectorDaemon"))
            //{
            //    throw new ApplicationException("The server is not a SIP-ALG-Detector daemon");
            //}

            string mirrorRequestFirstLineHeaders = Utils.Base64Decode(arrMirrorRequest[1]);
            string mirrorRequestBody = Utils.Base64Decode(arrMirrorRequest[3]);

            origenMirrorRequest = mirrorRequestFirstLineHeaders + "\r\n\r\n" + mirrorRequestBody;

            return origenMirrorRequest;
        }

        public static string GetMirrorRequest (string mirrorRequest)
        {
            string[] stringNewLine = new string[] { "\r\n" };
            string origenMirrorRequest;
            var arrMirrorRequest = mirrorRequest.Split(stringNewLine, StringSplitOptions.None);

            // 100: Request first line and headers
            string firstLine = arrMirrorRequest[1];
            if(!firstLine.Contains("Server: SipAlgDetectorDaemon"))
            {
                throw new ApplicationException("The server is not a SIP-ALG-Detector daemon");
            }

            var newArr = arrMirrorRequest[13].Split('\n');
            var mirrorHead = newArr.Take(newArr.Length-1);

            string mirrorRequestFirstLineHeaders = Utils.Base64Decode(string.Join("", mirrorHead));
            string mirrorRequestBody = Utils.Base64Decode(arrMirrorRequest[arrMirrorRequest.Length - 1]);

            origenMirrorRequest = mirrorRequestFirstLineHeaders +  mirrorRequestBody;

            return origenMirrorRequest;
        }

        public static bool CompareRequestAndMirror(string request, string mirrorRequest)
        {
            // Some stuff to make Diff working.
            string[] stringNewLine = new string[] { "\r\n" };
            List<string> lstDiff = new List<string>();
            var arrRequest = request.Split(stringNewLine, StringSplitOptions.None);

            var arrMirrorRequest = mirrorRequest.Split(stringNewLine, StringSplitOptions.None);

            for(int count = 0; count < arrRequest.Length; count++)
            {
                if( arrRequest[count] != arrMirrorRequest[count])                
                    lstDiff.Add(arrRequest[count] + " -- " + arrMirrorRequest[count]);                
            }

            if(lstDiff.Count > 0)
            {
                Console.WriteLine("\nINFO: There are differences between sent request and received mirrored request:");
                Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                foreach (var diff in lstDiff)     Console.WriteLine(diff);
                Console.WriteLine("----------------------------------------------------------");
                return true;
            }

            Console.WriteLine("\nINFO: No differences between sent request and received mirrored request");
            return false;
            
        }
        
    }
}
