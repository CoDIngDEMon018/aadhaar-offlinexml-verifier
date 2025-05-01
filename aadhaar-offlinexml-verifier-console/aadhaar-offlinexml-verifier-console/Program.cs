using System;

namespace aadhaar_offlinexml_verifier_console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Aadhaar Offline XML Signature Verifier\n");
            string xmlFilePath = "D:\\Amit\\netAadhar\\offlineaadhaar20191121011352794.xml";// Your xmlFilePath
            string publicKeyFilePath = "D:\\Amit\\netAadhar\\uidai_offline_publickey_19062019.cer";// Your publicKeyFilePath

            try
            {
                AadhaarVerifier verifier = new AadhaarVerifier();
                bool result = verifier.VerifyXmlSignature(xmlFilePath, publicKeyFilePath);

                if (result)
                    Console.WriteLine("Signature verification PASSED.");
                else
                    Console.WriteLine("Signature verification FAILED.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during verification:");
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }
}