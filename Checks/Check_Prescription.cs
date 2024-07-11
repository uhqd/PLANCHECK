using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows;
using System.Windows.Navigation;




namespace PlanCheck
{
    internal class Check_Prescription
    {
        private ScriptContext _ctx;
        private PreliminaryInformation _pinfo;
        private read_check_protocol _rcp;
        public Check_Prescription(PreliminaryInformation pinfo, ScriptContext ctx, read_check_protocol rcp)  //Constructor
        {
            _ctx = ctx;
            _pinfo = pinfo;
            _rcp = rcp;
            Check();

        }

        private List<Item_Result> _result = new List<Item_Result>();
        // private PreliminaryInformation _pinfo;
        private string _title = "Prescription";

        public void Check()
        {


            #region LISTE DES CIBLES DE LA PRESCRIPTION
            if (_pinfo.actualUserPreference.userWantsTheTest("prescriptionVolumes"))
            {
                Item_Result prescriptionVolumes = new Item_Result();
                if (_ctx.PlanSetup.RTPrescription.Status == "Approved")
                {
                    prescriptionVolumes.MeasuredValue = "Approuvée: ";
                    prescriptionVolumes.setToTRUE();
                }
                else
                {
                    prescriptionVolumes.MeasuredValue = "Non approuvée: ";

                    //                    prescriptionVolumes.Label = " Prescription non approuvée (" + targetNumber + " cible(s))";
                    prescriptionVolumes.setToFALSE();
                }
                int targetNumber = 0;
                //prescriptionVolumes.MeasuredValue = "";
                prescriptionVolumes.Infobulle = "information : liste des cibles de la prescription\n";
                foreach (var target in _ctx.PlanSetup.RTPrescription.Targets) //boucle sur les différents niveaux de dose de la prescription
                {
                    targetNumber++;
                    double tot = target.NumberOfFractions * target.DosePerFraction.Dose;
                    prescriptionVolumes.Infobulle += target.TargetId + " : " + target.NumberOfFractions + " x " + target.DosePerFraction.Dose.ToString("N2") + " Gy " + "(" + tot.ToString("N2") + " Gy)\n";
                    prescriptionVolumes.MeasuredValue += target.TargetId + " (" + tot.ToString("N2") + " Gy)  ";
                }

                prescriptionVolumes.ExpectedValue = "info";
                prescriptionVolumes.Label = " Approbation de la prescription pour " + targetNumber + " cible(s) : ";
                

                this._result.Add(prescriptionVolumes);
            }
            #endregion

            #region FRACTIONNEMENT - CIBLE LA PLUS HAUTE
            if (_pinfo.actualUserPreference.userWantsTheTest("fractionation"))
            {
                Item_Result fractionation = new Item_Result();
                //fractionation.Label = "Fractionnement du PTV principal";
                int nPrescribedNFractions = 0;
                double nPrescribedDosePerFraction = 0;
                string PrescriptionName = null;
                double PrescriptionValue = 0;

                fractionation.ExpectedValue = nPrescribedNFractions + " x " + nPrescribedDosePerFraction.ToString("N2") + " Gy";
                double diffDose = 0.0;
                double myDosePerFraction = 0.0;
                int nFraction = 0;
                foreach (var target in _ctx.PlanSetup.RTPrescription.Targets) //boucle sur les différents niveaux de dose de la prescription
                {


                    nPrescribedNFractions = target.NumberOfFractions;
                    if (target.DosePerFraction.Dose > nPrescribedDosePerFraction)  // get the highest dose per fraction level
                    {
                        nPrescribedDosePerFraction = target.DosePerFraction.Dose;
                        PrescriptionValue = target.Value;
                        PrescriptionName = target.TargetId;
                    }
                }
                if (!_pinfo.isTOMO)
                {
                    myDosePerFraction = _ctx.PlanSetup.DosePerFraction.Dose;
                    nFraction = (int)_ctx.PlanSetup.NumberOfFractions;
                }
                else //is tomo
                {
                    if (_pinfo.planReportIsFound)
                    {
                        myDosePerFraction = _pinfo.tprd.Trd.prescriptionDosePerFraction;
                        nFraction = _pinfo.tprd.Trd.prescriptionNumberOfFraction;
                        fractionation.Infobulle = "Données récupées du rapport Aria Documents Dosimétrie Tomotherapy du plan : " + _pinfo.tprd.Trd.planName;
                    }
                }
                if (((_pinfo.isTOMO) && (_pinfo.planReportIsFound)) || (!_pinfo.isTOMO))
                {


                    diffDose = Math.Abs(nPrescribedDosePerFraction - myDosePerFraction);
                    fractionation.MeasuredValue = "Plan : " + nFraction + " x " + myDosePerFraction.ToString("0.00") + " Gy - Prescrits : " + nPrescribedNFractions + " x " + nPrescribedDosePerFraction.ToString("0.00") + " Gy";
                    if ((nPrescribedNFractions == nFraction) && (diffDose < 0.005))
                        fractionation.setToTRUE();
                    else
                        fractionation.setToFALSE();

                    fractionation.Infobulle += "\n\nLe 'nombre de fractions' et la 'dose par fraction' du plan doivent\nêtre conformes à la plus forte prescription (" + _ctx.PlanSetup.RTPrescription.Id +
                        ") : " + nPrescribedNFractions.ToString() + " x " + nPrescribedDosePerFraction.ToString("N2") + " Gy.";

                }
                else
                {
                    fractionation.setToINFO();
                    fractionation.MeasuredValue = "Pas de rapport de plan Tomotherapy dans Aria Documents";
                    fractionation.Infobulle = "Pas de rapport de plan Tomotherapy dans Aria Documents";
                }

                fractionation.Label = "Fractionnement de la cible principale (" + PrescriptionName + ")";
                this._result.Add(fractionation);
            }
            #endregion

            // pas réussi à attraper le % dans la prescription (que dans le plan)
            #region POURCENTAGE DE LA PRESCRIPTION
            if (_pinfo.actualUserPreference.userWantsTheTest("percentage"))
                if (!_pinfo.isTOMO)
                {
                    Item_Result percentage = new Item_Result();
                    double myTreatPercentage = _ctx.PlanSetup.TreatmentPercentage;
                    myTreatPercentage = 100 * myTreatPercentage;
                    percentage.Label = "Pourcentage de traitement";
                    percentage.ExpectedValue = _rcp.prescriptionPercentage;
                    percentage.MeasuredValue = myTreatPercentage.ToString() + "%";
                    if (percentage.ExpectedValue == percentage.MeasuredValue)
                        percentage.setToTRUE();
                    else
                        percentage.setToFALSE();
                    percentage.Infobulle = "Le pourcentage de traitement (onglet Dose) doit être en accord avec";
                    percentage.Infobulle += "\nla valeur de pourcentage du check-protocol " + _rcp.protocolName + " (" + _rcp.prescriptionPercentage + ")";
                    this._result.Add(percentage);
                }
            #endregion

            #region NORMALISATION DU PLAN
            if (_pinfo.actualUserPreference.userWantsTheTest("normalisation"))
            {
                Item_Result normalisation = new Item_Result();
                normalisation.Label = "Mode de normalisation du plan";
                if (!_pinfo.isTOMO)
                {

                    //string normMethod = _ctx.PlanSetup.PlanNormalizationMethod;
                    normalisation.ExpectedValue = _rcp.normalisationMode;
                    normalisation.MeasuredValue = _ctx.PlanSetup.PlanNormalizationMethod;
                    normalisation.setToINFO();
                    if (normalisation.MeasuredValue.Contains("volume")) // si le mode de normalisation contient le mot volume
                    {
                        if (normalisation.ExpectedValue == normalisation.MeasuredValue)
                            normalisation.setToTRUE();
                        else
                            normalisation.setToFALSE();

                        normalisation.MeasuredValue += ": " + _ctx.PlanSetup.TargetVolumeID; // afficher ce volume

                    }
                    if (normalisation.MeasuredValue.Contains("point"))
                    {
                        if (normalisation.MeasuredValue.Contains("100% au point de référence"))
                        {
                            if (normalisation.ExpectedValue.Contains("100% au point de référence"))
                                normalisation.setToTRUE();
                            else
                                normalisation.setToFALSE();

                            if (normalisation.MeasuredValue.Contains("principal"))
                                normalisation.MeasuredValue += " (" + _ctx.PlanSetup.PrimaryReferencePoint.Id + ")";

                        }
                        else
                        {
                            normalisation.setToFALSE();
                        }
                    }

                    if (normalisation.MeasuredValue == "Aucune normalisation de plan")
                        normalisation.setToWARNING();




                    normalisation.Infobulle = "Le mode de normalisation (onglet Dose) doit être en accord avec le check-protocol. Cet item est en WARNING si Aucune normalisation";
                    //normalisation.Infobulle += "\nPour la TOMO l'item est mis en INFO";


                }
                else // tomo
                {
                    if (_pinfo.planReportIsFound)
                    {
                        normalisation.MeasuredValue = _pinfo.tprd.Trd.prescriptionMode;
                        normalisation.Infobulle = "attendue : Median";
                        if (_pinfo.tprd.Trd.prescriptionMode.Contains("Median"))
                        {

                            normalisation.setToTRUE();
                        }
                        else
                        {

                            normalisation.setToFALSE();
                        }
                    }
                    else
                    {
                        normalisation.MeasuredValue = "Pas de rapport de dosimétrie Tomotherapy dans ARIA documents";
                        normalisation.setToWARNING();
                    }
                }


                this._result.Add(normalisation);
            }
            #endregion

            #region NOM DE LA PRESCRIPTION

            if (_pinfo.actualUserPreference.userWantsTheTest("prescriptionName"))
            {
                Item_Result prescriptionName = new Item_Result();
                prescriptionName.Label = "Nom de la prescription";
                prescriptionName.MeasuredValue = _ctx.PlanSetup.RTPrescription.Id;

                String planName = String.Concat(_ctx.PlanSetup.Id.Where(c => !Char.IsWhiteSpace(c))); // remove spaces
                planName = planName.ToUpper();
                String prescriptionId = String.Concat(_ctx.PlanSetup.RTPrescription.Id.Where(c => !Char.IsWhiteSpace(c)));
                prescriptionId = prescriptionId.ToUpper();
                if (planName == prescriptionId)
                {
                    //prescriptionName.MeasuredValue ="OK";
                    prescriptionName.setToTRUE();
                }
                else
                {
                    prescriptionName.MeasuredValue += " (différent du nom du plan)";
                    prescriptionName.setToINFO();
                }
                prescriptionName.Infobulle = "La prescription et le plan doivent avoir le même nom";
                prescriptionName.Infobulle += "\nIl est recommandé de mettre ce nom en commentaire du course\n";
                if (_ctx.Course.Comment == _ctx.PlanSetup.RTPrescription.Id)
                    prescriptionName.Infobulle += "C'est le cas pour ce course";
                this._result.Add(prescriptionName);
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
