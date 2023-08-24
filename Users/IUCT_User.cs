using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PlanCheck
{
    public class IUCT_User
    {
        private string _userID;
        private string _userfamilyname;
        private string _userfirstname;
        private string _function;
        private string _gender;
        private SolidColorBrush _userbackgroundcolor;
        private SolidColorBrush _userforegroundcolor;
        private SolidColorBrush _doctorBackgroundColor;
        private SolidColorBrush _doctorForegroundColor;
        public string userId
        {
            get { return _userID; }
            set { _userID = value; }
        }
        public string Gender
        {
            get { return _gender; }
            set { _gender = value; }
        }

        public string UserFamilyName
        {
            get { return _userfamilyname; }
            set { _userfamilyname = value; }
        }

        public string UserFirstName
        {
            get { return _userfirstname; }
            set { _userfirstname = value; }
        }

        public SolidColorBrush UserBackgroundColor
        {
            get { return _userbackgroundcolor; }
            set { _userbackgroundcolor = value; }
        }

        public SolidColorBrush DoctorBackgroundColor
        {
            get { return _doctorBackgroundColor; }
            set { _doctorBackgroundColor = value; }
        }

        public SolidColorBrush DoctorForeGroundColor
        {
            get { return _doctorForegroundColor; }
            set { _doctorForegroundColor = value; }
        }


        public string Function
        {
            get { return _function; }
            set { _function = value; }
        }

        public SolidColorBrush UserForeGroundColor
        {
            get { return _userforegroundcolor; }
            set { _userforegroundcolor = value; }
        }

    }
}
