﻿using System;
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
    public class ProjectComparerViewModel : INotifyPropertyChanged
    {
        private string UserName { get; set; }
        private ProjectModel _currProject;
        public string ErrorMessage { get; set; }
        public ProjectModel currProject {
            get { return _currProject; } 
            set
            {
                _currProject = value;
                if ((_currProject != null) && (_currProject.Id > 0))
                    ProjectItemLoad();
            }
        }
        public IEnumerable<ProjectModel> ProjectList { get; set; }

        public ProjectItemViewModel currItem { get; set; }
        public IEnumerable<ProjectItemViewModel> ProjectItemList { get; set; }

        public ProjectComparerViewModel()
        {
            UserName = ConfigurationManager.AppSettings["username"];
            LoadProjectList(UserName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void LoadProjectList(string userName, int status = 0)
        {
            var conn = (SqlConnection)DatabaseConnection.GetConnectionDB1();
            try
            {
                conn.Open();
                ProjectList = conn.Query<ProjectModel>("bpBOVersionProject_TreeEnum", new { @UserName = userName, @Status = status }, commandType: CommandType.StoredProcedure);
                OnPropertyChanged("ProjectList");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                conn.Close();
            }

        }

        public RelayCommand LoadProjectItemCmd
        {
            get { return new RelayCommand(ProjectItemLoad, OnProjectItemCmdCanExecute); }
        }

        private bool OnProjectItemCmdCanExecute()
        {
            return ((currProject!=null)&&(currProject.Id > 0));
        }

        private void ProjectItemLoad()
        {
            var conn = (SqlConnection)DatabaseConnection.GetConnectionDB1();
            try
            {
                conn.Open();
                ProjectItemList = conn.Query<ProjectItemViewModel>("bpBOVersion_TreeEnum", new { Id = currProject.Id, UserName = currProject?.UserName??UserName }, commandType: CommandType.StoredProcedure);
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
                    exeProcess.Close();
                }
                
            }
            catch
            {
                // Log error.
            }

        }
    }
}
