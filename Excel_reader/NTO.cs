using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanCheck
{

    public class NTO
    {
        private string Mode;
        private double Priority;
        private double Falloff;
        private double StartPercentageDose;
        private double StopPercentageDose;
        private double DistanceTotTarget;


        public NTO(string _mode, double _priority, double _falloff, double _start, double _stop, double _distance)
        {

            Mode = _mode;
            Priority = _priority;
            Falloff = _falloff;
            StartPercentageDose = _start;
            StopPercentageDose = _stop;
            DistanceTotTarget = _distance;


        }

        public string mode
        {
            get { return Mode; }
        }
        public double priority
        {
            get { return Priority; }
        }
        public double theFalloff
        {
            get { return Falloff; }
        }

        public double startPercentageDose
        {
            get { return StartPercentageDose; }
        }

        public double stopPercentageDose
        {
            get { return StopPercentageDose; }
        }

        public double distanceTotTarget
        {
            get { return DistanceTotTarget; }
        }

    }
}
