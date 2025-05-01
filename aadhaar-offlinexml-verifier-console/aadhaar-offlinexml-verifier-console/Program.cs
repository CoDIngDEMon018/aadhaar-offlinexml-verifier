using System;
using System.IO;

namespace aadhaar_offlinexml_verifier_console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var (xmlPath, certPath) = ParseArguments(args);
                var verifier = new AadhaarVerifier();
                bool isValid = verifier.VerifyXmlSignature(xmlPath, certPath);

                Console.WriteLine(isValid ? "\n✅ Signature verification PASSED" : "\n❌ Signature verification FAILED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n⚠️ Error: {ex.Message}");
                DisplayHelp();
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static (string xmlPath, string certPath) ParseArguments(string[] args)
        {
            string xmlPath = null;
            string certPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-h":
                    case "--help":
                        DisplayHelp();
                        Environment.Exit(0);
                        break;
                    case "-x":
                    case "--xml":
                        if (i + 1 >= args.Length) throw new ArgumentException("Missing XML file path");
                        xmlPath = args[++i];
                        break;
                    case "-c":
                    case "--cert":
                        if (i + 1 >= args.Length) throw new ArgumentException("Missing certificate file path");
                        certPath = args[++i];
                        break;
                    default:
                        if (xmlPath == null && IsXmlFile(args[i]))
                        {
                            xmlPath = args[i];
                        }
                        else if (certPath == null && IsCertFile(args[i]))
                        {
                            certPath = args[i];
                        }
                        break;
                }
            }

            if (xmlPath == null) xmlPath = PromptForFile("Enter XML file path: ", ".xml");
            if (certPath == null) certPath = PromptForFile("Enter certificate path: ", ".cer");

            return (xmlPath, certPath);
        }

        private static string PromptForFile(string prompt, string extension)
        {
            string path;
            do
            {
                Console.Write(prompt);
                path = Console.ReadLine()?.Trim();
            } while (!IsValidFile(path, extension));
            
            return path;
        }

        private static bool IsValidFile(string path, string extension)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            if (!File.Exists(path))
            {
                Console.WriteLine($"File not found: {path}");
                return false;
            }
            if (!Path.GetExtension(path).Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Invalid file type. Expected {extension.ToUpper()}");
                return false;
            }
            return true;
        }

        private static bool IsXmlFile(string path) => 
            Path.GetExtension(path).Equals(".xml", StringComparison.OrdinalIgnoreCase);

        private static bool IsCertFile(string path) => 
            Path.GetExtension(path).Equals(".cer", StringComparison.OrdinalIgnoreCase);

        private static void DisplayHelp()
        {
            Console.WriteLine(@"
Aadhaar Offline XML Signature Verifier
=======================================
Usage:
  aadhaar-verifier [options]

Options:
  -x, --xml <path>    Path to Aadhaar XML file
  -c, --cert <path>   Path to UIDAI public certificate (.cer)
  -h, --help          Show this help message

Examples:
  aadhaar-verifier -x ""My Aadhaar.xml"" -c ""UIDAI.cer""
  aadhaar-verifier ""My Aadhaar.xml"" ""UIDAI.cer""
");
        }
    }
}