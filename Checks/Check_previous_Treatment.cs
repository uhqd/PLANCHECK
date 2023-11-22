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
      
        public Check_previous_Treatment(PreliminaryInformation pinfo, ScriptContext ctx)  //Constructor
        {
            _ctx = ctx;
            _pinfo = pinfo;
            Check();

        }

        private List<Item_Result> _result = new List<Item_Result>();
        // private PreliminaryInformation _pinfo;
        private string _title = "Traitements antérieurs";

        public void Check()
        {

            #region previous treatments
            Item_Result anteriorTraitement = new Item_Result();
            List<string> anteriorTraitementList = new List<string>();
            anteriorTraitement.Label = "Traitements antérieurs";
            anteriorTraitement.ExpectedValue = "...";
            var cultureInfo = new CultureInfo("fr-FR");

            foreach (Course c in _ctx.Patient.Courses) // loop courses
            {
                foreach (PlanSetup p in c.PlanSetups) // loop plan
                {
                    if ((c.Id != _ctx.Course.Id) && (p.Id != _ctx.PlanSetup.Id)) // if not same course AND same plan: in other course a plan with the same name can exist
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

                    }
                }
            }
            if (anteriorTraitementList.Count > 0)
            {
                anteriorTraitement.setToWARNING();
                anteriorTraitement.MeasuredValue = anteriorTraitementList.Count.ToString() + " traitements antérieurs détectés";
                anteriorTraitement.Infobulle = "Les plans suivants sont à l'état TreatmentApproved";
                anteriorTraitement.Infobulle += "\nIl peut s'agir de traitements concomitants ou de traitements antérieurs :\n";
                foreach (string s in anteriorTraitementList)
                    anteriorTraitement.Infobulle += "\n - " + s;

            }
            else
            {
                anteriorTraitement.setToTRUE();
                anteriorTraitement.MeasuredValue = anteriorTraitementList.Count.ToString() + " traitement antérieur détecté";
                anteriorTraitement.Infobulle = "Aucun Plan au status TreatmentApproved dans les courses du patient";
            }

            this._result.Add(anteriorTraitement);
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
