using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanCheck
{
    public class OARvolume
    {
        string _volumeName;
        double _volMin;
        double _volMax;

        public OARvolume() // constructor
        {

        }
        public String volumeName
        {
            get { return _volumeName; }
            set { _volumeName = value; }
        }

        public double volumeMin
        {
            get { return _volMin; }
            set { _volMin = value; }
        }

        public double volumeMax
        {
            get { return _volMax; }
            set { _volMax = value; }
        }
    }



}
