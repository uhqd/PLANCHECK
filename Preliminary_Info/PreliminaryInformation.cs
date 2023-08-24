using PlanCheck.Users;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;
using System.IO;
using PlanCheck;
using System.Windows.Input;
using VMS.OIS.ARIALocal.WebServices.Document.Contracts;
using Newtonsoft.Json;
using System.Configuration;
using System.Drawing;
using System.Net.Http;
using VMS.OIS.ARIAExternal.WebServices.Documents.Contracts;
using VMS.TPS.Common.Model.Types;


namespace PlanCheck
{
    public class PreliminaryInformation
    {
        private ScriptContext _ctx;
        private string _patientname;
        private string _patientdob;
        private DateTime _patientdob_dt;
        private string _coursename;
        private string _planname;
        private IUCT_User _plancreator;
        private IUCT_User _currentuser;
        private IUCT_User _doctor;
        private string _algoname;
        private string _mlctype;
        private string _treatmentType;
        private int _treatmentFieldNumber;
        private int _setupFieldNumber;
        //  private string[] _calculoptions;
        private string[] _POoptions;
        private bool _TOMO;
        private bool _NOVA;
        private bool _HALCYON;
        private bool _HYPERARC;
        private bool _isModulated;
        private string _machine;
        private TomotherapyPdfReportReader _tprd;
        private bool _dosecheckIsNeeded;
        private List<DateTime> dosimetrie = new List<DateTime>();
        private List<DateTime> dosecheck = new List<DateTime>();
        private List<DateTime> ficheDePosition = new List<DateTime>();
        private List<DateTime> autres = new List<DateTime>();
        //private string saveFilePath;
        private string documentList;
        //private string tomoReportPath;
        private bool planReportFound;
        private bool doseCheckReportFound;
        private bool positionReportFound;
        private int returnCode;
        public bool _advancedUserMode;
        public string _lastUsedCheckProtocol;
        public static List<OARvolume> referenceManOARVolume;//= new List<OARvolume>();
        public static List<OARvolume> referenceWomanOARVolume;//= new List<OARvolume>();

        public bool isARecentDocument(DateTime t)
        {
            int recent = 30;   // number of days 
            bool returnBool = false;
            DateTime myToday = DateTime.Today;
            int nDays = (myToday - t).Days;
            if (nDays > recent)
                returnBool = false;
            else
                returnBool = true;
            return returnBool;

        }
        public int isTheCorrectTomoReport(String s)  // 0 not a tomo report, 1 tomo report not the good one, 2 the good tomo report
        {

            // MessageBox.Show("s " + s + " " );
            String saveFilePathTemp = Directory.GetCurrentDirectory() + @"\__temp__.pdf";
            int startBinary = s.IndexOf("\"BinaryContent\"") + 17;
            int endBinary = s.IndexOf("\"Certifier\"") - 2;
            string binaryContent2 = s.Substring(startBinary, endBinary - startBinary);
            binaryContent2 = binaryContent2.Replace("\\", "");  // the \  makes the string a non valid base64 string                       
            File.WriteAllBytes(saveFilePathTemp, Convert.FromBase64String(binaryContent2));
            TomotherapyPdfReportReader _tempTprd = new TomotherapyPdfReportReader(saveFilePathTemp);

            if (_tempTprd.itIsATomoReport == false)
                returnCode = 0;
            else
            {
                _ctx.PlanSetup.DoseValuePresentation = DoseValuePresentation.Absolute; // set dose value to absolute presentation
                double planDoseMax = _ctx.PlanSetup.Dose.DoseMax3D.Dose;

                if (Math.Abs(_tempTprd.Trd.maxDose - planDoseMax) < 0.11) // < 0.11 Gy
                {

                    returnCode = 2;
                    _tprd = _tempTprd;
                    // MessageBox.Show("Extraction des données de plan depuis la Dosimétrie Tomo dans Aria Documents");
                }
                else
                    returnCode = 1;

            }

            File.Delete(saveFilePathTemp);

            return returnCode;
        }
        public String connectToAriaDocuments(ScriptContext ctx)
        {
            bool DocumentAriaIsConnected = true;
            DocSettings docSet = DocSettings.ReadSettings();
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
            string apiKeyDoc = docSet.DocKey;
            string hostName = docSet.HostName;
            string port = docSet.Port;

            string response = "";
            string request = "{\"__type\":\"GetDocumentsRequest:http://services.varian.com/Patient/Documents\",\"Attributes\":[],\"PatientId\":{ \"ID1\":\"" + ctx.Patient.Id + "\"}}";
            try
            {
                response = CustomInsertDocumentsParameter.SendData(request, true, apiKeyDoc, docSet.HostName, docSet.Port);
            }
            catch
            {
                MessageBox.Show("La connexion à Aria Documents a échoué. Les documents ne peuvent pas être récupérés");
                DocumentAriaIsConnected = false;
            }

            if (DocumentAriaIsConnected)
                return response;
            else
                return null;

        }
        public void getTheAriaDocuments(String response, ScriptContext ctx)
        {

            #region declaration of variables and deserialize response

            DocSettings docSet = DocSettings.ReadSettings();
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
            string apiKeyDoc = docSet.DocKey;
            string hostName = docSet.HostName;
            string port = docSet.Port;

            string doc1 = "Dosimétrie";
            string doc2 = "Dosecheck";
            string doc3 = "Fiche de positionnement";
            var VisitNoteList = new List<string>();
            int visitnoteloc = response.IndexOf("PtVisitNoteId");
            while (visitnoteloc > 0)
            {
                VisitNoteList.Add(response.Substring(visitnoteloc + 15, 2).Replace(",", ""));
                visitnoteloc = response.IndexOf("PtVisitNoteId", visitnoteloc + 1);
            }
            var response_Doc = JsonConvert.DeserializeObject<DocumentsResponse>(response); // get the list of documents
            var DocTypeList = new List<string>();
            var DateServiceList = new List<DateTime>();
            List<int> DocIndexList = new List<int>();
            var PatNameList = new List<string>();
            int loopnum = 0;

            //int tomoReportIndex = -1;
            string thePtId = "";
            string thePtVisitId = "";
            string theVisitNoteId = "";
            string request_docdetails = "";
            string response_docdetails = "";
            string thisDocType = "";
            int typeloc = 0;
            int enteredloc = 0;
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            #endregion

            #region loop on documents to select document of interest
            foreach (var document in response_Doc.Documents) // parse documents
            {
                bool trashDoc = false;

                #region get the general infos of this document , dismiss if date of service <0
                thePtId = document.PtId;
                thePtVisitId = document.PtVisitId.ToString();
                theVisitNoteId = VisitNoteList[loopnum];
                request_docdetails = "{\"__type\":\"GetDocumentRequest:http://services.varian.com/Patient/Documents\",\"Attributes\":[],\"PatientId\":{ \"PtId\":\"" + thePtId + "\"},\"PatientVisitId\":" + thePtVisitId + ",\"VisitNoteId\":" + theVisitNoteId + "}";
                response_docdetails = CustomInsertDocumentsParameter.SendData(request_docdetails, true, apiKeyDoc, docSet.HostName, docSet.Port);
                typeloc = response_docdetails.IndexOf("DocumentType");
                enteredloc = response_docdetails.IndexOf("EnteredBy");

                if (typeloc > 0)
                {
                    thisDocType = response_docdetails.Substring(typeloc + 15, enteredloc - typeloc - 18);

                }
                int nameloc = response_docdetails.IndexOf("PatientLastName");
                int dobloc = response_docdetails.IndexOf("PreviewText");
                if (nameloc > 0)
                    PatNameList.Add(response_docdetails.Substring(nameloc + 18, dobloc - nameloc - 21));
                int dateservloc = response_docdetails.IndexOf("DateOfService");
                int datesignloc = response_docdetails.IndexOf("DateSigned");

                if (dateservloc <= 0)
                    trashDoc = true;
                #endregion

                #region  dismiss if document is marked as error
                if (!trashDoc)
                {
                    int IsMarkedAsErrorIndex = response_docdetails.IndexOf("IsMarkedAsError"); // TRUE = ERROR   FALSE = OK :-)
                    string isError = response_docdetails.Substring(IsMarkedAsErrorIndex + 17, 4);

                    if (isError.ToUpper().Contains("TRU"))
                    {
                        trashDoc = true;
                    }

                }
                #endregion

                #region dismiss if document is old (> 30days)                
                if (!trashDoc)
                {

                    dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(response_docdetails.Substring(dateservloc + 23, datesignloc - dateservloc - 34)) / 1000).ToLocalTime();
                    if (!isARecentDocument(dtDateTime))
                        trashDoc = true;


                }
                #endregion

                #region dismiss if document has a useless type for plancheck
                if (!trashDoc)
                {
                    if ((thisDocType != doc1) && (thisDocType != doc2) && (thisDocType != doc3))
                        trashDoc = true;


                }
                #endregion

                #region dismiss if plan report is not ok 
                int tomoReportReturnCode = -1; // 0 not a tomo report, 1 tomo report not the good one, 2 the good tomo report


                if (!trashDoc)
                {
                    if (thisDocType == doc1)
                    {

                        tomoReportReturnCode = isTheCorrectTomoReport(response_docdetails); // check that is a TOMO report and has the same dose max than the plan
                                                                                            // fill _tprd
                                                                                            // MessageBox.Show("two " + tomoReportReturnCode);
                        if (_TOMO)
                        {
                            if (tomoReportReturnCode != 2)
                                trashDoc = true;
                        }
                        else
                        {
                            if (tomoReportReturnCode != 0)
                                trashDoc = true;
                        }

                    }


                }
                #endregion
                //MessageBox.Show("one " + thisDocType + " " + trashDoc );
                #region store index
                if (!trashDoc)
                {
                    DateServiceList.Add(dtDateTime);
                    DocTypeList.Add(thisDocType);
                    DocIndexList.Add(loopnum);
                }
                #endregion

                loopnum++;
            }
            #endregion

            #region count each document type
            for (int i = 0; i < DocTypeList.Count; i++)
            {
                if (DocTypeList[i] == doc1) { dosimetrie.Add(DateServiceList[i]); }
                else if (DocTypeList[i] == doc2) { dosecheck.Add(DateServiceList[i]); }
                else if (DocTypeList[i] == doc3) { ficheDePosition.Add(DateServiceList[i]); }
                else { autres.Add(DateServiceList[i]); }
            }
            #endregion

            #region check if one recent document exists
            if (dosecheck.Count == 0)
                doseCheckReportFound = false;
            else
                doseCheckReportFound = true;

            if (ficheDePosition.Count == 0)
                positionReportFound = false;
            else
                positionReportFound = true;

            if (dosimetrie.Count == 0)
                planReportFound = false;
            else
                planReportFound = true;


            #endregion

            #region uncomment to display document list 

            documentList += "Patient: " + PatNameList[0] + " - " + ctx.Patient.Id + "\n";
            documentList += "(" + dosimetrie.Count + ") " + doc1 + ":            " + dosimetrie.DefaultIfEmpty().Max().ToString("MM/dd/yy").Replace("01/01/01", "") + "\n";
            documentList += "(" + dosecheck.Count + ") " + doc2 + ":  " + dosecheck.DefaultIfEmpty().Max().ToString("MM/dd/yy").Replace("01/01/01", "") + "\n";
            documentList += "(" + ficheDePosition.Count + ") " + doc3 + ":       " + ficheDePosition.DefaultIfEmpty().Max().ToString("MM/dd/yy").Replace("01/01/01", "") + "\n";
            documentList += "(" + autres.Count + ") " + doc3 + ":       " + autres.DefaultIfEmpty().Max().ToString("MM/dd/yy").Replace("01/01/01", "") + "\n";
            /*
            MessageBox.Show(documentList);
            */
            #endregion

        }
        private IUCT_User GetUser(string searchtype, IUCT_Users iuct_users)
        {
            string tocheck;
            switch (searchtype)
            {
                case "doctor":
                    tocheck = _ctx.PlanSetup.RTPrescription.HistoryUserName;
                    break;
                case "creator":
                    tocheck = _ctx.PlanSetup.CreationUserName;
                    break;
                default:
                    tocheck = _ctx.CurrentUser.Name;
                    break;
            }


            //Generate Users list
            //IUCT_Users iuct_users = new IUCT_Users();

            IUCT_User user = new IUCT_User();
            user = iuct_users.UsersList.Where(name => name.UserFamilyName == "indefini").FirstOrDefault();
            foreach (IUCT_User user_tmp in iuct_users.UsersList)
            {


                if (tocheck.ToLower().Contains(user_tmp.userId.ToLower()))
                {
                    user = user_tmp;

                }
            }



            return user;
        }
        private string Check_mlc_type(PlanSetup plan)
        {
            string technique = "Technique non reconnue (ni RA, ni DCA)";

            if (plan.Beams.Any(b => (b.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.ArcDynamic)))
            {
                technique = "Arctherapie dynamique (DCA)";
            }
            if (plan.Beams.Any(b => (b.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.VMAT)))
            {
                technique = "Modulation d'intensite";
            }
            if (plan.Beams.Any(b => (b.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.Static)))
            {
                technique = "RTC";
            }

            return technique;
        }

        private void getOARreferenceVolumes()
        {



            referenceManOARVolume = new List<OARvolume>();
            referenceWomanOARVolume = new List<OARvolume>();

            String pathOAR_W = Directory.GetCurrentDirectory() + @"\plancheck_data\volumeOARs\OAR-woman.csv";
            String pathOAR_M = Directory.GetCurrentDirectory() + @"\plancheck_data\volumeOARs\OAR-man.csv";
            string[] OARlinesW = File.ReadAllLines(pathOAR_W);
            string[] OARlinesM = File.ReadAllLines(pathOAR_M);

            foreach (string line in OARlinesW)
            {
                OARvolume volumeone = new OARvolume();
                string[] partline = line.Split(';');
                volumeone.volumeName = partline[0];
                volumeone.volumeMin = Convert.ToDouble(partline[1]);
                volumeone.volumeMax = Convert.ToDouble(partline[2]);
                volumeone.nExpectedPart = Convert.ToInt32(partline[3]);
                volumeone.laterality = partline[4];
                referenceWomanOARVolume.Add(volumeone);
            }

            foreach (string line in OARlinesM)
            {
                OARvolume volumeone = new OARvolume();
                string[] partline = line.Split(';');
                volumeone.volumeName = partline[0];
                volumeone.volumeMin = Convert.ToDouble(partline[1]);
                volumeone.volumeMax = Convert.ToDouble(partline[2]);
                volumeone.nExpectedPart = Convert.ToInt32(partline[3]);
                volumeone.laterality = partline[4];
                referenceManOARVolume.Add(volumeone);
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        public PreliminaryInformation(ScriptContext ctx)  //Constructor
        {


            #region general info
            _ctx = ctx;

            if (_ctx.Patient.Name != null)
                _patientname = _ctx.Patient.Name;
            else
                _patientname = "no name";

            if (_ctx.Patient.DateOfBirth.HasValue)
            {
                _patientdob_dt = (DateTime)_ctx.Patient.DateOfBirth;
                _patientdob = _patientdob_dt.Day + "/" + _patientdob_dt.Month + "/" + _patientdob_dt.Year;
            }
            else
                _patientdob = "no DoB";


            IUCT_Users iuct_users = new IUCT_Users();

            _coursename = ctx.Course.Id;
            _planname = ctx.PlanSetup.Id;
            _plancreator = GetUser("creator", iuct_users);
            _currentuser = GetUser("currentuser", iuct_users);

            if (ctx.PlanSetup.RTPrescription != null)
                _doctor = GetUser("doctor", iuct_users);

            if (_ctx.PlanSetup.PhotonCalculationModel != null)
                _algoname = ctx.PlanSetup.PhotonCalculationModel;
            else
                _algoname = "no photon calculation model";

            _mlctype = Check_mlc_type(ctx.PlanSetup);
            /*
            _calculoptions = new string[ctx.PlanSetup.GetCalculationOptions(ctx.PlanSetup.PhotonCalculationModel).Values.Count];
            _calculoptions = ctx.PlanSetup.GetCalculationOptions(ctx.PlanSetup.PhotonCalculationModel).Values.ToArray();
            */

            int n = ctx.PlanSetup.GetCalculationOptions("PO_15605New").Values.Count;
            _POoptions = new string[n];
            _POoptions = ctx.PlanSetup.GetCalculationOptions("PO_15605New").Values.ToArray();


            getOARreferenceVolumes();


            #endregion


            #region machine
            _machine = ctx.PlanSetup.Beams.First().TreatmentUnit.Id.ToUpper();
            _NOVA = false;
            _TOMO = false;
            _HALCYON = false;
            _HYPERARC = false;
            if (_machine.Contains("NOVA"))
            {
                _NOVA = true;
                String fieldname = ctx.PlanSetup.Beams.FirstOrDefault(x => x.IsSetupField == false).Id;
                if (fieldname.Contains("HA"))
                    _HYPERARC = true;

            }
            else if (_machine.Contains("HALCYON"))
                _HALCYON = true;
            else if (_machine.Contains("TOM"))
            {
                _TOMO = true;
                foreach (PlanSetup p in ctx.Course.PlanSetups)
                {
                    if (p.Id.Contains("SEA"))
                    {
                        _machine = p.Beams.First().TreatmentUnit.Id;
                        _machine = _machine.Replace(" SEANCE", "");
                    }
                }
            }

            #endregion


            #region get ARIA documents infos


            String response = connectToAriaDocuments(ctx);

            if (response != null)
            {
                getTheAriaDocuments(response, ctx); // check if documents exists and get info from tomo report if needed
            }

            if ((isTOMO) && (!tomoReportIsFound))
                MessageBox.Show("Pas de rapport de plan Tomotherapy dans Aria Documents");

            #endregion


            #region set initial values



            foreach (Beam bn in ctx.PlanSetup.Beams)
            {

                if (bn.IsSetupField)  // count set up
                {
                    _setupFieldNumber++;
                }
                else
                {
                    _treatmentFieldNumber++;
                    //machineName = b.TreatmentUnit.Id;
                }
            }

            _isModulated = false;
            Beam b = ctx.PlanSetup.Beams.First(x => x.IsSetupField == false);

            if (b.MLCPlanType.ToString() == "VMAT")
            {
                _treatmentType = "VMAT";
                _isModulated = true;
            }
            else if (b.MLCPlanType.ToString() == "ArcDynamic")
                _treatmentType = "DCA";
            else if (b.MLCPlanType.ToString() == "DoseDynamic")
            {
                _treatmentType = "IMRT";
                _isModulated = true;
            }
            else if (b.MLCPlanType.ToString() == "Static")
                _treatmentType = "RTC (MLC)";
            else if (b.MLCPlanType.ToString() == "NotDefined")
            {
                if (b.Technique.Id == "STATIC")  // can be TOMO, Electrons or 3DCRT without MLC
                {
                    if (_machine.Contains("TOM"))
                    {
                        _treatmentType = "Tomotherapy";
                        _isModulated = true;
                    }
                    else if (b.EnergyModeDisplayName.Contains("E"))
                        _treatmentType = "Electrons";
                    else
                        _treatmentType = "RTC (sans MLC)";
                }
                else
                    _treatmentType = "Technique non statique inconnue : pas de MLC !";
            }

            #region dosecheck is needed ?
            _dosecheckIsNeeded = true;
            if (isHyperArc)
                _dosecheckIsNeeded = false;
            if (isTOMO && _ctx.Image.ImagingOrientation.ToString() == "Feet first")
                _dosecheckIsNeeded = false;
            string energy = "";
            foreach (Beam be in ctx.PlanSetup.Beams)
            {
                if (!be.IsSetupField)
                {
                    energy = be.EnergyModeDisplayName;
                }
            }

            if (isNOVA && isModulated && energy.Contains("6FFF"))
                _dosecheckIsNeeded = false;
            #endregion

            #endregion

        }


        #region GETS/SETS
        public string PatientName
        {
            get { return _patientname; }
        }
        /*
         public string[] Calculoptions
         {
             get { return _calculoptions; }
         }
        */
        public string[] POoptions
        {
            get { return _POoptions; }
        }
        public string PatientDOB
        {
            get { return _patientdob; }
        }
        public string documentsList
        {
            get { return documentList; }
        }
        public DateTime PatientDOB_dt
        {
            get { return _patientdob_dt; }
        }
        public string CourseName
        {
            get { return _coursename; }
        }
        public string PlanName
        {
            get { return _planname; }
        }
        public IUCT_User PlanCreator
        {
            get { return _plancreator; }
        }
        public bool isModulated
        {
            get { return _isModulated; }
        }
        public IUCT_User Doctor
        {
            get { return _doctor; }
        }
        public IUCT_User CurrentUser
        {
            get { return _currentuser; }
        }
        public string AlgoName
        {
            get { return _algoname; }
        }
        public string Mlctype
        {
            get { return _mlctype; }
        }
        public string treatmentType
        {
            get { return _treatmentType; }
        }
        public int treatmentFieldNumber
        {
            get { return _treatmentFieldNumber; }
        }
        public int setupFieldNumber
        {
            get { return _setupFieldNumber; }
        }

        public void setTreatmentType(string type)
        {
            _treatmentType = type;
        }
        public bool isTOMO
        {
            get { return _TOMO; }
        }
        public bool isNOVA
        {
            get { return _NOVA; }
        }
        public bool isHALCYON
        {
            get { return _HALCYON; }
        }
        public bool isHyperArc
        {
            get { return _HYPERARC; }
        }
        public bool doseCheckIsNeeded
        {
            get { return _dosecheckIsNeeded; }
        }
        public string machine
        {
            get { return _machine; }
        }
        public TomotherapyPdfReportReader tprd
        {
            get { return _tprd; }
        }

        /* public string tomoReportPathFile
         {
             get { return tomoReportPath; }
         }*/
        public bool tomoReportIsFound
        {
            get { return planReportFound; }
        }
        public bool doseCheckReportIsFound
        {
            get { return doseCheckReportFound; }
        }
        public bool positionReportIsFound
        {
            get { return positionReportFound; }
        }
        public bool advancedUserMode
        {
            get { return _advancedUserMode; }
            set { _advancedUserMode = value; }
        }
        public List<OARvolume> manOARVolumes
        {
            get { return referenceManOARVolume; }
        }
        public List<OARvolume> womanOARVolumes
        {
            get { return referenceWomanOARVolume; }
        }
        public string lastUsedCheckProtocol
        {
            get { return _lastUsedCheckProtocol; }
            set { _lastUsedCheckProtocol = value; }
        }


        #endregion

    }
}
