using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Loxonator.Common.Data;
using System.IO;
using System.Windows.Input;
using Loxonator.Common.Helpers;
using System.Windows;
using System.Collections.ObjectModel;

namespace Loxonator.Client
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private CommandHandler browseCommand;
        private CommandHandler loadCommand;
        private CommandHandler saveCommand;

        private Node root = new Node();
        private AsyncObservableCollection<Node> tree = new AsyncObservableCollection<Node>();
        private string importFile = String.Empty;
        private string projectFile = String.Empty;
        private string status = String.Empty;
        private bool isLoading = false;

        public ICommand BrowseCommand
        {
            get { return this.browseCommand; }
        }

        public ICommand LoadCommand
        {
            get { return this.loadCommand; }
        }

        public ICommand SaveCommand
        {
            get { return this.saveCommand; }
        }

        public Node Root
        {
            get { return this.root; }
        }

        public ObservableCollection<Node> Tree
        {
            get { return this.tree; }
        }

        public string ImportFile
        {
            get { return this.importFile; }
            set
            {
                if (this.importFile != value)
                {
                    this.importFile = value;
                    this.OnPropertyChanged("ImportFile");
                    this.loadCommand.OnCanExecuteChanged();
                }
            }
        }

        public string ProjectFile
        {
            get { return this.projectFile; }
            set
            {
                if (this.projectFile != value)
                {
                    this.projectFile = value;
                    this.OnPropertyChanged("ProjectFile");
                    this.saveCommand.OnCanExecuteChanged();
                }
            }
        }

        public string Status
        {
            get { return this.status; }
            set
            {
                if (this.status != value)
                {
                    this.status = value;
                    this.OnPropertyChanged("Status");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HandleTreePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasChildren")
                this.saveCommand.OnCanExecuteChanged();
            else if (e.PropertyName == "SelectedNode" && !isLoading)
                this.Status = String.Empty;
        }

        #endregion

        public MainViewModel()
        {
            this.browseCommand = new ParameterizedCommandHandler((extension) => this.BrowseForFile((string)extension));
            this.loadCommand = new SimpleCommandHandler(() => this.LoadImportFile(this.importFile), () => this.IsFileAvailable(this.importFile));
            this.saveCommand = new SimpleCommandHandler(() => this.SaveProjectFile(this.projectFile), () => this.IsFileAvailable(this.projectFile) && this.root.HasChildren);
            this.root.PropertyChanged += this.HandleTreePropertyChanged;
            this.tree.Add(this.root);
        }

        #region Commands

        private bool IsFileAvailable(string file)
        {
            return !String.IsNullOrEmpty(file) && File.Exists(file);
        }

        private void BrowseForFile(string extension)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();          
            dlg.DefaultExt = extension;
            string currentFile = String.Empty;
            if (".xml".Equals(extension, StringComparison.InvariantCultureIgnoreCase))
            {
                dlg.Filter = "Importdatei (.xml)|*.xml";
                currentFile = this.importFile;
            }
            else
            {
                dlg.Filter = "Loxone Projektdatei (.loxone)|*.loxone";
                currentFile = this.projectFile;
            }
            if (!String.IsNullOrEmpty(currentFile))
            {
                if (File.Exists(currentFile))
                    dlg.FileName = currentFile;
                if (Directory.Exists(Path.GetDirectoryName(currentFile)))
                    dlg.InitialDirectory = Path.GetDirectoryName(currentFile);
            }
            if (dlg.ShowDialog() == true)
            {
                if (".xml".Equals(extension, StringComparison.InvariantCultureIgnoreCase))
                    this.ImportFile = dlg.FileName;
                else
                    this.ProjectFile = dlg.FileName;
            }
        }

        private void LoadImportFile(string importFile)
        {
            try
            {
                this.isLoading = true;
                this.Status = "Wird geladen ...";
                ImportHelper.LoadImportFile(this.root, importFile);
                this.Status = "Geladen!";
            }
            catch (Exception ex)
            {
                this.Status = "Datei konnte nicht geladen werden!";
                Clipboard.SetText(ex.ToString());
            }
            finally
            {
                this.isLoading = false;
            }
        }

        private void SaveProjectFile(string projectFile)
        {
            try
            {
                this.Status = "Wird gespeichert ...";
                ExportHelper.ExportProjectFile(this.root, projectFile);
                this.Status = "Gespeichert!";
            }
            catch (Exception ex)
            {
                this.Status = "Datei konnte nicht gespeichert werden!";
                Clipboard.SetText(ex.ToString());
            }
        }

        #endregion

    }
}
