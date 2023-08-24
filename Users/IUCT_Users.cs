using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace PlanCheck.Users
{
    public class IUCT_Users
    {
        private List<IUCT_User> _users_list;
        public IUCT_Users()
        {
            _users_list = new List<IUCT_User>();

            #region open and read xlsx file with users 


            string userListFilePath = Directory.GetCurrentDirectory() + @"\plancheck_data\users\Users-IUCT.xlsx";
            // This command find the dir where the dll is executed. Better than getcurrentidir
            //string userListFilePath = Assembly.GetExecutingAssembly().GetDirectoryPath() + @"\users\Users-IUCT.xlsx";
           
            // If file doesn't exist open a file browser
            if (!File.Exists(userListFilePath))  
            {
                var fileDialog = new Microsoft.Win32.OpenFileDialog();
                fileDialog.DefaultExt = "xlsx";
                fileDialog.InitialDirectory = Directory.GetCurrentDirectory();

                if (!Directory.Exists(fileDialog.InitialDirectory))
                {
                    MessageBox.Show(fileDialog.InitialDirectory + "n'existe pas.");
                    fileDialog.InitialDirectory = @"C:\";
                }

                fileDialog.Multiselect = false;
                fileDialog.Title = "Liste des utilisateurs";
                fileDialog.ShowReadOnly = true;
                fileDialog.Filter = "XLSX files (*.xlsx)|*.xlsx";
                fileDialog.FilterIndex = 0;
                fileDialog.CheckFileExists = true;
                if (fileDialog.ShowDialog() == false)
                {
                    return;    // user canceled
                }
                userListFilePath = fileDialog.FileName; // full absolute path                                                  
                if (!System.IO.File.Exists(userListFilePath))
                {
                    MessageBox.Show("Listes utilisateurs introuvable");
                    return;
                }
            }

            Excel.Application xlApp = new Excel.Application();  // open excel                
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(userListFilePath, ReadOnly: true);// open file               
            Excel._Worksheet xlWorksheet1 = xlWorkbook.Sheets[1];   // open the sheet 1                                                       
            Excel.Range xlRange1 = xlWorksheet1.UsedRange; // get the used cells in workshit                
            int nRows = xlRange1.Rows.Count;// count lines

            for (int nUser = 2; nUser <= nRows; nUser++)
            {
                
                string id = xlRange1.Cells[nUser, 1].text;
                string firstname = xlRange1.Cells[nUser, 2].text;
                string lastname = xlRange1.Cells[nUser, 3].text;
                string sex = xlRange1.Cells[nUser, 4].text;
                string function = xlRange1.Cells[nUser, 5].text;

                string temp = xlRange1.Cells[nUser, 6].text;
                string temp2 = xlRange1.Cells[nUser, 7].text;
                SolidColorBrush mybgcolor = (SolidColorBrush)new BrushConverter().ConvertFromString(temp);
                SolidColorBrush myfgcolor = (SolidColorBrush)new BrushConverter().ConvertFromString(temp2);


                // see palette at https://learn.microsoft.com/fr-fr/dotnet/api/system.windows.media.brushes?view=windowsdesktop-6.0

                if ((id != "") && (id != null))
                {
                    IUCT_User myUser = new IUCT_User() { userId=id, UserFirstName = firstname, UserFamilyName = lastname, Gender = sex, Function = function, UserBackgroundColor = mybgcolor, UserForeGroundColor = myfgcolor };
                    _users_list.Add(myUser);
                }
            }

            xlWorkbook.Close(0);
            xlApp.Quit();

            #endregion


            #region old user definition (deprecated)
            /*
            
                        #region color the planners   
                       

                        IUCT_User carillo = new IUCT_User() { UserFirstName = "Fabienne", UserFamilyName = "Carillo", Gender = "F", Function = "Dosimetriste", UserBackgroundColor = System.Windows.Media.Brushes.Gold, UserForeGroundColor = System.Windows.Media.Brushes.DeepPink };
                        _users_list.Add(carillo);
                        IUCT_User recordon = new IUCT_User() { UserFirstName = "Frédérique", UserFamilyName = "Recordon", Gender = "F", Function = "Dosimetriste", UserBackgroundColor = System.Windows.Media.Brushes.Salmon, UserForeGroundColor = System.Windows.Media.Brushes.Navy };
                        _users_list.Add(recordon);
                        IUCT_User lacaze = new IUCT_User() { UserFirstName = "Thierry", UserFamilyName = "Lacaze", Gender = "H", Function = "Dosimetriste", UserBackgroundColor = System.Windows.Media.Brushes.Yellow, UserForeGroundColor = System.Windows.Media.Brushes.Navy };
                        _users_list.Add(lacaze);
                        IUCT_User defour = new IUCT_User() { UserFirstName = "Nathalie", UserFamilyName = "Defour", Gender = "F", Function = "Dosimetriste", UserBackgroundColor = System.Windows.Media.Brushes.AliceBlue, UserForeGroundColor = System.Windows.Media.Brushes.Black };
                        _users_list.Add(defour);
                        IUCT_User lanaspeze = new IUCT_User() { UserFirstName = "Christel", UserFamilyName = "Lanaspeze", Gender = "F", Function = "Dosimetriste", UserBackgroundColor = System.Windows.Media.Brushes.Firebrick, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(lanaspeze);
                        IUCT_User cavet = new IUCT_User() { UserFirstName = "Clémence", UserFamilyName = "Cavet", Gender = "F", Function = "Dosimetriste", UserBackgroundColor = System.Windows.Media.Brushes.Red, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(cavet);



                        #endregion

                        #region physicists
                        #region color scheme 1 whith background

                        IUCT_User arnaud = new IUCT_User() { UserFirstName = "FXavier", UserFamilyName = "Arnaud", Gender = "H",  Function = "Physicien", UserBackgroundColor = System.Windows.Media.Brushes.ForestGreen, UserForeGroundColor = System.Windows.Media.Brushes.Navy };
                        _users_list.Add(arnaud);

                        IUCT_User simon = new IUCT_User() { UserFirstName = "Luc", UserFamilyName = "Simon", Gender = "H", Function = "Physicien", UserBackgroundColor = System.Windows.Media.Brushes.DeepSkyBlue, UserForeGroundColor = System.Windows.Media.Brushes.Navy };
                        _users_list.Add(simon);

                        IUCT_User graulieres = new IUCT_User() { UserFirstName = "Eliane", UserFamilyName = "Graulieres", Gender = "F", Function = "Physicien", UserBackgroundColor = System.Windows.Media.Brushes.DeepPink, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(graulieres);

                        IUCT_User hangard = new IUCT_User() { UserFirstName = "Gregory", UserFamilyName = "Hangard", Gender = "H", Function = "Physicien", UserBackgroundColor = System.Windows.Media.Brushes.Gold, UserForeGroundColor = System.Windows.Media.Brushes.Black };
                        _users_list.Add(hangard);

                        IUCT_User parent = new IUCT_User() { UserFirstName = "Laure", UserFamilyName = "Parent", Gender = "F", Function = "Physicien", UserBackgroundColor = System.Windows.Media.Brushes.Yellow, UserForeGroundColor = System.Windows.Media.Brushes.Black };
                        _users_list.Add(parent);

                        IUCT_User brun = new IUCT_User() { UserFirstName = "Thomas", UserFamilyName = "Brun", Gender = "H", Function = "Physicien", UserBackgroundColor = System.Windows.Media.Brushes.RosyBrown, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(brun);

                        IUCT_User vieillevigne = new IUCT_User() { UserFirstName = "Laure", UserFamilyName = "Vieillevigne", Gender = "F", Function = "Physicien", UserBackgroundColor = System.Windows.Media.Brushes.LightGreen, UserForeGroundColor = System.Windows.Media.Brushes.Black };
                        _users_list.Add(vieillevigne);

                        IUCT_User stadler = new IUCT_User() { UserFirstName = "Marine", UserFamilyName = "Stadler", Gender = "F", Function = "Physicien", UserBackgroundColor = System.Windows.Media.Brushes.Aquamarine, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(stadler);

                        IUCT_User tournier = new IUCT_User() { UserFirstName = "Aurélie", UserFamilyName = "Tournier", Gender = "F", Function = "Physicien", UserBackgroundColor = System.Windows.Media.Brushes.DarkTurquoise, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(tournier);

                        #endregion

                        #endregion

                        #region oncologists 

                        #region color scheme 1
                        IUCT_User undefined = new IUCT_User() { UserFirstName = "indefini", UserFamilyName = "indefini", Gender = "F", Function = "indefini", UserBackgroundColor = System.Windows.Media.Brushes.White, UserForeGroundColor = System.Windows.Media.Brushes.Black };
                        _users_list.Add(undefined);

                        IUCT_User attal = new IUCT_User() { UserFirstName = "Justine", UserFamilyName = "Attal", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.LightPink, UserForeGroundColor = System.Windows.Media.Brushes.Black };
                        _users_list.Add(attal);

                        IUCT_User chira = new IUCT_User() { UserFirstName = "Ciprian", UserFamilyName = "Chira", Gender = "H", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.Orange, UserForeGroundColor = System.Windows.Media.Brushes.Black };
                        _users_list.Add(chira);

                        IUCT_User couarde = new IUCT_User() { UserFirstName = "Laetitia", UserFamilyName = "Couarde", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.DeepPink, UserForeGroundColor = System.Windows.Media.Brushes.Blue };
                        _users_list.Add(couarde);

                        IUCT_User dalmasso = new IUCT_User() { UserFirstName = "Céline", UserFamilyName = "Dalmasso", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.IndianRed, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(dalmasso);

                        IUCT_User desrousseaux = new IUCT_User() { UserFirstName = "Desrousseaux", UserFamilyName = "Jacques", Gender = "H", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.Orange, UserForeGroundColor = System.Windows.Media.Brushes.Black };
                        _users_list.Add(desrousseaux);

                        IUCT_User ducassou = new IUCT_User() { UserFirstName = "Anne", UserFamilyName = "Ducassou", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.HotPink, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(ducassou);

                        IUCT_User glemarec = new IUCT_User() { UserFirstName = "Gauthier", UserFamilyName = "Glemarec", Gender = "H", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.LightCoral, UserForeGroundColor = System.Windows.Media.Brushes.Navy };
                        _users_list.Add(glemarec);

                        IUCT_User izar = new IUCT_User() { UserFirstName = "Françoise", UserFamilyName = "Izar", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.Yellow, UserForeGroundColor = System.Windows.Media.Brushes.Black };
                        _users_list.Add(izar);

                        IUCT_User keller = new IUCT_User() { UserFirstName = "Audrey", UserFamilyName = "Keller", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.BlueViolet, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(keller);

                        IUCT_User khalifa = new IUCT_User() { UserFirstName = "Jonathan", UserFamilyName = "Khalifa", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.Gold, UserForeGroundColor = System.Windows.Media.Brushes.Black };
                        _users_list.Add(khalifa);

                        IUCT_User laprie = new IUCT_User() { UserFirstName = "Anne", UserFamilyName = "Laprie", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.MediumPurple, UserForeGroundColor = System.Windows.Media.Brushes.Navy };
                        _users_list.Add(laprie);

                        IUCT_User massabeau = new IUCT_User() { UserFirstName = "Carole", UserFamilyName = "Massabeau", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.BlueViolet, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(massabeau);

                        IUCT_User modesto = new IUCT_User() { UserFirstName = "Anouchka", UserFamilyName = "Modesto", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.GreenYellow, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(modesto);

                        IUCT_User moyal = new IUCT_User() { UserFirstName = "Elizabeth", UserFamilyName = "Moyal", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.Red, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(moyal);

                        IUCT_User piram = new IUCT_User() { UserFirstName = "Lucie", UserFamilyName = "Piram", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.LightBlue, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(piram);

                        IUCT_User pouedras = new IUCT_User() { UserFirstName = "Juliette", UserFamilyName = "Pouedras", Gender = "F", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.DarkTurquoise, UserForeGroundColor = System.Windows.Media.Brushes.Black };
                        _users_list.Add(pouedras);

                        IUCT_User preault = new IUCT_User() { UserFirstName = "Mickael", UserFamilyName = "Preault", Gender = "H", Function = "Radiothérapeute", UserBackgroundColor = System.Windows.Media.Brushes.Azure, UserForeGroundColor = System.Windows.Media.Brushes.AntiqueWhite };
                        _users_list.Add(preault);



                        #endregion



                        #endregion
            
            */
            #endregion
        }


        public List<IUCT_User> UsersList
        {
            get { return _users_list; }
            set { _users_list = value; }
        }

    }
}
