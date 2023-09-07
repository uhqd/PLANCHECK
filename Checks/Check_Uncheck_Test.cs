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
    internal class Check_Uncheck_Test
    {
        public Check_Uncheck_Test(PreliminaryInformation pinfo, ScriptContext ctx, read_check_protocol rcp)  //Constructor
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

        private string _title = "Uncheck";
        //test

        public void Check()
        {



            #region CT
            Item_Result CT = new Item_Result();
            CT.setToUNCHECK();
            CT.Label = "CT";
            CT.ExpectedValue = "dumb";
            CT.MeasuredValue = "CT correct ? ";

            if (_pinfo.AlgoName.ToUpper().Contains("CUROS"))
                CT.MeasuredValue += "\n  Accuros : pas d'injection";
            if (_context.PlanSetup.Id.ToUpper().Contains("DIBH"))
                CT.MeasuredValue += "\n  DIBH : verfifier le CT";
            if (_context.PlanSetup.Id.ToUpper().Contains("_R"))
                CT.MeasuredValue += "\n  Replanif. : verfifier le CT";


            CT.Infobulle = CT.MeasuredValue;
            this._result.Add(CT);
            #endregion

            #region Parametres Asservissement
            if (_rcp.enebleGating == "Oui")
            {


                Item_Result gatingParams = new Item_Result();
                gatingParams.Label = "Paramètres d'asservissement";
                gatingParams.ExpectedValue = "dumb";
                gatingParams.MeasuredValue = "Gating activé : vérifier les paramètres d'asservissment";
                gatingParams.setToUNCHECK();
                gatingParams.Infobulle = gatingParams.Infobulle = gatingParams.MeasuredValue;
                this._result.Add(gatingParams);
            }
            #endregion

            #region origine

            Item_Result origine = new Item_Result();
            origine.Label = "Origine";
            origine.ExpectedValue = "dumb";
            origine.MeasuredValue = "Vérifier la position de l'origine";
            if (_pinfo.AlgoName.ToUpper().Contains("CUROS"))
                CT.MeasuredValue += "\n  Accuros : les billes doivent être effacées";
            if (_pinfo.isTOMO)
                CT.MeasuredValue += "\n  Tomo : Décalages lat max 1.5 cm  (move couch) + 6 cm de contention désolidarisée.";

            origine.setToUNCHECK();
            origine.Infobulle = origine.MeasuredValue;
            this._result.Add(origine);
            #endregion

            #region contours

            Item_Result contours = new Item_Result();
            contours.Label = "Contours";
            contours.ExpectedValue = "dumb";
            contours.MeasuredValue = "Présence vérifié par Plancheck. Vérifier cohérence et en particulier les overlap";

            contours.setToUNCHECK();
            contours.Infobulle = contours.MeasuredValue;
            this._result.Add(contours);
            #endregion

            #region LOT TOMO
            if (_pinfo.isTOMO)
            {


                Item_Result LOT = new Item_Result();
                LOT.Label = "LOT en Tomo";
                LOT.ExpectedValue = "dumb";
                LOT.MeasuredValue = "mean > 100 ms max > 231 ms";
                LOT.setToUNCHECK();
                LOT.Infobulle = LOT.MeasuredValue;
                this._result.Add(LOT);
            }
            #endregion

            #region objectif optim
            if (_pinfo.isModulated)
            {


                Item_Result objOpt = new Item_Result();
                objOpt.Label = "Objectifs d'optimisation";
                objOpt.ExpectedValue = "dumb";
                objOpt.MeasuredValue = "Vérifier les objectifs et en particulier les PTVs";
                objOpt.setToUNCHECK();
                objOpt.Infobulle = objOpt.MeasuredValue;
                this._result.Add(objOpt);
            }
            #endregion

            #region distribution de dose


            Item_Result doseDistri = new Item_Result();
            doseDistri.Label = "Distribution de dose";
            doseDistri.ExpectedValue = "dumb";


            doseDistri.MeasuredValue = "Vérifier la distribution de dose";
            if (_context.PlanSetup.Id.ToUpper().Contains("FE"))
                doseDistri.MeasuredValue += "\n Plan FE ? vérifer fluence";
            if (_pinfo.isHyperArc)
                doseDistri.MeasuredValue += "\n HyperArc ? Utiliser le script";
            if (_context.PlanSetup.Id.ToUpper().Contains("STEC"))
                doseDistri.MeasuredValue += "\n STEC ? Utiliser le script";
            if (_context.PlanSetup.Id.ToUpper().Contains("STIC"))
                doseDistri.MeasuredValue += "\n STIC ? Utiliser le script";
            if (_pinfo.AlgoName.ToUpper().Contains("CUROS"))
                doseDistri.MeasuredValue += "\n Acuros ? vérifier OAR avec AAA";
            doseDistri.setToUNCHECK();
            doseDistri.Infobulle = doseDistri.MeasuredValue;
            this._result.Add(doseDistri);

            #endregion

            #region DRR

            if (!_pinfo.isTOMO)
            {
                Item_Result DRR = new Item_Result();
                DRR.Label = "DRR et paramètres";
                DRR.ExpectedValue = "dumb";
                DRR.MeasuredValue = "Vérifier les DRR et les paramètres";

                DRR.MeasuredValue += "\n contours des PTVs, trachée, ...";

                if (_context.PlanSetup.Id.ToUpper().Contains("STIC"))
                    DRR.MeasuredValue += "\n STIC : Vérifier Exactrac, 0.5 mm 0.5 °";

                DRR.setToUNCHECK();
                DRR.Infobulle = DRR.MeasuredValue;
                this._result.Add(DRR);
            }
            #endregion

            #region points de ref

            if (!_pinfo.isTOMO)
            {
                Item_Result refPoint = new Item_Result();
                refPoint.Label = "Points de référence";
                refPoint.ExpectedValue = "dumb";
                refPoint.MeasuredValue = "Limites de dose";
                if (!_pinfo.isModulated)
                    refPoint.MeasuredValue += "\n RTC ? limite de dose + 0.2 Gy";

                if (_context.PlanSetup.Id.ToUpper().Contains("DIBH"))
                    refPoint.MeasuredValue += "\n DIBH : pas de DCS";

                refPoint.setToUNCHECK();
                refPoint.Infobulle = refPoint.MeasuredValue;
                this._result.Add(refPoint);
            }
            #endregion

            #region programmation du plan

            Item_Result plancSchedule = new Item_Result();
            plancSchedule.Label = "Programmation du plan";
            plancSchedule.ExpectedValue = "dumb";
            plancSchedule.MeasuredValue = "En accord avec prescription";

            if (_pinfo.isTOMO)
            {
                plancSchedule.MeasuredValue += "\n TOMO : plan SEA programmé";
            }
            plancSchedule.setToUNCHECK();
            plancSchedule.Infobulle = plancSchedule.MeasuredValue;
            this._result.Add(plancSchedule);

            #endregion

            #region documents dosi

            if (!_pinfo.isTOMO)
            {
                Item_Result dosiReport = new Item_Result();
                dosiReport.Label = "Rapport de dosimétrie";
                dosiReport.ExpectedValue = "dumb";
                dosiReport.MeasuredValue = "Document correspondant au plan";

                if (_context.PlanSetup.Id.ToUpper().Contains("_R"))
                    dosiReport.MeasuredValue += "\n Replanif : somme HDV";

                dosiReport.setToUNCHECK();
                dosiReport.Infobulle = dosiReport.MeasuredValue;
                this._result.Add(dosiReport);
            }
            #endregion

            #region documents fiche de pos


            Item_Result setupreport = new Item_Result();
            setupreport.Label = "Fiche de positionnement";
            setupreport.ExpectedValue = "dumb";
            setupreport.MeasuredValue = "Document correspondant au plan";

            if (!_pinfo.isModulated)
                setupreport.MeasuredValue += "\n RTC ? : copie BEV";
            if (_context.PlanSetup.Id.ToUpper().Contains("STEC"))
                setupreport.MeasuredValue += "\n STEC ? : vérifier paramètres de gating";

            setupreport.setToUNCHECK();
            setupreport.Infobulle = setupreport.MeasuredValue;
            this._result.Add(setupreport);
            #endregion

            #region documents Dosecheck

            if (_pinfo.doseCheckIsNeeded)
            {
                Item_Result dosecheck = new Item_Result();
                dosecheck.Label = "Dosecheck";
                dosecheck.ExpectedValue = "dumb";
                dosecheck.MeasuredValue = "Document correspondant au plan";


                dosecheck.setToUNCHECK();
                dosecheck.Infobulle = dosecheck.MeasuredValue;
                this._result.Add(dosecheck);
            }

            #endregion
            #region approbation


            Item_Result approbation = new Item_Result();
            approbation.Label = "Approbation";
            approbation.ExpectedValue = "dumb";
            approbation.MeasuredValue = "Signature physicien";
            approbation.MeasuredValue += "\n Plan aria";
            if (_pinfo.doseCheckIsNeeded)
                approbation.MeasuredValue += "\n Dosecheck";
            approbation.MeasuredValue += "\n Fiche de positionnement";
            if (_pinfo.isTOMO)
                approbation.MeasuredValue += "\n Tomo : passage des anciens plans en Hold. Fractions ajustées";




            approbation.setToUNCHECK();
            approbation.Infobulle = approbation.MeasuredValue;
            this._result.Add(approbation);


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
