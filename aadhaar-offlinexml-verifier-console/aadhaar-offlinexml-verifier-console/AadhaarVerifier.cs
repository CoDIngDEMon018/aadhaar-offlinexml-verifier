using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace aadhaar_offlinexml_verifier_console
{
    public class AadhaarVerifier
    {
        public bool VerifyXmlSignature(string xmlFilePath, string publicKeyFilePath)
        {
            try
            {
                // Check if XML file exists
                if (!File.Exists(xmlFilePath))
                {
                    throw new FileNotFoundException("XML file not found at the specified path.");
                }

                // Check if Public Key file exists
                if (!File.Exists(publicKeyFilePath))
                {
                    throw new FileNotFoundException("Public Key file not found at the specified path.");
                }

                // Load XML document
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                // Extract and remove Signature node
                string signatureValue = xmlDoc.DocumentElement.ChildNodes[1].ChildNodes[1].InnerXml;
                XmlNode signatureNode = xmlDoc.DocumentElement.ChildNodes[1];
                xmlDoc.DocumentElement.RemoveChild(signatureNode);

                // Load and parse the public certificate
                X509Certificate2 x509Cert = new X509Certificate2(publicKeyFilePath, "public");
                X509CertificateParser parser = new X509CertificateParser();
                Org.BouncyCastle.X509.X509Certificate bcCert = parser.ReadCertificate(x509Cert.GetRawCertData());

                // Initialize signer
                ISigner signer = SignerUtilities.GetSigner("SHA256withRSA");
                signer.Init(false, bcCert.GetPublicKey());

                // Convert signature to bytes
                byte[] expectedSig = Convert.FromBase64String(signatureValue);
                byte[] messageBytes = Encoding.UTF8.GetBytes(xmlDoc.InnerXml);

                // Perform signature verification
                signer.BlockUpdate(messageBytes, 0, messageBytes.Length);
                bool isValid = signer.VerifySignature(expectedSig);

                // Output result
                Console.WriteLine(isValid ? "XML Verified Successfully." : "XML Verification Failed.");
                return isValid;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Error: File not found.");
                Console.WriteLine(ex.Message);
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("Error: Cryptographic verification failed.");
                Console.WriteLine(ex.Message);
            }
            catch (XmlException ex)
            {
                Console.WriteLine("Error: XML parsing failed.");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("General error during verification:");
                Console.WriteLine(ex.Message);
            }

            return false;
        }
    }
}