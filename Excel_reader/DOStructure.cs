using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanCheck
{
    public class DOstructure
    {
        public string Name { get; set; }               
        public List<string> listOfObjectives { get; set; }
        public DOstructure() ///constructor
        {
            listOfObjectives = new List<string>();
        }
    }
}
