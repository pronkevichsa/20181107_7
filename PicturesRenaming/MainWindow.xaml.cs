using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Globalization;
using System.Drawing.Imaging;
using System.Drawing;


namespace PicturesRenaming
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TreeViewDrivers();
        }
        public DirectoryInfo dir;
        public bool ModifyPicture;

        void TreeViewDrivers()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Tag = drive;
                item.Header = drive.ToString();
                item.Items.Add("*");
                //trw_Products.Items.Add(item);
                treeView1.Items.Add(item);
                item.Expanded += new RoutedEventHandler(item_DirExpanded);
            }
        }

        void Item_FileExpanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            item.Items.Clear();
            DirectoryInfo driv;
            if (item.Tag is DriveInfo)
            {
                DriveInfo dr = (DriveInfo)item.Tag;
                driv = dr.RootDirectory;
            }
            else driv = (DirectoryInfo)item.Tag;

            try
            {
                listView1.Items.Clear();
                foreach (FileInfo fi in driv.GetFiles())
                {
                    switch (fi.Extension.ToLower())
                    {
                        case ".jpg":
                        case ".jpeg":
                        case ".png":
                            {
                                TreeViewItem newItem1 = new TreeViewItem();
                                newItem1.Tag = fi;
                                newItem1.Header = fi.ToString();
                                newItem1.Items.Add("*");
                                item.Items.Add(newItem1);
                                listView1.Items.Add(fi);
                                break;
                            }                            
                    }
                }
            }
            catch { }
        }

        void item_DirExpanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            item.Items.Clear();         
            if (item.Tag is DriveInfo)
            {
                DriveInfo drive = (DriveInfo)item.Tag;
                dir = drive.RootDirectory;
            }
            else dir = (DirectoryInfo)item.Tag;
            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Tag = subDir;
                    newItem.Header = subDir.ToString();
                    newItem.Items.Add("*");
                    item.Items.Add(newItem);

                    newItem.Expanded += new RoutedEventHandler(Item_FileExpanded);
                }
            }
            catch
            { }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (RBtn1.IsChecked==true)
            {
                string str = (string)Label1.Content;
                DateTime dt = GetExif(str);
                FileAddText(str, dt.ToString());
                FileRename(dir.FullName, listView1.SelectedValue.ToString(), dt.ToString().Replace(':','_'));     
            }
            else
            {

            }

            //MessageBox.Show(RBtn1.IsChecked.ToString());

            //OpenFileDialog op = new OpenFileDialog();
            //op.Title = "Select a picture";
            //op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
            //  "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
            //  "Portable Network Graphic (*.png)|*.png";
            //if (op.ShowDialog() == true)
            //{               
            //    loadText.Text = op.FileName;
            //    BitmapImage bitmap= new BitmapImage(new Uri(op.FileName));
            //    image1.Source = new BitmapImage(new Uri(op.FileName));             
            //}            
        }

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                loadText.Text = listView1.SelectedValue.ToString();
                string str=dir.FullName+"\\"+ listView1.SelectedValue.ToString();                
                Label1.Content = str;
                //DateTime dt = GetExif(str);
                //FileAddText(str, dt.ToString());
                //FileRename(dir.FullName, listView1.SelectedValue.ToString(), dt.ToString().Replace(':','_'));             
            }  
            catch (IOException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void FileAddText(string path, string strAdd)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                System.Drawing.Image image = System.Drawing.Image.FromStream(fs);
                fs.Close();
                Bitmap b = new Bitmap(image);
                Graphics graphics = Graphics.FromImage(b);
                graphics.DrawString(strAdd, new Font("Arial", 72), System.Drawing.Brushes.Red, 100, 100);
                // b.Save(path, image.RawFormat);
                b.Save(path, image.RawFormat);
                image.Dispose();
                b.Dispose();
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }       

        static public DateTime GetExif(string path)
        {
            DateTime dateOfImage=Convert.ToDateTime("01/01/1900");
            string exiftitle;
            string title;
            try
            {
                using (FileStream file = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    BitmapDecoder decoder = JpegBitmapDecoder.Create(file, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                    BitmapMetadata metadata = (BitmapMetadata)decoder.Frames[0].Metadata.Clone();
                  //  title = metadata.Title;
                    dateOfImage = Convert.ToDateTime(metadata.DateTaken);
                    if (dateOfImage.ToString().Substring(0, 10) == "01.01.0001")
                        dateOfImage = System.IO.File.GetCreationTime(path);

                    exiftitle = (string)metadata.GetQuery(@"/app1/ifd/{ushort=40091}");
                }                
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.ToString());
            }
           return dateOfImage;
        }

        static public void FileRename(string filePath, string oldfileName, string newfileName)
        {
           // Directory.CreateDirectory(this.dir.FullName + "\\AddedData");
                string str1 = filePath + "\\" + oldfileName;
                string fileExt;
                fileExt = System.IO.Path.GetExtension(str1);
                string str2 = filePath + "\\" + newfileName+ fileExt;
             //   string strbak = filePath + "\\" + newfileName + ".bak";
            // File.Replace(str1, str2, strbak);
                File.Copy(str1, newfileName+fileExt);
            MessageBox.Show("Done");
            
        }

        private void BtnSortDate_Click(object sender, RoutedEventArgs e)
        {
            string[] extensions = new[] { ".jpg", ".jpeg", ".png" };
            List<DateTime> dtFile = new List<DateTime>();            
            try
            {
                FileInfo[] files = dir.GetFiles().Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();
                foreach (FileInfo fi in files)
                {
                    dtFile.Add (GetExif(fi.FullName));
                    try
                    {
                        string strDir = dir.FullName + "\\" + GetExif(fi.FullName).Year.ToString();
                        Directory.CreateDirectory(strDir);
                        File.Copy(fi.FullName, strDir + "\\" + fi.Name);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            catch { }
            MessageBox.Show("Sorted by Year");
        }

        private void AddDateToFoto_Checked(object sender, RoutedEventArgs e)
        {
            BtnSortDate.IsEnabled = false;
            btnAdd.IsEnabled = true;
            loadText.IsEnabled = true;
            RBtn1.IsEnabled = true;
            Rbtn2.IsEnabled = true;
        }

        private void AddDateToFoto_Unchecked(object sender, RoutedEventArgs e)
        {            
            BtnSortDate.IsEnabled = true;
            btnAdd.IsEnabled = false;            
            loadText.IsEnabled = false;
            RBtn1.IsEnabled = false;
            Rbtn2.IsEnabled = false;
        }


        public static void AddDataToAllFoto()
        {

        }

        public static void AddDataToSelectedFoto(string fotoPath)
        {

        }

        private void RBtn1_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void BtnRename_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(dir.FullName + "\\Renamed");
                string[] extensions = new[] { ".jpg", ".jpeg", ".png" };
                List<DateTime> dtFile = new List<DateTime>();
                FileInfo[] files = dir.GetFiles().Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();
                foreach (FileInfo fi in files)
                {
                    dtFile.Add(GetExif(fi.FullName));
                    string newName = GetExif(fi.FullName).ToString().Replace(':', '_');
                    FileRename(fi.DirectoryName, fi.Name, dir.FullName + "\\Renamed\\"+newName);

                    //try
                    //{
                    //    string strDir = dir.FullName + "\\" + GetExif(fi.FullName).Year.ToString();
                    //    Directory.CreateDirectory(strDir);
                    //    File.Copy(fi.FullName, strDir + "\\" + fi.Name);
                    //}
                    //catch (IOException ex)
                    //{
                    //    MessageBox.Show(ex.ToString());
                    //}
                    

                }

            }
            catch(IOException ex)
            {

            }
            
        }
    }
}
