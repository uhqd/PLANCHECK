using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
namespace PlanCheck.xaml_datas
{

    public partial class selectPTVWindow : Window
    {
        private static bool allTargetAreFound;
        private List<string> listOfTargets = new List<string>();
        private List<string> listOfStructures = new List<string>();
        private List<(string, string)> targetsAndStructList = new List<(string, string)>();
        private PreliminaryInformation _pinfo;
        private ScriptContext _ctx;
        public selectPTVWindow(ScriptContext ctx, PreliminaryInformation pinfo) // consturctor
        {
            _pinfo = pinfo;
            _ctx = ctx;
            InitializeComponent();
            this.Closing += selectPTWWindow_closing; // manage the closing of the window
            loadTargetList();


          //  if (allTargetAreFound)
           // {
                // Call close without clicking :
             //   RoutedEventArgs args = new RoutedEventArgs();
               // close_Click(null, args);
          //  }


        }
        private void loadTargetList()
        {

            foreach (var target in _ctx.PlanSetup.RTPrescription.Targets) // list of targets
            {
                listOfTargets.Add(target.TargetId);
               // targetsAndStructList.Add((target.TargetId, "temporary"));
            }
            foreach (Structure s in _ctx.StructureSet.Structures) // list of structures 
            {
                // if (s.Id.ToUpper().Contains("PTV"))// && !s.Id.ToUpper().Contains("-PTV"))
                listOfStructures.Add(s.Id);

            }

            targetList.ItemsSource = listOfTargets;  // binding text bloc --> list of targets

            bool oneTargetIsFound = false;
             allTargetAreFound = true;
            foreach (string element in listOfTargets) // create a combo box for each target
            {
                ComboBox comboBox = new ComboBox();
                comboBox.ItemsSource = listOfStructures; // fill combobox with ptv list
                comboBox.SelectedItem = listOfStructures[0]; // default is temporary first item
                #region get default value ...

                oneTargetIsFound = false;
                foreach (String s in listOfStructures)
                {


                    if (s.ToUpper().Replace(" ", "") == element.ToUpper().Replace(" ", ""))
                    {
                        comboBox.SelectedItem = s;
                        oneTargetIsFound = true;
                    }
                }
                #endregion

                if (oneTargetIsFound == false)
                    allTargetAreFound = false;

                stackPanel.Children.Add(comboBox); // Ajoutez la ComboBox au StackPanel
            }

           // targetsAndStructList.Clear();

            

        }
        private void close_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
        private void selectPTWWindow_closing(object sender, System.ComponentModel.CancelEventArgs e) // this is executed whatever the way of window closing
        {

            List<String> userchoices = new List<string>();
            foreach (ComboBox cb in stackPanel.Children)
            {
                userchoices.Add(cb.SelectedValue.ToString());

            }

            // zip both lists. 
            targetsAndStructList = listOfTargets.Zip(userchoices, (element1, element2) => (element1, element2)).ToList();


        }
        public bool allPTVarefound
        {
            get { return allTargetAreFound; }


        }
        // get set
        public List<(string, string)> targetStructList
        {
            get { return targetsAndStructList; }


        }


    }
}
