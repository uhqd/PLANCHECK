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

        private List<Item_Result> _result = new List<Item_Result>();
        private PreliminaryInformation _pinfo;
        private ScriptContext _context;
        private read_check_protocol _rcp;

        private string _title = "CT";
        //test
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


            #region days since CT
            Item_Result CT_age = new Item_Result();
            CT_age.Label = "Ancienneté du CT (jours)";
            CT_age.ExpectedValue = "12";
            DateTime myToday = DateTime.Today;
            int nDays = (myToday - (DateTime)_context.Image.Series.HistoryDateTime).Days;
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

            #region Origine placée
            if ((_pinfo.advancedUserMode)&&(_pinfo.isTOMO))
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

            #region courbe HU
            Item_Result HUcurve = new Item_Result();
            HUcurve.Label = "Courbe HU";
            if (!_pinfo.isTOMO)
            {
                String courbeHU = _context.Image.Series.ImagingDeviceId;
                String expectedHUcurve;

                if ((myToday - (DateTime)_context.Patient.DateOfBirth).Days < (14 * 365))
                    expectedHUcurve = "Scan_IUC_100kV";
                else
                    expectedHUcurve = "TDMRT";


                HUcurve.ExpectedValue = expectedHUcurve;
                HUcurve.MeasuredValue = courbeHU;
                HUcurve.Comparator = "=";
                HUcurve.Infobulle = "La courbe doit être TDMRT sauf si âge patient < 14";
                HUcurve.ResultStatus = testing.CompareDatas(HUcurve.ExpectedValue, HUcurve.MeasuredValue, HUcurve.Comparator);
            }
            else // tomo
            {
                HUcurve.MeasuredValue = _pinfo.tprd.Trd.HUcurve;

                HUcurve.ExpectedValue = "";
                if (HUcurve.MeasuredValue.Contains("IUC-120kV"))
                    HUcurve.setToTRUE();
                else
                    HUcurve.setToFALSE();
                HUcurve.Infobulle = "Pour Tomotherapy la courbe doit être IUC-120kV";
            }

            this._result.Add(HUcurve);
            #endregion

            #region CT series number
            if (_pinfo.advancedUserMode)
            {
                Item_Result deviceName = new Item_Result();
                String CT = _context.Image.Series.ImagingDeviceManufacturer + " ";
                CT = CT + _context.Image.Series.ImagingDeviceModel;
                CT = CT + _context.Image.Series.ImagingDeviceSerialNo;


                deviceName.Label = "CT series number";
                deviceName.ExpectedValue = "GE MEDICAL SYSTEMS Optima CT580";//XXXXX TO GET         
                deviceName.MeasuredValue = CT;
                deviceName.Comparator = "=";
                deviceName.Infobulle = "Vérification du modèle et du numéro de série du CT";
                deviceName.ResultStatus = testing.CompareDatas(deviceName.ExpectedValue, deviceName.MeasuredValue, deviceName.Comparator);
                this._result.Add(deviceName);
            }
            #endregion

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

                        checkComposition = checAVEcomposition(_context.Image.Series.Comment, 3);
                    }
                    else if (_context.Image.Id.ToUpper().Contains("AVG6") || _context.Image.Id.ToUpper().Contains("AVE6"))
                    {

                        checkComposition = checAVEcomposition(_context.Image.Series.Comment, 6);
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

            #region AVE3 or AVE6 is only for lung SBRT  (option)


            if (_pinfo.advancedUserMode)
            {

                Item_Result averageForSBRT = new Item_Result();
                averageForSBRT.Label = "Image Average";
                averageForSBRT.ExpectedValue = "none";
                averageForSBRT.Infobulle = "Les scanners AVERAGE doivent être utilisés pour les STEC poumons uniquement (avec enable Gating)";
                averageForSBRT.MeasuredValue = _context.Image.Id;


                if (_context.Image.Id.ToUpper().Contains("AVE") || _context.Image.Id.ToUpper().Contains("AVG"))
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

            }


            #endregion

            #region CT used for tomo Check date
            if (_pinfo.isTOMO)
            {
                Item_Result tomoReportCT_date = new Item_Result();


                tomoReportCT_date.Label = "Date du CT dans le rapport Tomotherapy";
                tomoReportCT_date.ExpectedValue = "";//XXXXX TO GET         
                tomoReportCT_date.MeasuredValue = _pinfo.tprd.Trd.CTDate;  //format 11 Apr 2023
                var parsedDate = DateTime.Parse(_pinfo.tprd.Trd.CTDate);
                if (DateTime.Compare(parsedDate, _context.Image.Series.HistoryDateTime) < 2) // different hours gives difference = 1
                    tomoReportCT_date.setToTRUE();
                else
                    tomoReportCT_date.setToFALSE();
                tomoReportCT_date.Infobulle = "Comparaison de la date du CT (" + parsedDate.ToString() + ") dans le rapport Tomo et de la date du scanner (" + _context.Image.Series.HistoryDateTime.ToString() + ")";
                this._result.Add(tomoReportCT_date);
            }
            #endregion

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
