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
using System.IO;
using PlanCheck.xaml_datas;

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
        private double getMedianDose(Structure s, DVHData dvh) // use a DVH with abs. dose and rel. volume. return abs median dose even in case of double leveled dose.             
        {
            double d = 0.0;
            double dose1 = 0.0;
            double dose2 = 0.0;
            double vol1 = 0.0;
            double vol2 = 0.0;
            double penteMin = 0.0;
            double pente = 0.0;
            int i = 0;
            string msg = String.Empty;

           
            foreach (DVHPoint pt in dvh.CurveData)
            {
                i++;
               
                // string line = string.Format("{0},{1}", pt.DoseValue.Dose, pt.Volume);
                if (pt.Volume < 50.0)  // if we parse dvh and meet the median dose --> break
                {
               
                    
                 //   MessageBox.Show("break car on passe les 50 " + pt.Volume.ToString("F2") + " " + pt.DoseValue.Dose.ToString("F2"));
                    d = pt.DoseValue.Dose;
                    break;
                }
                else if (pt.Volume < 95.0) // if we haven't found yet the median dose but the slope stops to decrase
                {
                    try
                    {
                        dose1 = pt.DoseValue.Dose;
                        vol1 = pt.Volume;
                        dose2 = dvh.CurveData[i + 5].DoseValue.Dose;
                        vol2 = dvh.CurveData[i + 5].Volume;
                        pente = (vol2 - vol1) / (dose2 - dose1);// / ;

                    }
                    catch
                    {
                        d = 9999;
                        break;
                    }
                    // msg += i.ToString() + ";" + penteMin.ToString() + ";" + pente.ToString() + ";" + dose1.ToString("F2") + ";" + dose2.ToString("F2") + ";" + vol1.ToString() + ";" + vol2.ToString() + "\n";

                    if (pente <= penteMin)
                    {
                        penteMin = pente;
                    }
                    else  // if slope stops to decrease, find the x for wich y (i.e. volume) is 50%
                    {
                        //MessageBox.Show("BREAK " + pt.DoseValue.Dose.ToString("F2") + " Gy");
                        double b = vol1 - pente * dose1; // y = ax + b  --> b = y-ax
                        d = (50 - b) / pente;
                        //d = pt.DoseValue.Dose;
                        break;
                    }


                }

            }
            
            return d;
        }
        private bool treatmentIsOnTheLeft() // looks if iso is left or right. For Tomo, looks if the first founded PTV is left or right
        {
            bool itisleft = false;



            if (_pinfo.isTOMO)
            {
                // if(_ctx.PlanSetup.RTPrescription.Id.ToLower().Contains("gche"))

                //Structure s = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper().Contains("PTV"));
                Structure s1 = null;
                foreach (Structure s in _ctx.StructureSet.Structures)
                {
                    String myId = s.Id.ToUpper();
                    if (myId.Contains("PTV") && !myId.Contains("-PTV"))
                    {
                        s1 = s;
                        break;
                    }
                }
                if (s1 == null)
                {
                    foreach (Structure s in _ctx.StructureSet.Structures)
                    {
                        if (s.DicomType == "BODY")
                        {
                            s1 = s;
                            break;
                        }

                    }
                }

                double ptvCenter = s1.MeshGeometry.Bounds.X + (0.5 * s1.MeshGeometry.Bounds.SizeX);
                if (ptvCenter > _pinfo.theXcenter)//0)
                {
                    // MessageBox.Show(s.Id + " " + ptvCenter + "\n center " + _pinfo.theXcenter);

                    itisleft = true;

                }
                // MessageBox.Show("it is left " + itisleft.ToString() + s.Id + " "+s.MeshGeometry.Bounds.X.ToString("")+" " + s.MeshGeometry.Bounds.SizeX.ToString(""));
            }
            else
            {
                Beam b = _ctx.PlanSetup.Beams.First();
                if (b.IsocenterPosition.x > _pinfo.theXcenter)  // if iso is left
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
        public string getResultForThisObjective(Structure s, DVHData dvh, string obj)
        {
            string resultMsg = string.Empty;
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

            }

            if (isInfObj || isSupObj)
            {
                theUnit = getTheUnit(theValueWithUnit); // extract Gy from 20.4Gy
                if (theUnit != "failed")
                {
                    theValue = theValueWithUnit.Replace(theUnit, "");// extract 20.4 from 20.4Gy
                    theValueDouble = Convert.ToDouble(theValue);
                    double result = getValueForThisObjective(s, dvh, theObjective, theUnit); // no need to pass the value, just the indicator and the output unit
                    if (isInfObj)
                    {

                        if (result <= theValueDouble)//success
                        {
                            resultMsg += "OK:\t" + s.Id + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit;
                            //successList.Add(structName + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit);
                            // MessageBox.Show("INF " + s.Id + " " + result + " " + theValueDouble + " success");
                        }
                        else // failed
                        {
                            resultMsg += "X:\t" + s.Id + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit;
                            //failedList.Add(structName + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit);
                            //MessageBox.Show("INF " + s.Id + " " + result + " " + theValueDouble + " success");
                        }
                    }
                    else if (isSupObj)
                    {
                        //MessageBox.Show("SUP " + s.Id + " " + result + " " + theValueDouble);
                        if (result >= theValueDouble)//success
                            resultMsg += "OK:\t" + s.Id + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit;
                        //successList.Add(structName + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit);
                        else // failed
                            resultMsg += "X:\t" + s.Id + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit;
                        //                        failedList.Add(structName + " " + obj + " --> " + result.ToString("0.00") + " " + theUnit);
                    }
                }
                else
                    MessageBox.Show("error in this objective: wrong unit: " + s.Id + " " + obj + "It will be ignored.");

            }


            return resultMsg;

        }
        private string makeMyWishAbsolute(string obj, double totalPrescribedDose) // D50%<30%  --> D50%<38.1Gy
        {
            string outText = String.Empty;
            double valueinObj = 0.0;
            int startIndex = 0;
            string beginOfObj = String.Empty;

            if (obj.Contains("<"))
                startIndex = obj.IndexOf('<');
            else if (obj.Contains(">"))
                startIndex = obj.IndexOf('>');
            else
                MessageBox.Show("ERROR DOSE DISTRIBUTION. Cet objectif devrait contenir un caractère < ou > : " + obj);

            int endIndex = obj.IndexOf('%', startIndex);

            if (!obj.Contains("D"))
                outText = obj;
            else if (startIndex != -1 && endIndex != -1)
            {
                string valeurString = obj.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
                double.TryParse(valeurString, out valueinObj);
                beginOfObj = obj.Substring(0, startIndex + 1);
                outText = beginOfObj + (valueinObj * totalPrescribedDose / 100.0).ToString("F2") + "Gy";
            }
            else
                outText = obj;
            return outText;
        }

        private List<Item_Result> _result = new List<Item_Result>();

        private string _title = "Dose Distribution";

        public void Check()
        {
            #region turquoise isodose
            if (_pinfo.actualUserPreference.userWantsTheTest("turquoiseIsodose"))
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

            #region Objectives to ptv in the prescription.
            if (_ctx.PlanSetup.RTPrescription == null)
                MessageBox.Show("Pas de prescription --> Pas de vérification de la dose aux PTV");
            else if (_pinfo.actualUserPreference.userWantsTheTest("prescribedObjectives") && _ctx.PlanSetup.RTPrescription.Targets.Count() > 0)
            {
               

               Item_Result prescribedObjectives = new Item_Result();
                prescribedObjectives.Label = "Dose aux PTVs";
                prescribedObjectives.ExpectedValue = "EN COURS";
                prescribedObjectives.MeasuredValue = "EN COURS";
                prescribedObjectives.setToINFO();


                /* 
                Dx%		 vol	%dos	
                Surrenale	98	100	PTV
                        STEC Poumon	95	100	
                         Foie	98	100	
                HA	99	100	
                STIC Homog	99	100	105%
                Os	98	100	PTV
                STIC 	99	100	             
                 */




                List<string> successListPTV = new List<string>();
                List<string> failedListPTV = new List<string>();
                double tolerance = 1.0; // (Gy) tolerance on median dose
                string myinfo = string.Empty;
                int nOK = 0;
                int nFailed = 0;
                List<(string, double)> listOfStructuresWithADose = new List<(string, double)>();


                // get objectives in check protocol
                DOstructure dosPTVHigh = _rcp.myDOStructures.FirstOrDefault(x => x.Name == "PTV_HAUTE_DOSE"); // get dose of objectives for the highest PTVs
                DOstructure dosPTVLow = _rcp.myDOStructures.FirstOrDefault(x => x.Name == "PTV_AUTRES"); // get dose of objectives for other PTVs

                bool thereIsAHighObjective = true;
                if (dosPTVHigh == null) // || (dosPTVLow == null))
                    thereIsAHighObjective = false;

                bool thereIsALowObjective = true;
                if (dosPTVLow == null) // || (dosPTVLow == null))
                    thereIsALowObjective = false;

                // find highest prescribed dose
                double highest_prescribed_total_dose = _ctx.PlanSetup.RTPrescription.Targets.Select(target => target.NumberOfFractions * target.DosePerFraction.Dose).Max();



                #region get a structure for each target if possible

                List<string> listOfTargets = new List<string>();
                List<string> listOfStructures = new List<string>();
                List<(string, string)> targetsAndStructList = new List<(string, string)>();

                foreach (var target in _ctx.PlanSetup.RTPrescription.Targets) // list of targets
                {
                    listOfTargets.Add(target.TargetId);

                }
                foreach (Structure s in _ctx.StructureSet.Structures) // list of structures 
                {
                    listOfStructures.Add(s.Id);
                }


                bool oneTargetIsFound = false;
                bool allTargetAreFound = true;
                foreach (string element in listOfTargets) // create a combo box for each target
                {


                    oneTargetIsFound = false;
                    foreach (String s in listOfStructures)
                    {


                        if (s.ToUpper().Replace(" ", "") == element.ToUpper().Replace(" ", ""))
                        {
                            targetsAndStructList.Add((element, s));
                            oneTargetIsFound = true;
                        }
                    }
                    

                    if (oneTargetIsFound == false)
                        allTargetAreFound = false;

                }

                #endregion


                #region if there is at least one target with no structure that has the same name 
                if (allTargetAreFound == false)
                {
                    var myChoiceWindow = new selectPTVWindow(_ctx, _pinfo); // create window                                                                       
                    myChoiceWindow.ShowDialog(); // display window,
                    targetsAndStructList.Clear();
                    targetsAndStructList = myChoiceWindow.targetStructList;
                }
                // if (myChoiceWindow.targetStructList.Count != _ctx.PlanSetup.RTPrescription.Targets.Count())
                if (targetsAndStructList.Count != _ctx.PlanSetup.RTPrescription.Targets.Count())
                    MessageBox.Show("ERREUR Check_DoseDistribution : Nombre de prescriptions incohérent");
                #endregion

                foreach (var target in _ctx.PlanSetup.RTPrescription.Targets)
                {
                    double totalPrescribedDose = target.NumberOfFractions * target.DosePerFraction.Dose; // get the total dose
                    double minAcceptedMedianDose = totalPrescribedDose - tolerance;
                    double maxAcceptedMedianDose = totalPrescribedDose + tolerance;//(1 +tolerance) * totalPrescribedDose;


                    // check if it it is a highest dose volume
                    bool highestDoseVolume = false;
                    if (target.NumberOfFractions * target.DosePerFraction.Dose == highest_prescribed_total_dose)
                        highestDoseVolume = true;



                    // get in targetStructList, the structure name corresponding to target
                    Structure correspondingStructure = null;
                    //foreach (var element in myChoiceWindow.targetStructList)
                    foreach (var element in targetsAndStructList)
                    {
                        //MessageBox.Show("in loop target : " + target.Id + " item 1 " + element.Item1 + " item 2 " + element.Item2);
                        if (element.Item1 == target.TargetId)
                        {
                            correspondingStructure = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id == element.Item2);
                        }

                    }
                    if (correspondingStructure == null)
                    {
                        MessageBox.Show("impossible de trouver la structure " + target.TargetId);
                    }

                    DVHData dvh = _ctx.PlanSetup.GetDVHCumulativeData(correspondingStructure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.1);
                    myinfo += "\u2022 " + target.TargetId + " (prescr. " + totalPrescribedDose.ToString("F1") + " Gy)\n";

                    #region check D95%
                    DoseValue d95 = _ctx.PlanSetup.GetDoseAtVolume(correspondingStructure, 95, VolumePresentation.Relative, DoseValuePresentation.Absolute);
                    double d95double = d95.Dose;
                    double d95pres = 0.95 * totalPrescribedDose;


                    if (d95double < d95pres)
                    {

                        nFailed++;
                        //                        myinfo += d95double.ToString("F2") + " < " + d95pres.ToString("F2");
                        myinfo += "\t- X:\t";
                        myinfo += "D95%>95% (" + d95double.ToString("F2") + "<" + d95pres.ToString("F2") + " Gy)\n";
                    }
                    else
                    {

                        nOK++;
                        //                      myinfo += d95double.ToString("F2") + " > " + d95pres.ToString("F2");
                        myinfo += "\t- OK:\t";
                        myinfo += "D95%>95% (" + d95double.ToString("F2") + ">" + d95pres.ToString("F2") + " Gy)\n";
                    }

                    #endregion

                    #region median dose

                    if (!_rcp.protocolName.Contains("STEC") && !_rcp.protocolName.Contains("hyperarc") && !_rcp.protocolName.Contains("STIC"))
                    {
                        double median = getMedianDose(correspondingStructure, dvh);


                        if (median > minAcceptedMedianDose && median < maxAcceptedMedianDose)
                        {
                            myinfo += "\t- OK:\t";
                            //myinfo += "OK\n";
                            nOK++;
                        }
                        else
                        {
                            myinfo += "\t- X:\t";
                            //                            myinfo += "X\n";
                            nFailed++;
                        }
                        myinfo += "Dose médiane* (Gy) :\t" + median.ToString("F2") + " (" + minAcceptedMedianDose.ToString("F2") + "<D<" + maxAcceptedMedianDose.ToString("F2") + ")\n";
                    }
                    #endregion



                    #region Check suppl. DO objective in checkprotocol

                    if (highestDoseVolume)
                    {
                        if (thereIsAHighObjective)
                            foreach (string obj in dosPTVHigh.listOfObjectives) // loop on list of objectives in check-protocol. 
                            {
                                // MessageBox.Show(correspondingStructure.Id+" " + obj);
                                string absobj = makeMyWishAbsolute(obj, totalPrescribedDose);
                                string msg_result = getResultForThisObjective(correspondingStructure, dvh, absobj);


                                if (msg_result.Contains("OK:"))
                                {
                                    successListPTV.Add("\t- " + msg_result);
                                    nOK++;
                                }
                                else
                                {
                                    failedListPTV.Add("\t- " + msg_result);
                                    nFailed++;
                                }

                            }
                    }
                    else
                    {
                        if (thereIsALowObjective)
                            foreach (string obj in dosPTVLow.listOfObjectives) // loop on list of objectives in check-protocol. 
                            {
                                // MessageBox.Show(correspondingStructure.Id + " " + obj);
                                string absobj = makeMyWishAbsolute(obj, totalPrescribedDose);
                                string msg_result = getResultForThisObjective(correspondingStructure, dvh, absobj);
                                if (msg_result.Contains("OK:"))
                                {
                                    successListPTV.Add("\t- " + msg_result);
                                    nOK++;
                                }
                                else
                                {
                                    failedListPTV.Add("\t- " + msg_result);
                                    nFailed++;
                                }

                            }
                    }
                    #endregion

                    foreach (string st in failedListPTV)
                        myinfo += st + "\n";
                    foreach (string st in successListPTV)
                        myinfo += st + "\n";
                    failedListPTV.Clear();
                    successListPTV.Clear();


                }



                prescribedObjectives.setToTRUE();
                prescribedObjectives.MeasuredValue = nOK.ToString() + "/" + (nOK + nFailed).ToString() + " objectifs atteints";
                prescribedObjectives.Infobulle = myinfo;
                if (nFailed > 1)
                    prescribedObjectives.setToWARNING();
                if (nFailed > 3)
                    prescribedObjectives.setToFALSE();

                if (!thereIsAHighObjective && !thereIsALowObjective)
                    prescribedObjectives.Infobulle += "\n\n Pas d'objectifs PTV dans le check-protocol (seuls les tests par défaut ont été réalisés)";

                if (!_rcp.protocolName.Contains("STEC") && !_rcp.protocolName.Contains("hyperarc") && !_rcp.protocolName.Contains("STIC"))
                    prescribedObjectives.Infobulle += "\n * La dose médiane est interpolée pour les PTV BD (non calculée pour les STEC/STIC)";


                this._result.Add(prescribedObjectives);



            }


#endregion

            #region Objectives OAR to reach (check protocol)
            if (_pinfo.actualUserPreference.userWantsTheTest("doseToOAR"))
            {


                Item_Result doseToOAR = new Item_Result();
                doseToOAR.Label = "Doses aux OARs (check-protocol)";
                doseToOAR.ExpectedValue = "EN COURS";
                List<string> successList = new List<string>();
                List<string> failedList = new List<string>();
                doseToOAR.setToINFO();
                doseToOAR.MeasuredValue = "en cours";
                // double result = 0.0;
                foreach (DOstructure dos in _rcp.myDOStructures) // loop on list structures with > 0 objectives in check-protocol
                {


                    if ((dos.Name != "PTV_HAUTE_DOSE") && (dos.Name != "PTV_AUTRES"))
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

                                    string msg_result = getResultForThisObjective(s, dvh, obj);
                                    if (msg_result.Contains("OK:"))
                                        successList.Add(msg_result);
                                    else
                                        failedList.Add(msg_result);
                                }
                        }
                    }
                }

                if ((successList.Count > 0) || (failedList.Count > 0))
                {
                    if (failedList.Count > 0)
                    {
                        doseToOAR.setToWARNING();

                    }
                    else
                    {
                        doseToOAR.setToTRUE();

                    }
                    doseToOAR.MeasuredValue = successList.Count + "/" + (failedList.Count + successList.Count).ToString() + " objectif(s) atteint(s)";

                    doseToOAR.Infobulle = "";
                    if (failedList.Count > 0)
                    {
                        doseToOAR.Infobulle += "Echecs : \n";
                        foreach (string s in failedList)
                            doseToOAR.Infobulle += "  - " + s + "\n";
                    }
                    if (successList.Count > 0)
                    {
                        doseToOAR.Infobulle += "Succès : \n";
                        foreach (string s in successList)
                            doseToOAR.Infobulle += "  - " + s + "\n";
                    }
                }
                else
                {
                    doseToOAR.setToINFO();
                    doseToOAR.MeasuredValue = "Aucun test réalisé sur des indicateurs de dose";
                    doseToOAR.Infobulle = "Aucun test réalisé sur des indicateurs de dose. Soit il n'en est spécifié aucun dans le check-protocol, soit les structures requises sont absentes.";

                }

                this._result.Add(doseToOAR);


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
