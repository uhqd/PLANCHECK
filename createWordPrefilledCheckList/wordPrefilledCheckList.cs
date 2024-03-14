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
       // public static int resulTableRowIndex = 0;
        private List<UserControl> _ListChecks;
        private string textfilename;

        private void cosmeticTable(Microsoft.Office.Interop.Word.Table table, WdColor color)
        {
            foreach (Column column in table.Columns)
            {
                column.AutoFit();
            }

            foreach (Row row in table.Rows)
            {
                foreach (Cell cell in row.Cells)
                {
                    cell.Range.Shading.BackgroundPatternColor = color;
                }
            }


        }
        private bool addToResultTable(Microsoft.Office.Interop.Word.Table table2, string resultType, Microsoft.Office.Interop.Word.Document document, int nTests, WdColor color, bool checkboxStatus)
        {
            //MessageBox.Show(resultType + " " + resulTableRowIndex.ToString());
            bool ok = true;
            int resulTableRowIndex = 0;


            if (nTests == 0)
                ok = false;
            else
            {


                foreach (CheckScreen_Global csg in _ListChecks)
                {
                    foreach (Item_Result ir in csg.Items)
                    {
                        if ((ir.ResultStatus.Item1 == resultType) && (ir.MeasuredValue != ""))
                        {

                            resulTableRowIndex++;

                            // column 1
                            var cell1 = table2.Rows[resulTableRowIndex].Cells[1];
                            var cell2 = table2.Rows[resulTableRowIndex].Cells[2];
                            var cell3 = table2.Rows[resulTableRowIndex].Cells[3];
                            var cell4 = table2.Rows[resulTableRowIndex].Cells[4];

                            cell1.Range.Text = ir.Label; //csg._title + " -> " + ir.Label;

                            ////cell1.Range.Font.Bold = 1;
                            ////cell1.Range.Font.Size = 8;
                            ////cell1.Shading.BackgroundPatternColor = color;//WdColor.wdColorLightGreen;
                            ////cell1.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            ////cell1.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;

                            //MessageBox.Show("END OF CELL 1");



                            // column 2
                            cell2.Range.Text = formatThatString(ir.MeasuredValue);

                            //cell2.Range.Font.Bold = 0;
                            //cell2.Range.Font.Size = 8;
                            //cell2.Shading.BackgroundPatternColor = color;//WdColor.wdColorLightGreen;
                            //cell2.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            //cell2.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                            // MessageBox.Show("END OF CELL 2");

                            //column 3
                            Microsoft.Office.Interop.Word.ContentControl checkboxControlN = cell3.Range.ContentControls.Add(WdContentControlType.wdContentControlCheckBox);
                            //cell3.Range.Font.Bold = 0;
                            //cell3.Range.Font.Size = 8;
                            //cell3.Shading.BackgroundPatternColor = color;//WdColor.wdColorLightGreen;
                            //cell3.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            //cell3.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                            //cell3.PreferredWidth = 3;
                            //MessageBox.Show("END OF CELL 3");


                            //column 4 : commentaire
                            if (ir.ResultStatus.Item1 == "X" || ir.ResultStatus.Item1 == "WARNING")
                                cell4.Range.Text = ir.Infobulle + "\n\nCommentaire :\n";
                            else if (ir.ResultStatus.Item1 == "UNCHECK")
                                cell4.Range.Text = "";
                            else
                                cell4.Range.Text = ir.Infobulle;


                            //cell4.Range.Font.Bold = 0;
                            //cell4.Range.Font.Size = 8;
                            //cell4.Shading.BackgroundPatternColor = color;//WdColor.wdColorLightGreen;
                            //cell4.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            //cell4.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                            //cell4.PreferredWidth = 47;//(float)0.45 * (float) tableUncheck.PreferredWidth;
                            //MessageBox.Show("END OF CELL 4");

                            checkboxControlN.Checked = checkboxStatus;
                            checkboxControlN.Title = csg._title;
                        }
                    }
                }
            }
            // MessageBox.Show("END OF ADD TABLE");
            return ok;
        }

        #region A JETER ?
        /*
        
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
                Microsoft.Office.Interop.Word.Table tableUncheck = document.Tables.Add(para2.Range, nTests + 1, 3, ref missing, ref missing);
                tableUncheck.Borders.Enable = 1; // Enable table borders
                //tableUncheck.PreferredWidth = 450.0f; 
                tableUncheck.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitContent); // Autofit table to content

                foreach (Microsoft.Office.Interop.Word.Row row in tableUncheck.Rows)
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


                tableUncheck.Rows[1].Cells[1].Range.Text = "Test";
                tableUncheck.Rows[1].Cells[2].Range.Text = "Résultats";
                tableUncheck.Rows[1].Cells[3].Range.Text = "Check";

                int testOK = 1;
                foreach (CheckScreen_Global csg in _ListChecks)
                {
                    foreach (Item_Result ir in csg.Items)
                    {
                        if (ir.ResultStatus.Item1 == resultType)
                        {
                            testOK++;
                            // column 1
                            tableUncheck.Rows[testOK].Cells[1].Range.Text = ir.Label; //csg._title + " -> " + ir.Label;

                            // column 2
                            tableUncheck.Rows[testOK].Cells[2].Range.Text = formatThatString(ir.MeasuredValue);// + "\n"+formatThisStringForTheCheckList(ir.Infobulle);// Label;


                            //column 3
                            Microsoft.Office.Interop.Word.ContentControl checkboxControlN = tableUncheck.Rows[testOK].Cells[3].Range.ContentControls.Add(WdContentControlType.wdContentControlCheckBox);
                            checkboxControlN.Checked = checkboxStatus;
                            checkboxControlN.Title = csg._title;
                        }
                    }
                }
            }
            return ok;
        }
        */
        #endregion
        private string formatThatString(string s)
        {
            s = s.Replace("  ", " ");
            return s;
        }
        public wordPrefilledCheckList(PreliminaryInformation pinfo, ScriptContext ctx, List<UserControl> ListChecks, MainWindow mw)  //Constructor
        {


            _ctx = ctx;
            _pinfo = pinfo;
            _ListChecks = ListChecks;

            // color of the table lines
            var wdcUncheck = (WdColor)(255 + 0x100 * 255 + 0x10000 * 213); // pale yellow
            var wdcX = (WdColor)(252 + 0x100 * 85 + 0x10000 * 62); // pale red
            var wdcWarn = (WdColor)(255 + 0x100 * 188 + 0x10000 * 143); // pale orange
            var wdcInfo = WdColor.wdColorGray05;//pale gray
            var wdcOk = (WdColor)(183 + 0x100 * 255 + 0x10000 * 183); // pale yellow

            #region loop on results count results of tests
            int testOK = 0;
            int testWarn = 0;
            int testError = 0;
            int testInfo = 0;
            int uncheckedTest = 0;
            //int nTests = 0;
            foreach (CheckScreen_Global csg in ListChecks)
            {


                foreach (Item_Result ir in csg.Items)
                {
                    if (ir.ResultStatus.Item1 == "OK")
                    {
                        testOK++;
                    }
                    else if (ir.ResultStatus.Item1 == "X")
                    {
                        testError++;
                    }
                    else if (ir.ResultStatus.Item1 == "INFO")
                    {
                        testInfo++;
                    }
                    else if (ir.ResultStatus.Item1 == "WARNING")
                    {
                        testWarn++;
                    }
                    else if (ir.ResultStatus.Item1 == "UNCHECK")
                    {
                        uncheckedTest++;
                    }
                    else
                    {
                        MessageBox.Show("Type inconnu " + ir.ResultStatus.Item1);
                    }
                }
            }
            //MessageBox.Show("test error : " + testError.ToString());
            //nTests = testOK + testError + testInfo + testWarn + uncheckedTest;

            #endregion

            #region init
            winword = new Microsoft.Office.Interop.Word.Application();
            winword.ShowAnimation = false;
            winword.Visible = false;
            missing = System.Reflection.Missing.Value;
            document = winword.Documents.Add(ref missing, ref missing, ref missing, ref missing);
            document.PageSetup.Orientation = WdOrientation.wdOrientLandscape;
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

            #region first table (header for general info) 
            document.Content.SetRange(0, 0);
            Microsoft.Office.Interop.Word.Paragraph para1 = document.Content.Paragraphs.Add(ref missing);
            para1.Range.Font.Size = 12;
            para1.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            para1.Range.Text = Environment.NewLine;
            para1.Range.Text = Environment.NewLine;


            Microsoft.Office.Interop.Word.Table table1 = document.Tables.Add(para1.Range, 4, 4, ref missing, ref missing);
            table1.PreferredWidth = 450.0f;
            table1.Borders.Enable = 1;
            //table1.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitContent); // Autofit table to content
            table1.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow);
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





            
            para1.Range.Text = Environment.NewLine;
            #endregion



            #region second table (results)

            Microsoft.Office.Interop.Word.Paragraph para2 = document.Content.Paragraphs.Add(ref missing);
            //para2.Range.InsertParagraphAfter();

            if (uncheckedTest > 0)
            {
                para2.Range.Text = "\n\n";
                para2.Range.Text = "NON VERIFIE PAR PLANCHECK\n";
                para2.Range.Text = "\n";


                Microsoft.Office.Interop.Word.Table tableUncheck = document.Tables.Add(para2.Range, uncheckedTest, 4, ref missing, ref missing);
                tableUncheck.Borders.Enable = 1; // Enable table borders                                       
                tableUncheck.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow); // Autofit table to conten            
                addToResultTable(tableUncheck, "UNCHECK", document, uncheckedTest, wdcUncheck, false);
                cosmeticTable(tableUncheck, wdcUncheck);
            }



            if (testError > 0)
            {
                para2.Range.Text = "\n";
                para2.Range.Text = "ERREUR DETECTE PAR PLANCHECK\n";
                para2.Range.Text = "\n";


                Microsoft.Office.Interop.Word.Table tableX = document.Tables.Add(para2.Range, testError, 4, ref missing, ref missing);
                tableX.Borders.Enable = 1; // Enable table borders                                       
                tableX.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow); // Autofit table to conten            
                addToResultTable(tableX, "X", document, testError, wdcX, false);
                cosmeticTable(tableX, wdcX);

            }

            if (testWarn > 0)
            {
                para2.Range.Text = "\n";
                para2.Range.Text = "WARNING DETECTE PAR PLANCHECK\n";
                para2.Range.Text = "\n";
                Microsoft.Office.Interop.Word.Table tableWarning = document.Tables.Add(para2.Range, testWarn, 4, ref missing, ref missing);
                tableWarning.Borders.Enable = 1; // Enable table borders                                       
                tableWarning.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow); // Autofit table to conten            
                addToResultTable(tableWarning, "WARNING", document, testWarn, wdcWarn, false);
                cosmeticTable(tableWarning, wdcWarn);
            }
            if (testInfo > 0)
            {
                para2.Range.Text = "\n";
                para2.Range.Text = "TESTS INFORMATIFS PAR PLANCHECK\n";
                para2.Range.Text = "\n";
                Microsoft.Office.Interop.Word.Table tableInfo = document.Tables.Add(para2.Range, testInfo, 4, ref missing, ref missing);
                tableInfo.Borders.Enable = 1; // Enable table borders                                       
                tableInfo.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow); // Autofit table to conten            
                addToResultTable(tableInfo, "INFO", document, testInfo, wdcInfo, false);
                cosmeticTable(tableInfo, wdcInfo);
            }

            if (testOK > 0)
            {
                //para2.Range.Font.Color = WdColor.wdColorBlue;
                para2.Range.Text = "\n";
                para2.Range.Text = "TESTS OK PAR PLANCHECK\n";
                para2.Range.Text = "\n";
                Microsoft.Office.Interop.Word.Table tableOK = document.Tables.Add(para2.Range, testOK, 4, ref missing, ref missing);
                tableOK.Borders.Enable = 1; // Enable table borders                                       
                         
                addToResultTable(tableOK, "OK", document, testOK, wdcOk, true);
                // tableOK.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow); // Autofit table to conten   

                cosmeticTable(tableOK,wdcOk);




            }



            foreach (Microsoft.Office.Interop.Word.Paragraph paragraph in document.Paragraphs)
            {
                paragraph.SpaceAfter = 0; // Set the space after the paragraph to 0 (remove the space)
            }
            #endregion



        }

        
        public void saveInDirectory(string dirname)
        {

            textfilename = dirname;
            textfilename += myToday.ToString();
            textfilename += "_temp1.docx";
            textfilename = textfilename.Replace(":", "_");
            textfilename = textfilename.Replace(" ", "_");
            textfilename = textfilename.Replace("/", "_");

            MessageBox.Show("Checklist préparée dans\n" + dirname + "\nDecommenter L673 MainWindow.xaml.cs pour envoi vers ARIA documents. ");
            object filename = textfilename;
            document.SaveAs2(ref filename);

        }


        public void saveToAria()
        {
            AriaSender asender = new AriaSender(_ctx, textfilename, document);

        }

        public void closeWDC()
        {
            document.Close(ref missing, ref missing, ref missing);
            winword.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(document);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(winword);
        }
    }
}
