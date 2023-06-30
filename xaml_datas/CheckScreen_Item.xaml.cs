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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlanCheck
{
    /// <summary>
    /// Interaction logic for CheckScreen_Item.xaml
    /// </summary>
    public partial class CheckScreen_Item : UserControl
    {

        private string _item_title;
        private string _item_result;
        private Brush _resultcolor = new SolidColorBrush(Colors.LightGreen);
        private string _comment;
        private string _tooltip;
        public CheckScreen_Item(Item_Result resultat)
        {
            DataContext = this;
            _item_title = " " + resultat.Label;

            // FX
            // _comment = resultat.ExpectedValue + " " + resultat.Comparator + " " + resultat.MeasuredValue + "?";
            //LS
            _comment = resultat.MeasuredValue;

            _item_result = resultat.ResultStatus.Item1;
            _tooltip = resultat.Infobulle;
            _resultcolor = resultat.ResultStatus.Item2;

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //if (CommentaireBlock.Visibility == Visibility.Collapsed)
            //{
            //    CommentaireBlock.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    CommentaireBlock.Visibility = Visibility.Collapsed;
            //}
        }

        private void StackPanel_Item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button_Click(sender, e);
        }

        #region GETS /SETS
        public string Item_Result
        {
            get => _item_result;
            set => _item_result = value;
        }

        public string Item_Title
        {
            get => _item_title;
            set => _item_title = value;
        }

        public string Comment 
        {
            get => _comment;
            set => _comment = value;
        }

        public string Tooltip
        {
            get => _tooltip;
            set => _tooltip = value;
        }

        public Brush ResultItemColor
        {
            get => _resultcolor;
            set => _resultcolor = value;
        }
        #endregion

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
