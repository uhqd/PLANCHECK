using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using System.Windows;
using System.Windows.Navigation;
using System.Drawing;



namespace PlanCheck
{
    internal class Check_Plan
    {

        private ScriptContext _ctx;
        private PreliminaryInformation _pinfo;
        private read_check_protocol _rcp;
        public Check_Plan(PreliminaryInformation pinfo, ScriptContext ctx, read_check_protocol rcp)  //Constructor
        {
            _rcp = rcp;
            _ctx = ctx;
            _pinfo = pinfo;
            Check();
        }


        private List<Item_Result> _result = new List<Item_Result>();
        // private PreliminaryInformation _pinfo;
        private string _title = "Plan";

        public void Check()
        {

            #region Gating

            Item_Result gating = new Item_Result();
            gating.Label = "Gating";

            if (_ctx.PlanSetup.UseGating)
                gating.MeasuredValue = "Gating activé";
            else
                gating.MeasuredValue = "Gating Désactivé";

            if (_rcp.enebleGating == "Oui")
                gating.ExpectedValue = "Gating activé";
            if (_rcp.enebleGating == "Non")
                gating.ExpectedValue = "Gating Désactivé";

            if (gating.ExpectedValue == gating.MeasuredValue)
                gating.setToTRUE();
            else
                gating.setToFALSE();

            gating.Infobulle = "La case Enable gating doit être en accord avec le check-protocol " + _rcp.protocolName + " (" + gating.ExpectedValue + ")";
            this._result.Add(gating);
            //
            #endregion

            #region Sens des arcs
            if (_pinfo.treatmentType == "VMAT")
            {
                Item_Result RAdirection = new Item_Result();
                RAdirection.Label = "Sens des arcs";

                RAdirection.ExpectedValue = "none";
                int nbeams = 0;
                bool isOk = true;
                string temp = "";
                foreach (Beam b in _ctx.PlanSetup.Beams)
                {
                    if (!b.IsSetupField)
                    {
                        nbeams++;
                        string directionOfThisOne = b.GantryDirection.ToString().ToUpper();

                        if (directionOfThisOne == temp)
                            isOk = false;

                        if (directionOfThisOne == "CLOCKWISE")
                            RAdirection.MeasuredValue += "CC, ";
                        else
                            RAdirection.MeasuredValue += "CCW, ";

                        temp = directionOfThisOne;

                    }


                }

                RAdirection.MeasuredValue = nbeams.ToString() + " arcs: " + RAdirection.MeasuredValue;
                if (isOk)
                    RAdirection.setToTRUE();
                else
                    RAdirection.setToWARNING();
                RAdirection.Infobulle = "Les arcs doivent être alternativement dans le sens horaire (CW) et antihoraire (CCW)";
                this._result.Add(RAdirection);
            }
            #endregion

            #region Colli non nul en VMAT
            if (_pinfo.treatmentType == "VMAT")
            {
                Item_Result colli = new Item_Result();
                colli.Label = "Collimateur RA";

                colli.ExpectedValue = "none";
                int nbeams = 0;
                bool isOk = true;
                foreach (Beam b in _ctx.PlanSetup.Beams)
                {
                    if (!b.IsSetupField)
                    {
                        nbeams++;
                        foreach (ControlPoint cp in b.ControlPoints)
                        {
                            if (cp.CollimatorAngle == 0.0)
                                isOk = false;
                        }

                    }


                }


                if (isOk)
                {
                    colli.MeasuredValue = "Pas de collimateur à 0° pour les " + nbeams + " champs RA";
                    colli.setToTRUE();
                }
                else
                {
                    colli.MeasuredValue = "Au moins un CP avec collimateur à 0°";
                    colli.setToFALSE();
                }
                colli.Infobulle = "Le collimateur ne doit pas être à 0° pour les champs RA ";
                this._result.Add(colli);
            }
            #endregion

            #region FE : MLC modifiés ou non ?
            // if  :
            //  It is a plan with extended fluence
            // And the same plan without FE is found
            // And advanced user mode
            if ((_pinfo.isFE) && (_pinfo.fondNonFEPlan))
            {
                Item_Result FE_MLC = new Item_Result();
                FE_MLC.Label = "Fluence étendue : modification MLC";
                FE_MLC.ExpectedValue = "EN COURS";
                PlanSetup pNonFE = _ctx.Course.PlanSetups.Where(p => p.Id == _pinfo.planIdwithoutFE).FirstOrDefault();
                List<string> modifiedMLC = new List<string>();
                List<string> nonModifiedMLC = new List<string>();

                int beamindex = 0;
                foreach (Beam b in _ctx.PlanSetup.Beams)
                {
                    // this is a test
                    if (!b.IsSetupField)
                    {
                        double sum1 = 0.0;
                        //int cpIndex = 0;
                        foreach (ControlPoint cp in b.ControlPoints)
                        {
                            //int lfIndex = 0;
                            foreach (float f in cp.LeafPositions)
                            {
                               // if(f != pNonFE.Beams.ElementAt(beamindex).ControlPoints.ElementAt(cpIndex).LeafPositions[lfIndex])
                                //{ }

                                sum1 += f;
                            }
                            //cpIndex++;
                        }
                        double sum2 = 0.0;
                        foreach (ControlPoint cp in pNonFE.Beams.ElementAt(beamindex).ControlPoints)
                        {
                            foreach (float f in cp.LeafPositions)
                            {
                                sum2 += f;
                            }
                        }
                        if (sum1 == sum2)
                            nonModifiedMLC.Add(b.Id);
                        else
                            modifiedMLC.Add(b.Id);

                    }
                    beamindex++;
                }

                FE_MLC.MeasuredValue = modifiedMLC.Count + " faisceaux modifiés et " + nonModifiedMLC.Count + " non modifiés";
                FE_MLC.setToINFO();
                FE_MLC.Infobulle = "Liste si le MLC est modifié entre le plan FE et non FE";

                this._result.Add(FE_MLC);
            }
            #endregion

            #region DCS
            /*
             * Impossible to get DCS value ?
            Item_Result dcs = new Item_Result();
            dcs.Label = "DCS";

            

            dcs.Infobulle = "Le plan doit avoir des valeurs de décalages de tables renseignés";
            this._result.Add(dcs);
            */
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

