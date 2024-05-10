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
    
    /*
     * POUR AJOUTER UN TEST
     * CREER LE TEST DANS LE CODE AVEC UN TEST IF USER WANT THE TEST (NOM DU TEST)
     * AJOUTER CE NOM DE TEST DANS LE FICHIER NEWS.PREFS
     * VOILA
    */
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

       /* private void medecin_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            List<int> medecinList = new List<int>() {4,5,6,7,21,24,27,28,29,30,31,58,59,60 };


            foreach (var item in itemsControl.Items)
            {
                i++;
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = FindChild<CheckBox>(container, null);
                if (medecinList.Contains(i))
                    checkBox.IsChecked = true;
                else
                    checkBox.IsChecked = false;
            }
        }

        private void dosimetriste_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            List<int> dosiList = new List<int>() { 2,4,5,6,7,8,9,10,11,12,13,14,15,16,19,21,22,23,26,31,32,34,36,37,38,39,40,41,42,43,44,45,49,52,53,54,55,56,57,58,59,60,61,62 };


            foreach (var item in itemsControl.Items)
            {
                i++;
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = FindChild<CheckBox>(container, null);
                if (dosiList.Contains(i))
                    checkBox.IsChecked = true;
                else
                    checkBox.IsChecked = false;
            }
        }*/
    }
}

/*
 * 
 * 
LISTE des tests  


1	currentCourseStatus;yes;Statut du course ouvert
2	planApprove;yes;Statut du plan ouvert
3	coursesStatus;yes;Statut des autres courses
4	tomoReportApproved;yes;Approbation du rapport Tomo
5	anteriorTraitement;yes;Recherche de traitements anterieurs
6	prescriptionVolumes;yes;Liste des cibles dans la prescription
7	fractionation;yes;Verification du fractionnement
8	percentage;yes;Pourcentage de la prescription
9	normalisation;yes;Mode de normalisation
10	prescriptionName;yes;Nom de la prescription
11	CT_age;yes;Age du CT
12	origin;yes;Origine modifiee
13	sliceThickness;yes;Epaisseur de coupes
14	HUcurve;yes;Courbe HU
15	deviceName;yes;Nom du scanner
16	image3Dnaming;yes;Nom de image3D
17	averageCT;no;Scanner Average composition
18	averageForSBRT;no;Scanner Average pour SBRT
19	tomoReportCT_date;yes;Date du rapport Tomo
20	otherSeries;yes;Series supplementaires necessaires
21	approbationStatus;yes;Approbation du groupe de structures
22	couchStructExist;yes;Existence de la table
23	correctCouch;yes;Table correcte
24	clinicalStructuresItem;yes;Liste des structures cliniques
25	optStructuresItem;yes;Liste des structures Optimisation
26	fixedHUVolumeList;yes;Liste des structures HU forcees
27	anormalVolumeList;yes;Vraissemblance des volumes des stuctures
28	shapeAnalyser;yes;Nombre de parties des structures
29	missingSlicesItem;yes;Coupes manquantes dans les structures
30	laterality;yes;Lateralite correcte des structures G et D
31	aPTVforEveryone;yes;Existence de PTV pour chaque CTV
32	allFieldsSameIso;yes;Unicite de l'isocentre
33	isoAtCenterOfPTV;yes;Isocentre au centre de la cible
34	distanceToOrigin;yes;Distance iso origine
35	isoTomo;yes;Position iso Tomo
36	gating;yes;Gating active
37	RAdirection;yes;Direction des arcs
38	colli;yes;Collimateur VMAT non nul
39	FE_MLC;yes;Modification des champs de FE
40	algo_name;yes;Nom algorithme
41	algo_grid;yes;Grille de calcul
42	algoOptions;yes;Options de l'algorithme
43	NTO;yes;parametres NTO
44	jawTrack;yes;Jaw tracking
45	POoptions;yes;Options PO
46	energy;no;Energie
47	doseRate;no;Debit de dose pour CQ
48	lowSteps;yes;Control Points trop lents
49	toleranceTable;yes;Table de tolerances des champs
50	fieldTooSmall;no;Champ trop petit
51	maxPositionMLCHalcyon;no;Position max des lames Halcyon
52	novaSBRT;yes;Nova SBRT
53	tomoParamsFieldWidth;yes;Parametres du plan TOMO
54	umPerGray;yes;UM par Gray
55	UMforFE;yes;UM du plan FE
56	wedged;no;Filtres
57	less10UM;no;Champs de 10 UM
58	turquoiseIsodose;no;Isodose Turquoise
59	prescribedObjectives;no;Objectifs de dose (PTV)
60	doseToOAR;yes;Objectifs de dose (OAR)
61	ariaDocuments;yes;Documents ARIA
62	preparedQA;yes;Preparation QA


 
 */
