using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace PlanCheck
{
    internal class timer
    {
        public static System.Diagnostics.Stopwatch watch;
        public static long t1, t2;
        public static StreamWriter outputFile; //= new StreamWriter(Path.Combine(docPath, "WriteLines.txt"))
                                               //        public static FileStyleUriParser 
        public timer()  //Constructor
        {
           

            string myfilename = @"\\srv015\sf_com\simon_lu\temp\";
            myfilename += "PlanCheck_Timer_" ;
            myfilename +=DateTime.Now.ToString("MM.dd.yyyy_H.mm.ss") + ".txt";



            outputFile = new StreamWriter(myfilename);
            watch = System.Diagnostics.Stopwatch.StartNew();

            t1 = 0;
            t2 = 0;

        }

        public void durationSinceLastCall(string comment)
        {



            watch.Stop();
            t2 = (watch.ElapsedMilliseconds / 1000) - t1;
            t1 += t2;
            watch.Start();
            //MessageBox.Show(comment + " : " + t1.ToString()+ " seconds");
            outputFile.WriteLine(comment + " : " + t2.ToString() + " seconds");
            //            return duration;
        }
        public void close()
        {
            outputFile.Close();

        }


    }
}
