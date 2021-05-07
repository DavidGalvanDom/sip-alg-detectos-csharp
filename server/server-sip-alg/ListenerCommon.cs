using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace server_sip_alg
{
    public class ListenerCommon
    {
        public static  void UDPServer(object arg)
        {
            Console.WriteLine("UDPServer thread started");

            try
            {
                UdpClient server = (UdpClient)arg;
                IPEndPoint remoteEP;
                byte[] buffer;

                while (true)
                {
                    remoteEP = null;
                    buffer = server.Receive(ref remoteEP);

                    if (buffer != null && buffer.Length > 0)
                    {
                        Console.WriteLine("UDP: " + Encoding.ASCII.GetString(buffer));
                    }
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10004) // unexpected
                    Console.WriteLine("UDPServerProc exception: " + ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UDPServerProc exception: " + ex);
            }

            Console.WriteLine("UDP server thread finished");
        }


        public static void TCPServer(object arg)
        {
            Console.WriteLine("TCP server thread started");
            
            string ipSender;
            string portSender;
            string requestFirstLine;
            string requestHeaders;
            string requestBody;
            //https://github.com/ibc/sip-alg-detector/blob/06628e49e334d4d2b4c6d95d6c6a1b529869fd2a/server/sip-alg-detector-daemon.rb
            try
            {
                TcpListener server = (TcpListener)arg;
                byte[] buffer = new byte[2048];
                int count;

                server.Start();

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();

                    using (var stream = client.GetStream())
                    {

                        while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            
                            try
                            {
                                var arrEndPoint = client.Client.RemoteEndPoint.ToString().Split(':');

                                string package = Encoding.ASCII.GetString(buffer, 0, count);
                                int indexHeader = package.IndexOf("\r\n\r\n");

                                ipSender = arrEndPoint[0];
                                portSender = arrEndPoint[1];

                                requestFirstLine = package.Substring(0, package.IndexOf("\r\n"));
                                requestHeaders = package.Substring(0, indexHeader);
                            
                                //int contentLength = Convert.ToInt32(package.Substring(indexHeader + 16, 3));

                                requestBody = package.Substring(indexHeader+8, package.Length - indexHeader);
                                Console.WriteLine(requestHeaders);


                                if(requestFirstLine.Substring(0,33) == "INVITE sip:sip-alg-detector-ttec@")
                                {                                    
                                    Console.WriteLine($"{System.DateTime.Now.ToString("yyyyMMddhhmss")}  TCP DEBUG: {requestFirstLine} from {ipSender}:{portSender}");
                                    GenerateResponses(requestFirstLine, requestHeaders, requestBody, client, ipSender, portSender);
                                }

                                Console.WriteLine(package);

                            }
                            catch(Exception exp)
                            {
                                Console.WriteLine($"Not valid client  {exp.Message}");
                            }

                            //if request_first_line = ~ / ^INVITE sip: sip - alg - detector - daemon@/
                            //             puts "#{log_time} TCP DEBUG: '#{log_ruri(request_first_line)}' from #{sender_ip}:#{sender_port}"

                            //    generate_responses(request_first_line, request_headers, request_body, io, sender_ip, sender_port)

                            //else
                            //        puts "#{log_time} TCP DEBUG: Invalid Request-URI: '#{log_ruri(request_first_line)}' from #{sender_ip}:#{sender_port}"

                            //    generate_error_response(request_first_line, request_headers, io, sender_ip, sender_port)
                            //    close_connection(io)
                            //end
                            //        request_first_line = io.readline("\r\n")


                       
                        }
                    }


                    client.Close();
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10004) // unexpected
                    Console.WriteLine("TCPServerProc exception: " + ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("TCPServerProc exception: " + ex);
            }

            Console.WriteLine("TCP server thread finished");
        }

        public static void GenerateResponses(string requestFirstLine, string requestHeaders, 
                                        string requestBody, TcpClient client, 
                                        string ipSender, string portSender)
        {

        }
    }
}
