﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

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
                    string response = "udp responde";

                    var bufferResp = Encoding.ASCII.GetBytes(response);

                    server.Send(bufferResp, bufferResp.Length, remoteEP);                    
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
                            
                                requestBody = package.Substring(indexHeader + 4, package.Length - 4 - indexHeader);
                                
                                if(requestFirstLine.Substring(0,33) == "INVITE sip:sip-alg-detector-ttec@")
                                {                                    
                                    Console.WriteLine($"{System.DateTime.Now.ToString("yyyyMMddhhmss")}  TCP DEBUG: {requestFirstLine} from {ipSender}:{portSender}");                                    
                                    //GenerateResponses( requestHeaders, requestBody, stream, ipSender, portSender);
                                    GenerateResponsesNoSIP(requestHeaders, requestBody, stream);
                                }
                                else
                                {
                                    Console.WriteLine($"{System.DateTime.Now.ToString("yyyyMMddhhmss")}  TCP DEBUG: Invalid Request-URI:  {requestFirstLine} from {ipSender}:{portSender}");
                                    GenerateErrorResponse(requestFirstLine, requestHeaders, stream, ipSender, portSender);
                                }
                            }
                            catch(Exception exp)
                            {
                                Console.WriteLine($"Not valid client  {exp.Message}");
                            }
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

        public static void GenerateErrorResponse(string requestFirstLine, string requestHeaders,
                                                 NetworkStream netStream, string ipSender, 
                                                 string portSender)
        {

            // Generate a 403 error response

            // IF an ACK ignore it.
            if (requestFirstLine.Contains("ACK")) return;
            
            string responseFirstLine = "SIP/2.0 403 You seem a real phone, get out\r\n";
            string responseHeaders = (string)requestHeaders.Clone();

            responseHeaders = "Server: SipAlgDetectorDaemon\r\n" + responseHeaders;
            responseHeaders = responseHeaders.Replace(";rport", $";received={ipSender};rport={portSender}");
            responseHeaders = responseHeaders.Insert(responseHeaders.IndexOf('>', responseHeaders.IndexOf("To:")) + 1,
                                                     $"; tag={Utils.RandomString(6, "0123456789")}");
            responseHeaders = responseHeaders.Replace("Content-Type: application/sdp", "");
            int indexResponseSize = responseHeaders.IndexOf("Content-Length: ");
            responseHeaders = responseHeaders.Remove(indexResponseSize, (responseHeaders.Length - indexResponseSize));
            
            string response = responseFirstLine + responseHeaders;

            var buffer = Encoding.ASCII.GetBytes(response);

            netStream.Write(buffer,0, buffer.Length);
        }

        public static void GenerateResponsesNoSIP(string requestHeaders, string requestBody,
                                            NetworkStream netStream)
        {
            string responsHeaders = Utils.Base64Encode(requestHeaders);
            string responsBody = Utils.Base64Encode(requestBody);
            string response = $"Header:\r\n{responsHeaders}\r\n Body:\r\n{responsBody}";
            Console.WriteLine(response);
            var buffer = Encoding.ASCII.GetBytes(response);
            netStream.Write(buffer, 0, buffer.Length);
        }

        public static void GenerateResponses(string requestHeaders, string requestBody, 
                                            NetworkStream netStream, string ipSender, 
                                            string portSender)
        {
            /// Generate a 180 reply containing the mirrored request first line and headers.
            string responseFirstLine = "SIP/2.0 180 Body contains mirrored request first line and headers\r\n";            
            string responsBody = Utils.Base64Encode(requestHeaders);
            string responseHeaders = (string) requestHeaders.Clone();

            responseHeaders = "Server: SipAlgDetectorDaemon\r\n" + responseHeaders;
            responseHeaders = responseHeaders.Replace(";rport", $";received={ipSender};rport={portSender}");
            responseHeaders = responseHeaders.Insert(responseHeaders.IndexOf('>', responseHeaders.IndexOf("To:"))+1,
                                                     $"; tag={Utils.RandomString(6, "0123456789")}");
            responseHeaders = responseHeaders.Replace("application/sdp", "text/plain");
            int indexResponseSize = responseHeaders.IndexOf("Content-Length: ") + 16;

            responseHeaders = responseHeaders.Remove(indexResponseSize, (responseHeaders.Length - indexResponseSize));
            responseHeaders = responseHeaders.Insert(indexResponseSize, $"{responsBody.Length}\r\n");
            string responseFH = responseFirstLine + responseHeaders + responsBody + "\r\n";

            var buffer = Encoding.ASCII.GetBytes(responseFH);
            netStream.Write(buffer, 0, buffer.Length);

            //Console.WriteLine($"\r\n\r\nSend bytes:\r\n{numSend}"); 

            /// Generate a 500 reply containing the mirrored request body.
            responseFirstLine = "SIP/2.0 500 Body contains mirrored request body\r\n";            
            responsBody = Utils.Base64Encode(requestBody);
            responseHeaders = (string)requestHeaders.Clone();
            responseHeaders = "Server: SipAlgDetectorDaemon\r\n" + responseHeaders;
            responseHeaders = responseHeaders.Replace(";rport", $";received={ipSender};rport={portSender}");
            responseHeaders = responseHeaders.Insert(responseHeaders.IndexOf('>', responseHeaders.IndexOf("To:")) + 1,
                                                   $"; tag={Utils.RandomString(6, "0123456789")}");            
            responseHeaders = responseHeaders.Replace("application/sdp", "text/plain");
            indexResponseSize = responseHeaders.IndexOf("Content-Length: ") + 16;

            responseHeaders = responseHeaders.Remove(indexResponseSize, (responseHeaders.Length - indexResponseSize));
            responseHeaders = responseHeaders.Insert(indexResponseSize, $"{responsBody.Length}\r\n");            
            string response = responseFirstLine + responseHeaders + responsBody;
          
            buffer = Encoding.ASCII.GetBytes(response);
            netStream.Write(buffer, 0, buffer.Length);
        }

       
    }
}
