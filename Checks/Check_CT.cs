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
        public Check_CT(PreliminaryInformation pinfo, ScriptContext ctx, read_check_protocol rcp)  //Constructor
        {
            // _testpartlabel = "Algorithme";
            _rcp = rcp;
            _context = ctx;
            _pinfo = pinfo;
            Check();
        }

        /*double checkIndexOfThisSerie(Image i)
        {
            double result = 0.0;

            int halfXSize = i.XSize / 2 ;

            i.GetVoxels()


            return result;

        }
        */
        private List<Item_Result> _result = new List<Item_Result>();
        private PreliminaryInformation _pinfo;
        private ScriptContext _context;
        private read_check_protocol _rcp;

        private string _title = "CT";
        //test

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
            bool iSChecked = true;
            String msg = String.Empty;

            // AVERAGE SERIES

            int zSizeAverage = ctx.Image.ZSize;
            int xSizeAverage = ctx.Image.XSize;
            int ySizeAverage = ctx.Image.YSize;
            int centralImageIndex = zSizeAverage / 2;
            int[,] myPlane = new int[xSizeAverage, ySizeAverage];
            ctx.Image.GetVoxels(centralImageIndex, myPlane);
            double checkSumAvergageSerie = 0.0;

            int i = 256;
            int j = 256;
            checkSumAvergageSerie = ctx.Image.VoxelToDisplayValue(myPlane[i, j]);


            MessageBox.Show("Checksum for image ave " + i + " " + j + " " + centralImageIndex + " : " + checkSumAvergageSerie + " " + ctx.Image.Id + " " + ctx.Image.Series.Study.Id);


            //  ctx.Image.ZSize is the number of images of the image3D of the plan. eg 189
            //  ctx.Image.Series.Images.Count() is the number of images in the series used to build the image3D eg 190 : 189 images + 1 image3D
            // ctx.Image.Series.Study.Series.Count() est le nombre de serie dans l'examen


            foreach (var v in ctx.Image.Series.Study.Series)
            {
                if (v.Modality.ToString() == "CT")
                {
                    if (threeOrSix == 6)
                    {
                        if (v.Comment.Contains("0%"))
                        {

                            foreach (var im in v.Images)
                            {
                                if (im.ZSize > 1) // is 3d
                                {
                                    int zPhaseSize = im.ZSize;
                                    int xPhaseSize = im.XSize;
                                    int yPhaseSize = im.YSize;

                                    if (xSizeAverage > xPhaseSize) // when adding table, the average image is larger
                                    {

                                    }

                                    int[,] myPlane2 = new int[im.XSize, im.YSize];
                                    im.GetVoxels(centralImageIndex, myPlane2);
                                    double checkSumSerie = 0.0;
                                    int k = im.XSize / 2;
                                    checkSumSerie += im.VoxelToDisplayValue(myPlane2[k, k]);
                                    MessageBox.Show("33% " + k + " " + k + " " + centralImageIndex + " : " + checkSumSerie);




                                }

                            }



                        }
                        if (v.Comment.Contains("16%"))
                        {

                        }
                        if (v.Comment.Contains("33%"))
                        {

                        }
                        if (v.Comment.Contains("50%"))
                        {

                        }
                        if (v.Comment.Contains("66%"))
                        {

                        }
                        if (v.Comment.Contains("83%"))
                        {

                        }
                    }
                    else if (threeOrSix == 3)
                    {

                        if (v.Comment.Contains("33%"))
                        {

                            foreach (var im in v.Images)
                            {
                                if (im.ZSize > 1) // is 3d
                                {

                                    int[,] myPlane2 = new int[im.XSize, im.YSize];
                                    im.GetVoxels(centralImageIndex, myPlane2);
                                    double checkSumSerie = 0.0;
                                    int xPhaseSize = im.XSize;
                                    int yPhaseSize = im.YSize;


                                    int k = xPhaseSize / 2 - ((xSizeAverage - xPhaseSize) / 2);
                                    int m = yPhaseSize / 2 - ((ySizeAverage - yPhaseSize) / 2);
                                    checkSumSerie += im.VoxelToDisplayValue(myPlane2[k, m]);
                                    MessageBox.Show("33% " + k + " " + m + " " + centralImageIndex + " : " + checkSumSerie);




                                }

                            }


                        }
                        if (v.Comment.Contains("50%"))
                        {


                            foreach (var im in v.Images)
                            {
                                if (im.ZSize > 1) // is 3d
                                {

                                    int[,] myPlane2 = new int[im.XSize, im.YSize];
                                    im.GetVoxels(centralImageIndex, myPlane2);
                                    double checkSumSerie = 0.0;
                                    int k = im.XSize / 2;
                                    checkSumSerie += im.VoxelToDisplayValue(myPlane2[k, k + 1]);
                                    MessageBox.Show("50% " + k + " " + k + " " + centralImageIndex + " : " + checkSumSerie + " " + im.Id + " " + v.Id + " " + ctx.Image.Series.Study.Id);



                                }

                            }

                        }
                        if (v.Comment.Contains("66%"))
                        {


                            foreach (var im in v.Images)
                            {
                                if (im.ZSize > 1) // is 3d
                                {
                                    int[,] myPlane2 = new int[im.XSize, im.YSize];
                                    im.GetVoxels(centralImageIndex, myPlane2);
                                    double checkSumSerie = 0.0;
                                    int k = im.XSize / 2;
                                    checkSumSerie += im.VoxelToDisplayValue(myPlane2[k, k + 1]);
                                    MessageBox.Show("66% " + k + " " + k + " " + centralImageIndex + " : " + checkSumSerie);



                                }

                            }
                        }

                    }
                    else
                        iSChecked = false;


                }


                //MessageBox.Show(v.Id + " " + v.Modality.ToString() + " " + v.Comment);

            }
            // MessageBox.Show(v.Id  + " " +v.Modality.ToString());
            return iSChecked;
        }

        private bool checAVEcomposition(String comment, int expectedPhase)
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

            if (_pinfo.actualUserPreference.userWantsTheTest("origin"))
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
                        origin.setToWARNING();
                        origin.MeasuredValue = "Origine non modifiée";
                        origin.Infobulle = "L'origine est confondue avec l'origine DICOM. Ce qui peut signifier que l'origine n'a pas été placée. A vérifier.";
                    }
                    else
                    {
                        origin.setToTRUE();
                        origin.MeasuredValue = "Origine modifiée";
                        origin.Infobulle = "L'origine n'est pas confondue avec l'origine DICOM. Dans le cas contraire cela peut signifier que l'origine n'a pas été placée";
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
            if (_pinfo.actualUserPreference.userWantsTheTest("HUcurve"))
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
                    HUcurve.Infobulle = "La courbe doit être " + expectedHUcurve + " sauf si âge patient < 14";
                    HUcurve.ResultStatus = testing.CompareDatas(HUcurve.ExpectedValue, HUcurve.MeasuredValue, HUcurve.Comparator);
                }
                else if (_pinfo.tomoReportIsFound) // tomo with a report
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
                deviceName.ExpectedValue = "Siemens Healthineers SOMATOM go.Open Pro130246";// GE MEDICAL SYSTEMS Optima CT580";//XXXXX TO GET         
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
                #region Composition of AVE3/AVE6 (option)


                if (_context.Image.Id.ToUpper().Contains("AVE") || _context.Image.Id.ToUpper().Contains("AVG"))
                {

                    Item_Result averageCT = new Item_Result();
                    averageCT.Label = "Composition de AVE3 ou AVE6";
                    averageCT.ExpectedValue = "none";
                    averageCT.Infobulle = "Si le nom de l'image contient AVG ou AVE, l'image 3D doit être composé des phases correctes.";
                    averageCT.Infobulle += "\n Une image AVG3 doit être composée des phases 33% 50% et 66%";
                    averageCT.Infobulle += "\n Une image AVG6 doit être composée des phases 0% 16% 33% 50% 66% et 83%";
                    averageCT.Infobulle += "\n\nLa composition est vérifiée dans la description de la serie";
                    averageCT.MeasuredValue = _context.Image.Series.Comment;
                    bool checkComposition = false;

                    if (_context.Image.Series.Comment.ToUpper().Contains("AVE"))
                    {

                        if (_context.Image.Id.ToUpper().Contains("AVG3") || _context.Image.Id.ToUpper().Contains("AVE3"))
                        {

                            //    checkComposition = checAVEcomposition(_context.Image.Series.Comment, 3);  // GE
                            // checkComposition = checAVEcompositionSiemensCT(_context, 3);  // SIEMENS
                            checkComposition = checkAVEcompositionSiemensCT(_context, 3); // SIEMENS
                        }
                        else if (_context.Image.Id.ToUpper().Contains("AVG") || _context.Image.Id.ToUpper().Contains("AVE"))
                        {

                            //  checkComposition = checAVEcomposition(_context.Image.Series.Comment, 6); // GE
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
                        averageCT.setToFALSE();
                    else
                        averageCT.setToTRUE();

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

                this._result.Add(averageForSBRT);




                #endregion
            }
            if (_pinfo.actualUserPreference.userWantsTheTest("tomoReportCT_date"))
            {
                #region CT used for tomo : Check date
                if (_pinfo.isTOMO)
                {
                    Item_Result tomoReportCT_date = new Item_Result();
                    tomoReportCT_date.Label = "Date du CT dans le rapport Tomotherapy";
                    tomoReportCT_date.ExpectedValue = "";//XXXXX TO GET        
                    if (_pinfo.tomoReportIsFound) // tomo with a report
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
            if (_pinfo.actualUserPreference.userWantsTheTest("otherSeries"))
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

                                if (matchingImageName(se.Comment, s))
                                {
                                    //MessageBox.Show(s + " found");
                                    found = true;
                                    break;
                                }

                                /*foreach (Image im in se.Images)
                                 {

                                    if (im.ZDirection.z == double.NaN)
                                        nan++;
                                    else
                                        nonnan++;
                                     // looking for a if statment to distinguish 3d and 2d
                                     //if (im.Id == s)
                                     if (!im.Id.Contains("Image")) // to change when found a good solution. Image2D IDs start with "Image..."
                                         if (matchingImageName(im.Id, s))
                                         {
                                             //MessageBox.Show(s + " found");
                                             found = true;
                                             break;
                                         }
                                 }*/
                                /*if (found) break;*/
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
