using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using Microsoft.Office.Interop.Word;

namespace PlanCheck.createWordPrefilledCheckList
{
    internal class wordPrefilledCheckList
    {
        private ScriptContext _ctx;
        private PreliminaryInformation _pinfo;
        private DateTime myToday = DateTime.Now;
        private Microsoft.Office.Interop.Word.Document document;
        private object missing;
        private Microsoft.Office.Interop.Word.Application winword;
        public static int resulTableRowIndex = 0;
        private List<UserControl> _ListChecks;
        private string textfilename;
        private bool addToResultTable(Microsoft.Office.Interop.Word.Table table2, string resultType, Microsoft.Office.Interop.Word.Document document, int nTests, WdColor color, bool checkboxStatus)
        {
            //MessageBox.Show(resultType + " " + resulTableRowIndex.ToString());
            bool ok = true;
            if (nTests == 0)
                ok = false;
            else
            {


                /*    foreach (Microsoft.Office.Interop.Word.Row row in table2.Rows)
                    {
                        foreach (Microsoft.Office.Interop.Word.Cell cell in row.Cells)
                        {
                            cell.Range.Font.Bold = 1;
                            cell.Range.Font.Size = 8;
                            cell.Shading.BackgroundPatternColor = color;//WdColor.wdColorLightGreen;
                            cell.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                        }
                    }*/


                foreach (CheckScreen_Global csg in _ListChecks)
                {
                    foreach (Item_Result ir in csg.Items)
                    {
                        if (ir.ResultStatus.Item1 == resultType)
                        {
                            resulTableRowIndex++;
                            // column 1
                            table2.Rows[resulTableRowIndex].Cells[1].Range.Text = ir.Label; //csg._title + " -> " + ir.Label;
                            table2.Rows[resulTableRowIndex].Cells[1].Range.Font.Bold = 1;
                            table2.Rows[resulTableRowIndex].Cells[1].Range.Font.Size = 8;
                            table2.Rows[resulTableRowIndex].Cells[1].Shading.BackgroundPatternColor = color;//WdColor.wdColorLightGreen;
                            table2.Rows[resulTableRowIndex].Cells[1].VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            table2.Rows[resulTableRowIndex].Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;


                            // column 2
                            table2.Rows[resulTableRowIndex].Cells[2].Range.Text = formatThatString(ir.MeasuredValue);// + "\n"+formatThisStringForTheCheckList(ir.Infobulle);// Label;
                            table2.Rows[resulTableRowIndex].Cells[2].Range.Font.Bold = 0;
                            table2.Rows[resulTableRowIndex].Cells[2].Range.Font.Size = 8;
                            table2.Rows[resulTableRowIndex].Cells[2].Shading.BackgroundPatternColor = color;//WdColor.wdColorLightGreen;
                            table2.Rows[resulTableRowIndex].Cells[2].VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            table2.Rows[resulTableRowIndex].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;


                            //column 3
                            Microsoft.Office.Interop.Word.ContentControl checkboxControlN = table2.Rows[resulTableRowIndex].Cells[3].Range.ContentControls.Add(WdContentControlType.wdContentControlCheckBox);
                            table2.Rows[resulTableRowIndex].Cells[3].Range.Font.Bold = 0;
                            table2.Rows[resulTableRowIndex].Cells[3].Range.Font.Size = 8;
                            table2.Rows[resulTableRowIndex].Cells[3].Shading.BackgroundPatternColor = color;//WdColor.wdColorLightGreen;
                            table2.Rows[resulTableRowIndex].Cells[3].VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            table2.Rows[resulTableRowIndex].Cells[3].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;




                            checkboxControlN.Checked = checkboxStatus;
                            checkboxControlN.Title = csg._title;
                        }
                    }
                }
            }
            return ok;
        }
        private bool drawTable(string resultType, Microsoft.Office.Interop.Word.Document document, int nTests, WdColor color, bool checkboxStatus)
        {
            bool ok = true;
            if (nTests == 0)
                ok = false;
            else
            {
                object missing = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Word.Paragraph para2 = document.Content.Paragraphs.Add(ref missing);
                para2.Range.Text = "\nRésultats " + resultType + " Plancheck";
                para2.Range.InsertParagraphAfter();
                Microsoft.Office.Interop.Word.Table table2 = document.Tables.Add(para2.Range, nTests + 1, 3, ref missing, ref missing);
                table2.Borders.Enable = 1; // Enable table borders
                //table2.PreferredWidth = 450.0f; 
                table2.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitContent); // Autofit table to content

                foreach (Microsoft.Office.Interop.Word.Row row in table2.Rows)
                {
                    foreach (Microsoft.Office.Interop.Word.Cell cell in row.Cells)
                    {
                        cell.Range.Font.Bold = 1;
                        cell.Range.Font.Size = 8;
                        cell.Shading.BackgroundPatternColor = color;//WdColor.wdColorLightGreen;
                        cell.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                        cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                    }
                }


                table2.Rows[1].Cells[1].Range.Text = "Test";
                table2.Rows[1].Cells[2].Range.Text = "Résultats";
                table2.Rows[1].Cells[3].Range.Text = "Check";

                int testOK = 1;
                foreach (CheckScreen_Global csg in _ListChecks)
                {
                    foreach (Item_Result ir in csg.Items)
                    {
                        if (ir.ResultStatus.Item1 == resultType)
                        {
                            testOK++;
                            // column 1
                            table2.Rows[testOK].Cells[1].Range.Text = ir.Label; //csg._title + " -> " + ir.Label;

                            // column 2
                            table2.Rows[testOK].Cells[2].Range.Text = formatThatString(ir.MeasuredValue);// + "\n"+formatThisStringForTheCheckList(ir.Infobulle);// Label;


                            //column 3
                            Microsoft.Office.Interop.Word.ContentControl checkboxControlN = table2.Rows[testOK].Cells[3].Range.ContentControls.Add(WdContentControlType.wdContentControlCheckBox);
                            checkboxControlN.Checked = checkboxStatus;
                            checkboxControlN.Title = csg._title;
                        }
                    }
                }
            }
            return ok;
        }
        private string formatThatString(string s)
        {
            s = s.Replace("  ", " ");
            return s;
        }
        public wordPrefilledCheckList(PreliminaryInformation pinfo, ScriptContext ctx, List<UserControl> ListChecks,MainWindow mw)  //Constructor
        {


            _ctx = ctx;
            _pinfo = pinfo;
            _ListChecks = ListChecks;


            #region loop on results count results of tests
            int testOK = 0;
            int testWarn = 0;
            int testError = 0;
            int testInfo = 0;
            int uncheckedTest = 0;
            int nTests = 0;
            foreach (CheckScreen_Global csg in ListChecks)
            {


                foreach (Item_Result ir in csg.Items)
                {
                    if (ir.ResultStatus.Item1 == "OK")
                    {
                        testOK++;
                    }
                    if (ir.ResultStatus.Item1 == "X")
                    {
                        testError++;
                    }
                    if (ir.ResultStatus.Item1 == "INFO")
                    {
                        testInfo++;
                    }
                    if (ir.ResultStatus.Item1 == "WARNING")
                    {
                        testWarn++;
                    }
                    if (ir.ResultStatus.Item1 == "UNCHECK")
                    {
                        uncheckedTest++;
                    }
                }
            }
            nTests = testOK + testError + testInfo + testWarn + uncheckedTest;

            #endregion

            #region init
            winword = new Microsoft.Office.Interop.Word.Application();
            winword.ShowAnimation = false;
            winword.Visible = false;
            missing = System.Reflection.Missing.Value;
            document = winword.Documents.Add(ref missing, ref missing, ref missing, ref missing);
            myToday = DateTime.Now;
            #endregion

            #region header of the document
            foreach (Microsoft.Office.Interop.Word.Section section in document.Sections)
            {
                Microsoft.Office.Interop.Word.Range headerRange = section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                headerRange.Fields.Add(headerRange, Microsoft.Office.Interop.Word.WdFieldType.wdFieldPage);
                headerRange.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                headerRange.Font.ColorIndex = Microsoft.Office.Interop.Word.WdColorIndex.wdBlue;
                headerRange.Font.Size = 12;
                headerRange.Text = "Checklist générée par Plancheck";
            }
            #endregion

            #region footers of the document  
            foreach (Microsoft.Office.Interop.Word.Section wordSection in document.Sections)
            {
                //Get the footer range and add the footer details.  
                Microsoft.Office.Interop.Word.Range footerRange = wordSection.Footers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                footerRange.Font.ColorIndex = Microsoft.Office.Interop.Word.WdColorIndex.wdBlack;
                footerRange.Font.Size = 10;
                footerRange.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                string footText = "Analyse réalisée le " + myToday + " par " + _pinfo.CurrentUser.UserFirstName + " " + _pinfo.CurrentUser.UserFamilyName;
                footerRange.Text = footText;// "Footer text goes here";
            }
            #endregion

            #region first table general info 
            document.Content.SetRange(0, 0);
            Microsoft.Office.Interop.Word.Paragraph para1 = document.Content.Paragraphs.Add(ref missing);
            para1.Range.Font.Size = 12;
            para1.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            para1.Range.Text = Environment.NewLine;
            para1.Range.Text = Environment.NewLine;


            Microsoft.Office.Interop.Word.Table table1 = document.Tables.Add(para1.Range, 4, 4, ref missing, ref missing);
            table1.PreferredWidth = 450.0f;
            table1.Borders.Enable = 1;
            table1.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitContent); // Autofit table to content
            foreach (Microsoft.Office.Interop.Word.Row row in table1.Rows)
            {
                foreach (Microsoft.Office.Interop.Word.Cell cell in row.Cells)
                {
                    cell.Range.Font.Bold = 1;
                    cell.Range.Font.Size = 8;

                    var wdc = (WdColor)(229 + 0x100 * 243 + 0x10000 * 229); // pale green
                    cell.Shading.BackgroundPatternColor = wdc;//WdColor.wdColorLightYellow;  // 229 243 229
                    cell.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                    if (cell.ColumnIndex % 2 != 0)
                        cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                    else
                        cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                }
            }


            table1.Rows[1].Cells[1].Range.Text = "Patient : ";
            string temp = mw.PatientFullName.Replace("    ", "");
            table1.Rows[1].Cells[2].Range.Text = temp;
            table1.Rows[1].Cells[3].Range.Text = "Oncologue : ";
            temp = mw.DoctorName.Replace("    ", "");
            table1.Rows[1].Cells[4].Range.Text = temp;
            table1.Rows[2].Cells[1].Range.Text = "Commentaire : ";
            temp = mw.prescriptionComment.Replace("    ", "");
            table1.Rows[2].Cells[2].Range.Text = temp;
            table1.Rows[2].Cells[3].Range.Text = "Plan (Course) : ";
            temp = mw.PlanAndCourseID.Replace("    ", "");
            table1.Rows[2].Cells[4].Range.Text = temp;
            table1.Rows[3].Cells[1].Range.Text = "Plan créé par : ";
            temp = mw.PlanCreatorName.Replace("    ", "");
            table1.Rows[3].Cells[2].Range.Text = temp;
            table1.Rows[3].Cells[3].Range.Text = "Machine : ";
            temp = mw.theMachine.Replace("    ", "");
            string temp2 = mw.theFields.Replace("    ", "");
            table1.Rows[3].Cells[4].Range.Text = temp + " (" + temp2 + ")";
            table1.Rows[4].Cells[1].Range.Text = "Imprimé par : ";
            temp = mw.CurrentUserName.Replace("    ", "");
            table1.Rows[4].Cells[2].Range.Text = temp;
            string[] protocolOk = _pinfo.lastUsedCheckProtocol.Split(':');
            table1.Rows[4].Cells[3].Range.Text = "Check Protocol : ";
            temp = mw.CurrentUserName.Replace(" ", "");
            table1.Rows[4].Cells[4].Range.Text = protocolOk[1];





            para1.Range.Text = Environment.NewLine;  // line return
            para1.Range.Text = Environment.NewLine;
            para1.Range.Text = Environment.NewLine;  // line return
            para1.Range.Text = Environment.NewLine;
            #endregion

            #region result tables (old)
            /*
             drawTable("UNCHECK", document,uncheckedTest, WdColor.wdColorLightYellow, false);
             drawTable("X", document, testError, WdColor.wdColorRed, false);
             drawTable("WARNING", document, testWarn, WdColor.wdColorOrange, false);
             drawTable("INFO", document, testInfo, WdColor.wdColorGray05, false);
             drawTable("OK", document, resulTableRowIndex, WdColor.wdColorAqua, true);
            */
            #endregion

            #region cosmetic
            // object missing = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Word.Paragraph para2 = document.Content.Paragraphs.Add(ref missing);
            para2.Range.InsertParagraphAfter();
            Microsoft.Office.Interop.Word.Table table2 = document.Tables.Add(para2.Range, nTests, 3, ref missing, ref missing);
            table2.Borders.Enable = 1; // Enable table borders
            table2.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitContent); // Autofit table to content


            var wdcUncheck = (WdColor)(255 + 0x100 * 255 + 0x10000 * 213); // pale yellow
            var wdcX = (WdColor)(252 + 0x100 * 85 + 0x10000 * 62); // pale red
            var wdcWarn = (WdColor)(255 + 0x100 * 188 + 0x10000 * 143); // pale orange
            var wdcInfo = WdColor.wdColorGray05;//pale gray
            var wdcOk = (WdColor)(183 + 0x100 * 255 + 0x10000 * 183); // pale yellow

            addToResultTable(table2, "UNCHECK", document, uncheckedTest, wdcUncheck, false);
            addToResultTable(table2, "X", document, testError, wdcX, false);
            addToResultTable(table2, "WARNING", document, testWarn, wdcWarn, false);
            addToResultTable(table2, "INFO", document, testInfo, wdcInfo, false);
            addToResultTable(table2, "OK", document, resulTableRowIndex, wdcOk, true);



            foreach (Microsoft.Office.Interop.Word.Paragraph paragraph in document.Paragraphs)
            {
                paragraph.SpaceAfter = 0; // Set the space after the paragraph to 0 (remove the space)
            }
            #endregion

          

        }
        public void saveInDirectory(string dirname)
        {
            #region Save the  word  document
             textfilename = dirname;
            textfilename += myToday.ToString();
            textfilename += "_temp1.docx";
            textfilename = textfilename.Replace(":", "_");
            textfilename = textfilename.Replace(" ", "_");
            textfilename = textfilename.Replace("/", "_");
            MessageBox.Show("Checklist préparée et  partiellement préremplie par Plancheck: \n" + textfilename);
            object filename = textfilename;
            document.SaveAs2(ref filename);
            document.Close(ref missing, ref missing, ref missing);
            document = null;
            winword.Quit(ref missing, ref missing, ref missing);
            winword = null;
            #endregion

            #region proposed by chatGPT to avoid memory leak

            // System.Runtime.InteropServices.Marshal.ReleaseComObject(checkboxControlN);
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
            //    System.Runtime.InteropServices.Marshal.ReleaseComObject(document);
            try
            {

                System.Runtime.InteropServices.Marshal.ReleaseComObject(winword);
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(checkboxControlN);

                //System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(document);
            }
            catch
            {
                ;
            }
            #endregion

        }


        public void saveToAria()
        {
            AriaSender asender = new AriaSender(_ctx, textfilename,document);

        }


    }
}
