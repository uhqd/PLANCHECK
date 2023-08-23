using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using System.Windows;

namespace PlanCheck
{
    internal class Check_UM
    {
        private ScriptContext _ctx;
        private PreliminaryInformation _pinfo;

        private read_check_protocol _rcp;
        public Check_UM(PreliminaryInformation pinfo, ScriptContext ctx, read_check_protocol rcp)  //Constructor
        {
            _ctx = ctx;
            _pinfo = pinfo;
            _rcp = rcp;
            Check();

        }

        private List<Item_Result> _result = new List<Item_Result>();
        // private PreliminaryInformation _pinfo;
        private string _title = "UM";
        double n_um = 0.0;
        double n_um_per_gray = 0.0;

        public void Check()
        {

            int uncorrFieldWithaWedge = 0;
            int FieldWithLessThan10UM = 0;

            #region UM per Gray
            Item_Result um = new Item_Result();
            um.Label = "UM";
            um.ExpectedValue = "EN COURS";
            if (!_pinfo.isTOMO)
            {

                n_um = 0.0;
                n_um_per_gray = 0.0;
                String myMLCType = null;

                foreach (Beam b in _ctx.PlanSetup.Beams)
                {
                    if (!b.IsSetupField)
                    {
                        if (b.Meterset.Value < 20.0)
                            if (b.Wedges.Count() > 0)
                                uncorrFieldWithaWedge++;
                        if (b.Meterset.Value < 9.5)
                        {
                            FieldWithLessThan10UM++;
                            //MessageBox.Show(b.Id + b.Meterset.Value.ToString());
                        }
                        myMLCType = b.MLCPlanType.ToString();
                        n_um = n_um + Math.Round(b.Meterset.Value, 1);
                    }
                }


                n_um_per_gray = n_um / (_ctx.PlanSetup.DosePerFraction.Dose / _ctx.PlanSetup.TreatmentPercentage);
                n_um_per_gray = Math.Round(n_um_per_gray / 100, 3);
                um.MeasuredValue = n_um.ToString() + " UM (" + n_um_per_gray + " UM/cGy)";

                // MessageBox.Show(n_um_per_gray.ToString("N2") + myMLCType);

                if ((myMLCType == "VMAT") || (myMLCType == "IMRT") || (myMLCType == "DoseDynamic"))
                {
                    um.setToTRUE();
                    if (n_um_per_gray > 4.5)
                        um.setToWARNING();

                    if (n_um_per_gray > 5)
                        um.setToFALSE();

                    //MessageBox.Show("IMRT " + n_um_per_gray);

                    um.Infobulle = "En VMAT/IMRT warning si > 4.5 et ERREUR si > 5.";

                }
                else
                {
                    if (n_um_per_gray > 2)
                        um.setToFALSE();
                    else if (n_um_per_gray > 1.5)
                        um.setToWARNING();
                    else
                        um.setToTRUE();
                    um.Infobulle = "En RTC/DCA  warning si > 1.5 et ERREUR si > 2.";

                }

            }
            else // tomo
            {
                if (_pinfo.tomoReportIsFound)
                {
                    um.Label = "Beam On Time";
                    um.MeasuredValue = "Beam on time: " + _pinfo.tprd.Trd.beamOnTime.ToString() + " s (" + (_pinfo.tprd.Trd.beamOnTime / 60).ToString("0.00") + " min)";
                    if (_pinfo.tprd.Trd.beamOnTime > 700)
                        um.setToWARNING();
                    else
                        um.setToTRUE();

                    um.Infobulle = "Attendu < 700 s";
                }
                else
                {
                    um.MeasuredValue = "Pas de rapport de dosimétrie Tomotherapy dans Aria documents";
                }

            }

            this._result.Add(um);

            #endregion

            #region UM fluence etendue ?
            if ((_ctx.PlanSetup.Id.Contains("FE"))&& (_rcp.protocolName == "sein"))
            {
                Item_Result FE = new Item_Result();
                FE.Label = "Fluence étendue";
                FE.ExpectedValue = "EN COURS";
                double nUmWithNoFE = 0.0;

                string planIdwithoutFE = _ctx.PlanSetup.Id.Split('F')[0];

                #region get UM of the plan without FE in name
                foreach (PlanSetup p in _ctx.Course.PlanSetups)
                {
                    if (p.Id == planIdwithoutFE)
                    {

                        foreach (Beam b in p.Beams)
                        {
                            if (!b.IsSetupField)
                            {



                                nUmWithNoFE += Math.Round(b.Meterset.Value, 1);
                            }
                        }

                    }

                }
                #endregion
                double diff = 100 * Math.Abs(n_um - nUmWithNoFE) / nUmWithNoFE;
                FE.MeasuredValue = n_um.ToString() + " UM vs. " + nUmWithNoFE.ToString() + " UM (" + diff.ToString("F2") + "%)";
                FE.Infobulle = " La différence d'UM avec le plan " + planIdwithoutFE + " doit être < 10%";
                if (diff > 10)
                {

                    FE.setToFALSE();


                }
                else
                {

                    FE.setToTRUE();


                }

                this._result.Add(FE);
            }
            #endregion

            #region UM Champs filtrés ?
            if (_pinfo.isNOVA)
            {
                Item_Result wedged = new Item_Result();
                wedged.Label = "Champs filtrés";
                wedged.ExpectedValue = "EN COURS";

                if (uncorrFieldWithaWedge != 0)
                {
                    wedged.MeasuredValue = uncorrFieldWithaWedge.ToString() + " champs filtrés avec < 20 UM";
                    wedged.setToFALSE();
                    wedged.Infobulle = uncorrFieldWithaWedge.ToString() + " champs filtrés avec moins de 20 UM";

                }
                else
                {
                    wedged.MeasuredValue = "OK";
                    wedged.setToTRUE();
                    wedged.Infobulle = "Pas de champs filtré avec moins de 20 UM";

                }
                if (_pinfo.machine.Contains("TOM") || _pinfo.machine.Contains("HALCYON"))
                {
                    wedged.setToINFO();
                    wedged.MeasuredValue = "TOMO ou HALCYON: non vérifié";
                }

                this._result.Add(wedged);
            }
            #endregion

            #region Champs < 10 UM ?
            if (!_pinfo.isTOMO)
            {
                Item_Result less10UM = new Item_Result();
                less10UM.Label = "Champs < 10 UM";
                less10UM.ExpectedValue = "EN COURS";

                if (FieldWithLessThan10UM != 0)
                {
                    less10UM.MeasuredValue = FieldWithLessThan10UM.ToString() + " champs avec < 10 UM";
                    less10UM.setToFALSE();
                    less10UM.Infobulle = FieldWithLessThan10UM.ToString() + " champs avec < 10 UM";

                }
                else
                {
                    less10UM.MeasuredValue = "OK";
                    less10UM.setToTRUE();
                    less10UM.Infobulle = "Pas de champs < 10 UM";

                }
                if (_pinfo.machine.Contains("TOM"))
                {
                    less10UM.setToINFO();
                    less10UM.MeasuredValue = "TOMO : non vérifié";
                }
                this._result.Add(less10UM);
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
