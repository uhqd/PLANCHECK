﻿using System;
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
        private List<string> listOfTargets = new List<string>();
        private List<string> listOfPTVs = new List<string>();
        private List<(string,string)> targetsAndStructList = new List<(string,string)> ();
        private PreliminaryInformation _pinfo;
        private ScriptContext _ctx;
        public selectPTVWindow(ScriptContext ctx, PreliminaryInformation pinfo) // consturctor
        {
            _pinfo = pinfo;
            _ctx = ctx;
            InitializeComponent();
            loadTargetList();

            this.Closing += selectPTWWindow_closing; // manage the closing of the window


        }
        private void loadTargetList()
        {

            foreach (var target in _ctx.PlanSetup.RTPrescription.Targets) // list of targets
            {
                listOfTargets.Add(target.TargetId);
                targetsAndStructList.Add((target.TargetId, "temporary"));
            }
            foreach (Structure s in _ctx.StructureSet.Structures) // list of structures 
            {
               // if (s.Id.ToUpper().Contains("PTV"))// && !s.Id.ToUpper().Contains("-PTV"))
                    listOfPTVs.Add(s.Id);

            }

            targetList.ItemsSource = listOfTargets;  // binding text bloc --> list of targets

            foreach (string element in listOfTargets) // create a combo box for each target
            {
                ComboBox comboBox = new ComboBox();
                comboBox.ItemsSource = listOfPTVs; // fill combobox with ptv list
                comboBox.SelectedItem = listOfPTVs[0]; // default is temporary first item
                #region get default value ...

                foreach (String s in listOfPTVs)
                {

                    
                    if (s.ToUpper().Replace(" ","") == element.ToUpper().Replace(" ", ""))
                        comboBox.SelectedItem = s;
                }
                #endregion
                

                stackPanel.Children.Add(comboBox); // Ajoutez la ComboBox au StackPanel
            }

            targetsAndStructList.Clear();

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


        // get set
        public List<(string,string)> targetStructList
        {
            get { return targetsAndStructList; }
           

        }
    }
}
