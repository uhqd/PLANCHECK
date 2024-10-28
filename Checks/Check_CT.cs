using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;
using VMS.TPS.Common.Model.API;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows;
using VMS.TPS.Common.Model.Types;
using System.Windows.Navigation;
using System.Drawing;





namespace PlanCheck
{
    internal class Check_CT
    {
        public Check_CT(PreliminaryInformation pinfo, ScriptContext ctx, read_check_protocol rcp,bool aPlanIsLoaded)  //Constructor
        {

            _rcp = rcp;
            _context = ctx;
            _pinfo = pinfo;
            _AplanIsloaded = aPlanIsLoaded;
            Check();
        }

        static List<string> listOfPhases = new List<string> { "0%", "16%", "33%", "5%", "66%", "83%" };
        private List<Item_Result> _result = new List<Item_Result>();
        private PreliminaryInformation _pinfo;
        private ScriptContext _context;
        private read_check_protocol _rcp;
        private string CheckAVEMessage;
        private string _title = "CT";
        private bool _AplanIsloaded;
        //test


        private bool containsAndOnlyContains(string comment, string percentage)
        {
            bool value = false;
            string comment2 = comment.Replace("50", "5");
            int i = 0;
            foreach (string phase in listOfPhases) // ch"eck if comment conaints only phase number
            {
                if (comment2.Contains(phase))
                    i++;
            }
            if (i == 1)
                value = true;
            else
                value = false;

            if (value) // check if it is the correct one
            {
                if (comment2.Contains(percentage))
                    value = true;
                else
                    value = false;
            }
            return value;
        }

        private bool matchingImageName(string iName, string format)
        {
            // return true if the string iName contains all the part of format that are separated with a *
            bool match = false;


            string[] subStrings = format.Split(new string[] { "**" }, StringSplitOptions.None);


            foreach (string ss in subStrings)
            {
                if (!iName.Contains(ss))
                {
                    match = false;
                    break;
                }
                else
                    match = true;
            }

            return match;
        }

        private bool checkAVEcompositionSiemensCT(ScriptContext ctx, int threeOrSix)
        {
            // testing patient : 202401149

            bool is6 = false;
            bool is3 = false;

            if (threeOrSix == 3)
                is3 = true;
            if (threeOrSix == 6)
                is6 = true;
           
            bool iSChecked = true;
            String msg = String.Empty;
            double checkSumSerie00 = -99999.0;
            double checkSumSerie16 = -99999.0;
            double checkSumSerie33 = -99999.0;
            double checkSumSerie50 = -99999.0;
            double checkSumSerie66 = -99999.0;
            double checkSumSerie83 = -99999.0;
            double checkSumSerie00B = -99999.0;
            double checkSumSerie16B = -99999.0;
            double checkSumSerie33B = -99999.0;
            double checkSumSerie50B = -99999.0;
            double checkSumSerie66B = -99999.0;
            double checkSumSerie83B = -99999.0;


            double tolerance = 3.00;


            #region  AVERAGE SERIES

            int zSizeAverage = ctx.Image.ZSize;
            int xSizeAverage = ctx.Image.XSize;
            int ySizeAverage = ctx.Image.YSize;
            int centralImageIndex = (zSizeAverage / 2);
            int[,] myPlane = new int[xSizeAverage, ySizeAverage];
            ctx.Image.GetVoxels(centralImageIndex, myPlane); // get voxel to myplane
            int i1 = xSizeAverage / 2;
            int j1 = ySizeAverage / 2;

          //  MessageBox.Show("Index x y z for " + ctx.Image.Id + " " + i1 + " " + j1 + " " + centralImageIndex);
            double checkSumAvergageSerie = ctx.Image.VoxelToDisplayValue(myPlane[i1, j1]);
            double checkSumAvergageSerieB = ctx.Image.VoxelToDisplayValue(myPlane[i1 + 4, j1 + 4]);

            CheckAVEMessage += "Image Average " + threeOrSix.ToString() + " " + ctx.Image.Id + "\n";
            CheckAVEMessage += " A:\t" + checkSumAvergageSerie.ToString("F2") + "\t";
            CheckAVEMessage += " B:\t" + checkSumAvergageSerieB.ToString("F2") + "\n";

            #endregion
            // ----------------------------------------------------------------------------------------------------------------------------------
            //  ctx.Image.ZSize is the number of images of the image3D of the plan. eg 189
            // ----------------------------------------------------------------------------------------------------------------------------------
            //  ctx.Image.Series.Images.Count() is the number of images in the series used to build the image3D eg 190 : 189 images + 1 image3D
            // ----------------------------------------------------------------------------------------------------------------------------------
            //  ctx.Image.Series.Study.Series.Count() est le nombre de serie dans l'examen
            // ----------------------------------------------------------------------------------------------------------------------------------

            #region phases series

            int binaryCodeToCheckThereIsOnlyOneSeriesPerPhase = 0; // +1 if a ph0% exists, +10 if a 16% exists.... 
            bool thereIsMoreThanOneSerieForAPhase = false;
            bool thereIsOneSeriesForPhase = false;

            #region LOOKING FOR A SERIES FOR EACH PHASE AND CHECK IF THERE IS ONLY ONE
            Image im00 = null;
            Image im16 = null;
            Image im33 = null;
            Image im50 = null;
            Image im66 = null;
            Image im83 = null;

            foreach (var v in ctx.Image.Series.Study.Series) // looking for phases
            {
                if (v.Modality.ToString() == "CT")
                {

                    if (containsAndOnlyContains(v.Comment, "0%"))
                    {
                        if (is6)
                            binaryCodeToCheckThereIsOnlyOneSeriesPerPhase += 1;
                        foreach (var im in v.Images)
                        {
                            if (im.ZSize > 1) // is 3d
                            {
                                im00 = im;

                            }
                        }
                    }
                    if (containsAndOnlyContains(v.Comment, "16%"))
                    {
                        if (is6)
                            binaryCodeToCheckThereIsOnlyOneSeriesPerPhase += 10;
                        foreach (var im in v.Images)
                        {
                            if (im.ZSize > 1) // is 3d
                            {
                                im16 = im;

                            }
                        }
                    }
                    if (containsAndOnlyContains(v.Comment, "33%"))
                    {
                        if ((is6) || (is3))
                            binaryCodeToCheckThereIsOnlyOneSeriesPerPhase += 100;
                        foreach (var im in v.Images)
                        {
                            if (im.ZSize > 1) // is 3d
                            {
                                im33 = im;

                            }
                        }
                    }
                    if (containsAndOnlyContains(v.Comment, "5%"))
                    {
                        if ((is6) || (is3))
                            binaryCodeToCheckThereIsOnlyOneSeriesPerPhase += 1000;
                        foreach (var im in v.Images)
                        {
                            if (im.ZSize > 1) // is 3d
                            {
                                im50 = im;

                            }
                        }
                    }
                    if (containsAndOnlyContains(v.Comment, "66%"))
                    {
                        if ((is6) || (is3))
                            binaryCodeToCheckThereIsOnlyOneSeriesPerPhase += 10000;
                        foreach (var im in v.Images)
                        {
                            if (im.ZSize > 1) // is 3d
                            {
                                im66 = im;

                            }
                        }
                    }
                    if (containsAndOnlyContains(v.Comment, "83%"))
                    {
                        if (is6)
                            binaryCodeToCheckThereIsOnlyOneSeriesPerPhase += 100000;
                        foreach (var im in v.Images)
                        {
                            if (im.ZSize > 1) // is 3d
                            {
                                im83 = im;

                            }
                        }

                    }
                }
            }

            if (is6)
            {
                if (binaryCodeToCheckThereIsOnlyOneSeriesPerPhase != 111111)
                    thereIsMoreThanOneSerieForAPhase = true;
                else if (binaryCodeToCheckThereIsOnlyOneSeriesPerPhase == 111111)
                    thereIsOneSeriesForPhase = true;
            }
            else if (is3)
            {
                if (binaryCodeToCheckThereIsOnlyOneSeriesPerPhase != 11100)
                    thereIsMoreThanOneSerieForAPhase = true;
                else if (binaryCodeToCheckThereIsOnlyOneSeriesPerPhase == 11100)
                    thereIsOneSeriesForPhase = true;

            }
            else
                iSChecked = false;
            #endregion


            if (thereIsMoreThanOneSerieForAPhase)
            {
                thereIsOneSeriesForPhase = false;
                MessageBox.Show("Il y a plus d'une serie correspondant à une des phases. Par exemple plusieurs series en phase 0%. Merci de vérifier. ");
                // open a user window to choose
            }


            int xPhaseSize, yPhaseSize, k, m;
           //MessageBox.Show("there is one " + thereIsOneSeriesForPhase);
            if (thereIsOneSeriesForPhase)
            {

                

                if (is6)
                {
                    int[,] myPlane00 = new int[im00.XSize, im00.YSize];
                    im00.GetVoxels(centralImageIndex, myPlane00);
                    xPhaseSize = im00.XSize;
                    yPhaseSize = im00.YSize;
                    k = xPhaseSize / 2;
                    m = yPhaseSize / 2;
                    checkSumSerie00 = im00.VoxelToDisplayValue(myPlane00[k, m]);
                    checkSumSerie00B = im00.VoxelToDisplayValue(myPlane00[k + 4, m + 4]);
                    CheckAVEMessage += im00.Id + " A: " + checkSumSerie00.ToString("F2") + "\tB:" + checkSumSerie00B.ToString("F2") + "\n";
                }

                if (is6)
                {
                    int[,] myPlane16 = new int[im16.XSize, im16.YSize];
                    im16.GetVoxels(centralImageIndex, myPlane16);
                    xPhaseSize = im16.XSize;
                    yPhaseSize = im16.YSize;
                    k = xPhaseSize / 2;
                    m = yPhaseSize / 2;
                   // MessageBox.Show("Index x y z for " + im16.Id + " " + k + " " + m + " " + centralImageIndex);
                    checkSumSerie16 = im16.VoxelToDisplayValue(myPlane16[k, m]);
                    checkSumSerie16B = im16.VoxelToDisplayValue(myPlane16[k + 4, m + 4]);
                    CheckAVEMessage += im16.Id + " A: " + checkSumSerie16.ToString("F2") + "\tB:" + checkSumSerie16B.ToString("F2") + "\n";
                }


                if (is6 || is3)
                {

                  //  MessageBox.Show("procees 33 ");
                    int[,] myPlane33 = new int[im33.XSize, im33.YSize];
                    im33.GetVoxels(centralImageIndex, myPlane33);
                    xPhaseSize = im33.XSize;
                    yPhaseSize = im33.YSize;
                    k = xPhaseSize / 2;
                    m = yPhaseSize / 2;
                    checkSumSerie33 = im33.VoxelToDisplayValue(myPlane33[k, m]);
                    checkSumSerie33B = im33.VoxelToDisplayValue(myPlane33[k + 4, m + 4]);
                    CheckAVEMessage += im33.Id + " A: " + checkSumSerie33.ToString("F2") + "\tB:" + checkSumSerie33B.ToString("F2") + "\n";
                }

                if (is6 || is3)
                {
                    //MessageBox.Show("procees 50 ");
                    int[,] myPlane50 = new int[im50.XSize, im50.YSize];
                    im50.GetVoxels(centralImageIndex, myPlane50);
                    xPhaseSize = im50.XSize;
                    yPhaseSize = im50.YSize;
                    k = xPhaseSize / 2;
                    m = yPhaseSize / 2;
                    checkSumSerie50 = im50.VoxelToDisplayValue(myPlane50[k, m]);
                    checkSumSerie50B = im50.VoxelToDisplayValue(myPlane50[k + 4, m + 4]);
                    CheckAVEMessage += im50.Id + " A: " + checkSumSerie50.ToString("F2") + "\tB:" + checkSumSerie50B.ToString("F2") + "\n";
                }

                if (is6 || is3)
                {
                    //MessageBox.Show("procees 66 ");
                    int[,] myPlane66 = new int[im66.XSize, im66.YSize];
                    im66.GetVoxels(centralImageIndex, myPlane66);
                    xPhaseSize = im66.XSize;
                    yPhaseSize = im66.YSize;
                    k = xPhaseSize / 2;
                    m = yPhaseSize / 2;
                    checkSumSerie66 = im66.VoxelToDisplayValue(myPlane66[k, m]);
                    checkSumSerie66B = im66.VoxelToDisplayValue(myPlane66[k + 4, m + 4]);
                    CheckAVEMessage += im66.Id + " A: " + checkSumSerie66.ToString("F2") + "\tB:" + checkSumSerie66B.ToString("F2") + "\n";
                }
                if (is6)
                {
                    int[,] myPlane83 = new int[im83.XSize, im83.YSize];
                    im83.GetVoxels(centralImageIndex, myPlane83);
                    xPhaseSize = im83.XSize;
                    yPhaseSize = im83.YSize;
                    k = xPhaseSize / 2;
                    m = yPhaseSize / 2;
                    checkSumSerie83 = im83.VoxelToDisplayValue(myPlane83[k, m]);
                    checkSumSerie83B = im83.VoxelToDisplayValue(myPlane83[k + 4, m + 4]);
                    CheckAVEMessage += im83.Id + " A: " + checkSumSerie83.ToString("F2") + "\tB:" + checkSumSerie83B.ToString("F2") + "\n";

                }
            }









            #endregion


            double checkSumComputedAverage = 0.0;
            double checkSumComputedAverageB = 0.0;

            if (threeOrSix == 3)
            {
                checkSumComputedAverage = (1.0 / 3.0) * (checkSumSerie33 + checkSumSerie50 + checkSumSerie66);
                checkSumComputedAverageB = (1.0 / 3.0) * (checkSumSerie33B + checkSumSerie50B + checkSumSerie66B);
            }
            else if (threeOrSix == 6)
            {
                checkSumComputedAverage = (1.0 / 6.0) * (checkSumSerie00 + checkSumSerie16 + checkSumSerie33 + checkSumSerie50 + checkSumSerie66 + checkSumSerie83);
                checkSumComputedAverageB = (1.0 / 6.0) * (checkSumSerie00B + checkSumSerie16B + checkSumSerie33B + checkSumSerie50B + checkSumSerie66B + checkSumSerie83B);


            }
            double diff1 = Math.Abs(checkSumComputedAverage - checkSumAvergageSerie);
            double diff2 = Math.Abs(checkSumComputedAverageB - checkSumAvergageSerieB);

            if ((diff1 < tolerance) && (diff2 < tolerance))
            {
                iSChecked = true;

            }
            else
            {

                iSChecked = false;


            }

            return iSChecked;



        }

        private bool checAVEcompositionGeneralElectrics4D(String comment, int expectedPhase) // General Electrics 4dct
        {
            // if exepected phase is 3, comment must contains these values and only these values : 33% 50% 66%
            // if exepected phase is 6, comment must contains these values  : 0% 16% 33% 50% 66% 83%
            bool isok = false;
            // Comment is : Ave - IP(3) 33 % _50 % _66 %

            if (comment.Contains("33%") && comment.Contains("50%") && comment.Contains("66%"))
            {

                comment = comment.Replace("33%", "x");
                comment = comment.Replace("50%", "x");
                comment = comment.Replace("66%", "x");
                if (expectedPhase == 3)
                {

                    if (comment.Contains("%")) // for AVE3phase : only these 3 phases. 
                    {

                        isok = false;
                    }
                    else
                    {

                        isok = true;
                    }
                }
                else if (expectedPhase == 6)
                {

                    if (comment.Contains("0%") && comment.Contains("16%") && comment.Contains("83%"))
                        isok = true;
                    else
                        isok = false;
                }
                else // wrong call, expected phase must be 3 or 6 
                    isok = false;
            }
            else // wrong call. Average must contains at least 33 50 and 66% 
                isok = false;


            return isok;
        }

        public void Check()
        {
            Comparator testing = new Comparator();
            DateTime myToday = DateTime.Today;
            if (_pinfo.actualUserPreference.userWantsTheTest("CT_age"))
            {
                #region days since CT

                Item_Result CT_age = new Item_Result();
                CT_age.Label = "Ancienneté du CT (jours)";
                CT_age.ExpectedValue = "12";

                int nDays = (myToday - (DateTime)_context.Image.Series.HistoryDateTime).Days;
                // _context.Image.Series.export
                CT_age.MeasuredValue = nDays.ToString();
                //CT_age.Comparator = "<";
                CT_age.Infobulle = "Le CT doit avoir moins de 12 jours. Warning si > 10 jours, ERREUR si > 30";
                //CT_age.ResultStatus = testing.CompareDatas(CT_age.ExpectedValue, CT_age.MeasuredValue, CT_age.Comparator);
                CT_age.setToTRUE();
                if (nDays > 12)
                    CT_age.setToWARNING();
                if (nDays > 30)
                    CT_age.setToFALSE();


                this._result.Add(CT_age);
                #endregion
            }

            if (_pinfo.actualUserPreference.userWantsTheTest("origin") && _AplanIsloaded)
            {
                #region Origine placée
                if ((!_pinfo.isTOMO))
                {
                    Item_Result origin = new Item_Result();
                    origin.Label = "Origine modifiée";
                    origin.ExpectedValue = "sans objet";
                    var image = _context.PlanSetup.StructureSet.Image;
                    if (!image.HasUserOrigin)
                    {
                        origin.setToFALSE();
                        origin.MeasuredValue = "Origine non modifiée";
                        origin.Infobulle = "L'origine est confondue avec l'origine DICOM. Ce qui signifie que l'origine n'a pas été placée. A vérifier.";
                    }
                    else
                    {
                        origin.setToTRUE();
                        origin.MeasuredValue = "Origine modifiée";
                        origin.Infobulle = "L'origine n'est pas confondue avec l'origine DICOM.";
                    }

                    this._result.Add(origin);
                }
                #endregion
            }
         
            if (_pinfo.actualUserPreference.userWantsTheTest("sliceThickness"))
            {
                #region Epaisseur de coupes
                Item_Result sliceThickness = new Item_Result();
                sliceThickness.Label = "Epaisseur de coupes (mm)";
                sliceThickness.ExpectedValue = _rcp.CTslicewidth.ToString();// "2.5";//XXXXX TO GET         
                sliceThickness.MeasuredValue = _context.Image.ZRes.ToString();
                //sliceThickness.Comparator = "=";
                sliceThickness.Infobulle = "L'épaisseur de coupe doit être " + sliceThickness.ExpectedValue + " mm comme spécfifié dans le fichier check-protocol: " + _rcp.protocolName;

                if (_rcp.CTslicewidth == _context.Image.ZRes)
                    sliceThickness.setToTRUE();
                else
                    sliceThickness.setToWARNING();

                //sliceThickness.ResultStatus = testing.CompareDatas(sliceThickness.ExpectedValue, sliceThickness.MeasuredValue, sliceThickness.Comparator);
                this._result.Add(sliceThickness);

                #endregion
            }
           
            if (_pinfo.actualUserPreference.userWantsTheTest("HUcurve") && _AplanIsloaded)
            {
                #region courbe HU
                Item_Result HUcurve = new Item_Result();
                HUcurve.Label = "Courbe HU";

                if (!_pinfo.isTOMO)
                {
                    String courbeHU = _context.Image.Series.ImagingDeviceId;
                    String expectedHUcurve;

                    if ((myToday - (DateTime)_context.Patient.DateOfBirth).Days < (14 * 365))
                        expectedHUcurve = "100kV_CT130246";
                    else
                        expectedHUcurve = "CT130246";//"TDMRT";


                    HUcurve.ExpectedValue = expectedHUcurve;
                    HUcurve.MeasuredValue = courbeHU;
                    HUcurve.Comparator = "=";
                    HUcurve.Infobulle = "La courbe doit être CT130246  sauf si âge patient < 14";
                    HUcurve.ResultStatus = testing.CompareDatas(HUcurve.ExpectedValue, HUcurve.MeasuredValue, HUcurve.Comparator);
                }
                else if (_pinfo.planReportIsFound) // tomo with a report
                {

                    HUcurve.MeasuredValue = _pinfo.tprd.Trd.HUcurve;

                    HUcurve.ExpectedValue = "";
                    if (HUcurve.MeasuredValue.Contains("CT130246"))
                        HUcurve.setToTRUE();
                    else
                        HUcurve.setToFALSE();
                    HUcurve.Infobulle = "Pour Tomotherapy la courbe doit être " + HUcurve.ExpectedValue;
                }
                else
                {
                    HUcurve.MeasuredValue = "Pas de rapport de dosimétrie Tomothérapie. Vérifiez la courbe HU";
                    HUcurve.setToUNCHECK();
                }


                this._result.Add(HUcurve);
                #endregion
            }
            
            if (_pinfo.actualUserPreference.userWantsTheTest("deviceName"))
            {
                #region CT series number

                Item_Result deviceName = new Item_Result();
                String CT = _context.Image.Series.ImagingDeviceManufacturer + " ";
                CT = CT + _context.Image.Series.ImagingDeviceModel;
                CT = CT + _context.Image.Series.ImagingDeviceSerialNo;


                deviceName.Label = "CT series number";

                if (_context.Image.Series.ImagingDeviceId.ToUpper().Contains("100KV"))
                {
                    deviceName.ExpectedValue = "Siemens Healthineers SOMATOM go.Open Pro";// GE MEDICAL SYSTEMS Optima CT580";//XXXXX TO GET         
                }
                else
                {
                    deviceName.ExpectedValue = "Siemens Healthineers SOMATOM go.Open Pro130246";// GE MEDICAL SYSTEMS Optima CT580";//XXXXX TO GET         
                }

                
                deviceName.MeasuredValue = CT;
                deviceName.Comparator = "=";
                deviceName.Infobulle = "Vérification du modèle et du numéro de série du CT";
                deviceName.ResultStatus = testing.CompareDatas(deviceName.ExpectedValue, deviceName.MeasuredValue, deviceName.Comparator);
                this._result.Add(deviceName);

                #endregion
            }
          
            if (_pinfo.actualUserPreference.userWantsTheTest("image3Dnaming"))
            {
                #region date dans le nom imaged 3d

                Item_Result image3Dnaming = new Item_Result();

                image3Dnaming.Label = "Nom de l'image 3D";

                // get the CT date in format: ddmmyy
                String imageDate = ((DateTime)_context.Image.CreationDateTime).ToString("dd");
                imageDate += ((DateTime)_context.Image.CreationDateTime).ToString("MM");
                imageDate += ((DateTime)_context.Image.CreationDateTime).ToString("yy");

                // get the CT date in format: ddmmyyyy
                String imageDate2 = ((DateTime)_context.Image.CreationDateTime).ToString("dd");
                imageDate2 += ((DateTime)_context.Image.CreationDateTime).ToString("MM");
                imageDate2 += ((DateTime)_context.Image.CreationDateTime).ToString("yyyy");


                if (_context.Image.Id.Contains(imageDate))
                {
                    image3Dnaming.setToTRUE();

                }
                else if (_context.Image.Id.Contains(imageDate2))
                {
                    image3Dnaming.setToTRUE();
                }
                else
                {
                    image3Dnaming.setToWARNING();

                }

                image3Dnaming.ExpectedValue = imageDate;
                image3Dnaming.MeasuredValue = _context.Image.Id;
                image3Dnaming.Infobulle = "Le nom de l'image 3D doit contenir la date du CT au format jjmmaa (" + imageDate + ") ou jjmmaaaa";
                this._result.Add(image3Dnaming);

                #endregion
            }
            if (_pinfo.actualUserPreference.userWantsTheTest("averageCT"))
            {
                #region Composition of AVE3/AVE6 


                if (_context.Image.Id.ToUpper().Contains("AVE") || _context.Image.Id.ToUpper().Contains("AVG"))
                {

                    Item_Result averageCT = new Item_Result();
                    averageCT.Label = "Composition de AVE3 ou AVE6";
                    averageCT.ExpectedValue = "none";
                    // averageCT.Infobulle = "Si le nom de l'image contient AVG ou AVE, l'image 3D doit être la moyenne des phases:";
                    // averageCT.Infobulle += "\n AVG3: moyenne des phases 33% 50% et 66%";
                    // averageCT.Infobulle += "\n AVG6: moyenne des phases 0% 16% 33% 50% 66% et 83%";
                    averageCT.Infobulle += "La composition est vérifiée en recalculant la moyenne pour deux pixels A et B\n";

                    averageCT.MeasuredValue = _context.Image.Id;
                    bool checkComposition = false;

                   

                    if (_context.Image.Series.Comment.ToUpper().Contains("AVE"))
                    {

                        //if (_context.Image.Id.ToUpper().Contains("AV") && _context.Image.Id.ToUpper().Contains("3"))
                        if (_context.Image.Series.Comment.ToUpper().Contains("3"))
                        {

                            //    checkComposition = checAVEcompositionGeneralElectrics4D(_context.Image.Series.Comment, 3);  // GE

                            checkComposition = checkAVEcompositionSiemensCT(_context, 3); // SIEMENS


                        }
                        //else if (_context.Image.Id.ToUpper().Contains("AV") && _context.Image.Id.ToUpper().Contains("6"))
                        else if (_context.Image.Series.Comment.ToUpper().Contains("6"))
                        {

                            //  checkComposition = checAVEcompositionGeneralElectrics4D(_context.Image.Series.Comment, 6); // GE
                            checkComposition = checkAVEcompositionSiemensCT(_context, 6); // SIEMENS

                        }
                        else
                        {

                            checkComposition = false;

                        }



                    }
                    else
                    {

                        checkComposition = false;

                    }
                    if (checkComposition == false)
                    {
                        averageCT.setToFALSE();
                        averageCT.MeasuredValue += " n'est pas la moyenne des phases";
                    }
                    else
                    {
                        averageCT.setToTRUE();
                        averageCT.MeasuredValue += " est bien la moyenne des phases";
                    }
                    averageCT.Infobulle += CheckAVEMessage;
                    this._result.Add(averageCT);

                }


                #endregion
            }
            if (_pinfo.actualUserPreference.userWantsTheTest("averageForSBRT"))
            {
                #region AVE3 or AVE6 is only for lung SBRT  (option)




                Item_Result averageForSBRT = new Item_Result();
                averageForSBRT.Label = "Image Average";
                averageForSBRT.ExpectedValue = "none";


                /*
                averageForSBRT.Infobulle = "Les scanners AVERAGE doivent être utilisés pour les STEC poumons uniquement (avec enable Gating)";
                averageForSBRT.MeasuredValue = _context.Image.Id;


                if (_context.Image.Id.ToUpper().Contains("AV"))
                {
                    averageForSBRT.setToTRUE();

                    if (!_context.PlanSetup.UseGating)
                    {
                        averageForSBRT.setToFALSE();

                    }


                    if (!_rcp.protocolName.ToUpper().Contains("STEC POUMON"))
                    {

                        averageForSBRT.setToFALSE();
                    }



                }
                else
                {
                    averageForSBRT.setToTRUE();
                    if (_rcp.protocolName.ToUpper().Contains("STEC poumon"))
                    {
                        averageForSBRT.setToFALSE();

                    }
                    if (_context.PlanSetup.UseGating)
                    {
                        averageForSBRT.setToFALSE();

                    }
                }
                */
                averageForSBRT.setToINFO();
                averageForSBRT.MeasuredValue = "Ce test n'est plus utile. Merci de le déselectionner dans vos préférences";
                this._result.Add(averageForSBRT);




                #endregion
            }
            if (_pinfo.actualUserPreference.userWantsTheTest("tomoReportCT_date") && _AplanIsloaded)
            {
                #region CT used for tomo : Check date
                if (_pinfo.isTOMO)
                {
                    Item_Result tomoReportCT_date = new Item_Result();
                    tomoReportCT_date.Label = "Date du CT dans le rapport Tomotherapy";
                    tomoReportCT_date.ExpectedValue = "";//XXXXX TO GET        
                    if (_pinfo.planReportIsFound) // tomo with a report
                    {

                        tomoReportCT_date.MeasuredValue = _pinfo.tprd.Trd.CTDate;  //format 11 Apr 2023
                        var parsedDate = DateTime.Parse(_pinfo.tprd.Trd.CTDate);
                        if (DateTime.Compare(parsedDate, _context.Image.Series.HistoryDateTime) < 2) // different hours gives difference = 1
                            tomoReportCT_date.setToTRUE();
                        else
                            tomoReportCT_date.setToFALSE();
                        tomoReportCT_date.Infobulle = "Comparaison de la date du CT (" + parsedDate.ToString() + ") dans le rapport Tomo et de la date du scanner (" + _context.Image.Series.HistoryDateTime.ToString() + ")";
                    }
                    else
                    {
                        tomoReportCT_date.MeasuredValue = "Pas de rapport de dosimétrie Tomothérapie, vérifiez la date";
                        tomoReportCT_date.setToUNCHECK();


                    }
                    this._result.Add(tomoReportCT_date);
                }
                #endregion
            }
            if (_pinfo.actualUserPreference.userWantsTheTest("otherSeries") && _AplanIsloaded)
            {
                #region other required series
                if (_rcp.needeSupplImages.Count > 0)
                {
                    Item_Result otherSeries = new Item_Result();
                    List<string> unfound3DImage = new List<string>();
                    //neededSupplImage.Add("T2 FLAIR 220823");
                    //neededSupplImage.Add("T3 FLAIR 220823");

                    otherSeries.Label = "Autres séries d'images";
                    //_context.Image.
                    string msg = string.Empty;
                    foreach (string s in _rcp.needeSupplImages)
                    {
                        bool found = false;
                        foreach (Study st in _context.Patient.Studies)
                        {
                            foreach (Series se in st.Series)
                            {

                                if (matchingImageName(se.Comment.ToUpper(), s.ToUpper()))
                                {
                                    //MessageBox.Show(s + " found");
                                    found = true;
                                    break;
                                }

                               
                            }
                            if (found) break;
                        }

                        if (!found)
                            unfound3DImage.Add(s);

                    }



                    otherSeries.MeasuredValue = _rcp.needeSupplImages.Count + " série(s) nécessaires, " + unfound3DImage.Count + " séries absentes (voir détail)";
                    otherSeries.Infobulle = "Séries supplémentaires nécessaires (ex. IRM). cf check-protocol Ligne 53 \n";
                    foreach (string s in _rcp.needeSupplImages)
                        otherSeries.Infobulle += " - " + s + "\n";
                    otherSeries.Infobulle += "Séries absentes \n";
                    foreach (string s in unfound3DImage)
                        otherSeries.Infobulle += " - " + s + "\n";

                    if (unfound3DImage.Count > 0)
                        otherSeries.setToFALSE();
                    else
                        otherSeries.setToTRUE();

                    otherSeries.ExpectedValue = "NA";

                    this._result.Add(otherSeries);
                }
                #endregion
            }
        }

        public string Title
        {
            get { return _title; }
        }
        public List<Item_Result> Result
        {
            get { return _result; }
            set { _result = value; }
        }
    }
}
