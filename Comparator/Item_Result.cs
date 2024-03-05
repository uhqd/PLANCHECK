using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PlanCheck
{
    public class Item_Result
    {
        private string _label;
        private string _expectedvalue;
        private string _measuredvalue;
        private string _comparator;
        private (string, SolidColorBrush) _resultstatus;
        private string _infobulle;
        //private string _detailedExplanation;


        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }
        public string ExpectedValue
        {
            get { return _expectedvalue; }
            set { _expectedvalue = value; }
        }
        public string MeasuredValue
        {
            get { return _measuredvalue; }
            set { _measuredvalue = value; }
        }
        public string Comparator
        {
            get { return _comparator; }
            set { _comparator = value; }
        }
        public (string, SolidColorBrush) ResultStatus
        {
            get { return _resultstatus; }
            set { _resultstatus = value; }
        }
        public string Infobulle
        {
            get { return _infobulle; }
            set { _infobulle = value; }
        }
        /*public string detailedExplanation
        {
            get { return _detailedExplanation; }
            set { _detailedExplanation = value; }
        }*/
        public void setToTRUE()
        {
            this.ResultStatus = ("OK", new SolidColorBrush(Colors.LightGreen));
        }
        public void setToUNCHECK()
        {
            this.ResultStatus = ("UNCHECK", new SolidColorBrush(Colors.LightBlue));
        }
        public void setToFALSE()
        {
            this.ResultStatus = ("X", new SolidColorBrush(Color.FromArgb(200, 255, 50, 50)));// .Red ?  fx -- LightSalmon
        }
        public void setToINFO()
        {
            this.ResultStatus = ("INFO", new SolidColorBrush(Colors.LightYellow));//LightGray
        }
        public void setToWARNING()
        {
            this.ResultStatus = ("WARNING", new SolidColorBrush(Colors.LightSalmon)); // .Light Salmon ? LightYellow
        }


    }
}
