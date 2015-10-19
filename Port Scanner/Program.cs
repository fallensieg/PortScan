using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Port_Scanner
{
    class Program
    {
        static void Main(string[] args)
        {
            String fileName         = null;
            String scanAddress      = null;
            IPAddress ipAddress     = null;
            DateTime startTime;
            DateTime endTime;

            try
            {

                //Check to see if a value was parsed into the application
                if (args.Length != 0)
                {
                    Console.WriteLine("Setting IP to be scanned to : " + args[0]);
                    scanAddress = args[0];
                }
                else
                {
                    Console.WriteLine("No value parsed into the app, setting IP to scan to localhost.");
                    scanAddress = "127.0.0.1";
                }

                // Both a hostname or an IP address are fine
                if (CheckIPIsValid(scanAddress))
                {
                    ipAddress = IPAddress.Parse(scanAddress);
                }

                //Now to validate the parameter parsed in is a valid IP.
                if (!LookupDNSName(scanAddress, ipAddress))
                {
                    Console.WriteLine("Error Looking Up: " + scanAddress);
                    return;
                }

                Console.WriteLine("Scanning Port: " + scanAddress + " (" + ipAddress.ToString() + ")");

                fileName = scanAddress + "_" + DateTime.UtcNow.ToString("yyyyMMdd-HHMMss") + ".prt.txt";

                //Check if file exists and delete it
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                } 

                StreamWriter file = new StreamWriter(fileName, true);


                startTime = DateTime.Now;
                //The actual port scanning

                //for (int x = IPEndPoint.MinPort; x < 10; x++)
                for (int x = IPEndPoint.MinPort; x < IPEndPoint.MaxPort; x++)
                {
                    Console.Write("Scanning Port: " + x);
                    file.Write("Scanning Port: " + x);
                    if (ScanPort(ipAddress, x))
                    {
                        Console.WriteLine(" OPEN");
                        file.WriteLine(" OPEN");
                    }
                    else
                    {
                        Console.WriteLine(" CLOSED");
                        file.WriteLine(" CLOSED");
                    }
                }

                endTime = DateTime.Now;
                Console.WriteLine("Finished Scanning");

                file.WriteLine("Scan Took: " + (endTime - startTime).TotalSeconds + " Seconds");

                file.Close();

                Process.Start(fileName);
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception caught!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }            

        }

        private static Boolean LookupDNSName(String ipToLookup, IPAddress ipAddress)
        {
            IPHostEntry dnsName;
            try
            {
                //Perform the DNS lookup
                dnsName = Dns.GetHostEntry(ipToLookup);
            }
            catch (Exception e)
            {
                return false;
            }
            
            //There should always be at least one here, if the address is valid./
            if(dnsName.AddressList.Length > 0)
            {
                ipAddress = dnsName.AddressList[0];
                return true;
            }

            return false;
        }

        private static Boolean CheckIPIsValid(String ipAddress)
        {
            Regex IpMatch = new Regex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b");
            return IpMatch.IsMatch(ipAddress);
        }

        private static Boolean ScanPort(IPAddress ip, Int32 port)
        {
            TcpClient Client = new TcpClient();
            try
            {
                //Attempt a connection on the ip and port number
                Client.Connect(ip, port);

                // TcpClient.Close does not actually close the underlying connection
                // http://support.microsoft.com/default.aspx?scid=kb%3Ben-us%3B821625

                NetworkStream ClientStream = Client.GetStream();
                ClientStream.Close();

                // Free the TCPClient resource
                Client.Close();
            }
            catch (Exception e)
            {
                //Connection probably failed if this is caught
                return false;
            }

            return true;
        }
    }
}
