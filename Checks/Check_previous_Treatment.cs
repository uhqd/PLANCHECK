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
using System.Globalization;





namespace PlanCheck
{
    internal class Check_previous_Treatment
    {
        private ScriptContext _ctx;
        private PreliminaryInformation _pinfo;
        private List<string> nameOfOtherCenters;
        public Check_previous_Treatment(PreliminaryInformation pinfo, ScriptContext ctx)  //Constructor
        {
            _ctx = ctx;
            _pinfo = pinfo;
            nameOfOtherCenters = new List<string>();
            
            // list of cancer centers
            nameOfOtherCenters.Add("CARCA");
            nameOfOtherCenters.Add("PASTEUR");
            nameOfOtherCenters.Add("IGR");
            nameOfOtherCenters.Add("TARBES");
            nameOfOtherCenters.Add("PAU");
            nameOfOtherCenters.Add("RODEZ");
            nameOfOtherCenters.Add("BAYONNE");
            nameOfOtherCenters.Add("NICE");
            nameOfOtherCenters.Add("ORION");
            nameOfOtherCenters.Add("PERPI");
            Check();

        }

        private List<Item_Result> _result = new List<Item_Result>();
        // private PreliminaryInformation _pinfo;
        private string _title = "Traitements antérieurs";

        public void Check()
        {

            #region previous treatments

            if (_pinfo.actualUserPreference.userWantsTheTest("anteriorTraitement"))
            {
                Item_Result anteriorTraitement = new Item_Result();
                List<string> anteriorTraitementList = new List<string>();
                anteriorTraitement.Label = "Traitements antérieurs";
                anteriorTraitement.ExpectedValue = "...";
                var cultureInfo = new CultureInfo("fr-FR");
                String msg = String.Empty;
                int nOtherCenterDetectedPlan = 0;
                foreach (Course c in _ctx.Patient.Courses) // loop courses
                {
                    foreach (PlanSetup p in c.PlanSetups) // loop plan
                    {
                        if ((c.Id != _ctx.Course.Id) || (p.Id != _ctx.PlanSetup.Id)) // if not same course OR same plan: in other course a plan with the same name can exist
                        {

                            bool validPlan = false;

                            try // exception for old tomo plan with no beam
                            {
                                int nBeams = p.Beams.Count();
                                validPlan = true;
                            }
                            catch
                            {
                                validPlan = false;// do nothing but catch is mandatory
                            }

                            if (validPlan)
                            {
                                if (p.ApprovalStatus.ToString() == "TreatmentApproved")
                                {
                                    var theDateTime = DateTime.Parse(p.TreatmentApprovalDate.ToString(), cultureInfo);
                                    anteriorTraitementList.Add(theDateTime.ToString("d") + "\t" + p.Id);
                                }
                            }
                            else
                            {
                                anteriorTraitementList.Add("Ancien plan corompu, vérifier " + p.Id);

                            }

                            foreach (string s in nameOfOtherCenters)
                            {
                                if (c.Id.ToUpper().Contains(s.ToUpper()) || p.Id.ToUpper().Contains(s.ToUpper()))
                                {
                                    nOtherCenterDetectedPlan++;
                                    msg = s ;
                                }

                            }



                        }
                    }
                }



                int nPlanTA = anteriorTraitementList.Count;
                int nExtDoc = _pinfo.nAriaDocumentExterieur;
                if ((nPlanTA > 0) || (nExtDoc > 0) || (nOtherCenterDetectedPlan > 0))
                {
                    anteriorTraitement.setToWARNING();

                    if (nPlanTA > 0)
                        anteriorTraitement.MeasuredValue += nPlanTA.ToString() + " plan(s) TreatApproved ";
                    if (nExtDoc > 0)
                        anteriorTraitement.MeasuredValue += nExtDoc + " document(s) extérieur(s) ";
                    if (nOtherCenterDetectedPlan > 0)
                        anteriorTraitement.MeasuredValue += nOtherCenterDetectedPlan + " plan(s) extérieur(s)"; ;
                    if (nPlanTA > 0)
                    {
                        anteriorTraitement.Infobulle = "Les plans suivants sont à l'état TreatmentApproved";
                        anteriorTraitement.Infobulle += "\nIl peut s'agir de traitements concomitants ou de traitements antérieurs :\n";
                        foreach (string s in anteriorTraitementList)
                            anteriorTraitement.Infobulle += "\n - " + s;
                    }
                    if (nExtDoc == 1)
                    {
                        anteriorTraitement.Infobulle += "\n" + _pinfo.nAriaDocumentExterieur + " document extérieur présent dans Aria Documents";
                    }
                    if (nOtherCenterDetectedPlan > 0)
                        anteriorTraitement.Infobulle += "\n" + "Un plan au moins contient le mot : " + msg;
                    else
                        anteriorTraitement.Infobulle += "\n" + _pinfo.nAriaDocumentExterieur + " documents extérieurs présents dans Aria Documents";

                }
                else
                {
                    anteriorTraitement.setToTRUE();
                    anteriorTraitement.MeasuredValue = "Aucun autre plan au status TreatApproved ni de documents exterieurs";
                    anteriorTraitement.Infobulle = "Aucun autre plan au status TreatApproved ni de documents exterieurs";
                }

                this._result.Add(anteriorTraitement);
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
