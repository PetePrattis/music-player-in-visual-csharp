using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;


namespace GreatMp3Player
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            panel1.Visible = false;//this panel will open every time we want to load new song or update a song's info
        }

        private void button2_Click(object sender, EventArgs e)//Play all button
        {
            if (listView1 != null)//there must be at least 1 song
            { 
                WMPLib.IWMPPlaylist playlist = axWindowsMediaPlayer1.playlistCollection.newPlaylist("myplaylist");//i create a playlist item
                WMPLib.IWMPMedia media;//and a media item
                for (int i = 0; i < listView1.Items.Count; i++)//for all the songs
                {
                    media = axWindowsMediaPlayer1.newMedia(listView1.Items[i].SubItems[1].Text);//we get the 2nd subitem of each row which is the full path
                    playlist.appendItem(media);//we create the playlist by adding all the paths
                    
                }
                axWindowsMediaPlayer1.currentPlaylist = playlist;//we initialize the playlist with the one we just created
                axWindowsMediaPlayer1.Ctlcontrols.play();//we play the playlist
            }

        }

        string[] saLvwItem = new string[4];
        int bt = 0;//this will be 1 if we clicked load song or -1 if we clicked update song
        int counter;//this will count the number of the songs we have checked for duplicate song
        private void btnLoadFiles_Click(object sender, EventArgs e)
        {
            counter = 0;
            //we open a dialog window to choose one audio file
            using (OpenFileDialog ofd = new OpenFileDialog() { Multiselect = false, ValidateNames = true, Filter = "Mp3 Audio File(*.mp3)|*.mp3|Windows Media Audio File(*.wma)|*.wma|WAV Audio file(*.wav)|*.wav|All files(*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)//if we chose a valid file
                {
                    for(int i=0; i<listView1.Items.Count; i++)//for all the songs we have loaded we check if we have chosen the same
                    {
                        if (listView1.Items[i].SubItems[1].Text == ofd.FileName)
                        {
                            MessageBox.Show("You already have this song in your playlist!");
                        }
                        else
                            counter++;//if the song we chose isn't in our playlist the counter should be equal to the number of the total songs
                    }
                    if (counter == listView1.Items.Count)
                    {
                        saLvwItem[1] = ofd.FileName;//we save the file's whole path
                        saLvwItem[0] = Path.GetFileNameWithoutExtension(saLvwItem[1]);//we get the name of the file without extensions
                        //we open a panel where the user will be able to fill each song's information
                        bt = 1;//this means we clicked the load file button
                        panel1.Visible = true;//we show the panel
                        button5.Visible = false;//we disable the form's objects
                        textBox1.Text = saLvwItem[0];//you can change the name
                        textBox2.Clear();
                        comboBox1.SelectedItem = "";
                        button1.Enabled = false;
                        button2.Enabled = false;
                        button4.Enabled = false;
                        btnLoadFiles.Enabled = false;
                        btnPlayAll.Enabled = false;
                        axWindowsMediaPlayer1.Ctlenabled = false;
                    }
                    
                }              
            }           
        }

        private void listView1_DoubleClick(object sender, EventArgs e)//double click name to play song
        {
            string selectedFile = listView1.FocusedItem.SubItems[1].Text;
            axWindowsMediaPlayer1.URL = @selectedFile;
        }

        private void button3_Click(object sender, EventArgs e)//the panel save button
        {
            if (bt == 1)//we clicked load song
            {
                if (textBox1.Text == "" || textBox2.Text == "" || comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Please complete the song's information");
                }
                else
                {
                    saLvwItem[0] = textBox1.Text;//title
                    saLvwItem[2] = textBox2.Text;//author
                    saLvwItem[3] = comboBox1.SelectedItem.ToString();//genre
                    ListViewItem lvi = new ListViewItem(saLvwItem);

                    listView1.Items.Add(lvi);//we add the new item

                    panel1.Visible = false;//we enable the form's objects
                    button1.Enabled = true;
                    button2.Enabled = true;
                    button4.Enabled = true;
                    btnLoadFiles.Enabled = true;
                    btnPlayAll.Enabled = true;
                    axWindowsMediaPlayer1.Ctlenabled = true;

                    InfoSaver();//we call this method that will save the new song into our database
                }
            }
            else if(bt ==-1)//we clicked update song
            {//same as the previous if
                if (textBox1.Text == "" || textBox2.Text == "" || comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Please complete the song's information");
                }
                else
                {
                    saLvwItem[0] = textBox1.Text;
                    saLvwItem[2] = textBox2.Text;
                    saLvwItem[3] = comboBox1.SelectedItem.ToString();
                    string connectionstring = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=Music.mdb";//an extra thing we need to do is to update our databe 
                    OleDbConnection conn = new OleDbConnection(connectionstring);
                    conn.Open();
                    string query = "Update Table1 set Title='" + textBox1.Text + "' where Path='" + uppath + "'";
                    string query1 = "Update Table1 set Author='" + textBox2.Text + "' where Path='" + uppath + "'";
                    string query2 = "Update Table1 set Genre='" + comboBox1.SelectedItem.ToString() + "' where Path='" + uppath + "'";
                    OleDbCommand cmd = new OleDbCommand(query, conn);
                    int i = cmd.ExecuteNonQuery();
                    OleDbCommand cmd1 = new OleDbCommand(query1, conn);
                    int i1 = cmd1.ExecuteNonQuery();
                    OleDbCommand cmd2 = new OleDbCommand(query2, conn);
                    int i2 = cmd2.ExecuteNonQuery();
                    conn.Close();                   

                    panel1.Visible = false;
                    button1.Enabled = true;
                    button2.Enabled = true;
                    button4.Enabled = true;
                    btnLoadFiles.Enabled = true;
                    btnPlayAll.Enabled = true;
                    axWindowsMediaPlayer1.Ctlenabled = true;                    
                    Loader();//we call this method that will clear the listview items and add them again with the changes
                }
            }
        }

        public void InfoSaver()//a method that saves new songs into the database
        {
            string connectionstring = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=Music.mdb";
            OleDbConnection conn = new OleDbConnection(connectionstring);
            conn.Open();
            string query = "Insert Into Table1(Path, Title, Author, Genre) values('" + saLvwItem[1] + "','" + saLvwItem[0] + "','" + saLvwItem[2] + "','" + saLvwItem[3] + "')";
            OleDbCommand cmd = new OleDbCommand(query, conn);
            int i = cmd.ExecuteNonQuery();
            conn.Close();

            Loader();
        }

        public void Loader()//method that will clear the listview items and add them again with the changes
        {
            listView1.Items.Clear();
            string connectionstring = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=Music.mdb";
            OleDbConnection conn = new OleDbConnection(connectionstring);
            conn.Open();
            string query = "SELECT * FROM Table1";
            OleDbCommand cmd = new OleDbCommand(query, conn);
            OleDbDataReader rdr = cmd.ExecuteReader();
            StringBuilder builder = new StringBuilder();
            while (rdr.Read())
            {
                saLvwItem[0] = rdr.GetString(2);
                saLvwItem[1] = rdr.GetString(1);
                saLvwItem[2] = rdr.GetString(3);
                saLvwItem[3] = rdr.GetString(4);
                ListViewItem lvi = new ListViewItem(saLvwItem);

                listView1.Items.Add(lvi);
            }
            conn.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Loader();//when  we open the app we load any songs that may be in our database
        }
        string d;//we will save the path that is to be deleted
        private void button1_Click(object sender, EventArgs e)//delete
        {
            if (listView1.FocusedItem != null)//if there is a chosen item
            {
                try//there may be an error if we delete a song taht it is also played by the windows media player
                {
                    d = listView1.FocusedItem.SubItems[1].Text;
                    WMPLib.IWMPControls3 controls = (WMPLib.IWMPControls3)axWindowsMediaPlayer1.Ctlcontrols;
                    controls.stop();//we stop the music
                    listView1.Items.Remove(listView1.FocusedItem);//we remove the item from the listview
                    //and we delete the item from the database
                    string connectionstring = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=Music.mdb";
                    OleDbConnection conn = new OleDbConnection(connectionstring);
                    conn.Open();
                    string query = "Delete from Table1 where Path='" + d + "'";
                    OleDbCommand cmd = new OleDbCommand(query, conn);
                    int i = cmd.ExecuteNonQuery();
                    conn.Close();
                    axWindowsMediaPlayer1.Ctlenabled = true;
                }catch
 {}
            }
        }

        string uppath;//the path that is to be updated or deleted 
        private void button4_Click(object sender, EventArgs e)//update
        {
            if(listView1.FocusedItem !=null)
            {
                bt = -1;//this means we have clicked the update button
                panel1.Visible = true;
                textBox1.Text = listView1.FocusedItem.SubItems[0].Text;//you can change the name
                textBox2.Text = listView1.FocusedItem.SubItems[2].Text;//you can change the author           
                comboBox1.SelectedItem = listView1.FocusedItem.SubItems[3].Text;
                button1.Enabled = false;
                button2.Enabled = false;
                btnLoadFiles.Enabled = false;
                btnPlayAll.Enabled = false;
                axWindowsMediaPlayer1.Ctlenabled = false;
            }          
        }

        private void button5_Click(object sender, EventArgs e)//cancel button inside panel
        {
            panel1.Visible = false;
            button1.Enabled = true;
            button2.Enabled = true;
            button4.Enabled = true;
            btnLoadFiles.Enabled = true;
            btnPlayAll.Enabled = true;
            axWindowsMediaPlayer1.Ctlenabled = true;
        }

        private void listView1_Click(object sender, EventArgs e)//selected item
        {
            uppath = listView1.FocusedItem.SubItems[1].Text;//the path that is to be updated or deleted
        }

        private void button2_Click_1(object sender, EventArgs e)//random song
        {
            if (listView1 != null)//if there are any songs
            {
                WMPLib.IWMPPlaylist playlist = axWindowsMediaPlayer1.playlistCollection.newPlaylist("myplaylist");//we create a new playlist 
                WMPLib.IWMPMedia media;

                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    media = axWindowsMediaPlayer1.newMedia(listView1.Items[i].SubItems[1].Text);
                    playlist.appendItem(media);
                }

                axWindowsMediaPlayer1.currentPlaylist = playlist;
                axWindowsMediaPlayer1.settings.setMode("shuffle", true);//with this seeting every time we click the random button or the next button a random song from our playlist will play
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }
    }
}
