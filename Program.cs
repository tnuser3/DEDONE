using System;
using System.Net;
using System.Net.Sockets;
using System.Management;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        
        static void Main(string[] args)
        {
            string question;
            string question2;
            string ip="";
            string ipv4="";
            string desktopname = "";
            string currentuser = "";
            string internet = "";
            string macaddress = "";
            string OS = "";
            Console.WriteLine("Hello, type START to initialize the process");
            question = Console.ReadLine();
            Console.WriteLine();
            if (question == "START")
            {
                Console.WriteLine("Started!");
                Console.WriteLine("Checking internet Connection!");
                internet = CheckForInternetConnection().ToString();
                if (internet == "False")
                {
                    ip = "NO INTERNET";
                    ipv4 = "NO INTERNET";
                    Console.WriteLine("NO INTERNET");
                    Console.WriteLine("would you like to retry connecting to internet?");
                    question2 = Console.ReadLine();
                    if (question2 == "Yes")
                    {
                        internet = CheckForInternetConnection().ToString();
                        if (internet == "False")
                        {
                            Console.WriteLine("NO INTERNET");
                        }
                        else
                        {
                            Console.WriteLine("Checking...");
                            Console.WriteLine("Succsess");
                            Console.WriteLine("Checking checkip.dyndns");
                            ip = GetIPAddress();
                            ipv4 = GetLocalIPAddress();
                            Console.WriteLine("Feched Ip Addresses!");
                        }
                    }
                    Console.WriteLine("No Internet - skipping Ip Address Stuff");
                }
                else
                {
                    Console.WriteLine("Checking checkip.dyndns");
                    ip = GetIPAddress();
                    ipv4 = GetLocalIPAddress();
                    Console.WriteLine("Feched Ip Addresses!");
                }
                desktopname = Environment.MachineName;
                currentuser = Environment.UserName;
                ManagementObject os = new ManagementObject("Win32_OperatingSystem=@");
                string serial = (string)os["SerialNumber"];
                macaddress = GetMacAddress(); 
                OS = GetOS();
                Console.WriteLine("Feched Machine Info!");

                Console.WriteLine("Done!");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Public Ip: " + ip);
                Console.WriteLine("Ipv4: " + ipv4);
                Console.WriteLine("Desktop Name: " + desktopname);
                Console.WriteLine("Current User Name: " + currentuser);
                Console.WriteLine("desktop serial number: " + serial);
                Console.WriteLine("desktop OS: " + OS);
                Console.WriteLine("desktop MAC Address: " + macaddress);
                Console.WriteLine("Ram Amount: " + getmem()+"GB");
                Console.Write("CPU: ");
                ManagementObjectSearcher mos =
                new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (ManagementObject mo in mos.Get())
                {
                    Console.Write(mo["Name"]);
                }
                Console.Read();
            }
            else
            {
                Console.WriteLine("ERROR Code: 1");
                Console.WriteLine("Please Type START");
                Thread.Sleep(3000);
                Environment.Exit(1);
            }
        }
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        static string getmem()
        {
            long memKb;
            GetPhysicallyInstalledSystemMemory(out memKb);
            var mem = (memKb / 1024 / 1024);
            return mem.ToString();
        }
        private static string GetOS()
        {
            string OS = "";
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    OS = "LINUX";
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    OS = "WINDOWS";
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    OS = "OSX";
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                {
                    OS = "FREEBSD";
                }
            }
            catch
            {
                Console.WriteLine("ERROR Code: 2");
                Console.WriteLine("Unable to identify OS");
                Thread.Sleep(3000);
            }
            return OS;
        }
        private static string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            return macAddress;
        }
        public static bool CheckForInternetConnection(int timeoutMs = 10000, string url = null)
        {
            try
            {
                url ??= CultureInfo.InstalledUICulture switch
                {
                    { Name: var n } when n.StartsWith("fa") => // Iran
                        "http://www.aparat.com",
                    { Name: var n } when n.StartsWith("zh") => // China
                        "http://www.baidu.com",
                    _ =>
                        "http://www.gstatic.com/generate_204",
                };

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
                using (var response = (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        static string GetIPAddress()
    {
        String address = "";
        WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
        using (WebResponse response = request.GetResponse())
        using (StreamReader stream = new StreamReader(response.GetResponseStream()))
        {
            address = stream.ReadToEnd();
        }

        int first = address.IndexOf("Address: ") + 9;
        int last = address.LastIndexOf("</body>");
        address = address.Substring(first, last - first);

        return address;
    }

    
    }
}