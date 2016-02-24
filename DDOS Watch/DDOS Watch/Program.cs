using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Net.Sockets;

namespace DDOS_Watch
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "DDOS Watch 1.0";
            Console.WriteLine("DDOS Watch 1.0 Started...");
            Console.WriteLine("Intializing...");

            string notifyEmail = "";
            string emailPassword = "";
            string emailServer = "";
            int emailPort = 0;
            bool Warned = false;
            string serverIP = "";
            int serverPort = 80;
            int checkDelay = 30; //Seconds 

            //load settings
            if(!File.Exists(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Settings.ini"))
            {
                Console.WriteLine("Unable to locate configuration file, please make sure you include the configuration file in the same path as the application.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            else
            {
                string[] config = File.ReadAllLines(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Settings.ini");

                for (int i = 0; i < config.Length; i++)
                    try
                    {
                        config[i] = config[i].Remove(config[i].LastIndexOf(';'));
                    }
                    catch { }


                notifyEmail = config[0].Split('=')[1];
                emailPassword = config[1].Split('=')[1];
                emailServer = config[2].Split('=')[1];
                emailPort = Int32.Parse(config[3].Split('=')[1]);
                serverIP = config[4].Split('=')[1];
                serverPort = Int32.Parse(config[5].Split('=')[1]);
                checkDelay = Int32.Parse(config[6].Split('=')[1]);
            }
            //end of settings load 

            Socket sck;

            ConsoleColor c = ConsoleColor.Red;
            string Status = "OFFLINE";

            Thread.Sleep(2500);

            while (true)
            {
                try
                {
                    sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sck.Connect(new IPEndPoint(IPAddress.Parse(serverIP), serverPort));

                    if (sck.Connected)
                    {
                        c = ConsoleColor.Green;
                        Status = "ONLINE";
                    }

                    Warned = false;

                    sck.Close();
                    sck.Dispose();
                }
                catch
                {
                    c = ConsoleColor.Red;
                    Status = "OFFLINE";
                    if (!Warned)
                    {
                        Warned = true;

                        MailMessage msg = new MailMessage();
                        msg.To.Add(notifyEmail);
                        msg.Subject = "DDOS Watch - Server: " + serverIP + ":" + serverPort + " Status: " + Status + "(" + DateTime.Now.ToString("HH:mm:ss d:m:y") + ")";
                        msg.From = new MailAddress(notifyEmail);
                        msg.Body = "WARNING, your server has been detected as OFFLINE! Please contact the network administrator immediately. If you are the network administrator, please check your servers settings, as well as incomming connections!";
                        SmtpClient client = new SmtpClient(emailServer, emailPort);
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(notifyEmail, emailPassword);
                        client.Send(msg);
                    }
                }

                for (int i = 0; i < checkDelay; i++)
                {
                    Console.Clear();
                    Console.Write("Server Status: ");
                    Console.ForegroundColor = c;
                    Console.Write(Status);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(", Next check in " + (checkDelay - i) + " Seconds.");
                    Thread.Sleep(1000);
                }

                Console.Clear();
                Console.WriteLine("Checking server status...");

                GC.Collect();
            }
        }
    }
}