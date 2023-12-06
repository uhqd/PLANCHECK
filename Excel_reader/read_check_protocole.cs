using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Runtime.CompilerServices;
using System.Reflection;
using PlanCheck;
using PlanCheck.Users;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Navigation;
using Excel = Microsoft.Office.Interop.Excel;// Interop.Excel;
using System.Text.RegularExpressions;


namespace PlanCheck
{
    public class read_check_protocol
    {
        private string _protocolName;
        private double _CTslicewidth;
        private string _algoName;
        private double _gridsize;
        private List<string> _neededSupplImage = new List<string>();
        private List<string> _optionComp = new List<string>();
        private List<string> _POoptions = new List<string>();
        private String _prescriptionPercentage;
        private double _prescriptionPercentageDouble;
        private String _normalisationMode;
        private String _enableGating;
        private String _energy;
        private String _tolTable;
        private NTO myNTO;
        //private bool _jawTracking;
        //private List<Tuple<string, double>> _couchStructures = new List<Tuple<string, double>>();
        // private List<Tuple<string, double, double, double>> _clinicalStructures = new List<Tuple<string, double, double, double>>();
        // private List<Tuple<string, double>> _optStructures = new List<Tuple<string, double>>();

        private List<expectedStructure> _myCouchExpectedStructures = new List<expectedStructure>();
        private List<expectedStructure> _myClinicalExpectedStructures = new List<expectedStructure>();
        private List<expectedStructure> _myOptExpectedStructures = new List<expectedStructure>();
        private List<DOstructure> _myDOStructures = new List<DOstructure>();
        private List<string> _listQAplans = new List<string>();
        public bool isANumber(string a)
        {
            double myNum = 0;

            if (Double.TryParse(a, out myNum))
                return true;
            else
                return false;
        }
        public double giveMeTheDouble(string s, int row, int col, string worksheet) // row and worsheet only for error message. 
        {
            // returns the number if it is a number
            // returns 9999 with error message if it is e.g. a letter
            // returns 9999 if empty
            if ((s != null) && (s != ""))
            {
                s = s.Replace(",", ".");
                if (isANumber(s))
                    return (Convert.ToDouble(s));
                else
                {
                    MessageBox.Show("Erreur dans le check protocol. Dans la feuille " + worksheet + " L" + row.ToString() + "C" + col + ": devrait être un nombre : " + s);
                    return 9999;
                }
            }
            else
                return 9999;
        }
        public int giveMeTheInt(string s, int row, int col, string worksheet) // row and worsheet only for error message. 
        {
            // returns the number if it is a number
            // returns 9999 with error message if it is e.g. a letter
            // returns 9999 if empty

            if ((s != null) && (s != ""))
            {
                s = s.Replace(",", ".");
                if (isANumber(s))
                    return (Convert.ToInt16(s));
                else
                {
                    MessageBox.Show("Erreur dans le check protocol. Dans la feuille " + worksheet + " L" + row.ToString() + "C" + col + ": devrait être un nombre : " + s);
                    return 9999;
                }
            }
            else
                return 9999;
        }
        public expectedStructure readAStructRow(Excel.Range r, int row)
        {
            expectedStructure es = new expectedStructure();
            var temp1 = r.Cells[row, 1].Value2;
            if ((temp1 != null)&&(temp1 != ""))
            {
                string temp2 = r.Cells[row, 2].Text; // column 2
                string temp3 = r.Cells[row, 3].Text; // column 3
                //string temp4 = r.Cells[row, 4].Text; // column 4
                //string temp5 = r.Cells[row, 5].Text; // column 5
                //string temp6 = r.Cells[row, 6].Text; // column 6
                //string temp7 = r.Cells[row, 7].Text; // column 7

                es.Name = (r.Cells[row, 1].Value2).ToString();
                //es.Name = es.Name.ToUpper();
                es.HU = giveMeTheDouble(temp2, row, 2, r.Worksheet.Name);
               /*
                es.volMin = giveMeTheDouble(temp3, row, 3, r.Worksheet.Name);
                es.volMax = giveMeTheDouble(temp4, row, 4, r.Worksheet.Name);
                es.expectedNumberOfPart = giveMeTheInt(temp5, row, 5, r.Worksheet.Name);
                if ((temp6 == "R") || (temp6 == "D"))
                    es.laterality = "R";
                else if ((temp6 == "L") || (temp6 == "G"))
                    es.laterality = "L";
                else
                    es.laterality = "NONE"; // laterality cell is simply ignored if it is not L or R
               */
                es.isMandatory = false;
//                if ((temp7.ToUpper() == "OUI") || (temp7.ToUpper() == "Y") || (temp7.ToUpper() == "YES") || (temp7.ToUpper() == "1")) es.isMandatory = true;
                if ((temp3.ToUpper() == "OUI") || (temp3.ToUpper() == "Y") || (temp3.ToUpper() == "YES") || (temp3.ToUpper() == "1")) es.isMandatory = true;

            }
            if ((temp1 != null) && (temp1 != ""))
                return es;
            else
                return null;
        }

        public DOstructure readADOStructRow(Excel.Range r, int row) /// read Dose Objective
        {

            DOstructure dos = new DOstructure();
            var temp1 = r.Cells[row, 1].Value2;// get struct name
            if (temp1 != null)
            {

                dos.Name = (r.Cells[row, 1].Value2).ToString();

                int i = 2;
                while (r.Cells[row, i].Text != "")
                {
                    string s = r.Cells[row, i].Text;
                    s = s.Replace(",", "."); // fuck that french excel

                    // this part check with regex that the objective is in the format 
                    // a>xa or a<xa where a is a string and x a number
                    //     "[A-Za-z0-9]+<[0-9]*\\.[0-9]+%"

                    Regex regex1 = new Regex("[A-Za-z0-9]+(%|Gy|cc|)+<(\\-?\\d*\\.?\\d+)+(%|Gy|cc)", RegexOptions.IgnoreCase); // obj inferior
                    Regex regex2 = new Regex("[A-Za-z0-9]+(%|Gy|cc|)+>(\\-?\\d*\\.?\\d+)+(%|Gy|cc)", RegexOptions.IgnoreCase); // ob superior
                                                                                                                               //                    Regex regex1 = new Regex("[A-Za-z0-9]+<[0-9]*\\.[0-9]+[a-zA-Z]+", RegexOptions.IgnoreCase); // obj inferior
                                                                                                                               //                  Regex regex2 = new Regex("[A-Za-z0-9]+>[0-9]*\\.[0-9]+[a-zA-Z]+", RegexOptions.IgnoreCase); // ob superior
                    if ((regex1.IsMatch(s)) || (regex2.IsMatch(s)))
                    {
                        dos.listOfObjectives.Add(s);

                        //MessageBox.Show("we have a match " + s);
                    }
                    else
                    {
                        string err = "Objectif invalide feuille 5 ligne " + row.ToString() + "  col " + i.ToString() + " : " + s;
                        err += "\n\nDoit être de la forme a<xu ou a>xu ou x est un double,  a un string u une unité parmi Gy,cc,%";
                        MessageBox.Show(err);
                    }
                    i++;

                    if (i > 100)
                        break;
                }

            }
            if (temp1 != null)
                return dos;
            else
                return null;
        }


        /*
         
         READ CHECK-PROTOCOL: get data in <check-protocol-file>.xls 
         
         */
        public read_check_protocol(string pathToProtocolCheck)  //Constructor. 
        {


            #region open xls file, get the cells
            // open excel
            Excel.Application xlApp = new Excel.Application();

            // open file
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(pathToProtocolCheck, ReadOnly: true);

            // open the sheet 1
            Excel._Worksheet xlWorksheet1 = xlWorkbook.Sheets[1];
            // get the cells 1
            Excel.Range xlRange1 = xlWorksheet1.UsedRange;

            // open the sheet 2
            Excel._Worksheet xlWorksheet2 = xlWorkbook.Sheets[2];
            // get the cells 2
            Excel.Range xlRange2 = xlWorksheet2.UsedRange;

            // open the sheet 3
            Excel._Worksheet xlWorksheet3 = xlWorkbook.Sheets[3];
            // get the cells 3
            Excel.Range xlRange3 = xlWorksheet3.UsedRange;

            // open the sheet 4
            Excel._Worksheet xlWorksheet4 = xlWorkbook.Sheets[4];
            // get the cells 4
            Excel.Range xlRange4 = xlWorksheet4.UsedRange;

            // open the sheet 5
            Excel._Worksheet xlWorksheet5 = xlWorkbook.Sheets[5];
            // get the cells 5
            Excel.Range xlRange5 = xlWorksheet5.UsedRange;
            #endregion

            #region sheet 1 General

            // line 1
            _protocolName = xlRange1.Cells[1, 2].Value2;

            // line 2
            _CTslicewidth = xlRange1.Cells[3, 2].Value2;



            // line 4
            _gridsize = xlRange1.Cells[4, 2].Value2;

            // line 5  : need this in string and double format
            _prescriptionPercentage = xlRange1.Cells[5, 2].Text;
            String tempString = _prescriptionPercentage.Replace("%", "");
            tempString = tempString.Replace(",", ".");
            _prescriptionPercentageDouble = Convert.ToDouble(tempString);
            _prescriptionPercentageDouble = _prescriptionPercentageDouble / 100.0;
            // line 6
            _normalisationMode = xlRange1.Cells[6, 2].Text;

            // line 7
            _enableGating = xlRange1.Cells[7, 2].Text;

            // line 8
            _energy = xlRange1.Cells[8, 2].Text;

            // line 9
            int nQA = 2;
            while (xlRange1.Cells[9, nQA].Text != "") // parse the excel line from col 2 to first empty cell
            {
                String oneQA = xlRange1.Cells[9, nQA].Text;
                _listQAplans.Add(oneQA);
                nQA++;
            }
            // line 10
            _tolTable = xlRange1.Cells[10, 2].Text;

            // line 12
            if (xlRange1.Cells[44, 2].Text != "")
            {
                myNTO = new NTO(xlRange1.Cells[44, 2].Text,
                    xlRange1.Cells[45, 2].Value2,
                    xlRange1.Cells[49, 2].Value2,
                    xlRange1.Cells[47, 2].Value2,
                    xlRange1.Cells[48, 2].Value2,
                    xlRange1.Cells[46, 2].Value2);
            }
            /*// line 14
            _jawTracking = false;
            if (xlRange1.Cells[14, 2].Text == "true")
                _jawTracking = true;
            */
            // line 16
            int k = 0;
            for (k = 17; k < 29; k++)
                _POoptions.Add(xlRange1.Cells[k, 2].Text);


            // line 31
            _algoName = xlRange1.Cells[31, 2].Value2;
            int optnumber = 32;
            String tempo1;
            String tempo2;
            while (xlRange1.Cells[optnumber, 2].Text != "") // parse the excel line from col 3 to first empty cell
            {
                tempo1 = xlRange1.Cells[optnumber, 2].Text;
                tempo2 = tempo1.Replace(',', '.');// replace , by .
                _optionComp.Add(tempo2);
                optnumber++;
            }

            // line 53
            int colNeededImage = 1;
            while (xlRange1.Cells[53, colNeededImage].Text != "") // parse the excel line from col 0 to first empty cell
            {
                _neededSupplImage.Add(xlRange1.Cells[53, colNeededImage].Text);                
                colNeededImage++;
            }
    
            #endregion

            #region sheet 2 clinical structures

            int nRowsClinicalStruct = xlRange2.Rows.Count;
            int i = 0;
            for (i = 2; i <= nRowsClinicalStruct; i++) // read all lines sheet 2
            {
                expectedStructure es = readAStructRow(xlRange2, i);
                _myClinicalExpectedStructures.Add(es);

            }
            /*
            string allname=null;
            foreach (expectedStructure es1 in _myClinicalExpectedStructures)
            {
                allname += " ";
                allname += es1.Name;
            }
            MessageBox.Show(allname);   
            */
            #endregion

            #region sheet 3 opt structures

            int nRowsOptlStruct = xlRange3.Rows.Count;

            for (i = 2; i <= nRowsOptlStruct; i++) // read all lines sheet 2
            {
                expectedStructure es = readAStructRow(xlRange3, i);
                _myOptExpectedStructures.Add(es);
            }

            #endregion

            #region sheet 4 Couch structures

            int nRowsCouchStruct = xlRange4.Rows.Count;
            for (i = 2; i <= nRowsCouchStruct; i++) // read all lines sheet 4
            {
                expectedStructure es = readAStructRow(xlRange4, i);
                _myCouchExpectedStructures.Add(es);
            }
            #endregion

            #region sheet 5 Structure with dose objectives

            int nRowsDOStruct = xlRange5.Rows.Count;

            for (i = 2; i <= nRowsDOStruct; i++) // read all lines sheet 2
            {
                DOstructure dos = readADOStructRow(xlRange5, i); // read a line sheet 5                
                if (dos != null)
                    if (dos.listOfObjectives.Count() > 0)
                    {
                        _myDOStructures.Add(dos); // 
                                                  //MessageBox.Show("in rcp read " + dos.Name);

                        //foreach( string s in dos.listOfObjectives ) MessageBox.Show("in rcp read " + dos.Name + " " +s);
                    }
            }
            /*
            foreach (DOstructure mydos in _myDOStructures)
                foreach(string s in mydos.listOfObjectives)
                    MessageBox.Show("in rcp read " + s);
            */
            #endregion

            #region cleanup excel
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Marshal.ReleaseComObject(xlRange1);
            Marshal.ReleaseComObject(xlRange2);
            Marshal.ReleaseComObject(xlRange3);
            Marshal.ReleaseComObject(xlWorksheet1);
            Marshal.ReleaseComObject(xlWorksheet2);
            Marshal.ReleaseComObject(xlWorksheet3);
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
            #endregion

        }

        #region GET/SET
        public double CTslicewidth
        {
            get { return _CTslicewidth; }
        }
        public string protocolName
        {
            get { return _protocolName; }
        }
        public string algoName
        {
            get { return _algoName; }
        }
        public double gridSize
        {
            get { return _gridsize; }
        }
        public List<string> optionComp
        {
            get { return _optionComp; }
        }
        public List<string> POoptions
        {
            get { return _POoptions; }
        }
        public List<string> needeSupplImages
        {
            get { return _neededSupplImage; }
        }
        public string prescriptionPercentage
        {
            get { return _prescriptionPercentage; }
        }
        public double  prescriptionPercentageDouble
        {
            get { return _prescriptionPercentageDouble; }
        }
        public string normalisationMode
        {
            get { return _normalisationMode; }
        }
        public string enebleGating
        {
            get { return _enableGating; }
        }
        public string energy
        {
            get { return _energy; }
        }
        public string toleranceTable
        {
            get { return _tolTable; }
        }
        public List<expectedStructure> myClinicalExpectedStructures
        {
            get { return _myClinicalExpectedStructures; }
        }
        public List<expectedStructure> myOptExpectedStructures
        {
            get { return _myOptExpectedStructures; }
        }
        public List<expectedStructure> myCouchExpectedStructures
        {
            get { return _myCouchExpectedStructures; }
        }
        public List<DOstructure> myDOStructures
        {
            get { return _myDOStructures; }
        }
        public List<String> listQAplans
        {
            get { return _listQAplans; }
        }
        public NTO NTOparams
        {
            get { return myNTO; }
        }
        /*public bool JawTracking
        {
            get { return _jawTracking; }
        }*/
        #endregion


    }
}
