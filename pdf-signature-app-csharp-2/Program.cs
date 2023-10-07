using System;
using System.IO;
using System.Linq;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Signatures;
using iText.Commons.Bouncycastle.Cert;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto;

namespace PdfSignerApp
{
    public class Program
    {
        static void Main()
        {
            string pdfFilePath = "C:\\Users\\lucas\\Downloads\\sign-test.pdf";
            string pfxFilePath = "C:\\Users\\lucas\\Downloads\\Telegram Desktop\\certificado.pfx";
            string pfxPassword = "lucaslopes";

            Pkcs12Store pfxKeyStore = new Pkcs12Store(new FileStream(pfxFilePath, FileMode.Open, FileAccess.Read), pfxPassword.ToCharArray());
            string alias = pfxKeyStore.Aliases.Cast<string>().FirstOrDefault(entryAlias => pfxKeyStore.IsKeyEntry(entryAlias));

            if (alias == null)
            {
                Console.WriteLine("Chave privada nao encontrada no certificado");
                return;
            }

            ICipherParameters privateKey = pfxKeyStore.GetKey(alias).Key;

            using (PdfReader reader = new PdfReader(pdfFilePath))
            using (FileStream os = new FileStream("C:\\Users\\lucas\\Downloads\\MyPDF_Signed.pdf", FileMode.Create))
            {
                PdfSigner signer = new PdfSigner(reader, os, new StampingProperties());
                PdfSignatureAppearance appearance = signer.GetSignatureAppearance()
                    .SetReason("Atividade de seguranca computacional")
                    .SetLocation("Alfenas-MG")
                    .SetPageRect(new Rectangle(360, 130, 510, 180))
                    .SetPageNumber(1);
                signer.SetFieldName("signature");

                IExternalSignature pks = new PrivateKeySignature(privateKey, "SHA-256");
                signer.SignDetached(pks, new[] { pfxKeyStore.GetCertificate(alias).Certificate }, null, null, null, 0, PdfSigner.CryptoStandard.CMS);
            }

            Console.WriteLine("PDF assinado com sucesso!");
        }
    }
}
