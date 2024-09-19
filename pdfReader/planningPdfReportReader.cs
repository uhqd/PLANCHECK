
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Runtime.CompilerServices;
using System.Reflection;
using PlanCheck;
using PlanCheck.Users;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Navigation;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;
using System.Globalization;
using iText;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;
using System.IO;
using iText.Layout.Element;
using iText.Layout.Properties;
//using iText.Kernel.Pdf.canvas.parser.PdfTextExtractor;
//using iTextSharp;
namespace PlanCheck
{
    public class planningPdfReportReader
    {

        private tomoReportData trd;
        private eclipseReportData erd;
        private bool _itisaTomoReport;
        private bool _itIsAEclipseFXPlanReport;
        public void displayInfo()
        {
            String s = null;
            s += " Plan name: " + trd.planName;
            s += "\n Plan type: " + trd.planType;
            s += "\n MachineID: " + trd.machineNumber;
            s += "\n Machine rev: " + trd.machineRevision;
            s += "\n prescriptionMode: " + trd.prescriptionMode;
            s += "\n prescriptionTotalDose: " + trd.prescriptionTotalDose;
            s += "\n prescriptionStructure: " + trd.prescriptionStructure;

            s += "\n prescriptionDosePerFraction: " + trd.prescriptionDosePerFraction;
            s += "\n prescriptionNumberOfFraction: " + trd.prescriptionNumberOfFraction;
            s += "\n approvalStatus: " + trd.approvalStatus;
            s += "\n MUplanned: " + trd.MUplanned;
            s += "\n MUplannedPerFraction: " + trd.MUplannedPerFraction;
            MessageBox.Show(s);

            s = null;
            s += "\n Field Width: " + trd.fieldWidth;

            s += "\n isDynamic: " + trd.isDynamic;

            s += "\n pitch: " + trd.pitch;
            s += "\n modulationFactor: " + trd.modulationFactor;
            s += "\n gantryNumberOfRotation: " + trd.gantryNumberOfRotation;

            s += "\n gantryPeriod: " + trd.gantryPeriod;
            s += "\n couchTravel: " + trd.couchTravel;
            s += "\n couchSpeed: " + trd.couchSpeed;
            s += "\n redLaserXoffset: " + trd.redLaserXoffset;
            s += "\n redLaserYoffset: " + trd.redLaserYoffset;
            s += "\n redLaserZoffset: " + trd.redLaserZoffset;
            s += "\n beamOnTime: " + trd.beamOnTime;

            s += "\n deliveryMode: " + trd.deliveryMode;
            s += "\n algorithm: " + trd.algorithm;
            s += "\n resolutionCalculation: " + trd.resolutionCalculation;
            s += "\n refDose: " + trd.refDose;
            s += "\n refPointX: " + trd.refPointX;
            s += "\n refPointY: " + trd.refPointY;
            s += "\n refPointZ: " + trd.refPointZ;

            s += "\n planningMethod: " + trd.planningMethod;
            s += "\n patientName: " + trd.patientName;
            s += "\n patientFirstName: " + trd.patientFirstName;
            s += "\n patientID: " + trd.patientID;


            s += "\n HUcurve: " + trd.HUcurve;
            s += "\n approverID: " + trd.approverID;
            s += "\n patientPosition: " + trd.patientPosition;
            s += "\n originX: " + trd.originX;
            s += "\n originY: " + trd.originY;
            s += "\n originZ: " + trd.originZ;





            MessageBox.Show(s);
        }
        public tomoReportData Trd { get => trd; set => trd = value; }
        public eclipseReportData Erd { get => erd; set => erd = value; }
        public planningPdfReportReader(string pathToPdf)  //Constructor. 
        {
            _itisaTomoReport = true;
            trd = new tomoReportData();
            erd = new eclipseReportData();

            #region convert pdf 2 text file
            string outpath = Directory.GetCurrentDirectory() + @"\plancheck_data\temp\tomoReportData.txt";
            PdfReader pdfReader = new PdfReader(pathToPdf);
            PdfDocument pdfDoc = new PdfDocument(pdfReader);
            String pageContent = null;
            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                pageContent += PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);

            }

            _itIsAEclipseFXPlanReport = false;
            _itisaTomoReport = false;

            if (pageContent.Contains("Accuray"))  /// remove old tomo plan
            {
                if (!pageContent.Contains("Accuray Precision 1.1.1.0"))
                    if (!pageContent.Contains("Accuray Precision 2.0.0.1"))
                        _itisaTomoReport = true;
            }
            else if (pageContent.Contains("appportdeplanificationderadiothérapie")) // FX printer script
            {
                _itIsAEclipseFXPlanReport = true;
            }

            File.WriteAllText(outpath, pageContent);
            pdfDoc.Close();
            pdfReader.Close();
            #endregion

            if (_itisaTomoReport)
            {

                #region read text file in a list of strings
                System.IO.StreamReader file = new System.IO.StreamReader(Directory.GetCurrentDirectory() + @"\plancheck_data\temp\tomoReportData.txt");
                String line = null;
                List<string> lines = new List<string>();

                while ((line = file.ReadLine()) != null)
                {
                    lines.Add(line);

                }

                #endregion


                #region Get the infos (see ex. at the end of file)




                string[] separatingStrings = { "rev" };
                string[] separatingStrings2 = { ": " };
                string[] separatingStrings3 = { ", " };
                string[] separatingStrings4 = { "of " };
                bool planNameFound = false;  // because Plan name is several times in the file
                bool patientNameFound = false;  // because patient name is several times in the file

                //* Write pdf in message box
                /* 
                                string fulltext = "";
                                foreach(string s in lines) 
                                {
                                    fulltext += "\n" + s;
                                }
                               MessageBox.Show(fulltext);
                */

                // Utilisez StreamWriter pour écrire dans le fichier
                // StreamWriter writer = new StreamWriter(@"\\srv015\sf_com\simon_lu\tototototo.txt");

                //writer.Write(lines[0]);
                // writer.Close();




                for (int i = 0; i < lines.Count; i++)
                {

                    if ((lines[i].Contains("Plan Name:")) && (!planNameFound))
                    {
                        trd.planName = lines[i + 2];
                        planNameFound = true;
                    }

                    if (lines[i].Contains("Plan Type:"))
                        trd.planType = lines[i + 2];

                    if (lines[i].Contains("Treatment Machine"))
                    {
                        trd.machineNumber = lines[i + 1];
                        //string[]  separatingStrings = { "rev" };
                        string[] sub1 = lines[i].Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                        string[] sub2 = sub1[1].Split('/');
                        trd.machineRevision = sub2[0];
                    }

                    if (lines[i].Contains("Prescription:")) //Prescription: Median of PTV sein, 50.00 Gy
                    {
                        //string[] separatingStrings2 = { ": " };
                        string[] sub1 = lines[i].Split(separatingStrings2, System.StringSplitOptions.RemoveEmptyEntries); // Prescription ... Median of PTV sein, 50.00 Gy

                        //string[] separatingStrings3 = { ", " };
                        string[] sub2 = sub1[1].Split(separatingStrings3, System.StringSplitOptions.RemoveEmptyEntries); // Median of PTV sein ... 50.00 Gy
                        string[] sub3 = sub2[1].Split(' ');                      // 
                        trd.prescriptionMode = sub2[0];
                        trd.prescriptionTotalDose = Convert.ToDouble(sub3[0]);

                        //string[] separatingStrings4 = { "of " };
                        trd.prescriptionStructure = sub2[0].Split(separatingStrings4, System.StringSplitOptions.RemoveEmptyEntries)[1];
                        trd.prescriptionMode = sub2[0].Split(separatingStrings4, System.StringSplitOptions.RemoveEmptyEntries)[0];


                    }


                    if (lines[i].Contains("Prescribed Dose per Fraction"))
                        trd.prescriptionDosePerFraction = Convert.ToDouble(lines[i].Split(separatingStrings2, System.StringSplitOptions.RemoveEmptyEntries)[1]);

                    if (lines[i].Contains("Planned Fractions"))
                        trd.prescriptionNumberOfFraction = Convert.ToInt32(lines[i].Split(separatingStrings2, System.StringSplitOptions.RemoveEmptyEntries)[1]);

                    if (lines[i].Contains("Maximum Dose"))
                        trd.maxDose = Convert.ToDouble(lines[i].Split(separatingStrings2, System.StringSplitOptions.RemoveEmptyEntries)[1]);



                    if (lines[i].Contains("Plan Status:"))
                    {

                        //                        MessageBox.Show("i  " + lines[i]);
                        try
                        {
                            trd.approvalStatus = lines[i].Split(separatingStrings2, System.StringSplitOptions.RemoveEmptyEntries)[1];
                        }
                        catch
                        {
                            trd.approvalStatus = "non_trouvé";
                        }
                        //                      MessageBox.Show("2");
                    }


                    if (lines[i].Contains("MU per Fraction:"))
                    {
                        string[] sub2 = lines[i].Split(':');
                        string[] sub3 = sub2[1].Split('/');
                        trd.MUplanned = Convert.ToDouble(sub3[0]);
                        trd.MUplannedPerFraction = Convert.ToDouble(sub3[1]);

                    }

                    if (lines[i].Contains("Field Width")) //Field Width (cm): 5.0, Dynamic
                    {

                        string[] sub2 = lines[i].Split(':');
                        string[] sub3 = sub2[1].Split(',');
                        trd.fieldWidth = Convert.ToDouble(sub3[0]);

                        if (sub3[1].Contains("Dynamic"))
                            trd.isDynamic = true;
                        else
                            trd.isDynamic = false;
                    }

                    if (lines[i].Contains("Pitch:"))
                    {

                        string[] sub2 = lines[i].Split(':');
                        trd.pitch = Convert.ToDouble(sub2[1]);
                    }
                    if (lines[i].Contains("Modulation Factor"))
                    {

                        string[] sub2 = lines[i].Split(':');
                        string[] sub3 = sub2[1].Split('/');
                        trd.modulationFactor = Convert.ToDouble(sub3[0]);

                    }
                    if (lines[i].Contains("Gantry Period (sec)"))
                    {

                        try
                        {
                            trd.gantryPeriod = Convert.ToDouble(lines[i + 2]);
                            trd.gantryNumberOfRotation = Convert.ToDouble(lines[i + 3]);
                        }
                        catch
                        {
                            trd.gantryPeriod = 99.999;
                            trd.gantryNumberOfRotation = 99.999;
                            //MessageBox.Show("gantry period " + lines[i + 2]);
                            //MessageBox.Show("n rotation" + lines[i + 3]);

                        }
                    }
                    if (lines[i].Contains("Couch Travel (mm)"))
                    {
                        try
                        {
                            trd.couchTravel = Convert.ToDouble(lines[i + 2]);
                            trd.couchSpeed = Convert.ToDouble(lines[i + 3]);
                        }
                        catch
                        {
                            trd.couchTravel = 99.99;
                            trd.couchSpeed = 99.99;
                        }
                    }

                    //ex.: Red Lasers Offset (IECf, mm): X: -11.58 Y: -85.14 Z: -26.32

                    if (lines[i].Contains("Red Lasers Offset (IECf, mm)"))
                    {
                        string[] sub2 = lines[i].Split(separatingStrings2, System.StringSplitOptions.RemoveEmptyEntries); // split with ": "

                        string[] sub3 = sub2[2].Split(' ');
                        trd.redLaserXoffset = Convert.ToDouble(sub3[0]);

                        string[] sub4 = sub2[3].Split(' ');
                        trd.redLaserYoffset = Convert.ToDouble(sub4[0]);

                        string[] sub5 = sub2[4].Split(' ');
                        trd.redLaserZoffset = Convert.ToDouble(sub5[0]);
                    }


                    if (lines[i].Contains("Exit Only"))
                    {
                        string[] sub2 = lines[i].Split(' ');
                        int nSpace = sub2.Count();
                        trd.blockedOAR.Add(sub2[nSpace - 11]);
                    }

                    if (lines[i].Contains("Beam On Time"))
                    {
                        string[] sub2 = lines[i].Split(':');
                        trd.beamOnTime = Convert.ToDouble(sub2[1]);
                    }
                    if (lines[i].Contains("Delivery Type"))
                    {

                        trd.deliveryMode = lines[i];
                    }
                    if (lines[i].Contains("Dose Calculation Algorithm:"))//Convolution-Superposition Spacing (IECp, mm) X: 1.27 Y: 2.50 Z: 1.27
                    {
                        string[] sub2 = lines[i + 2].Split('(');
                        string[] sub2b = sub2[0].Split(' ');
                        trd.algorithm = sub2b[0];

                        string[] sub3 = lines[i + 1].Split('/');
                        string[] sub4 = sub3[1].Split(':');
                        trd.resolutionCalculation = sub4[1].Trim(); // remove space char

                        string[] sub5 = lines[i + 2].Split(separatingStrings2, System.StringSplitOptions.RemoveEmptyEntries); // split with ": "
                        string[] sub6 = sub5[1].Split(' ');
                        trd.refPointX = Convert.ToDouble(sub6[0]);
                        string[] sub7 = sub5[2].Split(' ');
                        trd.refPointY = Convert.ToDouble(sub7[0]);
                        string[] sub8 = sub5[3].Split(' ');
                        trd.refPointZ = Convert.ToDouble(sub8[0]);
                    }
                    if (lines[i].Contains("Reference Dose (Gy)"))
                    {
                        string[] sub2 = lines[i].Split(':');
                        trd.refDose = Convert.ToDouble(sub2[1]);

                    }

                    if (lines[i].Contains("Planning Method:")) //Planning Method: Classic
                    {
                        string[] sub2 = lines[i].Split(':');
                        trd.planningMethod = sub2[1];

                    }
                    if (!patientNameFound)
                        if ((lines[i].Contains("Patient Name:")) && (lines[i].Contains("Plan Name:"))) //Patient Name: TOTO, TITI; Medical ID: 123456789; Plan Name: SeinG+gg; Version: Accuray Precision 
                        {
                            patientNameFound = true;

                            string[] sub2 = lines[i].Split(';');//Patient Name: TOTO, TITI    +     Medical ID: 123456789   +   ...


                            string[] sub3 = sub2[0].Split(separatingStrings2, System.StringSplitOptions.RemoveEmptyEntries); // split with ": "
                            string[] sub4 = sub3[1].Split(separatingStrings3, System.StringSplitOptions.RemoveEmptyEntries); // split with ", "
                            trd.patientName = sub4[0];
                            trd.patientFirstName = sub4[1];

                            string[] sub5 = sub2[1].Split(separatingStrings2, System.StringSplitOptions.RemoveEmptyEntries); // split with ": "
                            trd.patientID = sub5[1];

                        }

                    if (lines[i].Contains("Density Model:"))
                    {
                        string[] sub2 = lines[i].Split(':');
                        trd.HUcurve = sub2[1];
                    }
                    if (lines[i].Contains("User ID:"))
                    {
                        string[] sub2 = lines[i].Split(':');
                        trd.approverID = sub2[1];
                    }
                    if ((lines[i].Contains("Patient Position:")) && (!lines[i].Contains("Delivery"))) //Planning Method: Classic
                    {
                        trd.patientPosition = lines[i + 2];
                    }
                    if (lines[i].Contains("Origin")) //Origin(IECp, mm) -325.000 -122.500 325.000
                    {

                        string[] sub2 = lines[i].Split(' ');
                        trd.originX = Convert.ToDouble(sub2[3]);
                        trd.originY = Convert.ToDouble(sub2[4]);
                        trd.originZ = Convert.ToDouble(sub2[5]);
                    }

                    if (lines[i].Contains("Scan Date"))
                    {

                        string[] sub2 = lines[i + 3].Split(',');
                        trd.CTDate = sub2[0];
                    }



                }

                #endregion

            }
            else if (_itIsAEclipseFXPlanReport)
            {
                #region read text file in a list of strings
                System.IO.StreamReader file = new System.IO.StreamReader(Directory.GetCurrentDirectory() + @"\plancheck_data\temp\tomoReportData.txt");
                String line = null;
                List<string> lines = new List<string>();

                while ((line = file.ReadLine()) != null)
                {
                    lines.Add(line);

                }

                #endregion
                for (int i = 0; i < lines.Count; i++)
                {

                    if ((lines[i].Contains("Appro.plan:"))) // ex. Appro.plan: KellerAudrey(vendredi5juillet202416:57:00)
                    {
                        String brutedate = lines[i];
                        string pattern = @"\(([^)]+)\)";
                        Match match = Regex.Match(brutedate, pattern);
                        // MessageBox.Show(brutedate);

                        if (!match.Success) // sometimes date is on two lines
                        {
                            brutedate = lines[i] + lines[i + 1];
                            match = Regex.Match(brutedate, pattern);
                        }



                        if (match.Success)
                        {
                            string dateTimeString = match.Groups[1].Value;
                            // La chaîne extraite ressemble à "vendredi5juillet202416:57:00"

                            // Définir une autre expression régulière pour extraire les parties de la date et de l'heure
                            //pattern = @"(\w+)(\d+)(\w+)(\d{4})(\d{2}:\d{2}:\d{2})";
                            pattern = @"(\D+)(\d+)(\D+)(\d{4})(\d{2}:\d{2}:\d{2})";
                            match = Regex.Match(dateTimeString, pattern);

                            //                            MessageBox.Show("date brute en string " + dateTimeString);

                            if (match.Success)
                            {
                                string dayOfWeek = match.Groups[1].Value;
                                string day = match.Groups[2].Value;
                                //MessageBox.Show("day is " + day);

                                string month = match.Groups[3].Value;
                                string year = match.Groups[4].Value;
                                string time = match.Groups[5].Value;
                                string formattedDateTime = $"{day} {month} {year} {time}";

                                //  MessageBox.Show("1" + formattedDateTime);


                                var cultureInfo = new CultureInfo("fr-FR");
                                var dateTimeFormat = cultureInfo.DateTimeFormat;
                                DateTime.TryParseExact(formattedDateTime, "d MMMM yyyy HH:mm:ss", dateTimeFormat, DateTimeStyles.None, out erd.approDate);
                                //MessageBox.Show("2" + erd.approDate);

                                // MessageBox.Show("Date extraite " + erd.approDate.ToString() + " en string " + formattedDateTime);
                            }
                        }
                    }
                }
            }
        }




        public bool itIsAEclipsePlanReport
        {
            get { return _itIsAEclipseFXPlanReport; }
        }
        public bool itIsATomoReport
        {
            get { return _itisaTomoReport; }
        }
    }
}






/*
 
 Exemple fichier report

Accuray Precision® Plan Overview
Plan Name:
Plan Type:
SeinG+gg
Standard
Plan Saved Date: 13 Apr 2023, 3:28:09 PM (hr:min:sec)
Treatment Machine / Serial No.: 0210462 850 MU/min, rev 173 / 
210462
Prescription: Median of PTV sein, 50.00 Gy
Prescribed Dose per Fraction (Gy): 2.00
Planned Fractions: 25
Maximum Dose (Gy): 55.18
Plan Status: Approved
MU Planned / MU per Fraction: 182554.2 / 7302.2
Field Width (cm): 5.0, Dynamic
Pitch: 0.287
Modulation Factor / Actual: 3.500 / 1.8
Gantry Period (sec):
Gantry Rotations:
25.4
19.8
Sinogram Segments: 6.6
Couch Travel (mm):
Couch Speed (mm/sec):
286.9
0.6
Red Lasers Offset (IECf, mm): X: -11.58 Y: -85.14 Z: -26.32
Beam On Time (sec): 503.0
Plan Delivery Type / Mode: Helical / IMRT
Dose Calculation Algorithm:
Dose Calculation Resolution / Type: High / Final
Convolution-Superposition Spacing (IECp, mm) X: 1.27 Y: 2.50 Z: 1.27
Size (IECp, voxel) X: 512 Y: 165 Z: 512
Reference Dose (Gy): 55.18
X: 20.3 Y: 90.0 Z: 105.4 Reference Point (IECp, mm): Tracking Method: None
Image Angles (deg):
Tracking Target VOI:
Planning Method: Classic
Patient Name:
Medical ID:
TOTO, TITI
123456789 Version: Accuray Precision 3.3.1.3 [2]
Date Of Birth: 27 Jan 1955
Gender: Female Patient Key: 409349
Plan Overview
Page 1 of 8 13 Apr 2023, 4:32:47 PM (hr:min:sec)DVH: Absolute Dose
Plan Overview
Page 2 of 8 13 Apr 2023, 4:32:47 PM (hr:min:sec)
Patient Name: TOTO, TITI; Medical ID: 123456789; Plan Name: SeinG+gg; Version: Accuray Precision 
3.3.1.3VOI List
VOI Volume 
(cm³)
Min (Gy) Mean (Gy) Max (Gy) CI nCI HI Coverage 
%
Beam Inter.
Canal Med 72.99 0.29 3.61 16.65 n/a n/a n/a n/a Exit Only
Coeur 686.06 1.50 6.61 36.06 n/a n/a n/a n/a Exit Only
Foie 1468.48 0.17 1.85 22.61 n/a n/a n/a n/a Exit Only
IVA 3.87 5.58 11.78 27.83 n/a n/a n/a n/a Allowed
Larynx 12.68 1.14 5.51 34.32 n/a n/a n/a n/a Allowed
Oesophage 25.96 1.12 10.18 39.96 n/a n/a n/a n/a Allowed
PlexusDt 10.86 0.77 4.71 8.60 n/a n/a n/a n/a Allowed
PlexusGche 11.46 0.81 40.94 53.12 n/a n/a n/a n/a Allowed
PoumonDt 1621.31 0.69 4.77 21.96 n/a n/a n/a n/a Exit Only
PoumonGche 1281.07 1.11 10.88 52.77 n/a n/a n/a n/a Exit Only
Poumons 2902.33 0.69 7.47 52.77 n/a n/a n/a n/a Allowed
Sein contro 801.01 1.94 4.60 17.66 n/a n/a n/a n/a Exit Only
TetehumeraleDte 63.78 0.27 1.49 2.90 n/a n/a n/a n/a Allowed
TetehumeraleGche 64.77 0.44 11.61 42.40 n/a n/a n/a n/a Allowed
Thyroide 17.35 9.42 27.87 52.25 n/a n/a n/a n/a Allowed
Trachee 25.71 9.69 20.24 42.74 n/a n/a n/a n/a Allowed
BODY 23567.92 0.09 8.10 55.18 n/a n/a n/a n/a Allowed
CTV cmi 11.41 46.04 50.59 55.18 n/a n/a n/a n/a Allowed
CTV sein 853.28 27.36 49.34 55.12 n/a n/a n/a n/a Allowed
CTV sous clav 36.80 48.88 50.46 53.08 n/a n/a n/a n/a Allowed
CTV sus clav 19.49 47.26 50.17 53.82 n/a n/a n/a n/a Allowed
PTV cmi 31.77 40.71 49.54 55.18 39.90 90.15 1.10 44.25 n/a
PTV sein 901.79 32.31 49.77 55.12 1.24 2.49 1.10 49.99 n/a
PTV sous clav 87.30 40.01 49.70 53.08 12.86 25.72 1.10 49.97 n/a
PTV sus clav 52.52 39.27 49.66 53.82 24.73 57.29 1.10 43.18 n/a
CanalMed+5 207.93 0.26 3.69 21.25 n/a n/a n/a n/a Exit Only
PTVTOTAL 1062.90 32.31 49.75 55.18 n/a n/a n/a n/a Allowed
RingPTVgg 322.01 10.06 39.37 54.94 n/a n/a n/a n/a Allowed
RingPTVSein 914.16 6.94 35.81 54.94 n/a n/a n/a n/a Allowed
VOI
Plan Overview
Page 3 of 8 13 Apr 2023, 4:32:47 PM (hr:min:sec)
Patient Name: Toto, titi; Medical ID: 123456789; Plan Name: SeinG+gg; Version: Accuray Precision 
3.3.1.3All Target Regions n/a 32.31 49.75 55.18 0.00 0.00 0.00 0.00 n/a
All Critical Regions n/a 0.09 8.25 55.18 0.00 0.00 0.00 0.00 n/a
Soft Tissue n/a 0.08 2.05 38.51 0.00 0.00 0.00 0.00 n/a
DVH Dose (Gy) Dose (% of 50.00 
Gy Rx Dose)
Volume (cm³) Volume (%) Criteria Value
PTV sein 47.09 94.2 856.70 95.0 Dose (Gy) >= 47.50, Min: 45.00 47.09
PTV sus clav 47.05 94.1 49.90 95.0 Dose (Gy) >= 47.50, Min: 45.00 47.05
PTV sous clav 46.30 92.6 82.94 95.0 Dose (Gy) >= 47.50, Min: 45.00 46.30
PTV cmi 45.74 91.5 30.18 95.0 Dose (Gy) >= 47.50, Min: 45.00 45.74
Canal Med 10.00 20.0 2.29 3.1 Volume (%) <= 10.0 3.1
Coeur 15.00 30.0 33.55 4.9 Volume (%) <= 5.0, Max: 10.0 4.9
Coeur 25.00 50.0 2.03 0.3 Volume (%) <= 0.0, Max: 5.0 0.3
PoumonDt 5.00 10.0 642.86 39.7 Volume (%) <= 20.0, Max: 30.0 39.7
PoumonGche 5.00 10.0 768.62 60.0 Volume (%) <= 65.0, Max: 70.0 60.0
PoumonGche 20.00 40.0 215.15 16.8 Volume (%) <= 15.0, Max: 18.0 16.8
Poumons 5.00 10.0 1411.70 48.6 Volume (%) <= 42.0, Max: 50.0 48.6
Poumons 20.00 40.0 215.32 7.4 Volume (%) <= 10.0, Max: 15.0 7.4
Sein contro 5.00 10.0 229.74 28.7 Volume (%) <= 15.0, Max: 25.0 28.7
TetehumeraleGche 42.40 84.8 0.00 0.0 Dose (Gy) <= 45.00, Max: 47.50 42.40
Canal Med 16.65 33.3 0.00 0.0 Dose (Gy) <= 15.00, Max: 18.00 16.65
CanalMed+5 21.25 42.5 0.00 0.0 Dose (Gy) <= 18.00, Max: 20.00 21.25
PTV sein 52.25 104.5 18.04 2.0 Dose (Gy) <= 53.50 52.25
PTV sus clav 52.09 104.2 1.05 2.0 Dose (Gy) <= 53.50 52.09
PTV sous clav 51.72 103.4 1.75 2.0 Dose (Gy) <= 53.50 51.72
PTV cmi 53.28 106.6 0.64 2.0 Dose (Gy) <= 53.50 53.28
CanalMed+5 18.00 36.0 0.23 0.1
Dx Vx Values
Plan Overview
Page 4 of 8 13 Apr 2023, 4:32:47 PM (hr:min:sec)
Patient Name: TOTO, TITI; Medical ID: 123456789; Plan Name: SeinG+gg; Version: Accuray Precision 
3.3.1.3HU Relative Electron Density Mass Density (g/cm³)
-939 * 0.001
-672 * 0.290
-509 * 0.480
13 * 1.000
226 * 1.146
455 * 1.331
837 * 1.558
1214 * 1.822
30884 * 22.599
Density
Density Model: IUC-120kV
Plan Overview
Page 5 of 8 13 Apr 2023, 4:32:47 PM (hr:min:sec)
Patient Name: TOTO, TITI; Medical ID: 123456789; Plan Name: SeinG+gg; Version: Accuray Precision 
3.3.1.3Name: Couarde, Laetitia
13 Apr 2023, 3:28:42 PM (hr:min:sec)
User ID: couarde
Approval Date:
Approved by: Specialists
Radiation Oncologist:
Surgeon:
Referring Physician:
Physicist:
Treatment Site ICD: IUC-TOULOUSE
IUC-TOULOUSE Treatment Site:
Additional Site Codes: IUC-TOULOUSE
Additional Sites: IUC-TOULOUSE
Created in Version: Accuray Precision 3.3.1.3 [2]
Plan Key:
Last Modified by:
419928/23
DEFOUR, Nathalie
Saved Deliverable by: DEFOUR, Nathalie
Approval and ICD
Auto Generated Results:
Not Applicable
Patient Position: HFS Delivery Signature: ujc8v HJlkA 
Plan Overview
Page 6 of 8 13 Apr 2023, 4:32:47 PM (hr:min:sec)
Patient Name: TOTO, TITI; Medical ID: 123456789; Plan Name: SeinG+gg; Version: Accuray Precision 
3.3.1.3Report Snapshots
Plan Overview
Page 7 of 8 13 Apr 2023, 4:32:47 PM (hr:min:sec)
Patient Name: TOTO, TITI; Medical ID: 123456789; Plan Name: SeinG+gg; Version: Accuray Precision 
3.3.1.3Imaging Series
X Y Z
Origin (IECp, mm) -325.000 -122.500 325.000
Size (IECp, voxel) 512 165 512
Spacing (IECp, mm) 1.27 2.50 1.27
Primary Series:
Modality:
Scan Date: Number of Slices:
Yes
CT
11 Apr 2023, 09:19:11 AM (hr:min:sec) 165
Scanner Model:
Patient Position:
Optima CT580
HFS
Series UID/Description Study UID/Description:
1.2.840.113619.2.278.3.3523880722.495.1681188143.371 1.2.840.113619.2.278.3.3523880722.495.1681188143.366
SEIN Gauche Tomo TDM therapie SPC
Plan Overview
Page 8 of 8 13 Apr 2023, 4:32:47 PM (hr:min:sec)
Patient Name: TOTO, TITI; Medical ID: 123456789; Plan Name: SeinG+gg; Version: Accuray Precision 
3.3.1.3
 
 */


/*
 Exemple de fichier eclipse
Rappportdeplanificationderadiothérapie(v15.6.05)deGxxxx
Plan:Rectum
Rapportcrééle:08/07/202408:42:19par:DeborneJustine
1/6
PATIENT
Nom:gxxxx,gxxxx(202406228) DatedeNaissance:04/12/1987 Sexe:Féminin
PLAN PRESCRIPTIONDE
DOSE
IDplan Rectum Méthodenormalisation 100.00%couvre50.00%duvolumecible
IDdossierdetraitement: C1 Volumecible: PTV_HDOPT
IDimage: TDMRTE032724 Pointdereferenceprincipal PTV_HDOPT
Systèmed'imagerie: CT130246 Valeurdenormalisation: 99.8%
Orientationdutraitement: Tetedevant-DD Pourcentagededoseprescrite 100.0%
Décalagedel'origine/àDICOM: (0.05cm,-17.20cm,-32.80cm) Fractionnement:
Typedetraitement: VMAT(RA) DosePrescrite: 50.0Gy(2.00Gy/fraction)
Nombredefractions: 25
OPTIONSDECALCUL
Algorithmephoton: AAA_15605New
CalculationGridSizeInCM 0.125
CalculationGridSizeInCMForSRSAndHyperArc 0.125
FieldNormalizationType 100%toisocenter
HeterogeneityCorrection ON
APPROBATIONS
Plancréé: admin\carillo(05/07/202415:02:49)
Appro.plan: KellerAudrey(vendredi5juillet202416:57:00)
Appro.traitement: -Rappportdeplanificationderadiothérapie(v15.6.05)degxxxx,gxxxx
Plan:Rectum
Rapportcrééle:08/07/202408:42:19par:DeborneJustine
2/6
PARAMETRESDESFAISCEAUXDETRAITEMENT(1/1)
IDchamp
RA1 RA2 RA3 RA4
IDmachine HALCYON6 HALCYON6 HALCYON6 HALCYON6
Energie 6X-FFF 6X-FFF 6X-FFF 6X-FFF
Debitdedose 740UM/min 740UM/min 740UM/min 740UM/min
X1 14 14 14 14
X2 14 14 14 14
Y1 14 14 14 14
Y2 14 14 14 14
Tailledechamp 28x28cm² 28x28cm² 28x28cm² 28x28cm²
Angledubras 179° 181° 179° 181°
Sensderotationdubras SAH SH SAH SH
Angled'arrêtdubras 181° 179° 181° 179°
Rot.collimateur 315° 45° 315° 45°
Rot.detable 0° 0° 0° 0°
Bolus - - - -
IsocentreX 0.99cm 0.99cm 0.99cm 0.99cm
IsocentreY 1.54cm 1.54cm 1.54cm 1.54cm
IsocentreZ -2.17cm -2.17cm -2.17cm -2.17cm
DSP 93.6cm 93.6cm 93.6cm 93.6cm
Ponderation 0.906 0.86 0.83 0.841
Pointderéférence PTV_HDOPT PTV_HDOPT PTV_HDOPT PTV_HDOPT
Filtreencoin - - - -
Bloc - - - -
Plaque - - - -
Doseparfraction 0.527Gy 0.500Gy 0.483Gy 0.489Gy
UM 181.7UM 172.3UM 166.4UM 168.5UMRappportdeplanificationderadiothérapie(v15.6.05)degxxxx,gxxxx
Plan:Rectum
Rapportcrééle:08/07/202408:42:19par:DeborneJustine
3/6
HISTOGRAMMEDOSEVOLUMERappportdeplanificationderadiothérapie(v15.6.05)degxxxx,gxxxx
Plan:Rectum
Rapportcrééle:08/07/202408:42:19par:DeborneJustine
4/6
STATISTIQUESHISTOGRAMMEDOSE-VOLUME(1/1)
Structure Volume(cc) DoseMax(Gy) DoseMoyenne(Gy) Dose1cc(Gy) Dose0.035cc(Gy)
BODY 18327.1 51.9 8.7 51.2 51.7
Vessie 158.1 48.9 20.7 46.3 48.4
TeteFemorGche 120.8 27.7 11.6 25.6 27.3
TeteFemorDte 120.3 27.1 10.4 23.4 26.2
Sigmoide 34.9 49.1 43.3 46.8 48.3
Sacrum 227.7 51.4 31.3 48.8 51.0
SacroiliaquesG 216.0 46.8 16.8 45.6 46.6
SacroiliaquesD 213.8 47.0 16.4 45.7 46.7
ReinGche 30.6 0.4 0.3 0.4 0.4
ReinDt 133.0 1.0 0.5 0.9 1.0
Rectum 32.3 51.3 49.7 50.7 51.1
Rate 3.0 0.4 0.4 0.4 0.4
QueueDeCheval 19.9 18.9 2.8 13.0 18.1
Pancreas 18.4 0.5 0.4 0.4 0.5
Grele 1945.9 46.8 8.2 46.1 46.5
Foie 143.5 0.5 0.2 0.4 0.5
Estomac 17.1 0.4 0.3 0.4 0.4
Duodenum 27.3 1.3 0.5 1.0 1.2
CanalAnal 4.4 47.3 36.3 45.9 46.8
CTVBD 401.9 51.8 46.5 51.1 51.6
CTVHD 69.2 51.8 50.1 51.1 51.5
GTVT 7.3 51.2 49.9 50.3 50.9
OGE 111.0 14.2 4.5 12.9 13.9
PTVBD 687.7 51.9 46.2 51.2 51.7
PTVHD 134.1 51.9 49.9 51.2 51.7Rappportdeplanificationderadiothérapie(v15.6.05)degxxxx,gxxxx
Plan:Rectum
Rapportcrééle:08/07/202408:42:19par:DeborneJustine
5/6
DECALAGESDELATABLEPARRAPPORTALAPOSITIONDEREFERENCE
Isocentredeschamps:,RA1,RA2,RA3,RA4
Décaler: -0.99cm -1.54cm -2.17cm
Direction: Tableverslagaucheenregardantlebras Tableverslehaut Tableenlongverslebras(in)Rappportdeplanificationderadiothérapie(v15.6.05)degxxxx,gxxxx
Plan:Rectum
Rapportcrééle:08/07/202408:42:19par:DeborneJustine
6/6
 
 */