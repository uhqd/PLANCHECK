using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PlanCheck
{
    public  class Result_Status
    {
        //System.Windows.Media.Color mycolor = new System.Windows.Media.Color(#ffffffff);
        //"#FFFFFFFF"
        //Color slatddeBlue = Color.FromArgb() // FromValues("#ffffffff"); //FromName("SlateBlue");
        // SolidColorBrush mycolor = new SolidColorBrush
        (string, SolidColorBrush) _true = ("OK", new SolidColorBrush(Colors.LightGreen));
        (string, SolidColorBrush) _false = ("X", new SolidColorBrush(Color.FromArgb(200, 255, 50, 50))); // used to be red
        (string, SolidColorBrush) _variation = ("WARNING", new SolidColorBrush(Colors.LightSalmon));
        (string, SolidColorBrush) _INFO = ("INFO", new SolidColorBrush(Colors.LightYellow));
        (string, SolidColorBrush) _uncheck = ("UNCHECK", new SolidColorBrush(Colors.LightBlue));




        public (string, SolidColorBrush) True
        {
            get { return _true; }
        }
        public (string, SolidColorBrush) False
        {
            get { return _false; }
        }
        public (string, SolidColorBrush) Variation
        {
            get { return _variation; }
        }
        public (string, SolidColorBrush) INFO
        {
            get { return _INFO; }

        }
        public (string, SolidColorBrush) UNCHECK
        {
            get { return _uncheck; }

        }
    }
}
