﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using System.Windows;

namespace PlanCheck
{
    internal class Check_contours
    {
        private ScriptContext _ctx;
        private PreliminaryInformation _pinfo;
        private read_check_protocol _rcp;

        public Check_contours(PreliminaryInformation pinfo, ScriptContext ctx, read_check_protocol rcp)  //Constructor
        {
            _rcp = rcp;
            _ctx = ctx;
            _pinfo = pinfo;
            Check();

        }
        public static int _GetSlice(double z, StructureSet SS)
        {
            var imageRes = SS.Image.ZRes;
            return Convert.ToInt32((z - SS.Image.Origin.z) / imageRes);
        }


        private Structure isExistAndNotEmpty(String id)
        {
            
            bool isok = false;
            Structure s = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper() == id.ToUpper());
            if(s != null)
                if(!s.IsEmpty)
                    isok = true;

            if (isok)
                return s;
            else
                return null;

        }

        public double getXcenter()
        {
            double xCenter = 0.0;

            Structure centralStruct = isExistAndNotEmpty("CHIASMA");


            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("CANAL MED");
 
            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("RECTUM");
            
            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("VESSIE");
            
            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("CERVEAU");
            
            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("TRONC CEREBRAL");
            
            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("PROSTATE");
            
            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("HYPOPHYSE");
            
            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("BODY");
            
            if (centralStruct == null)
                centralStruct = isExistAndNotEmpty("CONTOUR EXTERNE");
            
            if (centralStruct != null)
                xCenter = centralStruct.MeshGeometry.Bounds.X + (centralStruct.MeshGeometry.Bounds.SizeX / 2.0);


            return xCenter;
        }
        public static int getNumberOfMissingSlices(Structure S, StructureSet SS)
        {

            int nHoles = 0;
            try
            {
                var mesh = S.MeshGeometry.Bounds;
                int meshLow = _GetSlice(mesh.Z, SS);
                int meshUp = _GetSlice(mesh.Z + mesh.SizeZ, SS);



                for (int i = meshLow; i <= meshUp; i++)
                {
                    VMS.TPS.Common.Model.Types.VVector[][] vvv = S.GetContoursOnImagePlane(i);

                    if (vvv.Length == 0)
                        nHoles++;

                }
            }
            catch
            {
                nHoles = 0;
            }
            return nHoles;
        }

        private List<Item_Result> _result = new List<Item_Result>();
        // private PreliminaryInformation _pinfo;
        private string _title = "Contours";

        public void Check()
        {
            var allStructures = _rcp.myClinicalExpectedStructures.Concat(_rcp.myOptExpectedStructures).Concat(_rcp.myCouchExpectedStructures).ToList();

            #region APPROVE ?  
            Item_Result approbationStatus = new Item_Result();
            approbationStatus.Label = "Approbation du groupe de structures";
            approbationStatus.ExpectedValue = "...";
            approbationStatus.Infobulle = "Les structures doivent être approuvées";
            if (_ctx.StructureSet.Structures.First().ApprovalHistory.First().ApprovalStatus.ToString() == "Approved")
            {
                approbationStatus.setToTRUE();
                approbationStatus.MeasuredValue = "Approuvé";
            }
            else
            {
                approbationStatus.setToWARNING();
                approbationStatus.MeasuredValue = "Non Approuvé";
            }



            this._result.Add(approbationStatus);
            #endregion

            #region COUCH STRUCTURES 
            if (!_pinfo.isTOMO)
            {
                Item_Result couchStructExist = new Item_Result();
                couchStructExist.Label = "Structures de table";
                couchStructExist.ExpectedValue = "EN COURS";

                List<string> missingCouchStructures = new List<string>();
                List<string> wrongHUCouchStructures = new List<string>();
                List<string> mandatoryMissingCouchStructures = new List<string>();
                List<string> overlapStructList = new List<string>();
                // double tolerancedOV = 4.0; // Tolerance for overlap couch vs. body
                foreach (expectedStructure es in _rcp.myCouchExpectedStructures) // foreach couch element in the xls protocol file
                {
                    double mydouble = 0;
                    Structure struct1 = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper() == es.Name.ToUpper()); // find a structure in ss with the same name
                    if (struct1 == null) // if structure doesnt exist in ss
                    {
                        missingCouchStructures.Add(es.Name);
                        if (es.isMandatory)
                            mandatoryMissingCouchStructures.Add(es.Name);
                    }
                    else if (struct1.IsEmpty) // else if it exists but empty --> same
                    {
                        missingCouchStructures.Add(es.Name);
                        if (es.isMandatory)
                            mandatoryMissingCouchStructures.Add(es.Name);
                    }
                    else // else struct is not empty
                    {
                        if (es.HU != 9999)
                        {
                            struct1.GetAssignedHU(out mydouble);   // check assigned HU


                            if (mydouble != es.HU)
                                wrongHUCouchStructures.Add(es.Name);
                        }

                        try
                        {
                            Structure body = _ctx.StructureSet.Structures.FirstOrDefault(x => x.DicomType == "EXTERNAL"); // find a structure BODY

                            double yBodyMax = body.MeshGeometry.Bounds.Y + body.MeshGeometry.Bounds.SizeY; // post limit of body
                            double ySetUpMin = struct1.MeshGeometry.Bounds.Y;
                            if ((yBodyMax - 4.0) > ySetUpMin) // overlap suspected 4 mm allowed
                            {
                                overlapStructList.Add(struct1.Id);
                            }
                            /*else // no overlap suspected
                            {
                                ;
                            }*/
                        }
                        catch
                        {
                            MessageBox.Show("No Body found for check contours: couch structures item " + es.Name);
                        }
                    }
                }


                if ((wrongHUCouchStructures.Count == 0) && (missingCouchStructures.Count == 0))
                {
                    couchStructExist.setToTRUE();
                    couchStructExist.MeasuredValue = "Présentes et UH corectes " + _rcp.myCouchExpectedStructures.Count.ToString() + "/" + _rcp.myCouchExpectedStructures.Count.ToString();
                    couchStructExist.Infobulle = "Structures de tables attendues pour le protocole " + _rcp.protocolName + " :\n";
                    foreach (expectedStructure es in _rcp.myCouchExpectedStructures) // foreach couch element in the xls protocol file
                    {
                        couchStructExist.Infobulle += " - " + es.Name + "\n";
                    }
                }
                else
                {
                    couchStructExist.setToWARNING();
                    couchStructExist.MeasuredValue = "Absentes, vides ou UH incorrectes (voir infobulle)";
                    if (missingCouchStructures.Count > 0)
                        couchStructExist.Infobulle = "Structures attendues pour le protocole " + _rcp.protocolName + " absentes ou vides dans le plan :\n";
                    foreach (string ms in missingCouchStructures)
                        couchStructExist.Infobulle += " - " + ms + "\n";
                    if (wrongHUCouchStructures.Count > 0)
                        couchStructExist.Infobulle += "Structures avec UH incorrectes :\n";
                    foreach (string ms in wrongHUCouchStructures)
                        couchStructExist.Infobulle += " - " + ms + "\n";

                    if (mandatoryMissingCouchStructures.Count > 0)
                    {
                        couchStructExist.setToFALSE();
                        couchStructExist.Infobulle += "\nAu moins une structure de table obligatoire est absente : \n";
                        foreach (string ms in mandatoryMissingCouchStructures)
                            couchStructExist.Infobulle += " - " + ms + "\n";
                    }
                }

                this._result.Add(couchStructExist);
            }

            #endregion

            #region overlap body vs couch structs. 
            /* marche pas
            Item_Result overlapCouchBody = new Item_Result();
            overlapCouchBody.Label = "Overlap Body vs. Table";
            overlapCouchBody.ExpectedValue = "EN COURS";

            if (overlapStructList.Count() > 0)
            {

                overlapCouchBody.MeasuredValue = "Overlap suspecté";
                overlapCouchBody.Infobulle = "La position Y max du BODY semble refléter un overlap avec les sturctures de tables :\n ";
                foreach (string s in overlapStructList)
                    overlapCouchBody.Infobulle += " - " + s;

                overlapCouchBody.setToWARNING();
            }
            else
            {
                overlapCouchBody.MeasuredValue = "Pas d'overlap détécté entre la table et le body";
                overlapCouchBody.Infobulle = "Pas d'overlap détécté entre la table et le body (Tolérance = " + tolerancedOV.ToString() + " mm)";
                overlapCouchBody.setToTRUE();
            }
            this._result.Add(overlapCouchBody);

            */
            #endregion

            #region CLINICAL STRUCTURES 

            Item_Result clinicalStructuresItem = new Item_Result();
            clinicalStructuresItem.Label = "Structures cliniques";
            clinicalStructuresItem.ExpectedValue = "EN COURS";


            List<string> missingClinicalStructures = new List<string>();
            List<string> wrongHUClinicalStructures = new List<string>();
            List<string> mandatoryMissingClinicalStructures = new List<string>();

            foreach (expectedStructure es in _rcp.myClinicalExpectedStructures) // foreach clinical struct in the xls check-protocol file
            {
                //MessageBox.Show("here is " + es.Name);
                double mydouble = 0;
                Structure struct1 = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper() == es.Name.ToUpper()); // find a structure in ss with the same name
                if (struct1 == null) // if structure doesnt exist in ss
                {
                    missingClinicalStructures.Add(es.Name);
                    if (es.isMandatory)
                        mandatoryMissingClinicalStructures.Add(es.Name);
                }
                else if (struct1.IsEmpty) // else if it exists but empty --> same
                {
                    missingClinicalStructures.Add(es.Name);
                    if (es.isMandatory)
                        mandatoryMissingClinicalStructures.Add(es.Name);
                }
                else
                {
                    if (es.HU != 9999) // 9999 if no assigned HU 
                    {
                        struct1.GetAssignedHU(out mydouble);
                        if (mydouble != es.HU)
                            wrongHUClinicalStructures.Add(es.Name);
                    }
                }
            }

            if ((wrongHUClinicalStructures.Count == 0) && (missingClinicalStructures.Count == 0))
            {
                clinicalStructuresItem.setToTRUE();
                clinicalStructuresItem.MeasuredValue = "Présentes et UH corectes " + _rcp.myClinicalExpectedStructures.Count.ToString() + "/" + _rcp.myClinicalExpectedStructures.Count.ToString();
                clinicalStructuresItem.Infobulle = "Structures attendues pour le protocole " + _rcp.protocolName + " :\n";
                foreach (expectedStructure es in _rcp.myClinicalExpectedStructures)
                {
                    clinicalStructuresItem.Infobulle += " - " + es.Name + "\n";
                }
            }
            else
            {
                clinicalStructuresItem.setToINFO(); // just info except if wrong HU --> warrning
                if (wrongHUClinicalStructures.Count > 0)
                    clinicalStructuresItem.setToWARNING();

                clinicalStructuresItem.MeasuredValue = "Absentes, vides ou UH incorrectes (voir infobulle)";
                if (missingClinicalStructures.Count > 0)
                    clinicalStructuresItem.Infobulle = "Structures attendues pour le protocole " + _rcp.protocolName + " absentes ou vides dans le plan :\n";
                foreach (string ms in missingClinicalStructures)
                    clinicalStructuresItem.Infobulle += " - " + ms + "\n";
                if (wrongHUClinicalStructures.Count > 0)
                    clinicalStructuresItem.Infobulle += "Structures avec UH incorrectes :\n";
                foreach (string ms in wrongHUClinicalStructures)
                    clinicalStructuresItem.Infobulle += " - " + ms + "\n";

                if (mandatoryMissingClinicalStructures.Count > 0)
                {
                    clinicalStructuresItem.setToFALSE();
                    clinicalStructuresItem.Infobulle += "Structures obligatoires manquantes :\n";
                    foreach (string ms in mandatoryMissingClinicalStructures)
                        clinicalStructuresItem.Infobulle += " - " + ms + "\n";
                }

            }


            this._result.Add(clinicalStructuresItem);
            #endregion

            #region OPT STRUCTURES 

            Item_Result optStructuresItem = new Item_Result();
            optStructuresItem.Label = "Structures d'optimisation";
            optStructuresItem.ExpectedValue = "EN COURS";


            List<string> missingOptStructures = new List<string>();
            List<string> wrongHUOptStructures = new List<string>();
            List<string> mandatoryMissingOptStructures = new List<string>();
            foreach (expectedStructure es in _rcp.myOptExpectedStructures)
            {
                double mydouble = 0;
                Structure struct1 = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper() == es.Name.ToUpper()); // find a structure in ss with the same name
                
                if (struct1 == null) // if structure doesnt exist in ss
                {
                    missingOptStructures.Add(es.Name);
                    if (es.isMandatory)
                        mandatoryMissingOptStructures.Add(es.Name);
                }
                else if (struct1.IsEmpty) // else if it exists but empty --> same
                {
                    missingOptStructures.Add(es.Name);

                    if (es.isMandatory)
                        mandatoryMissingOptStructures.Add(es.Name);
                }
                else
                {
                    if (es.HU != 9999) // 9999 if no assigned HU 
                    {
                        struct1.GetAssignedHU(out mydouble);
                        if (mydouble != es.HU)
                            wrongHUOptStructures.Add(es.Name);
                    }
                    //MessageBox.Show("YES we found in ss " + el.Item1 + " " + struct1.Id + " " + mydouble.ToString());
                }
            }



            if ((wrongHUOptStructures.Count == 0) && (missingOptStructures.Count == 0))
            {
                optStructuresItem.setToTRUE();
                optStructuresItem.MeasuredValue = "Présentes et UH corectes " + _rcp.myOptExpectedStructures.Count.ToString() + "/" + _rcp.myOptExpectedStructures.Count.ToString();
                optStructuresItem.Infobulle = "Structures attendues pour le protocole " + _rcp.protocolName + " :\n";
                foreach (expectedStructure es in _rcp.myOptExpectedStructures)
                {
                    optStructuresItem.Infobulle += " - " + es.Name + "\n";
                }
            }
            else
            {
                optStructuresItem.setToINFO();
                optStructuresItem.MeasuredValue = "Absentes, vides ou UH incorrectes (voir infobulle)";
                if (missingOptStructures.Count > 0)
                    optStructuresItem.Infobulle = "Structures attendues pour le protocole " + _rcp.protocolName + " absentes ou vides dans le plan :\n";
                foreach (string ms in missingOptStructures)
                    optStructuresItem.Infobulle += " - " + ms + "\n";
                if (wrongHUOptStructures.Count > 0)
                    optStructuresItem.Infobulle += "Structures avec UH incorrectes :\n";
                foreach (string ms in wrongHUOptStructures)
                    optStructuresItem.Infobulle += " - " + ms + "\n";

                if (mandatoryMissingOptStructures.Count > 0)
                {
                    optStructuresItem.setToFALSE();
                    optStructuresItem.Infobulle += "Structures obligatoires manquantes : \n";
                    foreach (string ms in mandatoryMissingOptStructures)
                        optStructuresItem.Infobulle += " - " + ms + "\n";
                }
            }


            this._result.Add(optStructuresItem);
            #endregion
            
            #region  Anormal Volume values (cc)
            // entre -3sigma et +3sigma >99.9% des cas
            List<string> anormalVolumeList = new List<string>();
            List<string> normalVolumeList = new List<string>();
            Item_Result anormalVolumeItem = new Item_Result();
            anormalVolumeItem.Label = "Volume des structures";
            anormalVolumeItem.ExpectedValue = "EN COURS";

            //foreach (expectedStructure es in _rcp.myClinicalExpectedStructures)
            foreach (expectedStructure es in allStructures)
            {

                Structure struct1 = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id == es.Name); // find a structure in ss with the same name
                if (struct1 != null) // if structure  exist 
                    if (!struct1.IsEmpty) //  and if not empty 
                        if (es.volMin != 9999) // and if a volume min is defined in protocol
                        {
                            if ((struct1.Volume > es.volMin) && (struct1.Volume < es.volMax)) //if volume ok
                                normalVolumeList.Add(es.Name);
                            else
                                anormalVolumeList.Add(es.Name + " (" + struct1.Volume.ToString("F2") + " cc). Attendu: " + es.volMin.ToString("F2") + " - " + es.volMax.ToString("F2") + " cc");

                        }


            }
            if (anormalVolumeList.Count > 0)
            {
                anormalVolumeItem.setToWARNING();
                anormalVolumeItem.MeasuredValue = "Volumes anormaux détectés";
                anormalVolumeItem.Infobulle = "Les volumes des structures suivantes ne sont\npas dans l'intervalle 6 sigma des volumes habituels\n";
                foreach (string avs in anormalVolumeList)
                    anormalVolumeItem.Infobulle += " - " + avs + "\n";


            }
            else if (normalVolumeList.Count > 0)
            {
                anormalVolumeItem.setToTRUE();
                anormalVolumeItem.MeasuredValue = "Volumes des structures OK";
                anormalVolumeItem.Infobulle = "Les volumes des structures suivantes sont\ndans l'intervalle 6 sigma des volumes habituels\n";
                foreach (string avs in normalVolumeList)
                    anormalVolumeItem.Infobulle += " - " + avs + "\n";


            }
            else
            {
                anormalVolumeItem.setToINFO();
                anormalVolumeItem.MeasuredValue = "Aucune analyse de volumes de structures";
                anormalVolumeItem.Infobulle = "Les structures présentes n'ont pas une valeur de volume (cc) attendu dans le check-protocol\n";
            }

            this._result.Add(anormalVolumeItem);


            #endregion

            #region Shape analyser (number of parts of a structure)
            /* Check if a structrure has the expected number of parts e.g. if a slice is missing */
            Item_Result shapeAnalyser = new Item_Result();
            shapeAnalyser.Label = "Nombre de parties des structures";
            shapeAnalyser.ExpectedValue = "wip...";


            List<string> correctStructs = new List<string>();
            List<string> uncorrectStructs = new List<string>();





            foreach (expectedStructure es in allStructures)
            {

                Structure struct1 = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id == es.Name); // find a structure in ss with the same name


                if (struct1 != null)
                    if (!struct1.IsEmpty)
                        if (es.expectedNumberOfPart != 9999) // expected number of parts exists
                        {
                            int n = struct1.GetNumberOfSeparateParts();


                            if (n != es.expectedNumberOfPart)
                            {

                                uncorrectStructs.Add(es.Name + " comporte " + n + " parties (attendu : " + es.expectedNumberOfPart + ")");

                            }
                            else
                            {

                                correctStructs.Add(es.Name + " comporte " + n + " parties (attendu : " + es.expectedNumberOfPart + ")");
                            }
                        }

            }
            if (uncorrectStructs.Count > 0)
            {
                shapeAnalyser.setToWARNING();
                shapeAnalyser.MeasuredValue = " Nombres de parties des structures incorrects";
                shapeAnalyser.Infobulle = "Les structures suivantes ont un de nombre de parties non-conforme au check-protocol :\n";
                foreach (string s in uncorrectStructs)
                    shapeAnalyser.Infobulle += s + "\n";
            }
            else if (correctStructs.Count > 0)
            {
                shapeAnalyser.setToTRUE();

                shapeAnalyser.MeasuredValue = " Nombres de parties des structures corrects";
                shapeAnalyser.Infobulle = "Les structures suivantes ont un de nombre de parties conforme au check-protocol :\n";
                foreach (string s in correctStructs)
                    shapeAnalyser.Infobulle += s + "\n";

            }
            else
            {
                shapeAnalyser.setToINFO();
                shapeAnalyser.MeasuredValue = " Aucune analyse du nombres de parties des structures";
                shapeAnalyser.Infobulle = "Les structures présentes n'ont pas de valeurs attendues de nombre de parties dans le check-protocol\n";
            }



            this._result.Add(shapeAnalyser);
            #endregion

            #region missing slices
            Item_Result missingSlicesItem = new Item_Result();
            missingSlicesItem.Label = "Contours manquants";
            missingSlicesItem.ExpectedValue = "wip...";

            int m = 0;
            int nAnalysedStructures = 0;
            List<string> structureswithAGap = new List<string>();
            foreach (Structure s in _ctx.StructureSet.Structures)
            {
                if ((s.Id != "Plombs") && (!s.IsEmpty)) // do no check marker structures
                {
                    nAnalysedStructures++;
                    m = getNumberOfMissingSlices(s, _ctx.StructureSet);
                    if (m > 0)
                        structureswithAGap.Add(m.ToString() + " contour(s) manquantes pour la structure : " + s.Id);
                }
            }
            if (structureswithAGap.Count > 0)
            {
                missingSlicesItem.MeasuredValue = "Certaines structures présentent des contours manquants";
                missingSlicesItem.setToWARNING();
                foreach (string s in structureswithAGap)
                    missingSlicesItem.Infobulle += s + "\n";

            }
            else
            {
                missingSlicesItem.MeasuredValue = "Aucune coupe non-contourée détectée";
                missingSlicesItem.setToTRUE();
                missingSlicesItem.Infobulle = nAnalysedStructures.ToString() + " structures analysées. Aucune coupe non-contourée détectée";
            }
            this._result.Add(missingSlicesItem);
            #endregion

            #region Laterality
            Item_Result laterality = new Item_Result();
            laterality.Label = "Lateralité";
            laterality.ExpectedValue = "wip...";

            List<string> goodLaterality = new List<string>();
            List<string> badLaterality = new List<string>();


           // Structure sbody = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper() == "BODY"); // find body

            /*            if (sbody == null)
                            sbody = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper() == "CONTOUR EXTERNE"); // find body

                        if (sbody == null)
                            MessageBox.Show("BODY NOT FOUND");

                        double bodyXcenter = sbody.MeshGeometry.Bounds.X + (sbody.MeshGeometry.Bounds.SizeX / 2.0);
            */
            double bodyXcenter = getXcenter();


            foreach (expectedStructure es in allStructures)
            {
                if (es.laterality != "NONE")
                {
                    Structure s = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id == es.Name); // find a structure in ss with the same name
                    double xpos = 0.0;
                    if (s != null)
                        if (!s.IsEmpty)
                        {
                            xpos = s.MeshGeometry.Bounds.X + (s.MeshGeometry.Bounds.SizeX / 2.0);  // (Left limit + size) /2

                            //MessageBox.Show("orientation : " + _ctx.Image.ImagingOrientation.ToString());
                            //if(_ctx.Image.ImagingOrientation) //
                            //MessageBox.Show("body " + bodyXcenter + " x " + xpos + " " + es.Name);
                            if (xpos > bodyXcenter) // THIS IS LEFT,  if Supine HF but also Prone HF, Supine FF...
                            {
                                if (es.laterality == "L")
                                    goodLaterality.Add(es.Name);
                                else if (es.laterality == "R")
                                    badLaterality.Add(es.Name);
                            }
                            else
                            {
                                if (es.laterality == "R")
                                    goodLaterality.Add(es.Name);
                                else if (es.laterality == "L")
                                    badLaterality.Add(es.Name);

                            }
                        }
                }
            }

            if (badLaterality.Count > 0)
            {
                laterality.MeasuredValue = "Mauvaise latéralité (voir détail)";
                laterality.setToFALSE();

                laterality.Infobulle = "Ces structures sont attendues à gauche ou à droite et semblent du mauvais côté : \n";
                foreach (string s in badLaterality)
                    laterality.Infobulle += " - " + s + "\n";
            }
            else
            {
                laterality.MeasuredValue = "Vérifiée pour " + goodLaterality.Count() + " structure(s)";
                laterality.setToTRUE();

                laterality.Infobulle = "Ces structures sont attendues à gauche ou à droite et semblent du bon côté : \n";
                foreach (string s in goodLaterality)
                    laterality.Infobulle += " - " + s + "\n";

                if (goodLaterality.Count == 0)
                {
                    laterality.Infobulle = "Aucune structure n'a une latéralité (G ou D) attendue dans le check-protocol\n";
                }

            }


            this._result.Add(laterality);
            #endregion

            #region A PTV for EACH CTV/GTV
            Item_Result aPTVforEveryone = new Item_Result();
            aPTVforEveryone.Label = "GTV/CTV/ITV sans PTV";
            aPTVforEveryone.ExpectedValue = "wip...";

            List<string> CTVandGTVs = new List<string>();
            List<string> PTVs = new List<string>();
            List<string> CTVwithoutAnyPTV = new List<string>();
            List<string> CTVwithPTV = new List<string>();
            foreach (Structure s in _ctx.StructureSet.Structures) // list all GTV/CTVs and PTVs
            {
                if ((s.Id.ToUpper().Contains("CTV")) || (s.Id.ToUpper().Contains("GTV")) || (s.Id.ToUpper().Contains("ITV"))) // look for ctv or Gtv or itv in name, case insensitive thanks to ToUpper
                {
                    if ((!s.Id.ToUpper().Contains("-CTV")) && (!s.Id.ToUpper().Contains("-GTV")) && (!s.Id.ToUpper().Contains("-ITV"))) // excludes lung-CTV
                        if ((!s.Id.ToUpper().Contains("RING"))) // exlude rings
                            if (!s.IsEmpty)
                                CTVandGTVs.Add(s.Id);
                }

                if (s.Id.ToUpper().Contains("PTV")) // look for ptv in name, case insensitive thanks to ToUpper
                {
                    if (!s.Id.ToUpper().Contains("-PTV")) // exlude lung-PTV
                        if (!s.IsEmpty)
                            PTVs.Add(s.Id);
                }
            }

            
            foreach (string CTV_ID in CTVandGTVs)
            {
                Structure myCTV = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id == CTV_ID); // get the CTV
                double CTV_xmin = myCTV.MeshGeometry.Bounds.X;
                double CTV_xmax = myCTV.MeshGeometry.Bounds.SizeX;
                double CTV_ymin = myCTV.MeshGeometry.Bounds.Y;
                double CTV_ymax = myCTV.MeshGeometry.Bounds.SizeY;
                double CTV_zmin = myCTV.MeshGeometry.Bounds.Z;
                double CTV_zmax = myCTV.MeshGeometry.Bounds.SizeZ;
                bool found = false;
                foreach (string PTV_ID in PTVs)
                {
                    Structure myPTV = _ctx.StructureSet.Structures.FirstOrDefault(x => x.Id == PTV_ID); // loop on PTV
                    double PTV_xmin = myPTV.MeshGeometry.Bounds.X;
                    double PTV_xmax = myPTV.MeshGeometry.Bounds.SizeX;
                    double PTV_ymin = myPTV.MeshGeometry.Bounds.Y;
                    double PTV_ymax = myPTV.MeshGeometry.Bounds.SizeY;
                    double PTV_zmin = myPTV.MeshGeometry.Bounds.Z;
                    double PTV_zmax = myPTV.MeshGeometry.Bounds.SizeZ;

                    if ((PTV_xmin < CTV_xmin) && ((PTV_xmax > CTV_xmax)))
                        if ((PTV_ymin < CTV_ymin) && ((PTV_ymax > CTV_ymax)))
                            if ((PTV_zmin < CTV_zmin) && ((PTV_zmax > CTV_zmax)))
                            {
                              
                                found = true;
                                break; // exit as soon as a PTV is found
                            }



                }
                if (found == false)
                    CTVwithoutAnyPTV.Add(CTV_ID);
                else
                    CTVwithPTV.Add(CTV_ID);

            }
            
            /*String infoMsg = null;
            foreach (string tttt in CTVwithPTV)
            {
                infoMsg += "\n" + tttt;
            }
            MessageBox.Show(infoMsg);*/

            if (CTVwithoutAnyPTV.Count() > 0) // at least one GTV/CTV has no PTV
            {
                aPTVforEveryone.setToFALSE();
                aPTVforEveryone.MeasuredValue = CTVwithoutAnyPTV.Count.ToString() + " GTV/CTV/ITV(s) n'ont pas de PTV (ou un PTV trop petit)";
                aPTVforEveryone.Infobulle = "Ces GTV/CTV n'ont pas de PTV : \n";
                foreach (string s in CTVwithoutAnyPTV)
                    aPTVforEveryone.Infobulle += " - " + s + "\n";
            }
            else
            {
                aPTVforEveryone.setToTRUE();
                aPTVforEveryone.MeasuredValue = CTVwithPTV.Count.ToString() +" GTV/CTV/ITV(s) détectés avec PTV";
                aPTVforEveryone.Infobulle = "Ces GTV/CTV/ITV(s) ont tous un PTV : \n";
                foreach (string s in CTVwithPTV)
                    aPTVforEveryone.Infobulle += " - " + s + "\n";
            }

            aPTVforEveryone.Infobulle += "\n\nUn GTV/CTV/ITV doit avoir une structure dont le nom contient 'PTV' et donc chacune des  6 dimensions (X+, X-, ...) est supérieure à celle du GTV/CTV \n";



            this._result.Add(aPTVforEveryone);
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
