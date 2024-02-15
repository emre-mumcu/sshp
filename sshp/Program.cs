using System;
using System.IO;
using System.Management;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace sshp
{
    class Program
    {
        private static bool masterPassSet = false;

        private static readonly string masterPass;

        static Program()
        {
            if (File.Exists(Path.Combine(Application.StartupPath, "init.io")))
            {
                masterPass = Encoding.UTF8.GetString(Convert.FromBase64String(CryptoLib.DeSerializeObject<string>(Application.StartupPath)));
            }
            else
            {
                // Request a masterpass from user:
                Console.Write("Enter master pass: ");

                var pass = string.Empty;

                ConsoleKey key;

                ConsoleKeyInfo keyInfo;

                do
                {
                    keyInfo = Console.ReadKey(true);
                    
                    key = keyInfo.Key;

                    if (key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        Console.Write("\b \b");
                        pass = pass.Remove(pass.Length - 1); // pass[0..^1];
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        Console.Write("*");
                        pass += keyInfo.KeyChar;
                    }
                }                
                while (keyInfo.Key != ConsoleKey.Enter);
                
                masterPass = pass;
                masterPassSet = true;
                CryptoLib.SerializeObject<string>(Convert.ToBase64String(Encoding.UTF8.GetBytes(masterPass)), Application.StartupPath);                
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            if (args.Length == 0)
            {
                Console.WriteLine("no-input, use one of the switches: [-du/-dp 'text' 'entropy'], [-s], [-a], [-v], [-k]");
            }
            else
            {
                if (args.Length == 1 && args[0] == "-s")
                {
                    Console.WriteLine(CpuInfo());
                }
                else if (args.Length == 1 && args[0] == "-a")
                {
                    Console.WriteLine(About());
                }
                else if (args.Length == 1 && args[0] == "-v")
                {
                    System.Diagnostics.Process.Start("https://emremumcu.com");                    
                }
                else if (args.Length == 1 && args[0] == "-k")
                {
                    Console.WriteLine(new CryptoLib(masterPass).KeySpace());
                }
                else if (args.Length == 3 && args[0] == "-dp")
                {
                    var result = SECURITY.DPAPI.Protect(args[1], args[2]);
                    //Console.WriteLine(result);
                    Clipboard.Clear();
                    Clipboard.SetText(result);
                    Console.WriteLine("protected");
                }
                else if (args.Length == 3 && args[0] == "-du")
                {
                    var result = SECURITY.DPAPI.Unportect(args[1], args[2]);
                    //Console.WriteLine(result);
                    Clipboard.Clear();
                    Clipboard.SetText(result);
                    Console.WriteLine("unprotected");
                }
                else
                {
                    CreateOutput(args[0]);
                }
            }

            if(masterPassSet)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("\b \b");
                Console.WriteLine($"Master pass is SET as: {masterPass}" );
                Console.Write("\b \b");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadLine();
                Console.Clear();
            }
        }

        static void CreateOutput(string input)
        {
            CryptoLib cl = new CryptoLib(masterPass);
            string output = cl.CreateOutput(input);
            Clipboard.Clear();
            Clipboard.SetText(output);
            Console.WriteLine("complete");
        }

        public static String CpuInfo()
        {
            StringBuilder cpuInfo = new StringBuilder();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "Select * FROM WIN32_Processor");

            foreach (ManagementObject obj in searcher.Get())
            {
                cpuInfo.Append(new string('-', 50)).Append(Environment.NewLine); ;
                cpuInfo.Append(obj["DeviceID"]).Append(": ").Append(obj["Manufacturer"]);
                cpuInfo.Append(Environment.NewLine).Append(new string('-', 50)).Append(Environment.NewLine);
                cpuInfo.Append(obj["Name"]);
                cpuInfo.Append(Environment.NewLine);
                cpuInfo.Append($"Number Of Cores: {obj["NumberOfCores"]}");
                cpuInfo.Append(Environment.NewLine);
                cpuInfo.Append($"Number Of Logica lProcessors: {obj["NumberOfLogicalProcessors"]}");
                cpuInfo.Append(Environment.NewLine);
                cpuInfo.Append($"Architecture: {obj["AddressWidth"]} Bit");
                cpuInfo.Append(Environment.NewLine);
                cpuInfo.Append($"Processor Id: {obj["ProcessorId"]}");
                cpuInfo.Append(Environment.NewLine);
                cpuInfo.Append($"STATUS: {obj["Status"]}");
            }

            return cpuInfo.ToString();
        }

        public static String About()
        {
            return 
                new StringBuilder()
                .Append("Emre Mumcu @2021")
                .Append(Environment.NewLine)
                .Append("https://mumcusoft.com")
                .ToString();
        }
    }
}
