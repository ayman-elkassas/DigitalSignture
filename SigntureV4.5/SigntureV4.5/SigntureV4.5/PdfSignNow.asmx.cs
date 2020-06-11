using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Services;

using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Security;
using Syncfusion.Pdf.Graphics;
//LDAP
using System.DirectoryServices;
using System.Security.Cryptography.X509Certificates;

namespace SigntureV4._5
{
    /// <summary>
    /// Summary description for PdfSignNow
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class PdfSignNow : System.Web.Services.WebService
    {
        PdfLoadedSignatureField signatureField;

        [WebMethod]
        public string pdfSignNow(string PdfPath, int numPage, string SignImgPath, string certificateName,int docId)
        {
            //License Valid to 4/7/2020
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjY3MDk0QDMxMzgyZTMxMmUzMFRuWlFMb0RXalJMQXBLWHBPeEJRVC9ERmgrTDZOcXJnaEk5ZVl0cWpTcE09");
//            byte[] imageBytes = Convert.FromBase64String(SignImgPath.Remove(0,22));
            byte[] imageBytes = Convert.FromBase64String(SignImgPath);
            //set the image path
            string imgPath = Path.Combine(Server.MapPath("\\testFiles"), "signtureImage.png");

            File.WriteAllBytes(imgPath, imageBytes);

            FileStream docStream = new FileStream(Server.MapPath("\\testFiles\\SignatureFields.pdf"), FileMode.Open, FileAccess.Read);     // parameter# 1    source path
            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
            //Gets the first page of the document

            //numOfPage = 0 as first page

            PdfLoadedPage page = loadedDocument.Pages[numPage] as PdfLoadedPage;
            //Gets the first signature field of the PDF document

            //Get All Signtures
            PdfLoadedFormFieldCollection signatureFieldCollection = loadedDocument.Form.Fields;

            foreach (var item in signatureFieldCollection)
            {
                signatureField = item as PdfLoadedSignatureField;
                if((signatureField.Name).Trim().StartsWith("S"))
                {
                    int index = signatureFieldCollection.IndexOf((Syncfusion.Pdf.Interactive.PdfField)item);
                    string name = signatureField.Name;

                    int n;

                    if (!(int.TryParse(signatureField.Name, out n)))
                    {
                        signatureField.SetName(index.ToString());

                        //How to get x509 certificate from **************LDAP*******************
                        string nid = "k";
                        string LDAPConnection = "LDAP://CN=Users,DC=MOD,DC=Local";
                        string ldapUserName="kasas@mod.local";
                        string password="P@ssw0rd";
                        string propertyName="userCertificate";
                        string Filter="physicalDeliveryOfficeName="+nid;


                        X509Certificate2 Certificate = GetUserCertificateFromAD(
                            LDAPConnection, ldapUserName, password, propertyName, Filter);

                        //How to get x509 certificate ***************client side***************** 
                        PdfCertificate cert = new PdfCertificate(Certificate);

                        //Put Your Signture
                        PdfSignature signature = new PdfSignature(loadedDocument, page, cert, name, signatureField);

                        FileStream imageStream = new FileStream(Server.MapPath("\\testFiles\\signtureImage.png"), FileMode.Open, FileAccess.Read);      // parameter# 4    e signature image

                        //Draw image
                        PdfBitmap signatureImage = new PdfBitmap(imageStream);

                        signature.Appearance.Normal.Graphics.DrawImage(signatureImage, 0, 0, 150, 30);

                        //Save the document 
                        loadedDocument.Save(Server.MapPath("\\testFiles\\file.pdf"));

                        //Close the document 
                        loadedDocument.Close(true);

                        docStream.Close();
                        imageStream.Close();

                        return "Done";
                    }
                }
            }

            return "Document has already fully signed";
        }

        /// <summary>
        /// Get user certificate from Active Directory
        /// </summary>
        /// <param name="ldapEntry">LDAP://DC=domain,DC=local</param>
        /// <param name="ldapUserName">User to connect to Active Directory, set null if not required.</param>
        /// <param name="ldapPassword">Password to connect to Active Directory, set null if not required.</param>
        /// <param name="userIdentificator">user name to search in AD</param>
        /// <param name="propertyName">Set property name, for example userCertificate</param>
        /// <param name="Filter">Ldap filter string like "(&(objectClass=contact)(name=John))".</param>
        /// <returns></returns>
        private X509Certificate2 GetUserCertificateFromAD(string ldapPath, string ldapUserName, string ldapPassword, string propertyName, string Filter)
        {
            try
            {
                DirectoryEntry de;

                if (String.IsNullOrEmpty(ldapUserName) || String.IsNullOrEmpty(ldapPassword))
                {
                    de = new DirectoryEntry(ldapPath, null, null, AuthenticationTypes.Anonymous);
                }
                else
                {
                    de = new DirectoryEntry(ldapPath, ldapUserName, ldapPassword, AuthenticationTypes.Secure);
                }

                DirectorySearcher dsearch = new DirectorySearcher(de);
                dsearch.Filter = String.Format(Filter);
                SearchResultCollection searchResults = dsearch.FindAll();

                foreach (System.DirectoryServices.SearchResult result in searchResults)
                {
                    //Find userCertificate
                    if (result.Properties.Contains(propertyName))
                    {
                        Byte[] certBytes = (Byte[])result.Properties[propertyName][0];

                        X509Certificate2 certificate = null;
                        certificate = new X509Certificate2(certBytes);

                        return certificate;
                    }
                    else
                    {
                        //implement logging
                        return null;
                    }
                }

                de.Close();
                de.Dispose();
                return null;
            }
            catch (Exception)
            {
                //implement logging
                return null;
            }

            throw new NotImplementedException();
        }
    }
}
