using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace PlanCheck.Users
{
    public class User_preference
    {

        private List<(string, bool)> userPrefsList;


        public bool userWantsTheTest(string testName)
        {

         
            return userPrefsList.Find(item => item.Item1 == testName).Item2;
        }
        public int setUserPref(string filename)
        {
            int returnValue = 0;
            int i=0;
            using (StreamReader sr = new StreamReader(filename))
            {
                // Liste pour stocker les tuples (colonne1, colonne2)
                // Lire chaque ligne du fichier
                bool errorInFile=false;
                while (!sr.EndOfStream)
                {
                    i++;
                    // Lire la ligne et la diviser en colonnes (supposons que les colonnes sont séparées par un point-virgule)
                    string ligne = sr.ReadLine();
                    string[] colonnes = ligne.Split(';');
                    bool yesOrNo=true;
                    
                    // Vérifier si la ligne contient suffisamment de colonnes
                    if (colonnes.Length == 2)
                    {
                        if (colonnes[1] == "yes") 
                            yesOrNo = true;
                        else
                            yesOrNo = false;
                        // Ajouter le tuple à la liste


                        userPrefsList.Add((colonnes[0], yesOrNo));


                    }
                    else
                    {
                        errorInFile = true;
                        MessageBox.Show("Erreur sur la ligne " + i +  colonnes[0]);
                    }
                }
                if (errorInFile)
                {
                    MessageBox.Show("Erreur dans le fichier de préférences \n" + filename);
                    returnValue = -1;
                }
                //foreach (var value in userPrefsList)
                  //  MessageBox.Show(value.Item1 + " " + value.Item2);
            }

            return returnValue;
        }
        public void updateUserPrefsFile(string newsFile, string Userfile)
        {
            // must check the user file, must check the new tests
        }
        public User_preference(string fulluserID) // i.e admin\simon_lu
        {
            userPrefsList = new List<(string, bool)>();
            bool userPrefFileExist = true;
            bool newsFileExist = true;
            string userid;
            if (fulluserID.Contains("\\"))
            {
                string[] sub = fulluserID.Split('\\');
                userid = sub[1];
            }
            else
                userid = fulluserID;


            string userListFilePath = userid + "_prefs.csv";

            userListFilePath = Directory.GetCurrentDirectory() + @"\plancheck_data\users\" + userListFilePath;
            string newsFilePath = Directory.GetCurrentDirectory() + @"\plancheck_data\users\newsPrefs.csv";


            if (!File.Exists(userListFilePath))
            {
                userPrefFileExist = false;
                userListFilePath = Directory.GetCurrentDirectory() + @"\plancheck_data\users\default_prefs.csv";
                if (!File.Exists(userListFilePath))
                {
                    MessageBox.Show("Aucun fichier de préférences trouvé. Tous les check seront effectués");
                    userPrefFileExist = false;


                }
                else
                    userPrefFileExist = true;
            }
            else
                userPrefFileExist = true;

            if (!File.Exists(newsFilePath))
                newsFileExist = false;

            if (userPrefFileExist && newsFileExist)
            {
                updateUserPrefsFile(newsFilePath, userListFilePath);
                setUserPref(userListFilePath);



            }





        }

        public List<(string, bool)> userPreferencesList
        {
            get { return userPrefsList; }
            set { userPrefsList = value; }
        }
        public  void Set(string targetString, bool newValue)
        {
            // Rechercher l'index de l'élément ayant la chaîne spécifique
            int index = userPrefsList.FindIndex(item => item.Item1 == targetString);

            // Si l'élément est trouvé, mettre à jour la partie booléenne
            if (index != -1)
            {
                var updatedItem = userPrefsList[index];
                userPrefsList[index] = (updatedItem.Item1, newValue);
            }
            // Sinon, ajouter un nouvel élément avec la chaîne spécifique et la nouvelle valeur booléenne
            else
            {
                userPrefsList.Add((targetString, newValue));
            }
        }



    }
}
