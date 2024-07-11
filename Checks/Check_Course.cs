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
using PlanCheck.Users;


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

            
            if (_pinfo.actualUserPreference.userWantsTheTest("currentCourseStatus"))
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
            }

            
            if (_pinfo.actualUserPreference.userWantsTheTest("planApprove"))
            {
                #region Is actual Plan PlanningApproved ? 

                Item_Result planApprove = new Item_Result();
                planApprove.Label = "Statut d'approbation du plan";
                planApprove.ExpectedValue = "EN COURS";



                planApprove.Infobulle = "Le plan doit être Planning Approved";
                if (!_pinfo.isTOMO)
                {
                    String[] beautifulDoctorName = _ctx.PlanSetup.PlanningApprover.Split('\\');
                    String[] TAname = _ctx.PlanSetup.TreatmentApprover.Split('\\');

                    if (_ctx.PlanSetup.ApprovalStatus.ToString() == "PlanningApproved")
                    {

                        planApprove.MeasuredValue = "Plan approuvé par le Dr " + beautifulDoctorName[1].ToUpper();// + _ctx.PlanSetup.PlanningApprover;s[0].ToString().ToUpper() + s.Substring(1);
                        planApprove.setToTRUE();

                    }
                    else if (_ctx.PlanSetup.ApprovalStatus.ToString() == "TreatmentApproved")
                    {

                        planApprove.MeasuredValue = "Treatment approved";
                        //Approuvé par le Dr " + _ctx.PlanSetup.TreatmentApprover + [1].ToUpper();// + _ctx.PlanSetup.PlanningApprover;s[0].ToString().ToUpper() + s.Substring(1);
                        planApprove.Infobulle += "\n\nLe plan est en état Treat Approved";
                        planApprove.Infobulle += "\nPlanning approver: " + beautifulDoctorName[1].ToUpper() + "\nTreatment approver " + TAname[1].ToUpper();
                        planApprove.setToWARNING();
                    }
                    else
                    {
                        planApprove.MeasuredValue = _ctx.PlanSetup.ApprovalStatus.ToString();// "Différent de Planning Approved";
                        planApprove.setToFALSE();
                        //planApprove.Infobulle = "Le plan doit être Planning Approved";
                    }
                }
                else // else this is a tomo plan. A plan SEA must be planning approved with non-zero UMs  and dose to ref point
                {

                    if (_pinfo.SEAplanName != "null")
                    {
                        PlanSetup p = _ctx.Course.PlanSetups.FirstOrDefault(x => x.Id.Equals(_pinfo.SEAplanName, StringComparison.OrdinalIgnoreCase));
                        string msg1 = p.PrimaryReferencePoint.DailyDoseLimit.Dose.ToString();
                        string msg2 = p.Beams.First().Meterset.Value.ToString();
                        // MessageBox.Show(msg);

                        String[] beautifulDoctorName = p.PlanningApprover.Split('\\');
                        String[] TAname = p.TreatmentApprover.Split('\\');

                        if (p.ApprovalStatus.ToString() == "PlanningApproved")
                        {
                            planApprove.MeasuredValue = "Tomo : Plan " + p.Id + " approuvé par le Dr " + beautifulDoctorName[1].ToUpper();// + _ctx.PlanSetup.PlanningApprover;s[0].ToString().ToUpper() + s.Substring(1);
                            planApprove.setToTRUE();

                        }
                        else if (p.ApprovalStatus.ToString() == "TreatmentApproved")
                        {
                            planApprove.MeasuredValue = "Treatment approved";
                            //Approuvé par le Dr " + _ctx.PlanSetup.TreatmentApprover + [1].ToUpper();// + _ctx.PlanSetup.PlanningApprover;s[0].ToString().ToUpper() + s.Substring(1);
                            planApprove.Infobulle += "\n\nLe plan  " + p.Id + " est en état Treat Approved";
                            planApprove.Infobulle += "\nPlanning approver: " + beautifulDoctorName[1].ToUpper() + "\nTreatment approver " + TAname[1].ToUpper();
                            planApprove.setToWARNING();
                        }
                        else
                        {
                            planApprove.MeasuredValue = "TOMO : " + p.Id + " : " + p.ApprovalStatus.ToString();// "Différent de Planning Approved";
                            planApprove.setToFALSE();

                        }

                        if (msg1.Contains("NaN"))
                        {
                            planApprove.setToFALSE();
                            planApprove.Infobulle += "\n\nATTENTION : le plan " + p.Id + " n'a pas de dose au point de référence prinicpal";
                        }
                        else if (msg2.Contains("NaN"))
                        {
                            planApprove.setToFALSE();
                            planApprove.Infobulle += "\n\nATTENTION : le plan " + p.Id + " n'a pas d'UMs";
                        }
                        else
                            planApprove.Infobulle += "\n\nUMs et Dose au point principal sont bien renseignés dans le plan " + p.Id;


                    }
                    else
                    {
                        planApprove.MeasuredValue = "TOMO : pas de plan SEA";
                        planApprove.Infobulle += "\n\nPour les plans TOMO un plan SEA doit exister et être approuvé";
                        planApprove.setToFALSE();
                    }

                }

                this._result.Add(planApprove);
                #endregion
            }

           
            if (_pinfo.actualUserPreference.userWantsTheTest("coursesStatus"))
            {
                #region other courses

                Item_Result coursesStatus = new Item_Result();
                coursesStatus.Label = "Statut des autres courses";

                List<string> otherCoursesTerminated = new List<string>();
                List<string> otherCoursesNotOKNotQA_butRecent = new List<string>();
                List<string> otherQACoursesOK = new List<string>();
                List<string> oldCourses = new List<string>();
                coursesStatus.ExpectedValue = "...";



                coursesStatus.Infobulle = "Les courses doivent être dans l'état TERMINE\n";
              /*  coursesStatus.Infobulle += "\nERREUR si au moins un course (CQ ou non) est EN COURS cours depuis > " + maxNumberOfDays + " jours";
                coursesStatus.Infobulle += "\nWARNING si au moins un course (non CQ) est en cours depuis moins de " + maxNumberOfDays + " jours";
                coursesStatus.Infobulle += "\nOK si tous les course sont TERMINE (CQ ou non) ou EN COURS (CQ) depuis moins de " + maxNumberOfDays + " jours";
              */


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
              // coursesStatus.Infobulle += "\n\nListe des courses\n";
                if (oldCourses.Count() > 0)
                {
                    coursesStatus.Infobulle += "\nAnciens et toujours EN COURS : \n";
                    foreach (string s in oldCourses)
                        coursesStatus.Infobulle += " - " + s + "\n";
                }
                if (otherCoursesNotOKNotQA_butRecent.Count() > 0)
                {
                    coursesStatus.Infobulle += "\nEN COURS mais récents (non CQ) : \n";
                    foreach (string s in otherCoursesNotOKNotQA_butRecent)
                        coursesStatus.Infobulle += " - " + s + "\n";
                }
                if (otherQACoursesOK.Count() > 0)
                {
                    coursesStatus.Infobulle += "\nEN COURS mais récents (CQ) : \n";
                    foreach (string s in otherQACoursesOK)
                        coursesStatus.Infobulle += " - " + s + "\n";
                }
                if (otherCoursesTerminated.Count() > 0)
                {
                    coursesStatus.Infobulle += "\nTerminés : \n";
                    foreach (string s in otherCoursesTerminated)
                        coursesStatus.Infobulle += " - " + s + "\n";
                }
                #endregion

                if (oldCourses.Count() > 0)
                {
                    coursesStatus.setToFALSE();
                    coursesStatus.MeasuredValue = "Au moins un course ancien est EN COURS (voir détail)\n";
                }
                else if (otherCoursesNotOKNotQA_butRecent.Count() > 0)
                {
                    coursesStatus.setToWARNING();
                    coursesStatus.MeasuredValue = "Au moins un course récent (hors CQ) est EN COURS (voir détail)\n";
                }
                else
                {
                    coursesStatus.setToTRUE();
                    coursesStatus.MeasuredValue = "Pas de courses EN COURS (voir détail)";
                }
                this._result.Add(coursesStatus);
                #endregion
            }

            
            if (_pinfo.actualUserPreference.userWantsTheTest("tomoReportApproved"))
            {
                #region Tomo report approved ?  
                if (_pinfo.isTOMO)
                {

                    Item_Result tomoReportApproved = new Item_Result();
                    tomoReportApproved.Label = "Approbation du rapport Tomo";
                    tomoReportApproved.ExpectedValue = "";

                    if (_pinfo.planReportIsFound)
                    {
                        if (_pinfo.tprd.Trd.approvalStatus == "Approved")
                        {
                            string str = _pinfo.tprd.Trd.approverID.Trim();
                            string str2 = char.ToUpper(str[0]) + str.Substring(1);
                            tomoReportApproved.MeasuredValue = "Rapport de Dosimétrie Tomotherapy approuvé par Dr " + str2; // Dr Dalmasso
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
