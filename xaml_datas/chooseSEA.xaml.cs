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
    public partial class chooseSEA : Window
    {
        private PreliminaryInformation _pinfo;
        private ScriptContext _ctx;
        public chooseSEA(ScriptContext ctx, PreliminaryInformation pinfo)
        {
            _pinfo = pinfo;
            _ctx = ctx;
            InitializeComponent();

            foreach (PlanSetup p in ctx.Course.PlanSetups)
            {
                if (p.Id.ToUpper().Contains("SEA"))
                    cbSEAplans.Items.Add(p.Id);
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            _pinfo.SEAplanName = cbSEAplans.SelectedValue.ToString();
            this.Close();

        }
    }
}
