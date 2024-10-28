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

        private List<(string, bool, string)> userPrefsList;
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
                    if (colonnes.Length == 3)
                    {
                        if (colonnes[1] == "yes")
                            yesOrNo = true;
                        else
                            yesOrNo = false;
                        // Ajouter le tuple à la liste


                        userPrefsList.Add((colonnes[0], yesOrNo, colonnes[2]));


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

        private bool isTheSameCheck(string a, string b)
        {
            bool same = false;

            int indexPointVirguleA = a.IndexOf(';');
            string checkIdA = a.Substring(0, indexPointVirguleA);

            int indexPointVirguleB = b.IndexOf(';');
            string checkIdB = b.Substring(0, indexPointVirguleB);

            if (checkIdA == checkIdB)
            {
                //MessageBox.Show("The same : " + checkIdB + " " + checkIdA);
                same = true;
            }
            return same;
        }
        public void updateUserPrefsFileFromNewsFile(string newsFile, string Userfile)
        {
            // if News file's date of  last modification is more recent than user file, update

            try
            {

                FileInfo InfoFileNew = new FileInfo(newsFile);
                FileInfo InfoFileUser = new FileInfo(Userfile);
                DateTime lastModifiedNew = InfoFileNew.LastWriteTime;
                DateTime lastModifieduser = InfoFileUser.LastWriteTime;


                if (lastModifiedNew > lastModifieduser)
                {
                    string[] NewsLines = File.ReadAllLines(newsFile);
                    string[] UserLines = File.ReadAllLines(Userfile);
                    bool fileChanged = false;


                    string fileName = "myTemp.csv";
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);



                    //
                    // the following part allows to add new check and remove deprecated check that  are not in the news file anymore
                    //
                    if (NewsLines.Length < UserLines.Length)
                    {
                        using (StreamWriter writer = new StreamWriter(filePath))
                        {
                            int i = 0;
                            for (i = 0; i < NewsLines.Length; i++)
                            {
                                writer.WriteLine(NewsLines[i]);
                            }
                            fileChanged = true;
                        }
                    }
                    else
                    {
                        using (StreamWriter writer = new StreamWriter(filePath))
                        {
                            int i = 0;
                            int j = 0;
                            for (i = 0; i < NewsLines.Length; i++)
                            {

                                bool thisCheckExistinUserfile = false;
                                for (j = 0; j < UserLines.Length; j++)
                                {
                                    if (isTheSameCheck(NewsLines[i], UserLines[j]))
                                    {
                                        thisCheckExistinUserfile = true;
                                        break;
                                    }
                                }
                                if (thisCheckExistinUserfile)
                                {
                                    writer.WriteLine(UserLines[j]);

                                }
                                else
                                {
                                    writer.WriteLine(NewsLines[i]);
                                    fileChanged = true;
                                }



                            }
                        }
                    }
                    if (fileChanged)
                    {
                        File.Copy(filePath, Userfile, true);
                    }
                    File.Delete(filePath);
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
                    foreach ((string text, bool valeurBool, string textExplication) in userPrefsList)
                    {
                        if (valeurBool == true)
                        {
                            writer.WriteLine(text + ";yes;" + textExplication);
                        }
                        else
                            writer.WriteLine(text + ";no;" + textExplication);
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
            userPrefsList = new List<(string, bool, string)>();

            string userid; //i.e admin\simon_lu --> cut admin\
            if (fulluserID.Contains("\\"))
            {
                string[] sub = fulluserID.Split('\\');
                userid = sub[1];
            }
            else
                userid = fulluserID;


            userListFilePath = userid + "_prefs.csv";

            userListFilePath = Directory.GetCurrentDirectory() + @"\plancheck_data\users\UserPrefs\" + userListFilePath;

            // This file contains yes for all test. All the new tests are here. 
            newsFilePath = Directory.GetCurrentDirectory() + @"\plancheck_data\users\UserPrefs\newsPrefs.csv";

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
        public List<(string, bool, string)> userPreferencesList
        {
            get { return userPrefsList; }
            set { userPrefsList = value; }
        }
        public void Set(string targetString, bool newValue, string text2)
        {
            // Rechercher l'index de l'élément ayant la chaîne spécifique
            int index = userPrefsList.FindIndex(item => item.Item1 == targetString);

            // Si l'élément est trouvé, mettre à jour la partie booléenne
            if (index != -1)
            {
                var updatedItem = userPrefsList[index];
                userPrefsList[index] = (updatedItem.Item1, newValue, text2);
            }
            // Sinon, ajouter un nouvel élément avec la chaîne spécifique et la nouvelle valeur booléenne
            else
            {
                userPrefsList.Add((targetString, newValue, text2));
            }
        }


    }
}
