
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
using System.IO;
using PlanCheck.Users;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Navigation;
using Excel = Microsoft.Office.Interop.Excel;
// Do "Add reference" in reference manager --> COM tab --> Microsoft Excel 16 object...

[assembly: AssemblyVersion("1.0.24.00")]
namespace VMS.TPS
{
    public class Script
    {
      // 
        public Script()
        {
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context)
        {

            #region check if a plan with dose is loaded, no verification plan allowed

            bool aPlanIsLoaded = true;

            if (context == null)
            {
                MessageBox.Show("Merci de charger un patient et un plan");
                return;
            }

            if (context.PlanSetup == null)
            {
                MessageBox.Show("Aucun plan charg�, les tests de plans et de dose ne seront pas effectu�s");
                aPlanIsLoaded = false;
            }
            if (aPlanIsLoaded)
            {
                if (context.PlanSetup.PlanIntent == "VERIFICATION")
                {
                    MessageBox.Show("Aucun plan de traitement charg�, les tests de plans et de dose ne seront pas effectu�s");
                    aPlanIsLoaded = false;
                }
                if (!context.PlanSetup.IsDoseValid)
                {
                    MessageBox.Show("Aucune dose dans le plan, les tests de plans et de dose ne seront pas effectu�s");
                    aPlanIsLoaded = false;
                }
            }

            #endregion

            string fullPath = Assembly.GetExecutingAssembly().Location; //get the full location of the assembly          
            string theDirectory = Path.GetDirectoryName(fullPath);//get the folder that's in                                                                  
            Directory.SetCurrentDirectory(theDirectory);// set current directory as the .dll directory

            Perform(context,aPlanIsLoaded);
        }


        public static void Perform(ScriptContext context,bool planIsLoaded)
        {
            
            PreliminaryInformation pinfo = new PreliminaryInformation(context,planIsLoaded);    //Get Plan information...      

            var window = new MainWindow(pinfo, context,planIsLoaded); // create window
            
            window.ShowDialog(); // display window, next lines not executed until it is closed
            
        }
    }
}
