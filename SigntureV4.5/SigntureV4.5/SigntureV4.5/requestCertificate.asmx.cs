using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Security.Cryptography.X509Certificates;
using Syncfusion.Pdf.Security;

namespace SigntureV4._5
{
    /// <summary>
    /// Summary description for requestCertificate
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class requestCertificate : System.Web.Services.WebService
    {

        [WebMethod]
        public List<string> getCertificateName()
        {
            var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.MaxAllowed);
            
            List<string> certificateNames = new List<string>();

            foreach (X509Certificate2 x509Cert in certStore.Certificates)
            {
                //PdfCertificate constructor to load the X509Certificate directly
                PdfCertificate cert = new PdfCertificate(x509Cert);
                certificateNames.Add(cert.IssuerName);
            }
            
            return certificateNames;
        }
    }
}
