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
using PlanCheck.xaml_datas;
using System.Globalization;


namespace PlanCheck
{
    public class PreliminaryInformation
    {
        private ScriptContext _ctx;
        private bool _aplanisloaded;
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
        private static bool myPlanReportIsFound;
        private string[] _POoptions;
        private bool _TOMO;
        private bool _NOVA;
        private bool _HALCYON;
        private bool _SRS;
        private bool _HYPERARC;
        private bool _isModulated;
        private bool _isFE;
        private int _nFraction;
        private string _machine;
        private string _planIdwithoutFE;
        private bool _findNonFEplan;
        private planningPdfReportReader _tprd;
        private bool _dosecheckIsNeeded;
        private List<DateTime> dosimetrie = new List<DateTime>();
        private List<DateTime> dosecheck = new List<DateTime>();
        private List<DateTime> ficheDePosition = new List<DateTime>();
        private List<DateTime> autres = new List<DateTime>();

        private bool planReportFound;
        private bool doseCheckReportFound;
        private bool positionReportFound;
        private int returnCode;
        private string SEA_planName;

        public string _lastUsedCheckProtocol;
        public static List<OARvolume> referenceManOARVolume;//= new List<OARvolume>();
        public static List<OARvolume> referenceWomanOARVolume;//= new List<OARvolume>();
        private User_preference _actualUserPreference;
        public int nAriaDocumentExterieur;
        private double myXcenter;
        private int nLoalisationHA;
        private String eclipseReportMessage;

        private DateTime convertToDateTime(String dateString) // string to Date
        {
            DateTime dt;

            var cultureInfo = new CultureInfo("fr-FR");
            DateTime.TryParseExact(dateString, "dddd d MMMM yyyy HH:mm:ss", cultureInfo, DateTimeStyles.None, out dt);

            return dt;

        }

        private Structure isExistAndNotEmpty(String id)
        {

            bool isok = false;
            Structure s = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper() == id.ToUpper());
            if (s != null)
                if (!s.IsEmpty)
                    isok = true;

            if (isok)
                return s;
            else
                return null;

        } // check if a sting is a non empty structure

        public double getXcenter()
        {
            double xCenter = 0.0;

            Structure centralStruct = isExistAndNotEmpty("CHIASMA");


            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("CANAL MED");

            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("RECTUM");

            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("VESSIE");

            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("CERVEAU");

            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("TRONC CEREBRAL");

            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("PROSTATE");

            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("HYPOPHYSE");

            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("BODY");

            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("CONTOUR EXTERNE");

            if (centralStruct != null)
                xCenter = centralStruct.MeshGeometry.Bounds.X + (centralStruct.MeshGeometry.Bounds.SizeX / 2.0);


            return xCenter;
        }  // get X center of CT, based on central OARs (brain...)

        public bool isARecentDate(DateTime t) // check if Today - a DateTime  < 30 days
        {
            int recent = 30;   // number of days 
            bool returnBool = false;
            DateTime myToday = DateTime.Today;
            int nDays = (myToday - t).Days;



            if (nDays > recent)//||(nDays < 0)) // some old tomo report have a date > year 2050
                returnBool = false;
            else
                returnBool = true;
            return returnBool;

        }
        public int isTheCorrectPlanReport(String response_details)  // 0 not a tomo/eclipse report, 1 tomo report not the good one, 2 the good tomo report, 3 eclipse not good one,4 eclipse good one
        {


            String saveFilePathTemp = Directory.GetCurrentDirectory() + @"\__temp__.pdf";
            int startBinary = response_details.IndexOf("\"BinaryContent\"") + 17;
            int endBinary = response_details.IndexOf("\"Certifier\"") - 2;
            string binaryContent2 = response_details.Substring(startBinary, endBinary - startBinary);
            binaryContent2 = binaryContent2.Replace("\\", "");  // the \  makes the string a non valid base64 string                       
            File.WriteAllBytes(saveFilePathTemp, Convert.FromBase64String(binaryContent2));
            planningPdfReportReader _tempTprd = new planningPdfReportReader(saveFilePathTemp);

            if ((_tempTprd.itIsATomoReport == false) && (_tempTprd.itIsAEclipsePlanReport == false))
                returnCode = 0;
            else if (_tempTprd.itIsATomoReport == true)
            {
                _ctx.PlanSetup.DoseValuePresentation = DoseValuePresentation.Absolute; // set dose value to absolute presentation
                double planDoseMax = _ctx.PlanSetup.Dose.DoseMax3D.Dose;

                if (Math.Abs(_tempTprd.Trd.maxDose - planDoseMax) < 0.11) // < 0.11 Gy
                {
                    returnCode = 2;
                    _tprd = _tempTprd;
                }
                else
                    returnCode = 1;

            }
            else if (_tempTprd.itIsAEclipsePlanReport == true)
            {
                try
                {
                    DateTime paDate = convertToDateTime(_ctx.PlanSetup.PlanningApprovalDate);




                    if (_tempTprd.Erd.approDate == paDate)
                    {
                        // MessageBox.Show("ok  " + _ctx.PlanSetup.PlanningApprovalDate.ToString() + " " + _tempTprd.Erd.approDate.ToString());

                        myPlanReportIsFound = true;
                        returnCode = 4;
                        eclipseReportMessage = "ok";

                    }
                    else
                    {
                        //  MessageBox.Show(" pas ok  " + _ctx.PlanSetup.PlanningApprovalDate.ToString() + " " + _tempTprd.Erd.approDate.ToString());


                        returnCode = 3;
                        eclipseReportMessage = "Date d'approbation du plan différente de la date d'approbation du plan dans le rapport de Dosi.";
                    }
                }
                catch { returnCode = 3; }
            }

            File.Delete(saveFilePathTemp);
            return returnCode;
        }
        public String connectToAriaDocuments(ScriptContext ctx) // connect to ARIA, return request response
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
        public void parseTheAriaDocuments(String response, ScriptContext ctx) // using the request response, parse the documents
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
            string doc4 = "Document extérieur";
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
                bool sendItToTrash = false;

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
                {
                    sendItToTrash = true;

                }
                #endregion

                #region  dismiss if document is marked as error
                if (!sendItToTrash)
                {
                    int IsMarkedAsErrorIndex = response_docdetails.IndexOf("IsMarkedAsError"); // TRUE = ERROR   FALSE = OK :-)
                    string isError = response_docdetails.Substring(IsMarkedAsErrorIndex + 17, 4);

                    if (isError.ToUpper().Contains("TRU"))
                    {
                        sendItToTrash = true;

                    }

                }
                #endregion

                #region dismiss if document is old (> 30days)                
                if (!sendItToTrash)
                {

                    dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(response_docdetails.Substring(dateservloc + 23, datesignloc - dateservloc - 34)) / 1000).ToLocalTime();

                    if (!isARecentDate(dtDateTime))
                    {
                        sendItToTrash = true;
                    }


                }
                #endregion

                #region dismiss if document has a useless type for plancheck
                if (!sendItToTrash)
                {
                    if ((thisDocType != doc1) && (thisDocType != doc2) && (thisDocType != doc3) && (thisDocType != doc4))
                    {
                        sendItToTrash = true;
                    }
                }
                #endregion

                #region dismiss if plan report is not ok 
                int tomoReportReturnCode = -1; // 0 not a tomo report, 1 tomo report not the good one, 2 the good tomo report


                if (!sendItToTrash)
                {
                    if ((thisDocType == doc1) && (!myPlanReportIsFound))
                    {
                        // 0 not a tomo/eclipse report, 1 tomo report not the good one, 2 the good tomo report, 3 eclipse not good one,4 eclipse good one

                        tomoReportReturnCode = isTheCorrectPlanReport(response_docdetails); // check that is a plan report and has the same dose max than the plan (tomo) or approval date (eclipse)

                        if (_TOMO)
                        {
                            if (tomoReportReturnCode != 2)
                            {
                                sendItToTrash = true;

                            }
                        }
                        else if (!_TOMO)
                        {
                            if (tomoReportReturnCode != 4)
                            {
                                sendItToTrash = true;
                            }

                        }
                    }
                }
                #endregion

                #region store index
                if (!sendItToTrash)
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
            nAriaDocumentExterieur = 0;
            for (int i = 0; i < DocTypeList.Count; i++)
            {
                //MessageBox.Show("nouveau document " + DocTypeList[i]);

                if (DocTypeList[i] == doc1) { dosimetrie.Add(DateServiceList[i]); }
                else if (DocTypeList[i] == doc2) { dosecheck.Add(DateServiceList[i]); }
                else if (DocTypeList[i] == doc3) { ficheDePosition.Add(DateServiceList[i]); }
                else if (DocTypeList[i] == doc4) { nAriaDocumentExterieur++; }
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
            /*
            documentList += "Patient: " + PatNameList[0] + " - " + ctx.Patient.Id + "\n";
           documentList += "(" + dosimetrie.Count + ") " + doc1 + ":            " + dosimetrie.DefaultIfEmpty().Max().ToString("MM/dd/yy").Replace("01/01/01", "") + "\n";
            documentList += "(" + dosecheck.Count + ") " + doc2 + ":  " + dosecheck.DefaultIfEmpty().Max().ToString("MM/dd/yy").Replace("01/01/01", "") + "\n";
            documentList += "(" + ficheDePosition.Count + ") " + doc3 + ":       " + ficheDePosition.DefaultIfEmpty().Max().ToString("MM/dd/yy").Replace("01/01/01", "") + "\n";
            documentList += "(" + autres.Count + ") " + doc3 + ":       " + autres.DefaultIfEmpty().Max().ToString("MM/dd/yy").Replace("01/01/01", "") + "\n";


            MessageBox.Show(documentList);
            */
            #endregion

        }
        private IUCT_User GetUser(string searchtype, IUCT_Users iuct_users)
        {
            string tocheck;


            if (_aplanisloaded)
            {
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
            }
            else
                tocheck = _ctx.CurrentUser.Name;


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

        public PreliminaryInformation(ScriptContext ctx, bool aPlanIsLoaded)  //Constructor
        {


            #region general info
            _ctx = ctx;
            _aplanisloaded = aPlanIsLoaded;
            _actualUserPreference = new User_preference(_ctx.CurrentUser.Id);
            myPlanReportIsFound = false;

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

            if (aPlanIsLoaded)
            {
                _coursename = ctx.Course.Id;
                _planname = ctx.PlanSetup.Id;
            }
            else
            {
                _coursename = "Pas de course chargé";
                _planname = "Pas de plan chargé";
            }
            _plancreator = GetUser("creator", iuct_users);
            _currentuser = GetUser("currentuser", iuct_users);

            if (aPlanIsLoaded)
                if (ctx.PlanSetup.RTPrescription != null)
                {
                    _doctor = GetUser("doctor", iuct_users);
                    _nFraction = (int)ctx.PlanSetup.RTPrescription.NumberOfFractions;
                }


            if (aPlanIsLoaded)
            {
                if (_ctx.PlanSetup.PhotonCalculationModel != null)
                    _algoname = ctx.PlanSetup.PhotonCalculationModel;
                else
                    _algoname = "no photon calculation model";
                _mlctype = Check_mlc_type(ctx.PlanSetup);
                int n = ctx.PlanSetup.GetCalculationOptions("PO_15605New").Values.Count;
                _POoptions = new string[n];
                _POoptions = ctx.PlanSetup.GetCalculationOptions("PO_15605New").Values.ToArray();

            }
            else
            {
                _algoname = "no algo";
                _mlctype = "no MLC";
                _POoptions = null;
            }



            getOARreferenceVolumes();


            #endregion


            #region machine
            if (aPlanIsLoaded)
            {
                _machine = ctx.PlanSetup.Beams.First().TreatmentUnit.Id.ToUpper();
                _NOVA = false;
                _SRS = false;
                _TOMO = false;
                _HALCYON = false;
                _HYPERARC = false;
                if (_machine.Contains("NOVA"))
                {


                    _NOVA = true;
                    String fieldname = ctx.PlanSetup.Beams.FirstOrDefault(x => x.IsSetupField == false).Id;
                    if (fieldname.Contains("HA"))
                        _HYPERARC = true;

                    Beam b1 = ctx.PlanSetup.Beams.FirstOrDefault(x => x.IsSetupField == false);

                    if (b1.Technique.Id.ToLower().Contains("srs"))
                        _SRS = true;

                    nLoalisationHA = 1;
                    if (_HYPERARC) // get number of locs
                    {
                        int index = _ctx.PlanSetup.Id.ToLower().IndexOf("locs");
                        char result = _ctx.PlanSetup.Id[index + 4];
                        try
                        {
                            nLoalisationHA = (int)Char.GetNumericValue(result);
                            //MessageBox.Show(nLoalisationHA.ToString());

                        }
                        catch
                        {
                            nLoalisationHA = 1;
                            MessageBox.Show("Plancheck n'a pas trouvé le nombre de locs dans le nom du plan: " + _ctx.PlanSetup.Id);
                        }

                    }

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
            }

            #endregion


            #region get ARIA documents infos

            if (aPlanIsLoaded)
            {
                String response = connectToAriaDocuments(ctx);

                if (response != null)
                {
                    parseTheAriaDocuments(response, ctx); // check if documents exists and get info from tomo report if needed
                }

                if ((isTOMO) && (!planReportIsFound))
                    MessageBox.Show("Pas de rapport de plan Tomotherapy dans Aria Documents\nPlanChek n'a pas trouvé un document Dosimétrie TOMO ayant la même dose max que le plan DTO");// + isTOMO.ToString() + planReportIsFound.ToString());
            }
            #endregion


            #region set initial values

            myXcenter = getXcenter();

            if (aPlanIsLoaded)
            {
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
            }
            _isModulated = false;

            if (aPlanIsLoaded)
            {
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
            }

            #region extended fluence
            if (aPlanIsLoaded)
            {
                if (_ctx.PlanSetup.Id.Contains("FE"))
                    if (!_ctx.PlanSetup.Id.ToUpper().Contains("FEMU")) // not femur
                    {
                        _isFE = true;
                        _planIdwithoutFE = _ctx.PlanSetup.Id.Split('F')[0];
                        _findNonFEplan = false;
                        foreach (PlanSetup p in _ctx.Course.PlanSetups)
                        {
                            if (p.Id == _planIdwithoutFE)
                            {
                                _findNonFEplan = true;
                            }

                        }
                        if (!_findNonFEplan)
                        {
                            // wip : open  a window to select the plan manually
                            var myChoiceWindow = new chooseNonFEplanWindow(_ctx, this); // create window
                            myChoiceWindow.ShowDialog(); // display window,
                        }

                    }
            }
            #endregion


            #region dosecheck is needed ?
            if (aPlanIsLoaded)
            {
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

                if (isNOVA && isModulated && energy.Contains("FFF"))
                    _dosecheckIsNeeded = false;
            }


            #endregion



            #region SEA plan name
            if (aPlanIsLoaded)
            {

                SEA_planName = "null";
                List<string> listSEA = new List<string>();
                if (_TOMO)
                {
                    foreach (PlanSetup p in _ctx.Course.PlanSetups)
                    {

                        if (p.Id.Contains("SEA"))
                        {
                            SEA_planName = p.Id;
                            listSEA.Add(SEA_planName);

                        }
                    }
                    if (listSEA.Count > 1)
                    {
                        var myChoiceWindow = new chooseSEA(_ctx, this); // create window
                        myChoiceWindow.ShowDialog(); // display window, 
                    }


                }
            }

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
        /* public string documentsList
         {
             get { return documentList; }
         }*/
        public DateTime PatientDOB_dt
        {
            get { return _patientdob_dt; }
        }
        public string CourseName
        {
            get { return _coursename; }
        }
        public string planIdwithoutFE
        {
            get { return _planIdwithoutFE; }
            set { _planIdwithoutFE = value; }
        }
        public bool fondNonFEPlan
        {
            get { return _findNonFEplan; }
            set { _findNonFEplan = value; }
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
        public bool isFE
        {
            get { return _isFE; }
        }
        public bool isNOVA
        {
            get { return _NOVA; }
        }
        public bool isSRS
        {
            get { return _SRS; }
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
        public planningPdfReportReader tprd
        {
            get { return _tprd; }
        }
        public bool planReportIsFound
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
        /*public int UserMode
        {
            get { return _UserMode; }
            set { _UserMode = value; }
        }*/
        public List<OARvolume> manOARVolumes
        {
            get { return referenceManOARVolume; }
        }
        public List<OARvolume> womanOARVolumes
        {
            get { return referenceWomanOARVolume; }
        }
        public double theXcenter
        {
            get { return myXcenter; }
        }
        public int nFractions
        {
            get { return _nFraction; }
        }
        public double nLocHA
        {
            get { return nLoalisationHA; }
        }
        public string lastUsedCheckProtocol
        {
            get { return _lastUsedCheckProtocol; }
            set { _lastUsedCheckProtocol = value; }
        }
        public string EclipseReportMessage
        {
            get { return eclipseReportMessage; }
            set { eclipseReportMessage = value; }
        }
        public string SEAplanName
        {
            get { return SEA_planName; }
            set { SEA_planName = value; }
        }
        public User_preference actualUserPreference
        {
            get { return _actualUserPreference; }
            set { _actualUserPreference = value; }

        }
        #endregion
    }
}

