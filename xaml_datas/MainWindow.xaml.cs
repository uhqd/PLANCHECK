﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Microsoft.Office.Interop.Word;
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
        #region Declarations

        private PlanSetup _plan;

        private PreliminaryInformation _pinfo;

        private ScriptContext _pcontext;
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
        public string theProtocol { get; set; }
        public string myFullFilename { get; set; }
        public string PhotonModel { get; set; }
        public IEnumerable<string> CalculationOptions { get; set; }
        public string OptimizationModel { get; set; }
        public List<UserControl> ListChecks { get; set; }

        #endregion

        private String setProtocolDisplay(String filename)
        {
            String protocol = "Check-protocol: ";  // theProtocol is not a file name. It s a string that display the file name with no extension
            protocol += Path.GetFileNameWithoutExtension(filename);

            return protocol;
        }
        private String getIntelligentDefaultProtocol()
        {

            String fileName = @"\plancheck_data\check_protocol\defaut.xlsx";
            String planName = _pcontext.PlanSetup.Id.ToUpper();
            String nFractions = _pcontext.PlanSetup.NumberOfFractions.ToString();
            bool isORL = false;
            String courseName = _pinfo.CourseName.ToUpper();
            if (planName.Contains("CAVUM") || planName.Contains("ORL") || planName.Contains("PHARYNX") || planName.Contains("PAROTIDE"))
                isORL = true;


            if (courseName.Contains("CAVUM") || courseName.Contains("ORL") || courseName.Contains("PHARYNX") || courseName.Contains("PAROTIDE"))
                isORL = true;



            if (planName.Contains("SEIN"))
            {
                bool gg = false;
                //  bool hypo = false;
                if (planName.Contains("GG"))
                    gg = true;
                //if (_pcontext.PlanSetup.NumberOfFractions == 15)
                //  hypo = true;

                if (gg)
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
                fileName = @"\plancheck_data\check_protocol\ORL.xlsx";
            else if (planName.ToUpper().Contains("VAGIN") || planName.ToUpper().Contains("VULVE") || planName.ToUpper().Contains("COL"))
            {
                fileName = @"\plancheck_data\check_protocol\gyneco.xlsx";

            }
            else if (planName.Contains("PAROI"))
            {

                fileName = @"\plancheck_data\check_protocol\paroi ganglions.xlsx";
            }
            else if (planName.Contains("LOGE") || planName.Contains("PROST"))
                fileName = @"\plancheck_data\check_protocol\prostate.xlsx";
            else if (_pinfo.isHyperArc)
                fileName = @"\plancheck_data\check_protocol\hyperarc.xlsx";
            else if (planName.ToUpper().Contains("STEC"))
            {
                if (planName.ToUpper().Contains("FOIE"))
                {
                    //fileName = @"\plancheck_data\check_protocol\STEC foie" + nFractions + "F.xlsx";
                    if (_pinfo.treatmentType == "VMAT")
                        fileName = @"\plancheck_data\check_protocol\STEC foie RA.xlsx";
                    else
                        fileName = @"\plancheck_data\check_protocol\STEC foie DCA.xlsx";
                }
                if (planName.ToUpper().Contains("POUM"))
                {
                    //fileName = @"\check_protocol\STEC poumon" + nFractions + "F.xlsx";
                    if (_pinfo.treatmentType == "VMAT")
                        fileName = @"\plancheck_data\check_protocol\STEC poumon RA.xlsx";
                    else
                        fileName = @"\plancheck_data\check_protocol\STEC poumon DCA.xlsx";
                }

            }
            else if (_pinfo.treatmentType.Contains("RTC"))
            {
                fileName = @"\plancheck_data\check_protocol\defaut RTC.xlsx";
            }

            String fullname = Directory.GetCurrentDirectory() + fileName;
            if (!File.Exists(fullname))
            {
                MessageBox.Show("Le check-protcol est introuvable :\n" + fullname + "\nUtilisation du fichier par défaut : prostate");
                fullname = Directory.GetCurrentDirectory() + @"\plancheck_data\check_protocol\prostate.xlsx";
            }
            if (!File.Exists(fullname))
                MessageBox.Show(fullname + "\nFichiers check-protocol introuvables");

            return fullname;
        }
        public MainWindow(PreliminaryInformation pinfo, ScriptContext pcontext) //Constructeur
        {

            DataContext = this;
            _pinfo = pinfo;
            _plan = pcontext.PlanSetup;
            _pcontext = pcontext;


            // an intelligent default protocol is chosen
            myFullFilename = getIntelligentDefaultProtocol();

            //old : 
            // myFullFilename = Directory.GetCurrentDirectory() + @"\check_protocol\prostate.xlsx";
            theProtocol = setProtocolDisplay(myFullFilename);//
            FillHeaderInfos(); //Filling datas binded to xaml
            _pinfo.lastUsedCheckProtocol = theProtocol;


            InitializeComponent(); // read the xaml


            // fill combo box for user mode
            UserMode.Items.Add("Basique");
            UserMode.Items.Add("Avancé");
            UserMode.SelectedIndex = 1;

            // fill combo box for Check Protocol
            /*string path = Directory.GetCurrentDirectory() + @"\check_protocol\";
            String[] fileNamesWithoutExtention = Directory.GetFiles(path).Select(fileName => Path.GetFileNameWithoutExtension(fileName)).ToArray();
            foreach (string s in fileNamesWithoutExtention)
            {
                cbCheckProtocol.Items.Add(s);
            }

            cbCheckProtocol.SelectedIndex = 1;
            */

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
                //sexBackgroundColor = System.Windows.Media.Brushes.DeepPink;
                sexBackgroundColor = System.Windows.Media.Brushes.Wheat;
                // sexForegroundColor = System.Windows.Media.Brushes.White;
                sexForegroundColor = System.Windows.Media.Brushes.DeepPink;
                strPatientDOB = "Née le " + _pinfo.PatientDOB; // for tooltip only
            }
            else
            {
                sex = "H";
                //sexBackgroundColor = System.Windows.Media.Brushes.Blue;
                //sexForegroundColor = System.Windows.Media.Brushes.White;
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
            if (_pcontext.PlanSetup.RTPrescription != null)
            {
                DoctorName = "    " + "Dr " + _pinfo.Doctor.UserFamilyName + "    ";
                DoctorBackgroundColor = _pinfo.Doctor.UserBackgroundColor; //System.Windows.Media.Brushes.DeepPink; // _pinfo.Doctor.DoctorBackgroundColor;
                DoctorForegroundColor = _pinfo.Doctor.UserForeGroundColor;// System.Windows.Media.Brushes.Wheat; // _pinfo.Doctor.DoctorForeGroundColor;

            }
            else DoctorName = "    " + "Pas de prescripteur";
            #endregion

            #region prescription comment
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
                /*
                 
d0.ToString("0.##");   //24.15
d1.ToString("0.##");   //24.16 (rounded up)
d2.ToString("0.##");   //24.1  
d3.ToString("0.##");   //24
                 */
                listOfDoses += " Gy (";
                prescriptionComment = listOfDoses;

                if (_pcontext.PlanSetup.RTPrescription.Notes.Length == 0)
                    prescriptionComment += "Pas de commentaire dans la presciption)";
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
            #endregion

            #region machine and fields


            // int setupFieldNumber = 0;
            //int TreatmentFieldNumber = 0;


            theMachine = "    " + _pinfo.machine;// machineName;


            if (!_pinfo.machine.Contains("TOM"))
            {
                theFields = _pinfo.treatmentType + " : " + _pinfo.treatmentFieldNumber + " champ(s) + " + _pinfo.setupFieldNumber + " set-up";
            }
            else
                theFields = "Tomotherapy";

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
            CalculationOptions = _plan.PhotonCalculationOptions.Select(e => e.Key + " : " + e.Value);
            PhotonModel = _plan.PhotonCalculationModel;
            OptimizationModel = _plan.GetCalculationModel(CalculationType.PhotonVMATOptimization);
            OptimizationModel = _plan.GetCalculationModel(CalculationType.PhotonVMATOptimization);
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
        private void Choose_file_button_Click(object sender, RoutedEventArgs e)
        {
            OK_button.IsEnabled = true;

            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.DefaultExt = "xlsx";
            fileDialog.InitialDirectory = Directory.GetCurrentDirectory() + @"\plancheck_data\check_protocol\";

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
            //theProtocol = "Check-protocol: " + Path.GetFileNameWithoutExtension(myFullFilename);// a method to get the file name only (no extension)
            defaultProtocol.Text = theProtocol; // refresh display of default value

            _pinfo.lastUsedCheckProtocol = theProtocol;
        }
        private void OK_button_click(object sender, RoutedEventArgs e)
        {
            this.cleanList();
            OK_button.IsEnabled = false;// Visibility.Collapsed;
            exportPDF_button.Visibility = Visibility.Visible;
            createCheckListWord_button.Visibility = Visibility.Visible;
            read_check_protocol rcp = new read_check_protocol(myFullFilename);


            #region THE CHECKS

            Check_Course c_course = new Check_Course(_pinfo, _pcontext);
            if (c_course.Result.Count > 0)
            {
                var check_point_course = new CheckScreen_Global(c_course.Title, c_course.Result);
                this.AddCheck(check_point_course);
            }

            if (_pcontext.PlanSetup.RTPrescription != null) // faire ce check seulement si il y a une prescription
            {
                Check_Prescription c_prescri = new Check_Prescription(_pinfo, _pcontext, rcp);
                if (c_prescri.Result.Count > 0)
                {
                    var check_point_prescription = new CheckScreen_Global(c_prescri.Title, c_prescri.Result);
                    this.AddCheck(check_point_prescription);
                }
            }


            Check_CT c_CT = new Check_CT(_pinfo, _pcontext, rcp);
            if (c_CT.Result.Count > 0)
            {
                var check_point_ct = new CheckScreen_Global(c_CT.Title, c_CT.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                this.AddCheck(check_point_ct);
            }

            Check_contours c_Contours = new Check_contours(_pinfo, _pcontext, rcp);
            if (c_Contours.Result.Count > 0)
            {
                var check_point_contours = new CheckScreen_Global(c_Contours.Title, c_Contours.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                this.AddCheck(check_point_contours);
            }

            Check_Isocenter c_Isocenter = new Check_Isocenter(_pinfo, _pcontext);
            if (c_Isocenter.Result.Count > 0)
            {
                var check_point_iso = new CheckScreen_Global(c_Isocenter.Title, c_Isocenter.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                this.AddCheck(check_point_iso);
            }

            Check_Plan c_Plan = new Check_Plan(_pinfo, _pcontext, rcp);
            if (c_Plan.Result.Count > 0)
            {
                var check_point_plan = new CheckScreen_Global(c_Plan.Title, c_Plan.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                this.AddCheck(check_point_plan);
            }


            Check_Model c_algo = new Check_Model(_pinfo, _pcontext, rcp);
            if (c_algo.Result.Count > 0)
            {
                var check_point_model = new CheckScreen_Global(c_algo.Title, c_algo.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                this.AddCheck(check_point_model);
            }

            Check_beams c_Beams = new Check_beams(_pinfo, _pcontext, rcp);
            if (c_Beams.Result.Count > 0)
            {
                var check_point_beams = new CheckScreen_Global(c_Beams.Title, c_Beams.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                this.AddCheck(check_point_beams);
            }


            Check_UM c_UM = new Check_UM(_pinfo, _pcontext);
            if (c_UM.Result.Count > 0)
            {
                var check_point_um = new CheckScreen_Global(c_UM.Title, c_UM.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                this.AddCheck(check_point_um);
            }

            Check_doseDistribution c_doseDistribution = new Check_doseDistribution(_pinfo, _pcontext, rcp);
            if (c_doseDistribution.Result.Count > 0)
            {
                var check_point_dose_distribution = new CheckScreen_Global(c_doseDistribution.Title, c_doseDistribution.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                this.AddCheck(check_point_dose_distribution);
            }


            Check_finalisation c_Finalisation = new Check_finalisation(_pinfo, _pcontext, rcp);
            if (c_Finalisation.Result.Count > 0)
            {
                var check_point_finalisation = new CheckScreen_Global(c_Finalisation.Title, c_Finalisation.Result); // faire le Add check item direct pour mettre les bonnes couleurs de suite
                this.AddCheck(check_point_finalisation);
            }
            #endregion
            //int i = 0;

            CheckList.Visibility = Visibility.Visible;

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
        private void UserMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OK_button.IsEnabled = true;
            if (UserMode.SelectedValue.ToString() == "Basique")
                _pinfo.advancedUserMode = false;
            if (UserMode.SelectedValue.ToString() == "Avancé")
                _pinfo.advancedUserMode = true;

        }
        private void exportPDF_button_Click(object sender, RoutedEventArgs e)
        {



            MigraDoc.DocumentObjectModel.Document migraDoc = new MigraDoc.DocumentObjectModel.Document();
            MigraDoc.DocumentObjectModel.Section section = migraDoc.AddSection();
            section.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Portrait;


            #region header
            MigraDoc.DocumentObjectModel.Tables.Table table = new MigraDoc.DocumentObjectModel.Tables.Table();
            table.Borders.Width = 1;
            table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.White;
            table.AddColumn(Unit.FromCentimeter(6));
            table.AddColumn(Unit.FromCentimeter(10));

            MigraDoc.DocumentObjectModel.Tables.Row row = table.AddRow();
            MigraDoc.DocumentObjectModel.Tables.Cell cell = row.Cells[0];
            cell.AddParagraph("Patient :");
            cell = row.Cells[1];
            MigraDoc.DocumentObjectModel.Paragraph paragraph = cell.AddParagraph();
            paragraph.AddFormattedText(PatientFullName, TextFormat.Bold);


            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Oncologue :");
            cell = row.Cells[1];
            paragraph = cell.AddParagraph();
            paragraph.AddFormattedText(DoctorName, TextFormat.Bold);

            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Commentaire : ");
            cell = row.Cells[1];
            cell.AddParagraph(prescriptionComment);



            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Plan (Course) :");
            cell = row.Cells[1];
            cell.AddParagraph(PlanAndCourseID);

            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Plan créé par :");
            cell = row.Cells[1];
            paragraph = cell.AddParagraph();
            paragraph.AddFormattedText(PlanCreatorName, TextFormat.Bold);

            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Machine : ");
            cell = row.Cells[1];
            cell.AddParagraph(theMachine);

            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Technique :");
            cell = row.Cells[1];
            cell.AddParagraph(theFields);

            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Imprimé par :");
            cell = row.Cells[1];
            cell.AddParagraph(CurrentUserName);

            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Check Protocol :");
            cell = row.Cells[1];
            cell.AddParagraph(_pinfo.lastUsedCheckProtocol);



            section.Add(table);
            #endregion


            #region pdf body


            MigraDoc.DocumentObjectModel.Paragraph paragraph2 = section.AddParagraph("\n\n");
            paragraph2.AddFormattedText("\n", TextFormat.Bold);




            //string msg1 = null;
            foreach (CheckScreen_Global csg in ListChecks)
            {


                MigraDoc.DocumentObjectModel.Paragraph paragraph1 = section.AddParagraph("\n\n" + csg._title + "\n\n");
                paragraph1.Format.Font.Bold = true;
                paragraph1.Format.Font.Size = 14;

                MigraDoc.DocumentObjectModel.Tables.Table table1 = new MigraDoc.DocumentObjectModel.Tables.Table();
                table1.Borders.Width = 1;
                table1.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Olive;

                table1.AddColumn(Unit.FromCentimeter(4.0));
                table1.AddColumn(Unit.FromCentimeter(2.8));
                table1.AddColumn(Unit.FromCentimeter(10.0));
                row = table1.AddRow();
                row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.PaleGoldenrod;
                row.Format.Font.Size = 8;
                row.Format.Font.Bold = true;

                cell = row.Cells[0];
                cell.AddParagraph("Item");
                cell = row.Cells[1];
                cell.AddParagraph("Valeur du plan");
                cell = row.Cells[2];
                cell.AddParagraph("Info");

                foreach (Item_Result ir in csg.Items)
                {
                    row = table1.AddRow();
                    row.Format.Font.Size = 6;
                    //row.Shading.Color = MigraDoc.DocumentObjectModel.Color.FromCmyk(ir.ResultStatus.Item2.Color.ScB, ir.ResultStatus.Item2.Color.ScR, ir.ResultStatus.Item2.Color.ScB, 0.0);
                    if (ir.ResultStatus.Item1 == "X")
                    {
                        row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.Red;
                        row.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.AntiqueWhite;
                    }
                    if (ir.ResultStatus.Item1 == "OK")
                    {
                        row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.LightGreen;
                        row.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                    }
                    if (ir.ResultStatus.Item1 == "WARNING")
                    {
                        row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.Orange;
                        row.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.DarkBlue;
                    }
                    if (ir.ResultStatus.Item1 == "INFO")
                    {
                        row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AntiqueWhite;
                        row.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                    }


                    row.Cells[0].AddParagraph("\n\n" + ir.Label + "\n\n");


                    row.Cells[1].AddParagraph("\n\n" + ir.MeasuredValue + "\n\n");
                    row.Cells[2].AddParagraph("\n\n" + ir.Infobulle + "\n\n");

                }
                section.Add(table1);
                section.AddPageBreak();

            }
            #endregion

            #region write pdf

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.None);

            string pdfFile = @"\\srv015\sf_com\simon_lu\temp\";
            pdfFile += "PlanCheck_" + _pcontext.Patient.Id + "_" + _pcontext.Patient.LastName + "_" + _pcontext.Patient.FirstName + "_" + _pcontext.PlanSetup.Id;
            pdfFile += Path.GetFileNameWithoutExtension(myFullFilename) + "_" + DateTime.Now.ToString("MM.dd.yyyy_H.mm.ss") + ".pdf";
            pdfRenderer.Document = migraDoc;
            pdfRenderer.RenderDocument();
            MessageBox.Show("Rapport PDF sauvegardé :\n" + pdfFile);
            pdfRenderer.PdfDocument.Save(pdfFile);
            System.Diagnostics.Process.Start(pdfFile);
            #endregion

        }
        private void createCheckListWord_button_Click(object sender, RoutedEventArgs e)
        {


            Microsoft.Office.Interop.Word.Application winword = new Microsoft.Office.Interop.Word.Application();
            winword.ShowAnimation = false;
            winword.Visible = false;
            object missing = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Word.Document document = winword.Documents.Add(ref missing, ref missing, ref missing, ref missing);
            DateTime myToday = DateTime.Now;
            // header
            foreach (Microsoft.Office.Interop.Word.Section section in document.Sections)
            {
                Microsoft.Office.Interop.Word.Range headerRange = section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                headerRange.Fields.Add(headerRange, Microsoft.Office.Interop.Word.WdFieldType.wdFieldPage);
                headerRange.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                headerRange.Font.ColorIndex = Microsoft.Office.Interop.Word.WdColorIndex.wdBlue;
                headerRange.Font.Size = 15;
                headerRange.Text = "Analyse Plancheck";
            }

            //the footers into the document  
            foreach (Microsoft.Office.Interop.Word.Section wordSection in document.Sections)
            {
                //Get the footer range and add the footer details.  
                Microsoft.Office.Interop.Word.Range footerRange = wordSection.Footers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                footerRange.Font.ColorIndex = Microsoft.Office.Interop.Word.WdColorIndex.wdBlack;
                footerRange.Font.Size = 10;
                footerRange.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                string footText = "Analyse réalisée le " + myToday + " par " + _pinfo.CurrentUser.UserFirstName + " " + _pinfo.CurrentUser.UserFamilyName;
                footerRange.Text = footText;// "Footer text goes here";
            }

            //adding text to document  
            document.Content.SetRange(0, 0);
            /*string headerstring = "Patient : " + PatientFullName + Environment.NewLine;
            headerstring += "Oncologue : " + DoctorName + Environment.NewLine;
            headerstring += "Commentaire : " + prescriptionComment + Environment.NewLine;
            headerstring += "Plan (Course) : " + PlanAndCourseID + Environment.NewLine;
            headerstring += "Plan créé par : " + PlanCreatorName + Environment.NewLine;
            headerstring += "Machine : " + theMachine + Environment.NewLine;
            headerstring += "Technique : " + theFields + Environment.NewLine;
            headerstring += "Imprimé par : " + CurrentUserName + Environment.NewLine;
            string[] protocolOk = _pinfo.lastUsedCheckProtocol.Split(':');
            headerstring += "Check Protocol : " + protocolOk[1] + Environment.NewLine;
            document.Content.Text = headerstring + Environment.NewLine;
            */

            Microsoft.Office.Interop.Word.Paragraph para1 = document.Content.Paragraphs.Add(ref missing);
            para1.Range.Font.Size = 12;
            Microsoft.Office.Interop.Word.Table table1 = document.Tables.Add(para1.Range, 9, 2, ref missing, ref missing);
            table1.PreferredWidth = 300.0f;
            table1.Borders.Enable = 0;
            foreach (Microsoft.Office.Interop.Word.Row row in table1.Rows)
            {
                foreach (Microsoft.Office.Interop.Word.Cell cell in row.Cells)
                {
                    cell.Range.Font.Bold = 1;
                    cell.Range.Font.Size = 8;
                    cell.Shading.BackgroundPatternColor = WdColor.wdColorLightGreen;
                    cell.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                    cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight; 
                }
            }


            table1.Rows[1].Cells[1].Range.Text = "Patient : ";
            table1.Rows[1].Cells[2].Range.Text = PatientFullName;
            table1.Rows[2].Cells[1].Range.Text = "Oncologue : ";
            table1.Rows[2].Cells[2].Range.Text = DoctorName;
            table1.Rows[3].Cells[1].Range.Text = "Commentaire : ";
            table1.Rows[3].Cells[2].Range.Text = prescriptionComment;
            table1.Rows[4].Cells[1].Range.Text = "Plan (Course) : ";
            table1.Rows[4].Cells[2].Range.Text = PlanAndCourseID;
            table1.Rows[5].Cells[1].Range.Text = "Plan créé par : ";
            table1.Rows[5].Cells[2].Range.Text = PlanCreatorName;
            table1.Rows[6].Cells[1].Range.Text = "Machine : ";
            table1.Rows[6].Cells[2].Range.Text = theMachine;
            table1.Rows[7].Cells[1].Range.Text = "Technique : ";
            table1.Rows[7].Cells[2].Range.Text = theFields;
            table1.Rows[8].Cells[1].Range.Text = "Imprimé par : ";
            table1.Rows[8].Cells[2].Range.Text = CurrentUserName;
            string[] protocolOk = _pinfo.lastUsedCheckProtocol.Split(':');
            table1.Rows[9].Cells[1].Range.Text = "Check Protocol : ";
            table1.Rows[9].Cells[2].Range.Text = protocolOk[1];

            table1.Rows[1].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            table1.Rows[2].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            table1.Rows[3].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            table1.Rows[4].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            table1.Rows[5].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            table1.Rows[6].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            table1.Rows[7].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            table1.Rows[8].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            table1.Rows[9].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            
            ///////////////////////
            
            Microsoft.Office.Interop.Word.Paragraph para2 = document.Content.Paragraphs.Add(ref missing);
            para2.Range.Text = "Points vérifiés par Plancheck";
            para2.Range.InsertParagraphAfter();
            Microsoft.Office.Interop.Word.Table table2 = document.Tables.Add(para1.Range, 1, 3, ref missing, ref missing);
            table1.Rows[1].Cells[1].Range.Text = "Test";
            table1.Rows[1].Cells[2].Range.Text = "Résultats";
            table1.Rows[1].Cells[3].Range.Text = "Check";


            //            table2.Rows[1].Cells[1].Range.Text = "Patient : ";
            foreach (CheckScreen_Global csg in ListChecks)
            {


                foreach (Item_Result ir in csg.Items)
                {
                    if (ir.ResultStatus.Item1 == "OK")
                    { 
                    table1.Rows.Add(ir);
                    
                    }
                }
            }


                        /*foreach (Microsoft.Office.Interop.Word.Row row in table1.Rows)
                        {
                            foreach (Microsoft.Office.Interop.Word.Cell cell in row.Cells)
                            {
                                //Header row  
                                if (cell.RowIndex == 1)
                                {
                                    cell.Range.Text = "Column " + cell.ColumnIndex.ToString();
                                    cell.Range.Font.Bold = 1;
                                    //other format properties goes here  
                                    cell.Range.Font.Name = "verdana";
                                    cell.Range.Font.Size = 10;
                                    //cell.Range.Font.ColorIndex = WdColorIndex.wdGray25;                              
                                    cell.Shading.BackgroundPatternColor = WdColor.wdColorGray25;
                                    //Center alignment for the Header cells  
                                    cell.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                                    cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;

                                }
                                //Data row  
                                else
                                {
                                    cell.Range.Text = (cell.RowIndex - 2 + cell.ColumnIndex).ToString();
                                }
                            }
                        }*/

                        //Add paragraph with Heading 1 style  
                        /*            Microsoft.Office.Interop.Word.Paragraph para1 = document.Content.Paragraphs.Add(ref missing);
                                    para1.Range.Font.Size = 8;
                                    para1.SpaceAfter = 10.0f;
                                    para1.SpaceBefore = 10.0f;
                                    //object styleHeading1 = "Heading 1";
                                   // para1.Range.set_Style(styleHeading1);
                                    para1.Range.Text = headerstring;
                                    //para1.Range.InsertParagraphAfter();
                        */
                        //Add paragraph with Heading 2 style  
                        //Microsoft.Office.Interop.Word.Paragraph para2 = document.Content.Paragraphs.Add(ref missing);
                        //object styleHeading2 = "Heading 2";
                        //para2.Range.set_Style(ref styleHeading2);
                        //para2.Range.Text = "Para 2 text";
                        //para2.Range.InsertParagraphAfter();

                        //Create a 5X5 table and insert some dummy record  
                        /* Microsoft.Office.Interop.Word.Table table1 = document.Tables.Add(para1.Range, 5, 5, ref missing, ref missing);

                         table1.Borders.Enable = 1;
                         foreach (Microsoft.Office.Interop.Word.Row row in table1.Rows)
                         {
                             foreach (Microsoft.Office.Interop.Word.Cell cell in row.Cells)
                             {
                                 //Header row  
                                 if (cell.RowIndex == 1)
                                 {
                                     cell.Range.Text = "Column " + cell.ColumnIndex.ToString();
                                     cell.Range.Font.Bold = 1;
                                     //other format properties goes here  
                                     cell.Range.Font.Name = "verdana";
                                     cell.Range.Font.Size = 10;
                                     //cell.Range.Font.ColorIndex = WdColorIndex.wdGray25;                              
                                     cell.Shading.BackgroundPatternColor = WdColor.wdColorGray25;
                                     //Center alignment for the Header cells  
                                     cell.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                                     cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;

                                 }
                                 //Data row  
                                 else
                                 {
                                     cell.Range.Text = (cell.RowIndex - 2 + cell.ColumnIndex).ToString();
                                 }
                             }
                         }*/

                        //Save the document  
                        string textfilename = @"\\srv015\sf_com\simon_lu\temp\";
            textfilename += myToday.ToString();
            textfilename += "_temp1.docx";
            textfilename = textfilename.Replace(":", "_");
            textfilename = textfilename.Replace(" ", "_");
            textfilename = textfilename.Replace("/", "_");
            MessageBox.Show(textfilename);
            object filename = textfilename;
            document.SaveAs2(ref filename);
            document.Close(ref missing, ref missing, ref missing);
            document = null;
            winword.Quit(ref missing, ref missing, ref missing);
            winword = null;
            //  MessageBox.Show("Document created successfully !");

        }

        /* private void createCheckListWord_button_Click(object sender, RoutedEventArgs e)
     {

         Document migraDoc = new Document();
         Section section = migraDoc.AddSection();
         section.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Portrait;


         #region header
         Table table = new Table();
         table.Borders.Width = 1;
         table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.White;
         table.AddColumn(Unit.FromCentimeter(6));
         table.AddColumn(Unit.FromCentimeter(10));

         Row row = table.AddRow();
         Cell cell = row.Cells[0];
         cell.AddParagraph("Patient :");
         cell = row.Cells[1];
         Paragraph paragraph = cell.AddParagraph();
         paragraph.AddFormattedText(PatientFullName, TextFormat.Bold);


         row = table.AddRow();
         cell = row.Cells[0];
         cell.AddParagraph("Oncologue :");
         cell = row.Cells[1];
         paragraph = cell.AddParagraph();
         paragraph.AddFormattedText(DoctorName, TextFormat.Bold);

         row = table.AddRow();
         cell = row.Cells[0];
         cell.AddParagraph("Commentaire : ");
         cell = row.Cells[1];
         cell.AddParagraph(prescriptionComment);



         row = table.AddRow();
         cell = row.Cells[0];
         cell.AddParagraph("Plan (Course) :");
         cell = row.Cells[1];
         cell.AddParagraph(PlanAndCourseID);

         row = table.AddRow();
         cell = row.Cells[0];
         cell.AddParagraph("Plan créé par :");
         cell = row.Cells[1];
         paragraph = cell.AddParagraph();
         paragraph.AddFormattedText(PlanCreatorName, TextFormat.Bold);

         row = table.AddRow();
         cell = row.Cells[0];
         cell.AddParagraph("Machine : ");
         cell = row.Cells[1];
         cell.AddParagraph(theMachine);

         row = table.AddRow();
         cell = row.Cells[0];
         cell.AddParagraph("Technique :");
         cell = row.Cells[1];
         cell.AddParagraph(theFields);

         row = table.AddRow();
         cell = row.Cells[0];
         cell.AddParagraph("Imprimé par :");
         cell = row.Cells[1];
         cell.AddParagraph(CurrentUserName);

         row = table.AddRow();
         cell = row.Cells[0];
         cell.AddParagraph("Check Protocol :");
         cell = row.Cells[1];
         cell.AddParagraph(_pinfo.lastUsedCheckProtocol);



         section.Add(table);
         #endregion


         #region pdf body


         Paragraph paragraph2 = section.AddParagraph("\n\n");
         paragraph2.AddFormattedText("\n", TextFormat.Bold);


         Paragraph paragraph1 = section.AddParagraph("\n\n" + "Liste des points vérifiés par Plancheck" + "\n\n");
         paragraph1.Format.Font.Bold = true;
         paragraph1.Format.Font.Size = 14;

         Table table1 = new Table();
         table1.Borders.Width = 1;
         table1.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Olive;

         table1.AddColumn(Unit.FromCentimeter(4.0));
         table1.AddColumn(Unit.FromCentimeter(10));
         table1.AddColumn(Unit.FromCentimeter(2.0));

         row = table1.AddRow();
         row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.PaleGoldenrod;
         row.Format.Font.Size = 8;
         row.Format.Font.Bold = true;

         cell = row.Cells[0];
         cell.AddParagraph("Item");
         cell = row.Cells[1];
         cell.AddParagraph("Explication");
         cell = row.Cells[2];
         cell.AddParagraph("Check");

         //string msg1 = null;
         foreach (CheckScreen_Global csg in ListChecks)
         {


             foreach (Item_Result ir in csg.Items)
             {
                 if (ir.ResultStatus.Item1 == "OK")
                 {
                     row = table1.AddRow();
                     row.Format.Font.Size = 6;
                     row.Cells[0].AddParagraph("\n" + ir.Label + "\n");
                     row.Cells[1].AddParagraph("\n" + ir.MeasuredValue + "\n" + ir.Infobulle + "\n\n");
                     Paragraph paragraph8 = row.Cells[2].AddParagraph("");
                     paragraph8.Format.Font.Size = 14; 
                     paragraph8.AddFormattedText("\u00fe", new Font("Wingdings"));


                     //paragraph.AddFormattedText(value ? "\u00fe" : "\u00A8", new Font("Wingdings"));


                 }



             }
         }
         section.Add(table1);
         //   section.AddPageBreak();


         #endregion

         #region write pdf

         PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.None);

         string pdfFile = @"\\srv015\sf_com\simon_lu\temp\test.pdf";
//            pdfFile += "PlanCheck_" + _pcontext.Patient.Id + "_" + _pcontext.Patient.LastName + "_" + _pcontext.Patient.FirstName + "_" + _pcontext.PlanSetup.Id;
//          pdfFile += Path.GetFileNameWithoutExtension(myFullFilename) + "_" + DateTime.Now.ToString("MM.dd.yyyy_H.mm.ss") + ".pdf";
         pdfRenderer.Document = migraDoc;
         pdfRenderer.RenderDocument();
         MessageBox.Show("Rapport PDF sauvegardé :\n" + pdfFile);
         pdfRenderer.PdfDocument.Save(pdfFile);
         System.Diagnostics.Process.Start(pdfFile);
         #endregion





     }

     */

        /*
         private void CheckProtocol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserMode.SelectedValue.ToString() == "Basique")
                _pinfo.advancedUserMode = false;
            if (UserMode.SelectedValue.ToString() == "Avancé")
                _pinfo.advancedUserMode = true;

        }
        */
    }
}
