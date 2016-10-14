using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Webbroser
{
    public partial class Form1 : Form
    {

        /*TO DO
         * Supporter erreurs 400, 403 et 404
         * Modifier les fav (supprimer)
         * charger l'historique au démarrage
         * multi thread pour avoir plusieurs pages
         * COMMENTER
         */
        Uri[] Url = new Uri[20];
        int index = 0;
        int fav = 0;
        ToolStripMenuItem[] history = new ToolStripMenuItem[50];
        // List<ToolStripMenuItem> history = new List<ToolStripMenuItem>();
        ToolStripMenuItem[] favorites = new ToolStripMenuItem[50];
        string FileFavorites = @"C:\Users\Public\TestFolder\Favorites.txt";
        string FileHistory = @"C:\Users\Public\TestFolder\History.txt";

        public void GetPage(string s)
        {
            try
            {
                if (!s.Equals(""))
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
                    if(resp.StatusCode == HttpStatusCode.NotFound) //404
                    {
                        MessageBox.Show("Error, 404 Not Found", "Error 404", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    if(resp.StatusCode == HttpStatusCode.BadRequest)//400
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
        //Previous function 
        public void Previous()
        {
            if (index >0)
            {
                GetPage(Url[index-1].ToString());
                index -= 2;
               
            }
        }
        //forward function
        public void Forward()
        {
            if (Url[index+1] != null)
            {
                GetPage(Url[index + 1].ToString());
                    
            }
        }

        public void History(string s)
        {
            if (Url[index] != null )
                {
                    history[index] = new ToolStripMenuItem();
                    history[index].Text =  s;
                   // history[index].Name = Url[index].ToString();
                    // history[index].ToolTipText = Url[index].ToString();
                    history[index].Click += new EventHandler(History_click);
                   if (index == 0 || history[index] != null)
                    historyToolStripMenuItem.DropDownItems.Add(history[index]);
            }
               

            }
            
           public void SetToFavorites(string s)
        {
            if (s != null && !checkfav(s))
            {
                favorites[fav] = new ToolStripMenuItem();
                favorites[fav].Text = s;
                favorites[fav].Name = s;
                favorites[fav].Click += new EventHandler(Favorites_Click);
                if (fav == 0 || favorites[fav] != null)
                {
                    favoritesToolStripMenuItem.DropDownItems.Add(favorites[fav]);
                }
            }
            fav++;

        }
        public bool checkfav(string s)
        {
            bool test = false;
            for (int i = 0; i < favorites.Length; i++)
            {
                if (favorites[i] != null)
                {
                    test = favorites[i].Text == s ? true : false;
                }
                if (test)
                {
                    break;
                }
            }
            return test;
        }
        public void Get_Fav(string file)
        {
            string line;
            if (File.Exists(file))
            {
                System.IO.StreamReader filetmp = new System.IO.StreamReader(file);

                if (file == FileHistory)
                {
                    while ((line = filetmp.ReadLine()) != null)
                        History(line);
                }

                else

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

        public void WriteFile(string file, ToolStripMenuItem[] tab)
        {
            if (File.Exists(file))
            {
                for (int i = 0; i < tab.Length; i++)
                {
                    if (tab[i] != null)
                    {
                        System.IO.File.WriteAllText(file, tab[i].Text);
                    }
                }
            }
        }

        private void Favorites_Click (object sender, EventArgs e)
        {
            ToolStripMenuItem clickedFavorites = (ToolStripMenuItem)sender;
            GetPage(clickedFavorites.Text);
        }

        private void History_click(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedHistory = (ToolStripMenuItem)sender;
            GetPage(clickedHistory.Text);
        }
        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            throw new NotImplementedException();
            
        }

     


        private void button2_Click(object sender, EventArgs e)
        {
            Forward();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Previous();
        }

       

        private void homeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.home != null)
            GetPage(Properties.Settings.Default.home);
        }

        private void setAsHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.home = textBox1.Text;
            Properties.Settings.Default.Save();
        }

       
      
        private void Form1_Load(object sender, EventArgs e)
        {
            //We get the Home Page URL loaded
            GetPage(Properties.Settings.Default.home);
            //We get the favorites
            Get_Fav(FileFavorites);
            Get_Fav(FileHistory);
        }

        private void setAsFavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetToFavorites(Url[index -1].ToString());
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //once the application closes, we write all the favorites/history in a text file
            WriteFile(FileFavorites, favorites);
            WriteFile(FileHistory, history);              
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
            GetPage(textBox1.Text);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GetPage(textBox1.Text);

            }
        }


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

       

        private void button1_Click_2(object sender, EventArgs e)
        {

        }

        private void changeColourToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void pinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColourMode(System.Drawing.Color.Black, System.Drawing.Color.White);
        }

        private void darkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColourMode(System.Drawing.Color.White, System.Drawing.Color.Black);

        }

        private void pinkToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ColourMode(System.Drawing.Color.Black, System.Drawing.Color.LightPink);

        }

        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColourMode(System.Drawing.Color.LightGreen, System.Drawing.Color.Blue);

        }

        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColourMode(System.Drawing.Color.Blue, System.Drawing.Color.Red);

        }
    }
}
