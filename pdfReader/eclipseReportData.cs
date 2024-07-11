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
using Excel = Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;
using iText;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;
using System.IO;
using iText.Layout.Element;
using iText.Layout.Properties;
//using iText.Kernel.Pdf.canvas.parser.PdfTextExtractor;
//using iTextSharp;


namespace PlanCheck
{

    public class eclipseReportData
    {
        public String planName;
        public String planType;
        public String machineNumber;
        public DateTime approDate;

      /* 
        public String prescriptionMode;
        public String prescriptionStructure;
        public double prescriptionTotalDose;
        public double prescriptionDosePerFraction;
        public int prescriptionNumberOfFraction;
        public String approvalStatus;
        public double MUplanned;
        public double MUplannedPerFraction;
        public double fieldWidth;
        public bool isDynamic;
        public double pitch;
        public double modulationFactor;
        public double gantryNumberOfRotation;
        public double gantryPeriod;
        public double couchTravel;
        public double couchSpeed;
        
       
        public String deliveryMode;       
        public String algorithm;
        public String resolutionCalculation;
        public double refDose;
        public double refPointX;
        public double refPointY;
        public double refPointZ;
        public String planningMethod;
        public String patientName;
        public String patientFirstName;
        public String patientID;
        //public String patientDateOfBirth;
        //public String patientSex;
        public String HUcurve;
        public String approverID;
        //public bool isHeadFirst;
        //public bool isSupine;
        public String patientPosition;
        public double originX;
        public double originY;
        public double originZ;
        public double maxDose;
        public string CTDate;
        public List<String> blockedOAR;// = new List<String>();
        //public int numberOfCTslices;
      */
        

        public eclipseReportData()  //Constructor. 
        {
            //blockedOAR = new List<String>();

        }
    }
}
