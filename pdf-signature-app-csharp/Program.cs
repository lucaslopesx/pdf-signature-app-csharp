using System;
using System.IO;
using System.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;

public class PdfSignerApp
{
    static void Main(string[] args)
    {
        //Carrega o pdf
        string pdfFilePath = "C:\\Users\\lucas\\Downloads\\sign-test.pdf";
        PdfReader pdfReader = new PdfReader(pdfFilePath);

        //Carrega o certificado
        string pfxFilePath = "C:\\Users\\lucas\\Downloads\\Telegram Desktop\\certificado.pfx";
        string pfxPassword = "exemplosenha";
        Pkcs12Store pfxKeyStore = new Pkcs12Store(new FileStream(pfxFilePath, FileMode.Open, FileAccess.Read), pfxPassword.ToCharArray());

        //Cria um novo pdf que será a versão assinada
        PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, new FileStream("C:\\Users\\lucas\\Downloads\\MyPDF_Signed.pdf", FileMode.Create), '\0', null, true);
        
        //(opcional) Adiciona a razao e localizacao como parte das informacoes da assinatura
        PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;
        signatureAppearance.Reason = "Atividade de seguranca computacional";
        signatureAppearance.Location = "Alfenas-MG";

        //(opcional) Deixa a assinatura de uma forma visivel no pdf
        float x = 360;
        float y = 130;
        signatureAppearance.Acro6Layers = false;
        signatureAppearance.Layer4Text = PdfSignatureAppearance.questionMark;
        signatureAppearance.SetVisibleSignature(new iTextSharp.text.Rectangle(x, y, x + 150, y + 50), 1, "signature");

        //Assinando o pdf
        string alias = pfxKeyStore.Aliases.Cast<string>().FirstOrDefault(entryAlias => pfxKeyStore.IsKeyEntry(entryAlias));

        if (alias != null)
        {
            ICipherParameters privateKey = pfxKeyStore.GetKey(alias).Key;
            IExternalSignature pks = new PrivateKeySignature(privateKey, DigestAlgorithms.SHA256);
            MakeSignature.SignDetached(signatureAppearance, pks, new Org.BouncyCastle.X509.X509Certificate[] { pfxKeyStore.GetCertificate(alias).Certificate }, null, null, null, 0, CryptoStandard.CMS);
        }
        else
        {
            Console.WriteLine("Chave privada nao encontrada no certificado");
        }

        //Salva o pdf assinado
        pdfStamper.Close();

        Console.WriteLine("PDF assinado com sucesso!");
    }
}
