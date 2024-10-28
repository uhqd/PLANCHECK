using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System.IO;
namespace PlanCheck.pdfreport
{
    internal class createPDFreport
    {
        private ScriptContext _ctx;
        private PreliminaryInformation _pinfo;
        private List<UserControl> _ListChecks;
        private MainWindow _mw;
        private MigraDoc.DocumentObjectModel.Document migraDoc;
        public createPDFreport(PreliminaryInformation pinfo, ScriptContext ctx, List<UserControl> ListChecks, MainWindow mw)  //Constructor
        {
            _ctx = ctx;
            _pinfo = pinfo;
            _ListChecks = ListChecks;
            _mw = mw;

                                
             migraDoc = new MigraDoc.DocumentObjectModel.Document();
            MigraDoc.DocumentObjectModel.Section section = migraDoc.AddSection();
            section.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Portrait;


            #region header
            MigraDoc.DocumentObjectModel.Tables.Table table = new MigraDoc.DocumentObjectModel.Tables.Table();
            table.Borders.Width = 1;
            table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.White;
            table.AddColumn(Unit.FromCentimeter(6));
            table.AddColumn(Unit.FromCentimeter(10));

            MigraDoc.DocumentObjectModel.Tables.Row row = table.AddRow();
            MigraDoc.DocumentObjectModel.Tables.Cell cell = row.Cells[0];
            cell.AddParagraph("Patient :");
            cell = row.Cells[1];
            MigraDoc.DocumentObjectModel.Paragraph paragraph = cell.AddParagraph();
            paragraph.AddFormattedText(mw.PatientFullName, TextFormat.Bold);


            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Oncologue :");
            cell = row.Cells[1];
            paragraph = cell.AddParagraph();
            paragraph.AddFormattedText(mw.DoctorName, TextFormat.Bold);

            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Commentaire : ");
            cell = row.Cells[1];
            cell.AddParagraph(mw.prescriptionComment);



            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Plan (Course) :");
            cell = row.Cells[1];
            cell.AddParagraph(mw.PlanAndCourseID);

            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Plan créé par :");
            cell = row.Cells[1];
            paragraph = cell.AddParagraph();
            paragraph.AddFormattedText(mw.PlanCreatorName, TextFormat.Bold);

            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Machine : ");
            cell = row.Cells[1];
            cell.AddParagraph(mw.theMachine);


            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Technique :");
            cell = row.Cells[1];
            cell.AddParagraph(mw.theFields);

            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Imprimé par :");
            cell = row.Cells[1];
            cell.AddParagraph(mw.CurrentUserName);

            row = table.AddRow();
            cell = row.Cells[0];
            cell.AddParagraph("Check Protocol :");
            cell = row.Cells[1];
            cell.AddParagraph(_pinfo.lastUsedCheckProtocol);



            section.Add(table);
            #endregion


            #region pdf body


            MigraDoc.DocumentObjectModel.Paragraph paragraph2 = section.AddParagraph("\n\n");
            paragraph2.AddFormattedText("\n", TextFormat.Bold);


            foreach (CheckScreen_Global csg in ListChecks)
            {


                MigraDoc.DocumentObjectModel.Paragraph paragraph1 = section.AddParagraph("\n\n" + csg._title + "\n\n");
                paragraph1.Format.Font.Bold = true;
                paragraph1.Format.Font.Size = 14;

                MigraDoc.DocumentObjectModel.Tables.Table table1 = new MigraDoc.DocumentObjectModel.Tables.Table();
                table1.Borders.Width = 1;
                table1.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Olive;

                table1.AddColumn(Unit.FromCentimeter(4.0));
                table1.AddColumn(Unit.FromCentimeter(2.8));
                table1.AddColumn(Unit.FromCentimeter(10.0));
                row = table1.AddRow();
                row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.PaleGoldenrod;
                row.Format.Font.Size = 8;
                row.Format.Font.Bold = true;

                cell = row.Cells[0];
                cell.AddParagraph("Item");
                cell = row.Cells[1];
                cell.AddParagraph("Valeur du plan");
                cell = row.Cells[2];
                cell.AddParagraph("Info");

                foreach (Item_Result ir in csg.Items)
                {
                    row = table1.AddRow();
                    row.Format.Font.Size = 6;
                    //row.Shading.Color = MigraDoc.DocumentObjectModel.Color.FromCmyk(ir.ResultStatus.Item2.Color.ScB, ir.ResultStatus.Item2.Color.ScR, ir.ResultStatus.Item2.Color.ScB, 0.0);
                    if (ir.ResultStatus.Item1 == "X")
                    {
                        row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.Red;
                        row.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.AntiqueWhite;
                    }
                    if (ir.ResultStatus.Item1 == "OK")
                    {
                        row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.LightGreen;
                        row.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                    }
                    if (ir.ResultStatus.Item1 == "WARNING")
                    {
                        row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.Orange;
                        row.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.DarkBlue;
                    }
                    if (ir.ResultStatus.Item1 == "INFO")
                    {
                        row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AntiqueWhite;
                        row.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                    }


                    row.Cells[0].AddParagraph("\n\n" + ir.Label + "\n\n");


                    row.Cells[1].AddParagraph("\n\n" + ir.MeasuredValue + "\n\n");
                    row.Cells[2].AddParagraph("\n\n" + ir.Infobulle + "\n\n");

                }
                section.Add(table1);
               // section.AddPageBreak();

            }
            #endregion

            




        }

        public void saveInDirectory(string dirname)
        {
            #region write pdf

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.None);

            string pdfFile = @"\\srv015\sf_com\simon_lu\temp\";
            pdfFile += "PlanCheck_" + _ctx.Patient.Id + "_" + _ctx.Patient.LastName + "_" + _ctx.Patient.FirstName+"_";
            pdfFile += Path.GetFileNameWithoutExtension(_mw.comboCP.SelectedItem.ToString().ToUpper()) + "_" + DateTime.Now.ToString("MM.dd.yyyy_H.mm.ss") + ".pdf";
            pdfRenderer.Document = migraDoc;
            pdfRenderer.RenderDocument();
          //  MessageBox.Show("Rapport PDF sauvegardé :\n" + pdfFile);
            pdfRenderer.PdfDocument.Save(pdfFile);
            System.Diagnostics.Process.Start(pdfFile);
            #endregion

        }

    }
}
