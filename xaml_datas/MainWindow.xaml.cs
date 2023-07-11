using System;
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

using PdfSharp.Pdf;
//using System.Windows.Forms;


namespace PlanCheck
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

       

        private String setProtocolDisplay(String filename)
        {
            String protocol = "Check-protocol: ";  // theProtocol is not a file name. It s a string that display the file name with no extension
            protocol += Path.GetFileNameWithoutExtension(filename);

            return protocol;
        }
        private String getIntelligentDefaultProtocol()
        {

            String fileName = @"\check_protocol\defaut.xlsx";
            String planName = _pcontext.PlanSetup.Id.ToUpper();
            String nFractions = _pcontext.PlanSetup.NumberOfFractions.ToString();
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
                      //  fileName = @"\check_protocol\sein ganglions hypo.xlsx";
                   // else
                        fileName = @"\check_protocol\sein ganglions.xlsx";
                }
                else
                {
                    //if (hypo)
                      //  fileName = @"\check_protocol\sein hypo.xlsx";
                    //else
                        fileName = @"\check_protocol\sein.xlsx";
                }


            }
            if (planName.Contains("PAROI"))
            {

                fileName = @"\check_protocol\paroi ganglions.xlsx";
            }
            if (planName.Contains("LOGE") || planName.Contains("PROST"))
                fileName = @"\check_protocol\prostate.xlsx";

            //  String FirstFieldName = _pcontext.PlanSetup.Beams.FirstOrDefault(x => x.IsSetupField == false).Id;
            //if (FirstFieldName.Contains("HA"))
            if (_pinfo.isHyperArc)
            {

                //fileName = @"\check_protocol\hyperarc" + nFractions + "F.xlsx";
                fileName = @"\check_protocol\hyperarc.xlsx";
            }

            if (planName.ToUpper().Contains("STEC"))
            {
                if (planName.ToUpper().Contains("FOIE"))
                {
                    //fileName = @"\check_protocol\STEC foie" + nFractions + "F.xlsx";
                    if(_pinfo.treatmentType == "VMAT")
                        fileName = @"\check_protocol\STEC foie RA.xlsx";
                    else
                        fileName = @"\check_protocol\STEC foie DCA.xlsx";
                }
                if (planName.ToUpper().Contains("POUM"))
                {
                    //fileName = @"\check_protocol\STEC poumon" + nFractions + "F.xlsx";
                    if (_pinfo.treatmentType == "VMAT")
                        fileName = @"\check_protocol\STEC poumon RA.xlsx";
                    else
                        fileName = @"\check_protocol\STEC poumon DCA.xlsx";
                }

            }


            String fullname = Directory.GetCurrentDirectory() + fileName;
            if (!File.Exists(fullname))
            {
                MessageBox.Show("Le check-protcol est introuvable :\n" + fullname + "\nUtilisation du fichier par défaut : prostate");
                fullname = Directory.GetCurrentDirectory() + @"\check_protocol\prostate.xlsx";
            }
            if (!File.Exists(fullname))
                MessageBox.Show(fullname + "\nFichier introuvable");

            return fullname;
        }
        #endregion

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

           

            InitializeComponent(); // read the xaml


            // fill combo box for user mode
            UserMode.Items.Add("Basique");
            UserMode.Items.Add("Avancé");
            UserMode.SelectedIndex = 1;

            // MessageBox.Show("Componrtnt inited");

            //OK_button.color BackColor = SystemColors.ButtonFace;
            //OK_button.ForeColor = default(Color);
            //OK_button.UseVisualStyleBackColor = true;
            // OK_button.Background = SystemColors.GrayTextBrush;
            // OK_button.Foreground = SystemColors.ScrollBarBrush;
            //OK_button.Background = new SolidColorBrush(Color.FromArgb(200, 255, 50, 50));// "#FFFFFF00";// SystemColors.GrayTextBrush;// buttonface;
            //OK_button.Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 50, 50)); //SystemColors.ScrollBarBrush;
            // OK_button.
            /*OK_button.Opacity = 0.50;
            OK_button.OpacityMask = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            OK_button.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));// "#FFFFFF00";// SystemColors.GrayTextBrush;// buttonface;
            OK_button.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)); //SystemColors.ScrollBarBrush;
            HELP_button.Opacity = 1;
            HELP_button.OpacityMask = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            HELP_button.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));// "#FFFFFF00";// SystemColors.GrayTextBrush;// buttonface;
            HELP_button.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)); //SystemColors.ScrollBarBrush;
            */
            /* Verif_button.Opacity = 0.25;
             Verif_button.OpacityMask = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
             Verif_button.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));// "#FFFFFF00";// SystemColors.GrayTextBrush;// buttonface;
                                                                                             // Verif_button.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 255)); //SystemColors.ScrollBarBrush;
             Verif_button.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));//"#FFEF0E0E");// ;
             //OK_button.OverridesDefaultStyle;*/
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
            fileDialog.InitialDirectory = Directory.GetCurrentDirectory() + @"\check_protocol\";

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
        }
        private void OK_button_click(object sender, RoutedEventArgs e)
        {
            this.cleanList();
            OK_button.IsEnabled = false;// Visibility.Collapsed;
            exportPDF_button.Visibility = Visibility.Visible;
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
                System.Diagnostics.Process.Start(@".\doc\plancheckhelp.pdf");
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
                cell.AddParagraph("Explication détaillée");


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
        private void exportPDF_button_Click(object sender, RoutedEventArgs e)
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



            section.Add(table);
            #endregion


            #region pdf body


            Paragraph paragraph2 = section.AddParagraph("\n\n");
            paragraph2.AddFormattedText("\n", TextFormat.Bold);




            //string msg1 = null;
            foreach (CheckScreen_Global csg in ListChecks)
            {


                Paragraph paragraph1 = section.AddParagraph("\n\n" + csg._title + "\n\n");
                paragraph1.Format.Font.Bold = true;
                paragraph1.Format.Font.Size = 14;

                Table table1 = new Table();
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



            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.None);

            string pdfFile = @"\\srv015\sf_com\simon_lu\";
            pdfFile += "PlanCheck_" + _pcontext.Patient.Id + "_" + _pcontext.Patient.LastName + "_" + _pcontext.Patient.FirstName + "_" + _pcontext.PlanSetup.Id;
            pdfFile += Path.GetFileNameWithoutExtension(myFullFilename) + "_" + DateTime.Now.ToString("MM.dd.yyyy_H.mm.ss") + ".pdf";
            pdfRenderer.Document = migraDoc;
            pdfRenderer.RenderDocument();
            MessageBox.Show("Rapport PDF sauvegardé :\n" + pdfFile);
            pdfRenderer.PdfDocument.Save(pdfFile);
            System.Diagnostics.Process.Start(pdfFile);


        }
        private void UserMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserMode.SelectedValue.ToString() == "Basique")
               _pinfo.advancedUserMode = false;
            if (UserMode.SelectedValue.ToString() == "Avancé")
                _pinfo.advancedUserMode = true;

        }
    }
}
