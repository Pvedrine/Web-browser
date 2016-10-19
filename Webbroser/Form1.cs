using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Webbroser
{
    public partial class Form1 : Form
    {

        //new array of Uri to save them
        public static Uri[] Url = new Uri[20];
        int index = 0;
        int fav = 0;
        int histo = 0;
        //history array
        static ToolStripMenuItem[] history = new ToolStripMenuItem[50];
        //favourites array
        ToolStripMenuItem[] favorites = new ToolStripMenuItem[50];
        //file to save the favourites
        string FileFavorites = @"Favorites.txt";
        //file to save the history
        string FileHistory = @"History.txt";


        //methods that gives the HTML code from an URL
        public void GetPage(string s)
        {
            try
            {
                if (!s.Equals("") && (s.StartsWith("http://")||s.StartsWith("https://")))//the string can't be empty and must begin with "http://" or "https://"
                {

                    Url[index] = new Uri(s);
                    //We get the Http text from the URL
                    textBox2.Text = Encoding.UTF8.GetString(Encoding.Default.GetBytes((new WebClient().DownloadString(s))));
                    textBox1.Text = s;
                    //We add the URL onto the history
                    History(s);
                    index++;
                    if (index == 20)
                    {
                        index = 0;
                    }

                }
            }

            //Handling errors

            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound) //404
                    {
                        MessageBox.Show("Error, 404 Not Found", "Error 404", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    if (resp.StatusCode == HttpStatusCode.BadRequest)//400
                    {
                        MessageBox.Show("Error, 400 Bad Request", "Error 400", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                    if (resp.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        MessageBox.Show("Error, 403 Forbidden", "Error 403", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                }

            }


        }
      
      
////////////////////HISTORy//////////////////////////////////
        public void History(string s)
        {
            if (Url[index] != null)
            {
                history[histo] = new ToolStripMenuItem();
                history[histo].Text = s; //giving the name of the URL
                history[histo].Click += new EventHandler(History_click);
                if (histo == 0 || history[histo] != null)
                    historyToolStripMenuItem.DropDownItems.Add(history[histo]);//adding the name to the menu
            }


        }
        //Method that makes us able to click the items in the history
        private void History_click(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedHistory = (ToolStripMenuItem)sender;
            GetPage(clickedHistory.Text);
        }

////////////////FAVOURITES///////////////////////////////////////////////////////////////////////


        //checking if the favourite is different from the others
        public bool checkfav(string s)
        {
            bool test = false;
            for (int i = 0; i < favorites.Length; i++)
            {
                if (favorites[i] != null)
                {
                    //return true if we find the same URL in the array
                    test = favorites[i].Text == s ? true : false;
                }
                if (test)//if we find the same URL, we stop
                {
                    break;
                }
            }
            return test;
        }

        //method to set the URL to a new tool strip menu item
        public void SetToFavorites(string s)
        {
            if (s != null && !checkfav(s)) //!checkfav because it returns true if the URL is the same (needs to be false then)
            {
                favorites[fav] = new ToolStripMenuItem();
                favorites[fav].Text = s; //giving the name of the URL
                favorites[fav].Name = s;
                favorites[fav].Click += new EventHandler(Favorites_Click); //Adding an event to click on the item
                if (fav == 0 || favorites[fav] != null)
                {
                    favoritesToolStripMenuItem.DropDownItems.Add(favorites[fav]);
                }
            }
            fav++;

        }
        //Method that makes us able to click th items in the Favourites
        private void Favorites_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedFavorites = (ToolStripMenuItem)sender;
            GetPage(clickedFavorites.Text);
        }

        //method to get either the history or the favourites from their respectives files
        public void Get_Fav(string file)
        {
            string line;
            if (File.Exists(file))
            {
                StreamReader filetmp = new StreamReader(file);
                //if the file in parameter is file history
                if (file == FileHistory)
                {
                    while ((line = filetmp.ReadLine()) != null)
                        History(line);
                }

                else
                    //the file in parameter is file favourites
                    while ((line = filetmp.ReadLine()) != null)
                    {
                        SetToFavorites(line);

                    }
                filetmp.Close();

            }
            //if the favorites files doesn't exist, we create it
            else
            {
                File.Create(file);
            }
        }

        //simple methods to write the content of an array in a file
        public void WriteFile(string file, ToolStripMenuItem[] tab)
        {
            if (File.ReadAllText(file) != null)

                for (int i = 0; i < tab.Length; i++)
                {
                    if (tab[i] != null)
                    {
                        File.AppendAllText(file, tab[i].Text + "\n");
                    }
                }

        }

       

        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            throw new NotImplementedException();

        }


///////////////////BUTTONS/////////////////////////////////////////////////////////////////////////////////////
        //Forward method
        private void button2_Click(object sender, EventArgs e)
        {
            if (Url[index + 1] != null)
            {
                GetPage(Url[index + 1].ToString());

            }
        }

        //previous method
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (index > 0)
            {
                GetPage(Url[index - 1].ToString());
                index -= 2;

            }
        }


        //Home Button
        private void homeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.home != null)
                GetPage(Properties.Settings.Default.home);
        }

        //Go button
        private void button1_Click(object sender, EventArgs e)
        {
            GetPage(textBox1.Text);
        }

////////////////OPENING AND CLOSING////////////////////////////
        //Methods called once the application is launched
        private void Form1_Load(object sender, EventArgs e)
        {
            //We get the Home Page URL loaded
            GetPage(Properties.Settings.Default.home);
            //We get the favorites
            Get_Fav(FileFavorites);
            Get_Fav(FileHistory);
        }

        //Closing application method, is executed when we close the application
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //once the application closes, we write all the favorites/history in a text file
            WriteFile(FileFavorites, favorites);
            WriteFile(FileHistory, history);
        }


/////////////////////FILE MENU ITEMS///////////////////////////////////////////////////////
        //Set as favourites button
        private void setAsFavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetToFavorites(Url[index - 1].ToString());
        }
        //Set as home item click
        private void setAsHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.home = textBox1.Text;
            Properties.Settings.Default.Save();

        }
        
      

////////////////////////////CHANGING COLOURS//////////////////////////////////////////////////////////////////
        private void ColourMode(System.Drawing.Color fore, System.Drawing.Color back)
        {
            menuStrip1.BackColor = back;
            menuStrip1.ForeColor = fore;
            textBox1.BackColor = back;
            textBox1.ForeColor = fore;
            textBox2.BackColor = back;
            textBox2.ForeColor = fore;
            BackColor = back;
            ForeColor = fore;
            button1.BackColor = back;
            button1.ForeColor = fore;
            button2.ForeColor = fore;
            button2.BackColor = back;
            goButton.ForeColor = fore;
            goButton.BackColor = back;

        }
        //White
        private void pinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColourMode(System.Drawing.Color.Black, System.Drawing.Color.White);
        }
        //Black
        private void darkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColourMode(System.Drawing.Color.White, System.Drawing.Color.Black);

        }
        //Pink
        private void pinkToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ColourMode(System.Drawing.Color.Black, System.Drawing.Color.LightPink);

        }
        //Blue
        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColourMode(System.Drawing.Color.LightGreen, System.Drawing.Color.Blue);

        }
        //Red
        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColourMode(System.Drawing.Color.Blue, System.Drawing.Color.Red);

        }
//////////////New Window ////////////////////////////////
        //Creating a new thread that opens a new window (KO)
        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread newWindow = new Thread(delegate ()
            {
                Process wind = new Process();
                wind.StartInfo.FileName = Application.ExecutablePath;
                wind.StartInfo.CreateNoWindow = true;
                wind.Start();


            });
        }


/////////////////////////////// CLearing methods/////////////////////////
        //Clearing History
        private void historyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Thread clearhistory = new Thread(delegate ()
            {
                File.Delete(FileHistory);
                //reseting the array
                for (int i = 0; i < histo; i++)
                {
                    history[i] = null;
                    historyToolStripMenuItem.DropDownItems.Remove(history[i]);
                }
                histo = 0;
                Get_Fav(FileHistory);

            });
        }
        //CLearing Favourites
        private void favoritesToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            Thread clearfav = new Thread(delegate ()
            {
                File.Delete(FileFavorites);
                for (int i = 0; i < fav; i++)
                {
                    favorites[i] = null;
                    favoritesToolStripMenuItem.DropDownItems.Remove(favorites[i]);
                }

                fav = 0;
                Get_Fav(FileFavorites);
            });
        }
        //CLearing Home
        private void homeToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.home = "";
            Properties.Settings.Default.Save();
            textBox1.Text = "";
            textBox2.Text = null;
        }

        /////////////////SHORTCUT////////////////////////////:::

        //Getting the URL by pressing Enter
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

                GetPage(textBox1.Text);

            }
        }

///////////////////////////////MISC///////////////////////////








        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {


        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void contextMenuStrip3_Opening(object sender, CancelEventArgs e)
        {

        }


        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void homeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }



        private void favoritesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public Form1()
        {
            InitializeComponent();
        }





        private void tabPage1_Click(object sender, EventArgs e)
        {

        }



        private void changeColourToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

    }
}
