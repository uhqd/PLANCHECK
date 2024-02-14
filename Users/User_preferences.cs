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
        private static string userListFilePath = String.Empty;
        private static string newsFilePath = String.Empty;
        public bool userWantsTheTest(string testName)
        {


            return userPrefsList.Find(item => item.Item1 == testName).Item2;
        }
        public int getUserPrefFromFile(string filename)
        {
            int returnValue = 0;
            int i = 0;
            using (StreamReader sr = new StreamReader(filename))
            {
                // Liste pour stocker les tuples (colonne1, colonne2)
                // Lire chaque ligne du fichier
                bool errorInFile = false;
                while (!sr.EndOfStream)
                {
                    i++;
                    // Lire la ligne et la diviser en colonnes (supposons que les colonnes sont séparées par un point-virgule)
                    string ligne = sr.ReadLine();
                    string[] colonnes = ligne.Split(';');
                    bool yesOrNo = true;

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
                        MessageBox.Show("Erreur sur la ligne " + i + colonnes[0]);
                    }
                }
                if (errorInFile)
                {
                    MessageBox.Show("Erreur dans le fichier de préférences \n" + filename);
                    returnValue = -1;
                }

            }

            return returnValue;
        }
        public void updateUserPrefsFileFromNewsFile(string newsFile, string Userfile)
        {
            // if number of lines are different, copy the files news to user

            try
            {
                string[] lines = File.ReadAllLines(newsFile);
                int numberOfLinesNews = lines.Length;
                string[] lines2 = File.ReadAllLines(Userfile);
                int numberOfLinesUserfile = lines2.Length;
                if (numberOfLinesNews != numberOfLinesUserfile)
                {
                    File.Copy(newsFile, Userfile, true);
                    MessageBox.Show("Votre fichier de préférence a été mis à jour pour la mise en place nouveaux tests. Vérifier vos préférences.");
                }
            }
            catch 
            {
                MessageBox.Show("Impossbile de comparer les fichiers " + newsFile + " et " + Userfile);
            }


        }

        public void updateUserPrefFileFromlist()
        {
            File.Delete(userListFilePath);


            try
            {
                // Créer un fichier et écrire du texte dedans
                using (StreamWriter writer = new StreamWriter(userListFilePath))
                {
                    foreach ((string text, bool valeurBool) in userPrefsList)
                    {
                        if(valeurBool == true)
                        {
                            writer.WriteLine(text+";yes");
                        }
                        else
                            writer.WriteLine(text + ";no");
                        //   writer.WriteLine("Hello, world!");
                        //   writer.WriteLine("Ceci est une nouvelle ligne de texte.");
                    }
                }

                MessageBox.Show("Votre fichier de préférences a été sauvegardé");
            }
            catch 
            {
                MessageBox.Show("Impossible de créer votre fichier de préférences");
            }
        


        

        }

        public User_preference(string fulluserID) // constructor 
        {
            userPrefsList = new List<(string, bool)>();

            string userid; //i.e admin\simon_lu --> cut admin\
            if (fulluserID.Contains("\\"))
            {
                string[] sub = fulluserID.Split('\\');
                userid = sub[1];
            }
            else
                userid = fulluserID;


            userListFilePath = userid + "_prefs.csv";

            userListFilePath = Directory.GetCurrentDirectory() + @"\plancheck_data\users\" + userListFilePath;

            // This file contains yes for all test. All the new tests are here. 
            newsFilePath = Directory.GetCurrentDirectory() + @"\plancheck_data\users\newsPrefs.csv";

            if (!File.Exists(newsFilePath))
            {
                MessageBox.Show("Impossible de trouver le fichier de préférences utilisateur : " + newsFilePath + "\n Contactez un physicien");
            }
            if (!File.Exists(userListFilePath))
            {
                try
                {
                    // Copier le fichier
                    File.Copy(newsFilePath, userListFilePath, true);
                    MessageBox.Show("Votre fichier de préférences a été créé. Vérifiez vos préférences. Celles-ci seront savegardées");
                }
                catch 
                {
                    MessageBox.Show("Impossible de copier " + newsFilePath + " vers " + userListFilePath);
                }
            }
            else
            {
                updateUserPrefsFileFromNewsFile(newsFilePath, userListFilePath);
            }

            if (File.Exists(userListFilePath))
                getUserPrefFromFile(userListFilePath);

        }
        //   GET, SET
        public List<(string, bool)> userPreferencesList
        {
            get { return userPrefsList; }
            set { userPrefsList = value; }
        }
        public void Set(string targetString, bool newValue)
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
