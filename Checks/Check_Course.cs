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
    internal class Check_Course
    {
        private ScriptContext _ctx;
        private PreliminaryInformation _pinfo;
        private int maxNumberOfDays = 8;
        public Check_Course(PreliminaryInformation pinfo, ScriptContext ctx)  //Constructor
        {
            _ctx = ctx;
            _pinfo = pinfo;
            Check();

        }

        private List<Item_Result> _result = new List<Item_Result>();
        // private PreliminaryInformation _pinfo;
        private string _title = "Statut des Courses et du plan";

        public void Check()
        {

            #region IS ACTUAL COURSE "EN COURS" ? 
            Item_Result currentCourseStatus = new Item_Result();
            currentCourseStatus.Label = "Course " + _ctx.Course.Id + " (Course ouvert)";
            currentCourseStatus.ExpectedValue = "EN COURS";
            if (_ctx.Course.CompletedDateTime == null)
            {
                currentCourseStatus.MeasuredValue = "EN COURS";
                currentCourseStatus.setToTRUE();
            }
            else
            {
                currentCourseStatus.MeasuredValue = "TERMINE";
                currentCourseStatus.setToFALSE();
            }
            currentCourseStatus.Infobulle = "L'état du course actuel doit être EN COURS";
            this._result.Add(currentCourseStatus);
            #endregion

            #region Is actual Plan PlanningApproved ? 
            Item_Result approve = new Item_Result();
            approve.Label = "Statut d'approbation du plan";
            approve.ExpectedValue = "EN COURS";



            approve.Infobulle = "Le plan doit être Planning Approved";
            if (!_pinfo.isTOMO)
            {
                String[] beautifulDoctorName = _ctx.PlanSetup.PlanningApprover.Split('\\');
                String[] TAname = _ctx.PlanSetup.TreatmentApprover.Split('\\');

                if (_ctx.PlanSetup.ApprovalStatus.ToString() == "PlanningApproved")
                {

                    approve.MeasuredValue = "Plan approuvé par le Dr " + beautifulDoctorName[1].ToUpper();// + _ctx.PlanSetup.PlanningApprover;s[0].ToString().ToUpper() + s.Substring(1);
                    approve.setToTRUE();

                }
                else if (_ctx.PlanSetup.ApprovalStatus.ToString() == "TreatmentApproved")
                {

                    approve.MeasuredValue = "Treatment approved";
                    //Approuvé par le Dr " + _ctx.PlanSetup.TreatmentApprover + [1].ToUpper();// + _ctx.PlanSetup.PlanningApprover;s[0].ToString().ToUpper() + s.Substring(1);
                    approve.Infobulle += "\n\nLe plan est en état Treat Approved";
                    approve.Infobulle += "\nPlanning approver: " + beautifulDoctorName[1].ToUpper() + "\nTreatment approver " + TAname[1].ToUpper();
                    approve.setToWARNING();
                }
                else
                {
                    approve.MeasuredValue = _ctx.PlanSetup.ApprovalStatus.ToString();// "Différent de Planning Approved";
                    approve.setToFALSE();
                    //approve.Infobulle = "Le plan doit être Planning Approved";
                }
            }
            else // else this is a tomo plan. A plan SEA must be planning approved
            {
                bool foundA_SEA_plan = false;
                foreach (PlanSetup p in _ctx.Course.PlanSetups)
                {
                    if (p.Id.Contains("SEA"))
                    {
                        foundA_SEA_plan = true;
                        String[] beautifulDoctorName = p.PlanningApprover.Split('\\');
                        String[] TAname = p.TreatmentApprover.Split('\\');

                        if (p.ApprovalStatus.ToString() == "PlanningApproved")
                        {
                            approve.MeasuredValue = "Tomo : Plan " + p.Id + " approuvé par le Dr " + beautifulDoctorName[1].ToUpper();// + _ctx.PlanSetup.PlanningApprover;s[0].ToString().ToUpper() + s.Substring(1);
                            approve.setToTRUE();

                        }
                        else if (p.ApprovalStatus.ToString() == "TreatmentApproved")
                        {
                            approve.MeasuredValue = "Treatment approved";
                            //Approuvé par le Dr " + _ctx.PlanSetup.TreatmentApprover + [1].ToUpper();// + _ctx.PlanSetup.PlanningApprover;s[0].ToString().ToUpper() + s.Substring(1);
                            approve.Infobulle += "\n\nLe plan  " + p.Id + " est en état Treat Approved";
                            approve.Infobulle += "\nPlanning approver: " + beautifulDoctorName[1].ToUpper() + "\nTreatment approver " + TAname[1].ToUpper();
                            approve.setToWARNING();
                        }
                        else
                        {
                            approve.MeasuredValue = "TOMO : " + p.Id + " : " + p.ApprovalStatus.ToString();// "Différent de Planning Approved";
                            approve.setToFALSE();
                            //approve.Infobulle = "Le plan doit être Planning Approved";
                        }
                    }
                }
                if (!foundA_SEA_plan)
                {
                    approve.MeasuredValue = "TOMO : pas de plan SEA";
                    approve.Infobulle += "\n\nPour les plans TOMO un plan SEA doit exister et être approuvé";
                    approve.setToFALSE();
                }

            }

            this._result.Add(approve);
            #endregion

            #region other courses
            Item_Result myCourseStatus = new Item_Result();
            myCourseStatus.Label = "Statut des autres courses";

            List<string> otherCoursesTerminated = new List<string>();
            List<string> otherCoursesNotOKNotQA_butRecent = new List<string>();
            List<string> otherQACoursesOK = new List<string>();
            List<string> oldCourses = new List<string>();
            myCourseStatus.ExpectedValue = "...";



            myCourseStatus.Infobulle = "Les courses doivent être dans l'état TERMINE\n";
            myCourseStatus.Infobulle += "\nERREUR si au moins un course (CQ ou non) est EN COURS cours depuis > " + maxNumberOfDays + " jours";
            myCourseStatus.Infobulle += "\nWARNING si au moins un course (non CQ) est en cours depuis moins de " + maxNumberOfDays + " jours";
            myCourseStatus.Infobulle += "\nOK si tous les course sont TERMINE (CQ ou non) ou EN COURS (CQ) depuis moins de " + maxNumberOfDays + " jours";



            foreach (Course courseN in _ctx.Patient.Courses) // loop on the courses
            {

                if (courseN.Id != _ctx.Course.Id) // do not test current course
                    if (courseN.CompletedDateTime != null) // --> terminated courses = there is a  completed date time
                    {
                        otherCoursesTerminated.Add(courseN.Id + " terminé le " + courseN.CompletedDateTime.ToString());
                    }
                    else // course not terminated
                    {
                        DateTime myToday = DateTime.Today;
                        int nDays = (myToday - (DateTime)courseN.StartDateTime).Days;
                        if (nDays < maxNumberOfDays) // if recent
                        {
                            int itIsaQA_Course = 0;
                            foreach (PlanSetup p in courseN.PlanSetups)
                            {
                                if (p.PlanIntent.ToString() != "VERIFICATION") // is there at least one  nonQA plan in the course ? 
                                {
                                    itIsaQA_Course = 0;  // yes --> not a QA course
                                    break;
                                }
                                itIsaQA_Course = 1;  // only QA Plans in the course --> QA course
                            }
                            if (itIsaQA_Course == 0) // en cours, recent, non QA
                            {
                                otherCoursesNotOKNotQA_butRecent.Add(courseN.Id + " (" + nDays + " jours)");

                            }
                            else // en cours, recent,  QA
                            {
                                otherQACoursesOK.Add(courseN.Id + " (" + nDays + " jours)");
                            }
                        }
                        else // if not recent
                        {
                            oldCourses.Add(courseN.Id + " (" + nDays + " jours)");
                        }


                    }
            }
            #region infobulle
            myCourseStatus.Infobulle += "\n\nListe des courses\n";
            if (oldCourses.Count() > 0)
            {
                myCourseStatus.Infobulle += "\nAnciens et toujours EN COURS : \n";
                foreach (string s in oldCourses)
                    myCourseStatus.Infobulle += " - " + s + "\n";
            }
            if (otherCoursesNotOKNotQA_butRecent.Count() > 0)
            {
                myCourseStatus.Infobulle += "\nEN COURS mais récents (non CQ) : \n";
                foreach (string s in otherCoursesNotOKNotQA_butRecent)
                    myCourseStatus.Infobulle += " - " + s + "\n";
            }
            if (otherQACoursesOK.Count() > 0)
            {
                myCourseStatus.Infobulle += "\nEN COURS mais récents (CQ) : \n";
                foreach (string s in otherQACoursesOK)
                    myCourseStatus.Infobulle += " - " + s + "\n";
            }
            if (otherCoursesTerminated.Count() > 0)
            {
                myCourseStatus.Infobulle += "\nTerminés : \n";
                foreach (string s in otherCoursesTerminated)
                    myCourseStatus.Infobulle += " - " + s + "\n";
            }
            #endregion

            if (oldCourses.Count() > 0)
            {
                myCourseStatus.setToFALSE();
                myCourseStatus.MeasuredValue = "Au moins un course ancien est EN COURS (voir détail)\n";
            }
            else if (otherCoursesNotOKNotQA_butRecent.Count() > 0)
            {
                myCourseStatus.setToWARNING();
                myCourseStatus.MeasuredValue = "Au moins un course récent (hors CQ) est EN COURS (voir détail)\n";
            }
            else
            {
                myCourseStatus.setToTRUE();
                myCourseStatus.MeasuredValue = "Pas de courses EN COURS (voir détail)";
            }
            this._result.Add(myCourseStatus);
            #endregion

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
                            if (p.ApprovalStatus.ToString() == "TreatmentApproved")
                            {
                                var theDateTime = DateTime.Parse(p.TreatmentApprovalDate.ToString(), cultureInfo);
                                anteriorTraitementList.Add(theDateTime.ToString("d") + "\t" + p.Id);
                            }
                    }
                }
            }
            if (anteriorTraitementList.Count > 0)
            {
                anteriorTraitement.setToWARNING();
                anteriorTraitement.MeasuredValue = anteriorTraitementList.Count.ToString() + " traitements antérieurs détéctés";
                anteriorTraitement.Infobulle = "Les plans suivants sont à l'état TreatmentApproved";
                anteriorTraitement.Infobulle += "\nIl peut s'agir de traitements concomitants ou de traitements antérieurs :\n";
                foreach (string s in anteriorTraitementList)
                    anteriorTraitement.Infobulle += "\n - " + s;

            }
            else
            {
                anteriorTraitement.setToTRUE();
                anteriorTraitement.MeasuredValue = anteriorTraitementList.Count.ToString() + " traitement antérieur détécté";
                anteriorTraitement.Infobulle = "Aucun Plan au status TreatmentApproved dans les courses du patient";
            }

            this._result.Add(anteriorTraitement);
            #endregion

            #region Tomo report approved ?  
            if (_pinfo.isTOMO)
            {
             
                Item_Result tomoReportApproved = new Item_Result();
                tomoReportApproved.Label = "Approbation du rapport Tomo";
                tomoReportApproved.ExpectedValue = "";

                if (_pinfo.positionReportIsFound)
                {
                    if(_pinfo.tprd.Trd.approvalStatus == "Approved")
                    {
                        string str = _pinfo.tprd.Trd.approverID.Trim();
                        string str2 = char.ToUpper(str[0]) + str.Substring(1);
                        tomoReportApproved.MeasuredValue = "Rapport de Dosimétrie Tomotherapy approuvé par Dr " +  str2; // Dr Dalmasso
                        tomoReportApproved.setToTRUE();
                    }
                    else
                    {
                        tomoReportApproved.MeasuredValue = "Rapport de Dosimétrie Tomotherapy non approuvé";
                        tomoReportApproved.setToFALSE();
                    }
                        


                }
                else
                {
                    tomoReportApproved.setToFALSE();
                    tomoReportApproved.MeasuredValue = "Pas de rapport de Dosimétrie Tomotherapy dans Aria Documents";
                }

                tomoReportApproved.Infobulle = "Le rapport pdf du plan Tomotherapy doit être approuvé";
                this._result.Add(tomoReportApproved);
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
