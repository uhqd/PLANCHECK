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
    internal class Check_beams
    {
        private ScriptContext _ctx;
        private PreliminaryInformation _pinfo;
        private read_check_protocol _rcp;
        private static bool done = false;
        public Check_beams(PreliminaryInformation pinfo, ScriptContext ctx, read_check_protocol rcp)  //Constructor
        {
            _ctx = ctx;
            _pinfo = pinfo;
            _rcp = rcp;
            Check();

        }

        private List<Item_Result> _result = new List<Item_Result>();
        // private PreliminaryInformation _pinfo;
        private string _title = "Faisceaux";



        private bool gantryAnglesAreCorrect(ScriptContext _ctx)
        {
            bool isCorrect = true;
            List<double> intAngle = new List<double>();
            List<double> extAngle = new List<double>();

            //List<Beam> myBeamList = new List<Beam>();
            foreach (Beam b in _ctx.PlanSetup.Beams)
            {
                if (b.Id.ToLower().Contains("int"))
                {
                    intAngle.Add(b.ControlPoints.First().GantryAngle);
                }
                if (b.Id.ToLower().Contains("ext"))
                {
                    extAngle.Add(b.ControlPoints.First().GantryAngle);
                }
            }
            intAngle.Sort();
            extAngle.Sort();
            for (int i = 0; i < intAngle.Count - 1; i++)
            {
                if (intAngle[i + 1] - intAngle[i] != 7)
                {
                    isCorrect = false;
                    break;
                }
            }
            if (isCorrect)
                for (int i = 0; i < extAngle.Count - 1; i++)
                {
                    if (extAngle[i + 1] - extAngle[i] != 7)
                    {
                        isCorrect = false;
                        break;
                    }
                }

            return isCorrect;

        }


        private bool isItProstateWithoutNodes()
        {
            bool value = false;

            bool therisAnOverlapGrele = false;
            Structure s = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper() == "OverlapGrele");
            if (s != null)
                if (!s.IsEmpty)
                    therisAnOverlapGrele = true;

            bool thereAreNodes = false;
            if (_pinfo.PlanName.ToUpper().Contains("GG") || therisAnOverlapGrele)
                thereAreNodes = true;

            if (_rcp.protocolName.ToUpper().Contains("PROSTATE"))
            {
                if (!thereAreNodes)
                    value = true;
            }



            return value;

        }
        private bool fieldIsTooSmall(double surfaceZX, double surfaceZY, double X1, double X2, double Y1, double Y2)
        {
            bool itIsTooSmall = false;
            double tolerance = 1.2;
            double surfaceJaw = tolerance * (Math.Abs(X1) + Math.Abs(X2)) * (Math.Abs(Y1) + Math.Abs(Y2));
            if ((surfaceJaw < surfaceZX) && (surfaceJaw < surfaceZY))
                itIsTooSmall = true;
            return itIsTooSmall;

        }


        public void Check()
        {
            if (_pinfo.actualUserPreference.userWantsTheTest("energy"))
            {
                #region ENERGY 

                if ((!_pinfo.isTOMO) && (!_pinfo.isHALCYON)) // not checked if mono energy machine
                {
                    Item_Result energy = new Item_Result();
                    energy.Label = "Energie";
                    energy.ExpectedValue = "NA";



                    if ((_rcp.energy == "") || (_rcp.energy == null)) // no energy specified in check-protocol
                    {
                        energy.setToINFO();
                        energy.MeasuredValue = "Energie non vérifiée";
                        energy.Infobulle = "Aucune énergie spécifiée dans le protocole:" + _rcp.protocolName;
                    }
                    else
                    {

                        List<string> energyList = new List<string>();
                        List<string> distinctEnergyList = new List<string>();
                        foreach (Beam b in _ctx.PlanSetup.Beams)
                            if (!b.IsSetupField)
                                energyList.Add(b.EnergyModeDisplayName);

                        distinctEnergyList = energyList.Distinct().ToList(); // remove doublons
                        energy.MeasuredValue += "Energies : ";
                        foreach (string distinctEnergy in distinctEnergyList)
                            energy.MeasuredValue += distinctEnergy + " ";
                        energy.Infobulle = "Valeur spécifiée dans le check-protocol : " + _rcp.energy;
                        if (distinctEnergyList.Count > 1)
                        {
                            energy.setToWARNING();
                        }
                        else
                        {
                            if (distinctEnergyList[0] == _rcp.energy)
                                energy.setToTRUE();
                            else
                                energy.setToFALSE();
                        }
                    }
                    this._result.Add(energy);
                }
                #endregion
            }
            bool userWantsDoseRate = _pinfo.actualUserPreference.userWantsTheTest("doseRate");
            bool userWantsLowStep = _pinfo.actualUserPreference.userWantsTheTest("lowSteps");
            if (userWantsDoseRate || userWantsLowStep)
            {
                #region DOSERATE FOR QA PREDICTION AND GANTRY SPEED 

                if (_pinfo.isModulated)
                    if (_pinfo.isNOVA || _pinfo.isHALCYON) // not checked if mono energy machine
                    {
                        int nLowStepDetected = 0;
                        int nTotalSteps = 0;
                        //int lowStepDetected = 0;
                        double maxDoseRateEnergy = 0.0;
                        string s = string.Empty;
                        Item_Result doseRate = new Item_Result();
                        doseRate.Label = "Débit de dose pour QA";
                        doseRate.ExpectedValue = "NA";
                        string textOut = string.Empty;

                        foreach (Beam b in _ctx.PlanSetup.Beams)
                        {

                            if (!b.IsSetupField)
                            {
                                if (_pinfo.isHALCYON)
                                    maxDoseRateEnergy = 740.0;
                                else if (_pinfo.isNOVA)
                                {
                                    if (b.EnergyModeDisplayName == "6X")
                                        maxDoseRateEnergy = 600.0;
                                    else if (b.EnergyModeDisplayName == "6X-FFF")
                                        maxDoseRateEnergy = 1400.0;
                                    else if (b.EnergyModeDisplayName == "10X")
                                        maxDoseRateEnergy = 600.0;
                                    else if (b.EnergyModeDisplayName == "10X-FFF")
                                        maxDoseRateEnergy = 2400.0;


                                }

                                double maxGantrySpeed = 6.0;
                                double maxDoseRate = b.DoseRate;
                                int numberOfCPs = b.ControlPoints.Count();
                                double beamMeterSet = b.Meterset.Value;

                                List<double> diffMeterset = new List<double>();
                                List<double> angleDifference = new List<double>();


                                for (int i = 1; i < numberOfCPs; i++)
                                {
                                    ControlPoint cp_curr = b.ControlPoints[i];
                                    ControlPoint cp_prev = b.ControlPoints[i - 1];
                                    if (b.GantryDirection == GantryDirection.Clockwise)
                                        angleDifference.Add(cp_curr.GantryAngle - cp_prev.GantryAngle < 0 ? cp_curr.GantryAngle - cp_prev.GantryAngle + 360 : cp_curr.GantryAngle - cp_prev.GantryAngle);
                                    else
                                        angleDifference.Add(cp_prev.GantryAngle - cp_curr.GantryAngle < 0 ? cp_prev.GantryAngle - cp_curr.GantryAngle + 360 : cp_prev.GantryAngle - cp_curr.GantryAngle);
                                    diffMeterset.Add(cp_curr.MetersetWeight - cp_prev.MetersetWeight);



                                }



                                var timePerCP = angleDifference.Select(x => x / maxGantrySpeed).ToList();
                                var relativeMU = diffMeterset.Select(x => x * beamMeterSet).ToList();
                                var doseRateTheory = relativeMU.Zip(timePerCP, (x, y) => x / y * 60).ToList(); // multiply values from the two lists 

                                var doseRateDoubleList = doseRateTheory.Select(x => (x >= maxDoseRate) ? maxDoseRate : x).ToList();
                                var gantrySpeed = doseRateDoubleList.Zip(relativeMU, (x, y) => x / y).Zip(angleDifference, (x, y) => x * y / 60).ToList();

                                var lowStepDetected = gantrySpeed.Where(x => x < 1.0).ToList();
                                nTotalSteps += doseRateDoubleList.Count();
                                nLowStepDetected += lowStepDetected.Count();


                                var avDoseRate = doseRateDoubleList.Count > 0 ? doseRateDoubleList.Average() : 0.0;
                                textOut += b.Id + ": " + avDoseRate.ToString("F0") + (b.Id == _ctx.PlanSetup.Beams.Last(bb => !bb.IsSetupField).Id ? string.Empty : ", ");



                                // chatGPT : I LOVE YOU. Make my histogram



                                double binWidth = 20; // Définir la largeur du bin
                                double minValue = 0.0;// doseRateDoubleList.Min(); // Trouver la valeur minimale
                                double maxValue = maxDoseRateEnergy;// 600.0;// doseRateDoubleList.Max(); // Trouver la valeur maximale
                                int numberOfBins = (int)Math.Ceiling((maxValue - minValue) / binWidth);

                                int[] histogram = new int[numberOfBins];
                                double nVal = Convert.ToDouble(doseRateTheory.Count);
                                foreach (var value in doseRateTheory)
                                {
                                    int binIndex = (int)Math.Floor((value - minValue) / binWidth);
                                    if (binIndex >= 0 && binIndex < numberOfBins)
                                    {
                                        histogram[binIndex]++;
                                    }
                                }

                                // Affichage toutes les les valeurs de l'histogramme

                                // for (int i = 0; i < numberOfBins; i++)
                                //{
                                //    double binStart = minValue + i * binWidth;
                                //   double binEnd = binStart + binWidth;
                                //  double d = Convert.ToDouble(histogram[i]) / nVal * 100.0 ;
                                /// s += "Bin " + (i + 1).ToString() + " " + binStart + "-" + binEnd + " UM/min --> " + histogram[i] + " " + d.ToString("F2") + "\n";
                                //}
                                //MessageBox.Show("this is s " + s);



                                int j = numberOfBins - 1;

                                double binStart = minValue + j * binWidth;
                                double binEnd = binStart + binWidth;


                                double d = Convert.ToDouble(histogram[j]) / nVal * 100.0;

                                s += b.Id + " " + d.ToString("F2") + "%% ";

                                doseRate.Infobulle = "Dernier bin de l'histogramme de débit de dose" + binStart + "-" + binEnd + " UM/min ";


                            }



                        }

                        doseRate.MeasuredValue = s;
                        doseRate.setToTRUE();
                        if (userWantsDoseRate)
                            this._result.Add(doseRate);

                        if (_pinfo.treatmentType != "IMRT")
                        {

                            Item_Result lowSteps = new Item_Result();
                            lowSteps.Label = "CP trop lents (< 1 deg/s)";
                            double ratio = 100.0 * Convert.ToDouble(nLowStepDetected) / Convert.ToDouble(nTotalSteps);
                            lowSteps.MeasuredValue = nLowStepDetected.ToString() + " / " + nTotalSteps.ToString() + " (" + ratio.ToString("F1") + "%)";
                            lowSteps.Infobulle = "Nombre de CP ayant une vitesse < 1 deg/s";

                            if (nLowStepDetected > 1)
                                lowSteps.setToWARNING();
                            else
                                lowSteps.setToTRUE();
                            if (userWantsLowStep)
                                this._result.Add(lowSteps);
                        }
                    }

                #endregion
            }
            if (_pinfo.actualUserPreference.userWantsTheTest("toleranceTable"))
            {
                #region tolerance table
                if (!_pinfo.isTOMO)
                {
                    Item_Result toleranceTable = new Item_Result();
                    toleranceTable.Label = "Table de tolérance";
                    toleranceTable.ExpectedValue = "NA";


                    bool toleranceOK = true;
                    List<string> listOfTolTable = new List<string>();
                    String firstTT = null;
                    bool firstTTfound = false;
                    bool allSame = false;

                    foreach (Beam b in _ctx.PlanSetup.Beams)
                    {


                        listOfTolTable.Add(b.Id + "\t(" + b.ToleranceTableLabel.ToUpper() + ")");
                        // this part is to check if the tol table are all the same
                        if (!firstTTfound)
                        {
                            firstTTfound = true;
                            allSame = true;
                            firstTT = b.ToleranceTableLabel.ToUpper();
                        }
                        else
                        {
                            if (b.ToleranceTableLabel.ToUpper() != firstTT)
                                allSame = false;
                        }
                        // this part is to check if the tol table are as specified in schek protocol
                        if (b.ToleranceTableLabel.ToUpper() != _rcp.toleranceTable.ToUpper())
                        {
                            toleranceOK = false;

                        }
                    }
                    if (toleranceOK)
                    {
                        toleranceTable.setToTRUE();
                        toleranceTable.MeasuredValue = _rcp.toleranceTable;
                        toleranceTable.Infobulle = "Tous les champs ont bien la table de tolérance spécifiée dans le check-protocol:\n";
                    }
                    else
                    {
                        toleranceTable.setToFALSE();
                        toleranceTable.MeasuredValue = "Table de tolérances des champs à revoir (voir détail)";
                        toleranceTable.Infobulle += "\n\nCertains des chams suivants n'ont pas la bonne table de tolérance\n";

                    }
                    if (_rcp.toleranceTable == "") // if no table specidfied in RCP
                    {

                        toleranceTable.MeasuredValue = "Table de tolérances unique  (voir détail) ";
                        toleranceTable.Infobulle = "Pas de table de tolérance spécifiée dans le check-protocol " + _rcp.protocolName;
                        if (allSame)
                        {
                            toleranceTable.Infobulle += "\nUnse seule table de tolérance est utilisée pour tous les faisceaux\n";
                            toleranceTable.MeasuredValue = "Table de tolérances unique  (voir détail) ";
                            toleranceTable.setToTRUE();
                        }
                        else
                        {
                            toleranceTable.Infobulle += "\nPlusieurs tables de tolérance utilisées pour les faisceaux\n";
                            toleranceTable.MeasuredValue = "Table de tolérances différentes  (voir détail) ";
                            toleranceTable.setToFALSE();
                        }

                    }
                    foreach (String field in listOfTolTable)
                        toleranceTable.Infobulle += "\n - " + field;

                    if (_pinfo.isTOMO)
                    {
                        toleranceTable.Infobulle += "\nNon vérifié pour les tomos\n";
                        toleranceTable.MeasuredValue = "Tomo (pas de table de tolérance)";
                        toleranceTable.setToINFO();
                    }
                    this._result.Add(toleranceTable);
                }
                #endregion
            }
            if (_pinfo.actualUserPreference.userWantsTheTest("fieldTooSmall"))
            {
                #region FIELD SIZE GENERAL
                if (!_pinfo.isTOMO)
                {
                    bool giveup = false;
                    Item_Result fieldTooSmall = new Item_Result();


                    List<String> fieldTooSmallList = new List<String>();
                    fieldTooSmall.Label = "Champs trop petits";
                    fieldTooSmall.ExpectedValue = "NA";
                    fieldTooSmall.Infobulle = "Les champs doivent avoir une dimension adaptée au PTV";
                    String targetName = _ctx.PlanSetup.TargetVolumeID;
                    Structure target = null;
                    double surfaceZX = 0;
                    double surfaceZY = 0;
                    //int n = 0;
                    string listOfWrongBeam = null;
                    try // do we have a target volume ? 
                    {
                        target = _ctx.StructureSet.Structures.Where(s => s.Id == targetName).FirstOrDefault();
                        surfaceZX = target.MeshGeometry.Bounds.SizeZ * target.MeshGeometry.Bounds.SizeX;
                        surfaceZY = target.MeshGeometry.Bounds.SizeZ * target.MeshGeometry.Bounds.SizeY;
                    }
                    catch // no we don't
                    {
                        giveup = true;
                    }



                    if (!giveup)
                    {
                        foreach (Beam b in _ctx.PlanSetup.Beams)
                        {
                            if (!b.IsSetupField)
                            {


                                foreach (ControlPoint cp in b.ControlPoints)
                                {
                                    if (fieldIsTooSmall(surfaceZX, surfaceZY, cp.JawPositions.X1, cp.JawPositions.X2, cp.JawPositions.Y1, cp.JawPositions.Y2))
                                    {
                                        listOfWrongBeam += "\n - " + b.Id;
                                        break;
                                    }
                                    //                            n++;
                                }
                            }
                        }
                    }


                    if (giveup)
                    {
                        fieldTooSmall.setToINFO();
                        fieldTooSmall.MeasuredValue = "Test non réalisé";
                        fieldTooSmall.Infobulle += "\n\nCe test n'est pas réalisé pour les Tomos ou si le plan n'a pas de volume cible";
                    }
                    else
                    {
                        if (listOfWrongBeam == null)
                        {
                            fieldTooSmall.setToTRUE();
                            fieldTooSmall.MeasuredValue = "Dimensions des Jaws correctes";
                            fieldTooSmall.Infobulle += "\n\nTous les champs ou Control Points ont des dimensions de machoîres cohérentes par rapport au volume cible";
                        }
                        else
                        {
                            fieldTooSmall.setToWARNING();
                            fieldTooSmall.MeasuredValue = "Un ou plusieurs champs trop petits";
                            fieldTooSmall.Infobulle += "\n\nAu moins un champ ou un Control Point a des dimensions de machoîres trop petites par rapport au volume cible" + listOfWrongBeam;
                        }


                    }

                    this._result.Add(fieldTooSmall);
                }
                #endregion
            }
            if (_pinfo.actualUserPreference.userWantsTheTest("maxPositionMLCHalcyon"))
            {
                #region MLC SIZE HALCYON
                if (_pinfo.isHALCYON) // if  HALCYON XxY must be < 20x20
                {
                    Item_Result maxPositionMLCHalcyon = new Item_Result();
                    maxPositionMLCHalcyon.Label = "Lames MLC Halcyon < 10 cm";
                    maxPositionMLCHalcyon.ExpectedValue = "NA";
                    maxPositionMLCHalcyon.Infobulle = "Les lames du MLC pour l'Halcyon doivent être < 100 mm (tolérance 5 mm)";

                    // List<String> mlcTooLarge = new List<String>();
                    double thisleafnotok = 0;
                    bool allLeavesOK = true;
                    int cpNotOk = -1;
                    int totalNumberofCP = -1;
                    String beamNotOk = null;
                    int leafNumbernotOK = -1;
                    //int i = 0;

                    foreach (Beam b in _ctx.PlanSetup.Beams)
                    {
                        if (!b.IsSetupField)
                        {
                            foreach (ControlPoint cp in b.ControlPoints)
                            {
                                int leafnumber = 0;

                                //                            for(int i = 0; i < cp.LeafPositions.Length; i++)

                                if (!done)
                                {
                                    done = true;
                                    //    MessageBox.Show(cp.LeafPositions.Length.ToString());
                                }
                                foreach (float f in cp.LeafPositions)
                                {
                                    //float g = cp.LeafPositions[28 + leafnumber];

                                    leafnumber++;


                                    //MessageBox.Show()

                                    if ((f > 105) || (f < -105))
                                    {
                                        allLeavesOK = false; // break loop on leaves
                                        thisleafnotok = f;

                                        cpNotOk = cp.Index;
                                        totalNumberofCP = b.ControlPoints.Count;
                                        beamNotOk = b.Id;
                                        leafNumbernotOK = leafnumber;

                                        break;
                                    }
                                }

                                if (!allLeavesOK)
                                {

                                    break; // break loop on cp
                                }
                            }
                            // +" "+ cp.JawPositions.X1.ToString()  +" " +cp.JawPositions.X2.ToString()+" ");

                            if (!allLeavesOK)
                            {
                                break; // break beam loop
                            }
                        }
                    }


                    // if (mlcTooLarge.Count > 0)
                    if (!allLeavesOK)
                    {
                        //MessageBox.Show("i = " + i.ToString());
                        maxPositionMLCHalcyon.setToINFO();
                        maxPositionMLCHalcyon.MeasuredValue = "Au moins une lame MLC > 100 mm (" + thisleafnotok + ")";
                        maxPositionMLCHalcyon.Infobulle += "\nBeam: " + beamNotOk + " cp: " + cpNotOk + "/" + totalNumberofCP + " leaf: " + leafNumbernotOK;
                    }
                    else
                    {
                        maxPositionMLCHalcyon.setToTRUE();
                        maxPositionMLCHalcyon.MeasuredValue = "Toutes les lames MLC < 100 mm";
                    }
                    this._result.Add(maxPositionMLCHalcyon);

                }

                #endregion
            }
            if (_pinfo.actualUserPreference.userWantsTheTest("novaSBRT"))
            {
                #region NOVA SBRT 
                if (_pinfo.isNOVA)
                {
                    Item_Result novaSBRT = new Item_Result();
                    novaSBRT.Label = "NOVA SBRT ou NOVA";
                    novaSBRT.MeasuredValue = _pinfo.machine;
                    if (_pinfo.treatmentType == "VMAT")
                    {


                        novaSBRT.Infobulle = "Pour les Nova en VMAT, la machine NOVA SBRT doit être utilisée si X ou Y < 7 cm,\n";
                        novaSBRT.Infobulle += "sauf pour les prostate sans Ganglions (NOVA SBRT dans tous les cas)";


                        bool aFieldIsSmall = false;
                        double maxJawsX1 = 0.0;
                        double maxJawsX2 = 0.0;
                        double maxJawsY1 = 0.0;
                        double maxJawsY2 = 0.0;
                        double maxJawsX = 0.0;
                        double maxJawsY = 0.0;
                        double limit = 70.0;
                        foreach (Beam b in _ctx.PlanSetup.Beams)
                        {


                            foreach (ControlPoint cpi in b.ControlPoints)
                            {
                                if (Math.Abs(cpi.JawPositions.X1) > maxJawsX1)
                                    maxJawsX1 = Math.Abs(cpi.JawPositions.X1);
                                if (Math.Abs(cpi.JawPositions.X2) > maxJawsX2)
                                    maxJawsX2 = Math.Abs(cpi.JawPositions.X2);

                                if (Math.Abs(cpi.JawPositions.Y1) > maxJawsY1)
                                    maxJawsY1 = Math.Abs(cpi.JawPositions.Y1);
                                if (Math.Abs(cpi.JawPositions.Y2) > maxJawsY2)
                                    maxJawsY2 = Math.Abs(cpi.JawPositions.Y2);

                            }
                            maxJawsX = maxJawsX1 + maxJawsX2;
                            maxJawsY = maxJawsY1 + maxJawsY2;

                            if ((maxJawsY < limit) || (maxJawsX < limit))
                            {
                                aFieldIsSmall = true;
                                break;
                            }


                        }




                        bool mustBeSBRT = false;
                        bool isSBRT = false;
                        bool isProstateWithoutNodes = isItProstateWithoutNodes();

                        if (isProstateWithoutNodes) { mustBeSBRT = true; }
                        else if (_pinfo.isHyperArc)
                        {

                            //MessageBox.Show("n loc HA " + nLocHA);
                            if(_pinfo.nLocHA == 1)
                                mustBeSBRT = true;
                            else
                                mustBeSBRT = false;


                        }
                        else
                        {
                            if (aFieldIsSmall) { mustBeSBRT = true; }
                        }



                        if (_pinfo.machine == "NOVA SBRT")
                            isSBRT = true;
                        novaSBRT.MeasuredValue = "NOVA SBRT ou NOVA: " + _pinfo.machine;
                        if (isSBRT == mustBeSBRT)
                        {
                            novaSBRT.setToTRUE();


                        }
                        else
                        {
                            novaSBRT.setToFALSE();


                        }
                        if (isProstateWithoutNodes)
                            novaSBRT.Infobulle += "\nCe plan : Prostate sans gg --> NOVA SBRT";
                        else if (aFieldIsSmall)
                            novaSBRT.Infobulle += "\nCe plan : X ou Y < 7 cm --> NOVA SBRT";
                        else
                            novaSBRT.Infobulle += "\nCe plan : X et Y > 7 cm --> NOVA";
                    }
                    else
                    {
                        novaSBRT.Infobulle = "Nova non VMAT : machine NOVA SBRT interdite";
                        if (_pinfo.machine == "NOVA SBRT")
                        {
                            novaSBRT.setToFALSE();
                        }
                        else
                        {
                            novaSBRT.setToTRUE();
                        }
                    }
                    this._result.Add(novaSBRT);
                }

                #endregion
            }
            if (_pinfo.actualUserPreference.userWantsTheTest("tomoParamsFieldWidth"))
            {
                #region TOMO PARAMETERS
                if ((_pinfo.isTOMO) && (_pinfo.planReportIsFound))
                {
                    Item_Result tomoParamsFieldWidth = new Item_Result();
                    Item_Result tomoParamsGantryPeriod = new Item_Result();
                    Item_Result tomoParamsPitch = new Item_Result();
                    Item_Result tomoParamsModulationFactor = new Item_Result();


                    tomoParamsFieldWidth.Label = "Field Width";
                    tomoParamsGantryPeriod.Label = "Gantry period";
                    tomoParamsPitch.Label = "Pitch";
                    tomoParamsModulationFactor.Label = "Modulation factor";


                    tomoParamsFieldWidth.MeasuredValue = _pinfo.tprd.Trd.fieldWidth.ToString();
                    tomoParamsGantryPeriod.MeasuredValue = _pinfo.tprd.Trd.gantryPeriod.ToString();
                    tomoParamsPitch.MeasuredValue = _pinfo.tprd.Trd.pitch.ToString();
                    tomoParamsModulationFactor.MeasuredValue = _pinfo.tprd.Trd.modulationFactor.ToString();


                    if (_pinfo.tprd.Trd.fieldWidth == 5.0)
                        tomoParamsFieldWidth.setToTRUE();
                    else
                        tomoParamsFieldWidth.setToINFO();

                    tomoParamsFieldWidth.Infobulle = "Attendu : 5.0 cm";


                    if ((_pinfo.tprd.Trd.gantryPeriod < 52.0) && (_pinfo.tprd.Trd.gantryPeriod > 12.0))
                        tomoParamsGantryPeriod.setToTRUE();
                    else
                        tomoParamsGantryPeriod.setToWARNING();

                    tomoParamsGantryPeriod.Infobulle = "Attendu (s) : 12 < x < 52 ";


                    if ((_pinfo.tprd.Trd.pitch < 0.44) && (_pinfo.tprd.Trd.pitch > 0.4))
                        tomoParamsPitch.setToTRUE();
                    else
                        tomoParamsPitch.setToINFO();

                    tomoParamsPitch.Infobulle = "Attendu : 0.4 < x < 0.44";

                    if ((_pinfo.tprd.Trd.modulationFactor < 3.5) && (_pinfo.tprd.Trd.modulationFactor > 2.0))
                        tomoParamsModulationFactor.setToTRUE();
                    else
                        tomoParamsModulationFactor.setToINFO();

                    tomoParamsModulationFactor.Infobulle = "Attendu : 2 < x < 3.5";

                    Item_Result blockedOAR = new Item_Result();
                    blockedOAR.Label = "Blocage OAR";
                    blockedOAR.MeasuredValue = _pinfo.tprd.Trd.blockedOAR.Count + " OAR bloqués (voir détail)";
                    blockedOAR.setToINFO();
                    blockedOAR.Infobulle = "Liste des OAR bloqués en EXIT ONLY\n";
                    foreach (string blocOAR in _pinfo.tprd.Trd.blockedOAR)
                        blockedOAR.Infobulle += "\n" + blocOAR;

                    this._result.Add(blockedOAR);
                    this._result.Add(tomoParamsFieldWidth);
                    this._result.Add(tomoParamsGantryPeriod);
                    this._result.Add(tomoParamsPitch);
                    this._result.Add(tomoParamsModulationFactor);


                }

                #endregion
            }
            if (_pinfo.actualUserPreference.userWantsTheTest("gantryAnglesForBreast"))
            {
                #region ANGLES SHIFT IS 7 DEGRES BREAST IMRT 
                if (_rcp.protocolName == "sein" && _pinfo.treatmentType == "IMRT")
                {
                    Item_Result gantryAngleBreastIMRT = new Item_Result();
                    gantryAngleBreastIMRT.Label = "Angles de bras en IMRT du sein";
                    gantryAngleBreastIMRT.ExpectedValue = "";

                    bool isCorrect = false;
                    isCorrect = gantryAnglesAreCorrect(_ctx);
                    if (isCorrect)
                    {
                        gantryAngleBreastIMRT.setToTRUE();
                        gantryAngleBreastIMRT.MeasuredValue = "7 deg. entre les différents champs";
                    }
                    else
                    {
                        gantryAngleBreastIMRT.setToFALSE();
                        gantryAngleBreastIMRT.MeasuredValue = "7 deg. entre les différents champs : non respecté";

                    }
                    gantryAngleBreastIMRT.Infobulle = "Les différents champs intenes doivent être séparés de 7 degrés (idem externes)";

                    this._result.Add(gantryAngleBreastIMRT);

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
