using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PdfSharp.Pdf;
//using CommunityToolkit.Mvvm;
//using CommunityToolkit.Mvvm.Input;
//using CommunityToolkit.Mvvm.ComponentModel;
using VMS.TPS.Common.Model.API;
using VMS.OIS.ARIALocal.WebServices.Document.Contracts;
using PdfSharp.Pdf.IO;
using MigraDoc.DocumentObjectModel;
using Microsoft.Office.Interop.Word;
using System.Windows;
namespace PlanCheck
{
    public class AriaSender
    {

        private static ScriptContext _ctx;
        private static string _filepath;
        //private byte[] _binaryContent;
        private static string _patientId;
        private static User _appUser;
        private static string _templateName;
        private static DocumentType _documentType;
        private static Microsoft.Office.Interop.Word.Document _document;
        public AriaSender(ScriptContext ctx, string path, Microsoft.Office.Interop.Word.Document document) // constructor
        {
            _ctx = ctx;
            _filepath = path;
            _document = document;
            GetDocInfo();
            SendToAria();
        }

        private void GetDocInfo()
        {
            _patientId = _ctx.Course.Patient.Id;
            _appUser = _ctx.CurrentUser;
            _templateName = _ctx.ExternalPlanSetup.Id;
            _documentType = new DocumentType
            {
                DocumentTypeDescription = "Chek List Validation Physicien"  //must be an existing type
            };
        }
        public static byte[] ConvertWordDocumentToBinary(Microsoft.Office.Interop.Word.Document document)
        {
            // Step 1: Save the document to a temporary file
            string tempFilePath = @"\\srv015\sf_com\simon_lu\toto.docx";

            document.SaveAs2(tempFilePath, WdSaveFormat.wdFormatDocumentDefault);

            // Step 2: Read the content of the temporary file into a byte array
            byte[] binaryContent = File.ReadAllBytes(tempFilePath);

            // Step 3: Delete the temporary file (optional)
            File.Delete(tempFilePath);

            return binaryContent;
        }
        private void SendToAria()
        {
           

           
            byte[] _binaryContent = File.ReadAllBytes(_filepath);// ConvertWordDocumentToBinary(_document);
            //Creation du document a envoyer, recuperaion des infos et envoi vers aria
            CustomInsertDocumentsParameter.PostDocumentDataPush(_patientId, _appUser, _binaryContent, _templateName, _documentType);
            
        }


    }
}
