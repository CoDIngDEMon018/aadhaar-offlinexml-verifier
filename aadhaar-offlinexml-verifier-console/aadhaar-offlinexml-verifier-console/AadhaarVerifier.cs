using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace aadhaar_offlinexml_verifier_console
{
    public class AadhaarVerifier
    {
        public bool VerifyXmlSignature(string xmlFilePath, string publicKeyFilePath)
        {
            ValidateInputs(xmlFilePath, publicKeyFilePath);
            
            using var xmlDoc = LoadXmlDocument(xmlFilePath);
            var (signatureValue, signedData) = ExtractSignature(xmlDoc);
            var publicKey = LoadPublicKey(publicKeyFilePath);

            return VerifyDigitalSignature(signedData, signatureValue, publicKey);
        }

        private void ValidateInputs(string xmlPath, string certPath)
        {
            if (!File.Exists(xmlPath))
                throw new FileNotFoundException("XML file not found", xmlPath);
            
            if (!File.Exists(certPath))
                throw new FileNotFoundException("Certificate file not found", certPath);
        }

        private XmlDocument LoadXmlDocument(string path)
        {
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.Load(path);
            return xmlDoc;
        }

        private (string signatureValue, string signedData) ExtractSignature(XmlDocument xmlDoc)
        {
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

            var signatureNode = xmlDoc.SelectSingleNode("//ds:Signature", nsManager) 
                ?? throw new XmlException("Signature element not found in XML");

            var signatureValue = signatureNode.SelectSingleNode(".//ds:SignatureValue", nsManager)?.InnerText 
                ?? throw new XmlException("SignatureValue not found in XML");

            signatureNode.ParentNode?.RemoveChild(signatureNode);
            
            return (signatureValue, xmlDoc.OuterXml);
        }

        private AsymmetricKeyParameter LoadPublicKey(string certPath)
        {
            using var cert = new X509Certificate2(certPath, "public");
            return new X509CertificateParser()
                .ReadCertificate(cert.GetRawCertData())
                .GetPublicKey();
        }

        private bool VerifyDigitalSignature(string signedData, string signatureValue, AsymmetricKeyParameter publicKey)
        {
            var signer = SignerUtilities.GetSigner("SHA256withRSA");
            signer.Init(false, publicKey);

            var dataBytes = Encoding.UTF8.GetBytes(signedData);
            var signatureBytes = Convert.FromBase64String(signatureValue);

            signer.BlockUpdate(dataBytes, 0, dataBytes.Length);
            return signer.VerifySignature(signatureBytes);
        }
    }
}