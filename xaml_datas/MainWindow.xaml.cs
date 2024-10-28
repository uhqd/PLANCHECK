using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;
using System.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Microsoft.Office.Interop.Word;
using PlanCheck;
using PlanCheck.createWordPrefilledCheckList;
using PlanCheck.pdfreport;
using PlanCheck.Users;
using PlanCheck.xaml_datas;
//using Microsoft.Office.Tools;
//using PdfSharp.Pdf;

/*using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
*/
//using System.Windows.Forms;

// C:\WINDOWS\assembly\GAC_MSIL\
namespace PlanCheck
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        #region Variable Declarations of the class
        private PlanSetup _plan;
        private PreliminaryInformation _pinfo;
        private ScriptContext _pcontext;
        private timer myTimer;
        public string PatientFullName { get; set; }
        public string strPatientDOB { get; set; }
        public string PlanAndCourseID { get; set; }
        public string prescriptionComment { get; set; }
        public string PlanCreatorName { get; set; }
        public SolidColorBrush PlanCreatorBackgroundColor { get; set; }
        public SolidColorBrush PlanCreatorForegroundColor { get; set; }
        public SolidColorBrush sexBackgroundColor { get; set; }
        public SolidColorBrush sexForegroundColor { get; set; }
        public string machineBackgroundColor { get; set; }
        public string machineForegroundColor { get; set; }
        public string CurrentUserName { get; set; }
        public SolidColorBrush CurrentUserBackgroundColor { get; set; }
        public SolidColorBrush CurrentUserForegroundColor { get; set; }
        public string DoctorName { get; set; }
        public SolidColorBrush DoctorBackgroundColor { get; set; }
        public SolidColorBrush DoctorForegroundColor { get; set; }
        public string User { get; set; }
        public System.Windows.Media.Color UserColor { get; set; }
        public string theMachine { get; set; }
        public string theFields { get; set; }
        //public string theProtocol { get; set; }
        // public string myFullFilename { get; set; }
        public string PhotonModel { get; set; }
        public IEnumerable<string> CalculationOptions { get; set; }
        public string OptimizationModel { get; set; }
        public List<UserControl> ListChecks { get; set; }
        private bool _AplanIsloaded;
        #endregion

        public MainWindow(PreliminaryInformation pinfo, ScriptContext pcontext, bool planIsloaded) //Constructeur
        {
            _AplanIsloaded = planIsloaded;
            DataContext = this;
            //            _actualUserPreference = actualUserPreference;
            _pinfo = pinfo;
            if (_AplanIsloaded)
                _plan = pcontext.PlanSetup;
            _pcontext = pcontext;
            // myTimer = new timer();






            // an intelligent default protocol is chosen
            //myFullFilename = getIntelligentDefaultProtocol();

            // theProtocol = setProtocolDisplay(myFullFilename);//
            FillHeaderInfos(); //Filling datas binded to xaml
                               //  myTimer.durationSinceLastCall("fill header");

            //  _pinfo.lastUsedCheckProtocol = theProtocol;

            InitializeComponent(); // read the xaml
                                   //  myTimer.durationSinceLastCall("Initialize component");

            #region Fill combox with CP names, get default value as selected default value
            string folderPath = Directory.GetCurrentDirectory() + @".\plancheck_data\check_protocol";
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show(folderPath + " n'existe pas.");
            }
            string[] csvFiles = Directory.GetFiles(folderPath, "*.xlsx");
            foreach (string filePath in csvFiles)
            {
                String fileName = Path.GetFileNameWithoutExtension(filePath);
                comboCP.Items.Add(fileName);
            }

            String mydefaultPath = getIntelligentDefaultProtocol(_AplanIsloaded);
            foreach (var item in comboCP.Items)
            {
                if (mydefaultPath.Contains(item.ToString()))
                {
                    comboCP.SelectedItem = item;
                    break;
                }
            }
            #endregion


        }
        public void FillHeaderInfos()
        {
            //Patient, plan and others infos to bind to xml

            #region PATIENT NAME, SEX AND AGE
            DateTime PatientDOB = (DateTime)_pinfo.PatientDOB_dt;// .Patient.DateOfBirth;         
            DateTime zeroTime = new DateTime(1, 1, 1);
            DateTime myToday = DateTime.Today;
            TimeSpan span = myToday - _pinfo.PatientDOB_dt;
            int years = (zeroTime + span).Year - 1;
            String sex;
            if (_pcontext.Patient.Sex == "Female")
            {
                sex = "F";
                sexBackgroundColor = System.Windows.Media.Brushes.Wheat;
                sexForegroundColor = System.Windows.Media.Brushes.DeepPink;
                strPatientDOB = "Née le " + _pinfo.PatientDOB; // for tooltip only
            }
            else
            {
                sex = "H";
                sexBackgroundColor = System.Windows.Media.Brushes.Wheat;
                sexForegroundColor = System.Windows.Media.Brushes.Blue;
                strPatientDOB = "Né le " + _pinfo.PatientDOB; // for tooltip only
            }
            PatientFullName = _pinfo.PatientName + " " + sex + "/" + years.ToString();
            #endregion

            #region course and plan ID format:  PlanID (CourseID)

            PlanAndCourseID = _pinfo.PlanName + " (" + _pinfo.CourseName + ")";

            #endregion

            #region creator name

            PlanCreatorName = "    " + _pinfo.PlanCreator.UserFirstName + " " + _pinfo.PlanCreator.UserFamilyName;
            PlanCreatorBackgroundColor = _pinfo.PlanCreator.UserBackgroundColor;
            PlanCreatorForegroundColor = _pinfo.PlanCreator.UserForeGroundColor;
            #endregion

            #region User
            CurrentUserName = "    " + _pinfo.CurrentUser.UserFirstName + " " + _pinfo.CurrentUser.UserFamilyName;
            CurrentUserBackgroundColor = _pinfo.CurrentUser.UserBackgroundColor;
            CurrentUserForegroundColor = _pinfo.CurrentUser.UserForeGroundColor;
            #endregion

            #region doctor in the prescription
            if (_AplanIsloaded)
                if (_pcontext.PlanSetup.RTPrescription != null)
                {
                    DoctorName = "    " + "Dr " + _pinfo.Doctor.UserFamilyName + "    ";
                    DoctorBackgroundColor = _pinfo.Doctor.UserBackgroundColor; //System.Windows.Media.Brushes.DeepPink; // _pinfo.Doctor.DoctorBackgroundColor;
                    DoctorForegroundColor = _pinfo.Doctor.UserForeGroundColor;// System.Windows.Media.Brushes.Wheat; // _pinfo.Doctor.DoctorForeGroundColor;

                }
                else DoctorName = "    " + "Pas de prescripteur";
            else DoctorName = "    " + "Pas de prescripteur";

            #endregion

            #region prescription comment
            if (_AplanIsloaded)
                if (_pcontext.PlanSetup.RTPrescription != null)
                {
                    //prescriptionComment = _pcontext.PlanSetup.RTPrescription.Name;
                    // prescriptionComment += " (R" + _pcontext.PlanSetup.RTPrescription.RevisionNumber + "): ";

                    int nFractions = 0;
                    List<double> nDosePerFraction = new List<double>();
                    foreach (var target in _pcontext.PlanSetup.RTPrescription.Targets) //boucle sur les différents niveaux de dose de la prescription
                    {
                        nFractions = target.NumberOfFractions;
                        nDosePerFraction.Add(target.DosePerFraction.Dose);
                    }
                    string listOfDoses = nFractions.ToString() + " x " + nDosePerFraction[0].ToString("0.##");
                    for (int i = 1; i < nDosePerFraction.Count(); i++)
                        if (nDosePerFraction[i] != nDosePerFraction[i - 1])
                        {
                            //MessageBox.Show(nDosePerFraction[i].ToString("0.##"));
                            listOfDoses += "/" + nDosePerFraction[i].ToString("0.##");
                        }

                    listOfDoses += " Gy (";
                    prescriptionComment = listOfDoses;

                    if (_pcontext.PlanSetup.RTPrescription.Notes.Length == 0)
                        prescriptionComment += "Pas de commentaire dans la prescription)";
                    else
                    {
                        string noEndline = _pcontext.PlanSetup.RTPrescription.Notes.Replace("\n", "").Replace("\r", " - "); // replace newline by -
                        prescriptionComment += noEndline + ")";

                        // Just in case but revision name and number are not useful
                        //+ _pcontext.PlanSetup.RTPrescription.RevisionNumber + ": " + ": " + _pcontext.PlanSetup.RTPrescription.Id + ": " + _pcontext.PlanSetup.RTPrescription.Notes;
                        //prescriptionComment = "Commentaire de la presciption : " + _pcontext.PlanSetup.RTPrescription.Notes;
                    }
                }
                else
                    prescriptionComment = "pas de prescription";
            else
                prescriptionComment = "pas de prescription";
            #endregion

            #region machine and fields


            // int setupFieldNumber = 0;
            //int TreatmentFieldNumber = 0;

            if (_AplanIsloaded)
            {
                theMachine = "    " + _pinfo.machine;// machineName;
                if (!_pinfo.machine.Contains("TOM"))
                {
                    theFields = _pinfo.treatmentType + " : " + _pinfo.treatmentFieldNumber + " champ(s) + " + _pinfo.setupFieldNumber + " set-up";
                }
                else
                    theFields = "Tomotherapy";

            }
            else
            {
                theMachine = "   no machine";
                theFields = "   no machine";
            }

           

            #region color the machines first theme

            // see palette at https://learn.microsoft.com/fr-fr/dotnet/api/system.windows.media.brushes?view=windowsdesktop-6.0


            if (_pinfo.machine == "V4")
            {
                machineBackgroundColor = "PowderBlue";
                machineForegroundColor = "Blue";
            }
            else if (_pinfo.machine == "TOM")
            {
                machineBackgroundColor = "Orange";
                machineForegroundColor = "White";
            }
            else if (_pinfo.machine == "TOMO2")
            {
                machineBackgroundColor = "Orange";
                machineForegroundColor = "White";
            }
            else if (_pinfo.machine == "NOVA3")
            {
                machineBackgroundColor = "Green";
                machineForegroundColor = "White";
            }
            else if (_pinfo.machine == "TOMO4")
            {
                machineBackgroundColor = "Red";
                machineForegroundColor = "White";
            }
            else if (_pinfo.machine == "NOVA5")
            {
                machineBackgroundColor = "Yellow";
                machineForegroundColor = "Black";
            }
            else if (_pinfo.machine == "NOVA SBRT")
            {
                machineBackgroundColor = "Gold";
                machineForegroundColor = "Blue";
            }
            else if (_pinfo.machine == "HALCYON6")
            {
                machineBackgroundColor = "LightBlue";
                machineForegroundColor = "White";
            }
            else if (_pinfo.machine == "TOMO7")
            {
                machineBackgroundColor = "Brown";
                machineForegroundColor = "White";
            }
            else if (_pinfo.machine == "HALCYON8")
            {
                machineBackgroundColor = "DeepSkyBlue";
                machineForegroundColor = "White";
            }
            else
            {
                machineBackgroundColor = "Gray";
                machineForegroundColor = "White";
            }

            #endregion





            #endregion

            #region other infos
            //Plans infos
            if (_AplanIsloaded)
            {
                CalculationOptions = _plan.PhotonCalculationOptions.Select(e => e.Key + " : " + e.Value);
                PhotonModel = _plan.PhotonCalculationModel;
                OptimizationModel = _plan.GetCalculationModel(CalculationType.PhotonVMATOptimization);
                OptimizationModel = _plan.GetCalculationModel(CalculationType.PhotonVMATOptimization);
               

            }
            ListChecks = new List<UserControl>();
            #endregion

        }
        public void cleanList()
        {
            ListChecks.Clear();
        }
        public void AddCheck(UserControl checkScreen)
        {
            ListChecks.Add(checkScreen);
            CheckList.ItemsSource = new List<UserControl>();
            CheckList.ItemsSource = ListChecks;
        }

        /*
        private void Choose_file_button_Click(object sender, RoutedEventArgs e)
        {
            OK_button.IsEnabled = true;

            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.DefaultExt = "xlsx";
            fileDialog.InitialDirectory = Directory.GetCurrentDirectory() + @"\plancheck_data\check_protocol";

            if (!Directory.Exists(fileDialog.InitialDirectory))
            {
                MessageBox.Show(fileDialog.InitialDirectory + "n'existe pas.");
                fileDialog.InitialDirectory = @"C:\";
            }

            fileDialog.Multiselect = false;
            fileDialog.Title = "Selection du check-protocol";
            fileDialog.ShowReadOnly = true;
            fileDialog.Filter = "XLSX files (*.xlsx)|*.xlsx";
            fileDialog.FilterIndex = 0;
            fileDialog.CheckFileExists = true;
            if (fileDialog.ShowDialog() == false)
            {
                return;    // user canceled
            }
            myFullFilename = fileDialog.FileName; // full absolute path                                                  
            if (!System.IO.File.Exists(myFullFilename))
            {
                MessageBox.Show(string.Format("Le check-protocol '{0}'  n'existe pas ", theProtocol));
                return;
            }
            theProtocol = setProtocolDisplay(myFullFilename);
            defaultProtocol.Text = theProtocol; // refresh display of default value
            //_pinfo.lastUsedCheckProtocol = theProtocol;
        }
        */
        private void OK_button_click(object sender, RoutedEventArgs e)
        {
            this.cleanList();
            // OK_button.IsEnabled = false;// Visibility.Collapsed;



            String absolutePathToCP = Directory.GetCurrentDirectory() + @".\plancheck_data\check_protocol\" + comboCP.SelectedItem.ToString() + ".xlsx";
            if (!File.Exists(absolutePathToCP))
                MessageBox.Show(absolutePathToCP + " n'existe pas");

            //            read_check_protocol rcp = new read_check_protocol(myFullFilename);
            read_check_protocol rcp = new read_check_protocol(absolutePathToCP);

            _pinfo.lastUsedCheckProtocol = rcp.protocolName;


            #region User log file

            string filePath = @"\\srv015\SF_COM\SIMON_LU\userLogPlancheck\log.csv";

            using (StreamWriter writer = new StreamWriter(filePath, append: true)) // append: true pour ajouter sans écraser
            {
                writer.WriteLine(_pinfo.CurrentUser.UserFamilyName + ";" + DateTime.Today.ToString() + ";" + _pcontext.Patient.Id.ToString() + ";" + _pcontext.Patient.Name + ";" + rcp.protocolName);
            }


            #endregion


            #region PERFORM THE CHECK
            myTimer = new timer();
            myTimer.durationSinceLastCall("user click");

            #region c_course
            if (_AplanIsloaded)
            {
                Check_Course c_course = new Check_Course(_pinfo, _pcontext);
                if (c_course.Result.Count > 0)
                {
                    var check_point_course = new CheckScreen_Global(c_course.Title, c_course.Result);
                    this.AddCheck(check_point_course);
                }
            }
            myTimer.durationSinceLastCall("Check course");

            #endregion

            #region Check_previous_Treatment
            if (_AplanIsloaded)
            {
                Check_previous_Treatment c_previous_traitements = new Check_previous_Treatment(_pinfo, _pcontext);
                if (c_previous_traitements.Result.Count > 0)
                {
                    var check_point_prevTTT = new CheckScreen_Global(c_previous_traitements.Title, c_previous_traitements.Result);
                    this.AddCheck(check_point_prevTTT);
                }
            }
            myTimer.durationSinceLastCall("Previous treatments");

            #endregion

            #region Check_Prescription
            if (_AplanIsloaded)
            {
                if (_pcontext.PlanSetup.RTPrescription != null) // faire ce check seulement si il y a une prescription
                {
                    Check_Prescription c_prescri = new Check_Prescription(_pinfo, _pcontext, rcp);
                    if (c_prescri.Result.Count > 0)
                    {
                        var check_point_prescription = new CheckScreen_Global(c_prescri.Title, c_prescri.Result);
                        this.AddCheck(check_point_prescription);
                    }
                }
            }
            myTimer.durationSinceLastCall("prescription");
            #endregion

            #region c_CT
            Check_CT c_CT = new Check_CT(_pinfo, _pcontext, rcp, _AplanIsloaded);
            if (c_CT.Result.Count > 0)
            {
                var check_point_ct = new CheckScreen_Global(c_CT.Title, c_CT.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                this.AddCheck(check_point_ct);
            }
            myTimer.durationSinceLastCall("CT");

            #endregion

            #region Check_contours

            Check_contours c_Contours = new Check_contours(_pinfo, _pcontext, rcp);
            if (c_Contours.Result.Count > 0)
            {
                var check_point_contours = new CheckScreen_Global(c_Contours.Title, c_Contours.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                this.AddCheck(check_point_contours);
            }
            myTimer.durationSinceLastCall("contours");

            #endregion

            #region Check_Isocenter
            if (_AplanIsloaded)
            {
                Check_Isocenter c_Isocenter = new Check_Isocenter(_pinfo, _pcontext, rcp);
                if (c_Isocenter.Result.Count > 0)
                {
                    var check_point_iso = new CheckScreen_Global(c_Isocenter.Title, c_Isocenter.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                    this.AddCheck(check_point_iso);
                }
            }
            myTimer.durationSinceLastCall("isocenter");

            #endregion

            #region c_Plan
            if (_AplanIsloaded)
            {
                Check_Plan c_Plan = new Check_Plan(_pinfo, _pcontext, rcp);
                if (c_Plan.Result.Count > 0)
                {
                    var check_point_plan = new CheckScreen_Global(c_Plan.Title, c_Plan.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                    this.AddCheck(check_point_plan);
                }
            }
            myTimer.durationSinceLastCall("plan");

            #endregion

            #region c_algo
            if (_AplanIsloaded)
            {
                Check_Model c_algo = new Check_Model(_pinfo, _pcontext, rcp);
                if (c_algo.Result.Count > 0)
                {
                    var check_point_model = new CheckScreen_Global(c_algo.Title, c_algo.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                    this.AddCheck(check_point_model);
                }
            }
            myTimer.durationSinceLastCall("model");

            #endregion

            #region c_Beams
            if (_AplanIsloaded)
            {
                Check_beams c_Beams = new Check_beams(_pinfo, _pcontext, rcp); //
                if (c_Beams.Result.Count > 0)
                {
                    var check_point_beams = new CheckScreen_Global(c_Beams.Title, c_Beams.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                    this.AddCheck(check_point_beams);
                }
            }
            myTimer.durationSinceLastCall("beams");

            #endregion

            #region c_UM
            if (_AplanIsloaded)
            {
                Check_UM c_UM = new Check_UM(_pinfo, _pcontext, rcp);
                if (c_UM.Result.Count > 0)
                {
                    var check_point_um = new CheckScreen_Global(c_UM.Title, c_UM.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                    this.AddCheck(check_point_um);
                }
            }
            myTimer.durationSinceLastCall("UM");

            #endregion

            #region Check_doseDistribution
            if (_AplanIsloaded)
            {
                Check_doseDistribution c_doseDistribution = new Check_doseDistribution(_pinfo, _pcontext, rcp);
                if (c_doseDistribution.Result.Count > 0)
                {
                    var check_point_dose_distribution = new CheckScreen_Global(c_doseDistribution.Title, c_doseDistribution.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                    this.AddCheck(check_point_dose_distribution);
                }

            }
            myTimer.durationSinceLastCall("dose distribution");
            #endregion

            #region Check_finalisation
            if (_AplanIsloaded)
            {
                Check_finalisation c_Finalisation = new Check_finalisation(_pinfo, _pcontext, rcp);
                if (c_Finalisation.Result.Count > 0)
                {
                    var check_point_finalisation = new CheckScreen_Global(c_Finalisation.Title, c_Finalisation.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                    this.AddCheck(check_point_finalisation);
                }
            }
            myTimer.durationSinceLastCall("finalisation");

            #endregion

            #region Check_Uncheck_Test
            if (_AplanIsloaded)
            {
                Check_Uncheck_Test c_Uncheck = new Check_Uncheck_Test(_pinfo, _pcontext, rcp);
                if (c_Uncheck.Result.Count > 0)
                {
                    var check_point_uncheck = new CheckScreen_Global(c_Uncheck.Title, c_Uncheck.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                    check_point_uncheck.Visibility = Visibility.Collapsed;
                    this.AddCheck(check_point_uncheck);
                }
            }
            myTimer.durationSinceLastCall("uncheck");

            #endregion

            myTimer.close();
            #endregion


            // NO need to click on pdf button anymore --> YES if i want to reprint. 

            //exportPDF_button.Visibility = Visibility.Hidden;
            createPDFreport myPDF_report = new createPDFreport(_pinfo, _pcontext, ListChecks, this);
            string dirname = @"\\srv015\sf_com\simon_lu\temp\";
            myPDF_report.saveInDirectory(dirname);


            CheckList.Visibility = Visibility.Visible;
            createCheckListWord_button.Visibility = Visibility.Visible;
            ariaRadio_button.Visibility = Visibility.Visible;
            exportPDF_button.Visibility = Visibility.Visible;
        }
        private void Button_Click_help(object sender, RoutedEventArgs e)
        {

            #region old online user guide
            // Chrome is not installed on citrix and if hte navigator is not specified IE is launched and can't read google doc.. So,...pdf
            //System.Diagnostics.Process.Start("Chrome.exe", "https://docs.google.com/document/d/1SKk-R7JMUk4_7oHblT3idBDCOFeZwGlmQ-nVpAzsgLo");
            // System.Diagnostics.Process.Start("https://docs.google.com/document/d/1SKk-R7JMUk4_7oHblT3idBDCOFeZwGlmQ-nVpAzsgLo");


            try
            {
                System.Diagnostics.Process.Start("Chrome.exe", "https://docs.google.com/document/d/1SKk-R7JMUk4_7oHblT3idBDCOFeZwGlmQ-nVpAzsgLo");
            }
            catch
            {
                System.Diagnostics.Process.Start(@".\plancheck_data\doc\plancheckhelp.pdf");
            }
            #endregion
            #region dynamic documentation (deprecated)
            /*
            #region copy intro pdf to user guide

            Document migraDoc2 = new Document();
            Section section0 = migraDoc2.AddSection();
            String fileNameInro = @".\doc\intro-user-guide.pdf#1"; // page 1
            MigraDoc.DocumentObjectModel.Shapes.Image ima1 = section0.AddImage(fileNameInro);
            fileNameInro = @".\doc\intro-user-guide.pdf#2"; // page2 
            section0.AddImage(fileNameInro);
            fileNameInro = @".\doc\intro-user-guide.pdf#3"; //Page 3
            section0.AddImage(fileNameInro);
            fileNameInro = @".\doc\intro-user-guide.pdf#4"; //Page 4
            section0.AddImage(fileNameInro);
            #endregion

            #region pdf body
            Section section = migraDoc2.AddSection();
            section.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Portrait;


            // Header of the table
            Table table = new Table();
            table.Borders.Width = 1;
            table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.White;
            table.AddColumn(Unit.FromCentimeter(6));
            table.AddColumn(Unit.FromCentimeter(10));
            Row row = table.AddRow();
            Cell cell = row.Cells[0];
            cell.AddParagraph("Explications des checks");
            section.Add(table);

            Paragraph paragraph2 = section.AddParagraph("\n\n");
            paragraph2.AddFormattedText("\n", TextFormat.Bold);
            foreach (CheckScreen_Global csg in ListChecks)
            {


                Paragraph paragraph1 = section.AddParagraph("\n\n" + csg._title + "\n\n");
                paragraph1.Format.Font.Bold = true;
                paragraph1.Format.Font.Size = 14;

                Table table1 = new Table();
                table1.Borders.Width = 1;
                table1.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Olive;

                table1.AddColumn(Unit.FromCentimeter(4.0));
                table1.AddColumn(Unit.FromCentimeter(14.0));
                // table1.AddColumn(Unit.FromCentimeter(10.0));
                row = table1.AddRow();
                row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.PaleGoldenrod;
                row.Format.Font.Size = 8;
                row.Format.Font.Bold = true;

                cell = row.Cells[0];
                cell.AddParagraph("Item");
                cell = row.Cells[1];
                cell.AddParagraph("Explication détaillée");doc\


                foreach (Item_Result ir in csg.Items)
                {
                    row = table1.AddRow();
                    row.Format.Font.Size = 6;

                    row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AntiqueWhite;
                    row.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;



                    row.Cells[0].AddParagraph("\n\n" + ir.Label + "\n\n");
                    row.Cells[1].AddParagraph("\n\n" + ir.Infobulle + "\n\n");
//                    row.Cells[1].AddParagraph("\n\n" + ir.detailedExplanation + "\n\n");

                }
                section.Add(table1);
                section.AddPageBreak();

            }
            #endregion

            #region PDF SAVING

            PdfDocumentRenderer pdfRenderer2 = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.None);
            string pdfFile = @".\doc\";
            pdfFile += "User_Guide_" + DateTime.Now.ToString("MM.dd.yyyy_H.mm.ss") + ".pdf";
            pdfRenderer2.Document = migraDoc2;
            pdfRenderer2.RenderDocument();
            pdfRenderer2.PdfDocument.Save(pdfFile);

            #endregion
            */
            #endregion
        }

        private void preferences_button_Click(object sender, RoutedEventArgs e)
        {
            var myPrefWindow = new chooseUserPreferences(_pcontext, _pinfo); // create window
            myPrefWindow.ShowDialog(); // display window,
                                       // MessageBox.Show("yes");

        }


        private void exportPDF_button_Click(object sender, RoutedEventArgs e)
        {

            createPDFreport myPDF_report = new createPDFreport(_pinfo, _pcontext, ListChecks, this);
            string dirname = @"\\srv015\sf_com\simon_lu\temp\";
            myPDF_report.saveInDirectory(dirname);

        }
        private void createCheckListWord_button_Click(object sender, RoutedEventArgs e)
        {
            wordPrefilledCheckList wpcl = new wordPrefilledCheckList(_pinfo, _pcontext, ListChecks, this);
            string dirname = @"\\srv015\sf_com\simon_lu\temp\";


            // uncomment to save in temp/
            wpcl.saveInDirectory(dirname);
            wpcl.closeWDC();

            // uncomment to send to aria
            bool buttonState = ariaRadio_button.IsChecked.GetValueOrDefault();
            if (buttonState)
                wpcl.saveToAria();






        }
        private String setProtocolDisplay(String filename)
        {
            String protocol = "Check-protocol: ";  // theProtocol is not a file name. It s a string that display the file name with no extension
            protocol += Path.GetFileNameWithoutExtension(filename);

            return protocol;
        }
        private String getIntelligentDefaultProtocol(bool AplanIsLoaded)
        {

            String fileName = @"\plancheck_data\check_protocol\defaut.xlsx";

            if (AplanIsLoaded)
            {
                String planName = _pcontext.PlanSetup.Id.ToUpper();
                String nFractions = _pcontext.PlanSetup.NumberOfFractions.ToString();
                bool isORL = false;
                bool isCranial = false;
                String courseName = _pinfo.CourseName.ToUpper();
                if (planName.Contains("CAVUM") || planName.Contains("ORL") || planName.Contains("PHARYNX") || planName.Contains("PAROTIDE") || planName.Contains("BUC"))
                    isORL = true;

                if (planName.Contains("ASTRO") || planName.Contains("GLI"))
                    isCranial = true;


                if (planName.Contains("SEIN"))
                {
                    bool gg = false;
                    //  bool hypo = false;
                    if (planName.Contains("GG"))
                        gg = true;
                    //if (_pcontext.PlanSetup.NumberOfFractions == 15)
                    //  hypo = true;
                    if (planName.Contains("DV"))
                    {
                        fileName = @"\plancheck_data\check_protocol\seinDV.xlsx";
                    }
                    else if (gg)
                    {
                        //if (hypo)
                        //  fileName = @"\plancheck_data\check_protocol\sein ganglions hypo.xlsx";
                        // else
                        fileName = @"\plancheck_data\check_protocol\sein ganglions.xlsx";
                    }
                    else if (planName.ToUpper().Contains("DIBH"))
                    {
                        fileName = @"\plancheck_data\check_protocol\sein DIBH.xlsx";
                    }
                    else
                    {
                        //if (hypo)
                        //  fileName = @"\check_protocol\sein hypo.xlsx";
                        //else
                        fileName = @"\plancheck_data\check_protocol\sein.xlsx";
                    }


                }
                else if (isORL)
                {
                    fileName = @"\plancheck_data\check_protocol\ORL.xlsx";
                }
                else if ((planName.Contains("POUMON")) && (!planName.Contains("STEC")))
                {
                    fileName = @"\plancheck_data\check_protocol\poumon.xlsx";
                }
                else if (planName.Contains("VAGIN") || planName.Contains("VULVE") || planName.Contains("COL"))
                {
                    fileName = @"\plancheck_data\check_protocol\gynecologie.xlsx";

                }
                else if (planName.Contains("PAROI"))
                {

                    fileName = @"\plancheck_data\check_protocol\paroi ganglions.xlsx";
                }
                else if (planName.Contains("LOGE") || planName.Contains("PROST"))
                {
                    fileName = @"\plancheck_data\check_protocol\prostate.xlsx";
                }
                else if (_pinfo.isHyperArc)
                { fileName = @"\plancheck_data\check_protocol\hyperarc.xlsx"; }
                else if (planName.Contains("STIC"))
                {
                    fileName = @"\plancheck_data\check_protocol\STIC.xlsx";
                }
                else if (planName.Contains("STEC") || planName.Contains("STEREO") || planName.Contains("SBRT"))
                {
                    if (planName.Contains("FOIE"))
                    {
                        //fileName = @"\plancheck_data\check_protocol\STEC foie" + nFractions + "F.xlsx";
                        if (_pinfo.treatmentType == "VMAT")
                            fileName = @"\plancheck_data\check_protocol\STEC foie RA.xlsx";
                        else
                            fileName = @"\plancheck_data\check_protocol\STEC foie DCA.xlsx";
                    }
                    if (planName.Contains("POUM"))
                    {
                        //fileName = @"\check_protocol\STEC poumon" + nFractions + "F.xlsx";
                        if (_pinfo.treatmentType == "VMAT")
                            fileName = @"\plancheck_data\check_protocol\STEC poumon RA.xlsx";
                        else
                            fileName = @"\plancheck_data\check_protocol\STEC poumon DCA.xlsx";
                    }
                    if (planName.Contains("COTE") || planName.Contains("OS"))
                    {
                        //fileName = @"\check_protocol\STEC poumon" + nFractions + "F.xlsx";
                        if (_pinfo.nFractions == 3)
                            fileName = @"\plancheck_data\check_protocol\STEC Os 3F.xlsx";
                        else if (_pinfo.nFractions == 5)
                            fileName = @"\plancheck_data\check_protocol\STEC Os 5F.xlsx";
                    }
                    if (planName.Contains("REIN"))
                    {
                        //fileName = @"\check_protocol\STEC poumon" + nFractions + "F.xlsx";
                        if (_pinfo.nFractions == 3)
                            fileName = @"\plancheck_data\check_protocol\STEC rein 3F.xlsx";
                        else if (_pinfo.nFractions == 5)
                            fileName = @"\plancheck_data\check_protocol\STEC rein 5F.xlsx";
                    }


                }
                else if (_pinfo.treatmentType.Contains("RTC"))
                {
                    fileName = @"\plancheck_data\check_protocol\defaut RTC.xlsx";
                }
                else if (isCranial)
                {
                    fileName = @"\plancheck_data\check_protocol\intracranien-non-stereo.xlsx";
                }



            }
            String fullname = Directory.GetCurrentDirectory() + fileName;
            if (!File.Exists(fullname))
            {
                MessageBox.Show("Le check-protcol est introuvable :\n" + fullname + "\nUtilisation du fichier par défaut");
                fullname = Directory.GetCurrentDirectory() + @"\plancheck_data\check_protocol\prostate.xlsx";
            }
            if (!File.Exists(fullname))
                MessageBox.Show(fullname + "\nFichiers check-protocol introuvables");
            return fullname;
        }

        private void comboCP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            _pinfo.planIdwithoutFE = cbListPlan.SelectedValue.ToString();
            _pinfo.fondNonFEPlan = true;
            this.Close();*/
        }
    }// end class



}/// end namespace
