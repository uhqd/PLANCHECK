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
    /// <summary>
    /// Logique d'interaction pour chooseNonFEplanWindow.xaml
    /// </summary>
    public partial class chooseUserPreferences : Window
    {
        public List<string> MaListe { get; set; }
        private PreliminaryInformation _pinfo;
        private ScriptContext _ctx;
        public chooseUserPreferences(ScriptContext ctx, PreliminaryInformation pinfo)
        {
            _pinfo = pinfo;
            _ctx = ctx;

            InitializeComponent();



            MaListe = new List<string>();
            foreach ((string text, bool valeurBool,string text2) in _pinfo.actualUserPreference.userPreferencesList)
            {
                MaListe.Add(text2);
            }



            // Définition du contexte de données (chatGPT ?)
            DataContext = this;
            // Ceci est appelé juste avant l'affichage de la fenêtre
            Loaded += beforeWindowIsDisplayed;

        }
        // Méthode pour trouver un enfant de type T dans un élément visuel (chatGPT)
        private T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);

                    if (child != null && child is T && (string.IsNullOrEmpty(childName) || (child as FrameworkElement)?.Name == childName))
                    {
                        return (T)child;
                    }

                    var childOfChild = FindChild<T>(child, childName);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
        // called just before the window dispay (chat GPT)
        // allows to set the check box status according to the prefs file
        private void beforeWindowIsDisplayed(object sender, RoutedEventArgs e)
        {
            // Accéder aux CheckBox à l'intérieur de l'ItemsControl
            int i = 0;
            foreach (var item in itemsControl.Items)
            {
                i++;
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = FindChild<CheckBox>(container, null);


                bool userPreferenceForThisTest = _pinfo.actualUserPreference.userPreferencesList.FirstOrDefault(x => x.Item3 == checkBox.Content.ToString()).Item2;
                
                checkBox.IsChecked = userPreferenceForThisTest;   // set the checkbox status with the user pref

                // checkbox label name : checkBox.Content.ToString()

                /*
                 * TESTER LES CHECK BOX
                if (checkBox != null)
                {
                    // Lire le statut de la case à cocher avant qu'elle n'apparaisse. 
                    bool isChecked = checkBox.IsChecked ?? false;
                    //if (isChecked) MessageBox.Show("is Checked");
                    //else MessageBox.Show("is not checked");
                    // Faites quelque chose avec le statut (isChecked)
                }*/
            }
        }

       
      

        private void tous_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in itemsControl.Items)
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = FindChild<CheckBox>(container, null);
                checkBox.IsChecked = true;
            }
        }

        private void aucun_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in itemsControl.Items)
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = FindChild<CheckBox>(container, null);
                checkBox.IsChecked = false;
            }
        }
        private void close_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in itemsControl.Items) // loop on checkboxes
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = FindChild<CheckBox>(container, null);



                bool isChecked = checkBox.IsChecked ?? false; // get checkboxstatus

                // update list preference
                _pinfo.actualUserPreference.Set(
                    _pinfo.actualUserPreference.userPreferencesList.FirstOrDefault(x => x.Item3 == checkBox.Content.ToString()).Item1,
                    isChecked,
                    _pinfo.actualUserPreference.userPreferencesList.FirstOrDefault(x => x.Item3 == checkBox.Content.ToString()).Item3);
            }
            // update user file  preference
            _pinfo.actualUserPreference.updateUserPrefFileFromlist();
            this.Close();
        }
       
    }
}
