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
    /// Interaction logic for CheckScreen_Global.xaml
    /// </summary>
    public partial class CheckScreen_Global : UserControl
    {

        public string _title;
        private (string, SolidColorBrush) _globalresult_status;
        public string _result_string;
        public Brush _resultcolor = new SolidColorBrush(Colors.LightGreen);
        private List<Item_Result> _items;
        public List<UserControl> _listchecks_item;

        public CheckScreen_Global(string title, List<Item_Result> items)
        {
            //Definition des différents items des tests
            _listchecks_item = new List<UserControl>();
            _items = new List<Item_Result>();
            _items = items;
            //_globalresult_status = true;

            //If one test in sublist is not true => global result status is false and color is red
            Result_Status res = new Result_Status();
            _globalresult_status = res.True;


            int nTrue = 0;
            int nFalse = 0;
            int nWarning = 0;
            int nInfo = 0;
            int nUncheck = 0;
            _globalresult_status = res.True;
            foreach (Item_Result i in _items)
            {
                if (i.ResultStatus.Item1 == "X") nFalse++;
                if (i.ResultStatus.Item1 == "WARNING") nWarning++;
                if (i.ResultStatus.Item1 == "OK") nTrue++;
                if (i.ResultStatus.Item1 == "INFO") nInfo++;
                if (i.ResultStatus.Item1 == "UNCHECK") nUncheck++;// nInfo++;

            }
            if (nFalse > 0)  // si un item faux, global faux
                _globalresult_status = res.False;
            else if (nWarning > 0)
                _globalresult_status = res.Variation;
            else if (nTrue > 0)
                _globalresult_status = res.True;
            else if(nInfo > 0)
                _globalresult_status = res.INFO;
            else
                _globalresult_status = res.UNCHECK;
            //Fill user control list to display it

            foreach (Item_Result item in _items)
                {
                    _listchecks_item.Add(new CheckScreen_Item(item));
                }

            //Set class attributes
            _resultcolor = _globalresult_status.Item2;
            _result_string = _globalresult_status.Item1;
            _title = title;

            //init all
            InitializeComponent();
            CheckScreen_lsv.ItemsSource = new List<UserControl>();
            CheckScreen_lsv.ItemsSource = _listchecks_item;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CheckScreen_lsv.Visibility == Visibility.Collapsed)
            {
                CheckScreen_lsv.Visibility = Visibility.Visible;
            }
            else
            {
                if (CheckScreen_lsv.Visibility == Visibility.Visible)
                {
                    CheckScreen_lsv.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button_Click(sender, e);
        }

        #region GETS/SETS

        public List<UserControl> ListChecks_Item
        {
            get => _listchecks_item;
            set => _listchecks_item = value;
        }

        public List<Item_Result> Items
        {
            get => _items;
            set => _items = value;
        }

        public Brush ResultGlobalColor
        {
            get => _resultcolor;
            set => _resultcolor = value;
        }
        public string ResultGlobalString
        {
            get => _result_string;
            set => _result_string = value;
        }
        public string TitleGlobal
        {
            get => _title;
            set => _title = value;
        }

        #endregion

    }
}
