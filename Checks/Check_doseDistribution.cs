using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows;
using System.Text.RegularExpressions;

namespace PlanCheck
{
    internal class Check_doseDistribution
    {
        private ScriptContext _ctx;
        private PreliminaryInformation _pinfo;
        private read_check_protocol _rcp;
        public Check_doseDistribution(PreliminaryInformation pinfo, ScriptContext ctx, read_check_protocol rcp)  //Constructor
        {
            _ctx = ctx;
            _pinfo = pinfo;
            _rcp = rcp;
            Check();

        }

        private bool treatmentIsOnTheLeft() // looks if iso is left or right. For Tomo, looks if the first founded PTV is left or right
        {
            bool itisleft = false;



            if (_pinfo.isTOMO)
            {
                // if(_ctx.PlanSetup.RTPrescription.Id.ToLower().Contains("gche"))

                Structure s = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper().Contains("PTV"));
                if (s.MeshGeometry.Bounds.X > 0)
                    itisleft = true;
            }
            else
            {
                Beam b = _ctx.PlanSetup.Beams.First();
                if (b.IsocenterPosition.x > 0)  // if iso is left
                    itisleft = true;
            }

            return itisleft;


        }
        public string replaceHomoIn(string _structName)  // get poumonHOMO and return PoumonGche or PoumonDt depending on iso x position (return null if they don t exist)
        {
            String structname = _structName;
            // bool isLeft = false;

            if (treatmentIsOnTheLeft())
            {
                if (_structName.Contains("HOMOE"))
                    structname = _structName.Replace("HOMOE", "Gche");
                else
                    structname = _structName.Replace("HOMO", "Gche");
            }
            else
            {
                if (_structName.Contains("HOMOE"))
                    structname = _structName.Replace("HOMOE", "Dte");
                else
                    structname = _structName.Replace("HOMO", "Dt");
            }
            Structure s = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper() == structname.ToUpper());

            if (s != null)
                return structname;
            else
                return null;
        }
        public string replaceControIn(string _structName)  // get poumonHOMO and return PoumonGche or PoumonDt depending on iso x position (return null if they don t exist)
        {
            String structname = _structName;
            // bool isLeft = false;
            Beam b = _ctx.PlanSetup.Beams.First();
            if (treatmentIsOnTheLeft())  // if iso is left
            {
                if (_structName.Contains("CONTROE"))
                    structname = _structName.Replace("CONTROE", "Dte");
                else
                    structname = _structName.Replace("CONTRO", "Dt");

            }
            else
            {
                if (_structName.Contains("CONTROE"))
                    structname = _structName.Replace("CONTROE", "Gche");
                else
                    structname = _structName.Replace("CONTRO", "Gche");


            }

            Structure s = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper() == structname.ToUpper());

            if (s != null)
                return structname;
            else
                return null;
        }

        public string getTheUnit(string theValueWithUnit) // get 20Gy or 12.5cc and return Gy or cc
        {
            string FormatPattern = "(?<vali>\\-?\\d*\\.?\\d+)(?<uniti>%|Gy|cc)";
            MatchCollection mi = Regex.Matches(theValueWithUnit, FormatPattern);
            string s = "";
            if (mi != null) // get an objective <
            {
                if (mi.Count > 0)
                {

                    //Group gVal = mi[0].Groups["vali"];
                    Group gUnit = mi[0].Groups["uniti"];
                    s = gUnit.Value;
                }
                else
                    s = "failed";
            }
            else
                s = "failed";

            return s;

        }
        public double getValueForThisObjective(Structure s, DVHData dvh, string theObjective, string outputUnit)
        {
            double result = 0.0;


            #region mean dose (Gy only)
            if (theObjective == "mean")
            {
                var myMeanDose = dvh.MeanDose;
                result = myMeanDose.Dose;
            }
            #endregion

            #region Vxx
            // exemple V20.1Gy<32.1cc  means that the user wants the result in cc
            // the value 32.1 is not pass in this function (but the unit is passed)
            // V20.1Gy or V20.1%
            if (theObjective.Substring(0, 1) == "V")
            {

                string v_at_d_pattern = @"^V(?<evalpt>\d+\p{P}\d+|\d+)(?<unit>(%|Gy))$"; // matches V50.4Gy or V50.4% 
                                                                                         //var
                var testMatch = Regex.Matches(theObjective, v_at_d_pattern);
                if (testMatch.Count != 0) // count is 1
                {
                    Group eval = testMatch[0].Groups["evalpt"];
                    Group myunit = testMatch[0].Groups["unit"];
                    DoseValue.DoseUnit du = DoseValue.DoseUnit.Gy;
                    if (myunit.Value == "Gy")
                    {
                        du = DoseValue.DoseUnit.Gy;
                        DoseValue myRequestedDose = new DoseValue(Convert.ToDouble(eval.Value), du);
                        if (outputUnit == "cc")
                            result = _ctx.PlanSetup.GetVolumeAtDose(s, myRequestedDose, VolumePresentation.AbsoluteCm3);
                        else if (outputUnit == "%")
                            result = _ctx.PlanSetup.GetVolumeAtDose(s, myRequestedDose, VolumePresentation.Relative);
                        else
                            result = -1.0;
                    }
                    else if (myunit.Value == "%")
                    {
                        du = DoseValue.DoseUnit.Percent;
                        DoseValue myRequestedDose = new DoseValue(Convert.ToDouble(eval.Value), du);
                        if (outputUnit == "cc")
                            result = _ctx.PlanSetup.GetVolumeAtDose(s, myRequestedDose, VolumePresentation.AbsoluteCm3);
                        else if (outputUnit == "%")
                            result = _ctx.PlanSetup.GetVolumeAtDose(s, myRequestedDose, VolumePresentation.Relative);
                        else
                            result = -1.0;
                    }
                    else
                        result = -1.0;
                }
                else
                    result = -1.0;
            }


            #endregion

            #region Dxx
            // must handle D33%<20Gy or D33cc>10% 

            if (theObjective.Substring(0, 1) == "D")
            {
                //string v_at_d_pattern = @"^V(?<evalpt>\d+\p{P}\d+|\d+)(?<unit>(%|Gy))$"; // matches V50.4Gy or V50.4% 
                string d_at_v_pattern = @"^D(?<evalpt>\d+\p{P}\d+|\d+)(?<unit>(%|cc))$";
                var testMatch = Regex.Matches(theObjective, d_at_v_pattern);
                if (testMatch.Count != 0) // count is 1
                {
                    Group eval = testMatch[0].Groups["evalpt"];
                    Group unit = testMatch[0].Groups["unit"];

                    DoseValue.DoseUnit da = DoseValue.DoseUnit.Gy;
                    DoseValue.DoseUnit dr = DoseValue.DoseUnit.Percent;
                    DoseValue myDabs_something = new DoseValue(50.1000, da);
                    DoseValue myDrel_something = new DoseValue(50.0000, dr);

                    double myD = Convert.ToDouble(eval.Value);//33
                    if (unit.Value == "%")
                    {
                        if (outputUnit == "%")
                        {
                            myDrel_something = _ctx.PlanSetup.GetDoseAtVolume(s, myD, VolumePresentation.Relative, DoseValuePresentation.Relative);
                            result = myDrel_something.Dose / _rcp.prescriptionPercentageDouble;
                        }
                        else if (outputUnit == "Gy")
                        {
                            myDabs_something = _ctx.PlanSetup.GetDoseAtVolume(s, myD, VolumePresentation.Relative, DoseValuePresentation.Absolute);
                            result = myDabs_something.Dose;
                        }
                        else
                            result = -1.0;

                    }
                    else if (unit.Value == "cc")
                    {
                        if (outputUnit == "%")
                        {
                            myDrel_something = _ctx.PlanSetup.GetDoseAtVolume(s, myD, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Relative);
                            result = myDrel_something.Dose / _rcp.prescriptionPercentageDouble;
                        }
                        else if (outputUnit == "Gy")
                        {
                            myDabs_something = _ctx.PlanSetup.GetDoseAtVolume(s, myD, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute);
                            result = myDabs_something.Dose;
                        }
                        else
                            result = -1.0;
                    }
                    else
                        result = -1.0;
                }
                else
                    result = -1.0;
            }
            #endregion

            if (result == -1.0)
            {
                MessageBox.Show("The following objective for " + s.Id + " is not in the correct format: " + theObjective + " (see check-protocol). This objective will be ignored");
            }
            return result;
        }

        private List<Item_Result> _result = new List<Item_Result>();
        // private PreliminaryInformation _pinfo;
        private string _title = "Dose Distribution";

        public void Check()
        {
            #region turquoise isodose
            if (_pinfo.advancedUserMode)
            {
                Item_Result turquoiseIsodose = new Item_Result();
                turquoiseIsodose.Label = "Isodose Turquoise";
                turquoiseIsodose.ExpectedValue = "EN COURS";
                turquoiseIsodose.Infobulle = "L'isodose turquoise doit avoir la valeur de 95% ou 100% d'une prescription";
                Isodose i = _ctx.PlanSetup.Dose.Isodoses.FirstOrDefault(x => x.Color.ToString() == "#FF80FFFF");

                if (i == null)
                {
                    turquoiseIsodose.MeasuredValue = "Pas d'isodose turquoise";
                    turquoiseIsodose.setToWARNING();
                }
                else
                {
                    bool foundIt = false;
                    double levelMatch = 0.0;

                    List<double> possibleValue = new List<double>();
                    possibleValue.Add(95.0);
                    try
                    {
                        foreach (var target in _ctx.PlanSetup.RTPrescription.Targets)
                        {
                            double d = (target.NumberOfFractions * target.DosePerFraction.Dose) / (_ctx.PlanSetup.TotalDose.Dose);
                            possibleValue.Add(d);
                        }
                        foreach (double d in possibleValue)
                        {
                            if (i.Level.Dose - d < 0.00001)
                            {
                                foundIt = true;
                                levelMatch = d;
                            }
                        }
                    }
                    catch { foundIt = false; }



                    if (foundIt)
                    {
                        turquoiseIsodose.MeasuredValue = levelMatch.ToString("0.00");
                        turquoiseIsodose.setToTRUE();
                    }
                    else
                    {
                        turquoiseIsodose.setToWARNING();
                        turquoiseIsodose.MeasuredValue = "Isodose turquoise sans rapport avec la prescription";
                    }
                }
                this._result.Add(turquoiseIsodose);
            }

            #endregion

            #region Objectives to reach 
            if (_pinfo.advancedUserMode)
            {
                Item_Result dd = new Item_Result();
                dd.Label = "Objectifs de dose";
                dd.ExpectedValue = "EN COURS";
                List<string> successList = new List<string>();
                List<string> failedList = new List<string>();
                dd.setToINFO();
                dd.MeasuredValue = "en cours";
                double result = 0.0;
                foreach (DOstructure dos in _rcp.myDOStructures) // loop on list structures with > 0 objectives in check-protocol
                {
                    string structName = dos.Name.ToUpper();
                    if (dos.Name.ToUpper().Contains("HOMO"))
                        structName = replaceHomoIn(dos.Name);
                    if (dos.Name.ToUpper().Contains("CONTRO"))
                        structName = replaceControIn(dos.Name);

                    if (structName != null)
                    {
                        Structure s = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper() == structName.ToUpper()); // get the chosen structure
                        structName = null;
                        DVHData dvh = null;

                        if (s != null) // get the dvh once per struct
                        {
                            dvh = _ctx.PlanSetup.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.1);
                            structName = s.Id;
                        }
                        if (dvh != null)
                            foreach (string obj in dos.listOfObjectives) // loop on list of objectives in check-protocol. 
                            {
                                //----------------------------------
                                //  Ex. of objective V20.0Gy<33.1%
                                //----------------------------------

                                //MessageBox.Show("start processing " + obj);

                                string theObjective = "";
                                string theValue = "";
                                double theValueDouble = 0.0;
                                string theValueWithUnit = "";//0.0;
                                string theUnit = "";
                                string[] elementI = null;
                                string[] elementS = null;
                                bool isInfObj = false;
                                bool isSupObj = false;

                                elementS = obj.Split('>');  // split around > or <   Get V20.0Gy an 33.1%
                                elementI = obj.Split('<');

                                if (elementS.Length > 1)
                                {
                                    isSupObj = true; // it is a sup obective
                                    theObjective = elementS[0];
                                    theValueWithUnit = elementS[1];
                                }
                                else if (elementI.Length > 1)
                                {
                                    isInfObj = true; // it is a inf obective
                                    theObjective = elementI[0];
                                    theValueWithUnit = elementI[1];
                                }
                                else
                                {
                                    MessageBox.Show("This objective is not correct " + obj + "(must contain < or >). It will be ignored");
                                    break;
                                }

                                if (isInfObj || isSupObj)
                                {
                                    theUnit = getTheUnit(theValueWithUnit); // extract Gy from 20.4Gy
                                    if (theUnit != "failed")
                                    {
                                        theValue = theValueWithUnit.Replace(theUnit, "");// extract 20.4 from 20.4Gy
                                        theValueDouble = Convert.ToDouble(theValue);
                                        result = getValueForThisObjective(s, dvh, theObjective, theUnit); // no need to pass the value, just the indicator and the output unit
                                        if (isInfObj)
                                        {

                                            if (result <= theValueDouble)//success
                                            {
                                                successList.Add(structName + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit);
                                               // MessageBox.Show("INF " + s.Id + " " + result + " " + theValueDouble + " success");
                                            }
                                            else // failed
                                            {
                                                failedList.Add(structName + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit);
                                                //MessageBox.Show("INF " + s.Id + " " + result + " " + theValueDouble + " success");
                                            }
                                        }
                                        else if (isSupObj)
                                        {
                                            //MessageBox.Show("SUP " + s.Id + " " + result + " " + theValueDouble);
                                            if (result >= theValueDouble)//success
                                                successList.Add(structName + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit);
                                            else // failed
                                                failedList.Add(structName + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit);
                                        }
                                    }
                                    else
                                        MessageBox.Show("error in this objective: wrong unit: " + structName + " " + obj + "It will be ignored.");


                                    // MessageBox.Show("End of process for " + obj + " Result : " + result.ToString("0.00") + " " + theUnit);
                                }

                            }
                    }

                }
                dd.setToINFO();
                dd.MeasuredValue = "Aucun test réalisé sur des indicateurs de dose";
                dd.Infobulle = "Aucun test réalisé sur des indicateurs de dose. Soit il n'en est spécifié aucun dans le check-protocol, soit les structures requises sont absentes.";
                if ((successList.Count > 0) || (failedList.Count > 0))
                {
                    if (failedList.Count > 0)
                    {
                        dd.setToWARNING();
                        dd.MeasuredValue = "Au moins un objectif non atteint (voir détail)";
                    }
                    else
                    {
                        dd.setToTRUE();
                        dd.MeasuredValue = "Tous les objectifs atteints";
                    }
                    dd.Infobulle = "";
                    if (failedList.Count > 0)
                    {
                        dd.Infobulle += "Echecs : \n";
                        foreach (string s in failedList)
                            dd.Infobulle += "  - " + s + "\n";
                    }
                    if (successList.Count > 0)
                    {
                        dd.Infobulle += "Succès : \n";
                        foreach (string s in successList)
                            dd.Infobulle += "  - " + s + "\n";
                    }
                }

                this._result.Add(dd);
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
