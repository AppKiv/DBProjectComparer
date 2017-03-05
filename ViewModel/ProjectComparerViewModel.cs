using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using System.ComponentModel;
using DBProjectComparer.DB;
using System.Data.SqlClient;
using Dapper;
using System.Data;
using DBProjectComparer.Model;
using System.IO;
using System.Configuration;
using System.Diagnostics;

namespace DBProjectComparer.ViewModel
{
    public class ProjectComparerViewModel: INotifyPropertyChanged
    {
        public string ProjectName { get; set; }
        public string ErrorMessage { get; set; }
        private string UserName { get; set; }
        public ProjectItemViewModel currItem { get; set;}

        public ProjectComparerViewModel()
        {
            ProjectName = "project id";
            UserName = ConfigurationManager.AppSettings["username"];
        }
        public IEnumerable<ProjectItemViewModel> ProjectItemList { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public RelayCommand LoadProjectItemCmd
        {
            get { return new RelayCommand(ProjectItemLoad, OnProjectItemCmdCanExecute); }
        }

        private bool OnProjectItemCmdCanExecute()
        {
            return ((ProjectName != null) && (ProjectName.Length > 0));
        }

        private void ProjectItemLoad()
        {
            var conn = (SqlConnection)DatabaseConnection.GetConnectionDB1();
            try
            {
                conn.Open();
                // todo replace ProjectName - Id
                ProjectItemList = conn.Query<ProjectItemViewModel>("bpBOVersion_TreeEnum", new { Id = ProjectName, UserName = UserName }, commandType: CommandType.StoredProcedure);
                OnPropertyChanged("ProjectItemList");
            }
            catch (Exception ex)
            {
                ErrorMessage=ex.Message;
            }
            finally
            {
                conn.Close();
            }
        }


        public RelayCommand CompareItemCmd
        {
            get { return new RelayCommand(CompareItem, OnCompareItemCmdCanExecute); }
        }

        private bool OnCompareItemCmdCanExecute()
        {
            return ((currItem != null)&&(currItem.TreeObjectType==5));
        }

        // сравниваем два файла
        private void CompareItem()
        {
            var item1 = new ItemModel();
            var item2 = new ItemModel();
            var conn1 = (SqlConnection)DatabaseConnection.GetConnectionDB1();
            var conn2 = (SqlConnection)DatabaseConnection.GetConnectionDB2();
            try
            {
                conn1.Open();
                item1 = conn1.Query<ItemModel>("bpBOMethod_GetText", new { BOObjectName = currItem.BOObjectName, BOMethodName = currItem.Name }, commandType: CommandType.StoredProcedure).FirstOrDefault();

                conn2.Open();
                item2 = conn2.Query<ItemModel>("bpBOMethod_GetText", new { BOObjectName = currItem.BOObjectName, BOMethodName = currItem.Name }, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                conn1.Close();
                conn2.Close();
            }

            
            string baseDir = Directory.GetCurrentDirectory(); //System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var text = "";
            if (item1 != null) text = item1.MethodText;
            System.IO.File.WriteAllText(baseDir+@"\tmp\file1.sql", text);
            text = "";
            if (item2 != null) text = item2.MethodText;
            System.IO.File.WriteAllText(baseDir + @"\tmp\file2.sql", text);

            var winMergePath = ConfigurationManager.AppSettings["winmergepath"];
            var winMergeArgs = ConfigurationManager.AppSettings["winmergecmd"];
            LaunchCommandLineApp(winMergePath, winMergeArgs);

            /*
            exec bpBOVersionProject_TreeEnum @UserName='user1',@Status=0
            exec bpBOVersion_TreeEnum @Id=524,@UserName='user1'
            EXEC bpBOMethod_GetText @BOObjectName='CRMEcInterface', @BOMethodName='ClientAddressSupplyList'
                        */
        }

        private void LaunchCommandLineApp(string path, string args)
        {
            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = path;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = args;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                // Log error.
            }

        }
    }
}
