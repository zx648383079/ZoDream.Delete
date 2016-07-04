using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Delete.Model;
using ZoDream.Helper.Local;

namespace ZoDream.Delete.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {

        }

        /// <summary>
        /// The <see cref="Message" /> property's name.
        /// </summary>
        public const string MessagePropertyName = "Message";

        private string _message = string.Empty;

        /// <summary>
        /// Sets and gets the Message property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                Set(MessagePropertyName, ref _message, value);
            }
        }

        /// <summary>
        /// The <see cref="Pattern" /> property's name.
        /// </summary>
        public const string PatternPropertyName = "Pattern";

        private string _pattern = string.Empty;

        /// <summary>
        /// Sets and gets the Pattern property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Pattern
        {
            get
            {
                return _pattern;
            }
            set
            {
                Set(PatternPropertyName, ref _pattern, value);
            }
        }


        /// <summary>
        /// The <see cref="Source" /> property's name.
        /// </summary>
        public const string SourcePropertyName = "Source";

        private string _source = string.Empty;

        /// <summary>
        /// Sets and gets the Source property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Source
        {
            get
            {
                return _source;
            }
            set
            {
                Set(SourcePropertyName, ref _source, value.TrimEnd('\\'));
            }
        }

        /// <summary>
        /// The <see cref="Disk" /> property's name.
        /// </summary>
        public const string DiskPropertyName = "Disk";

        private string _disk = string.Empty;

        /// <summary>
        /// Sets and gets the Disk property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Disk
        {
            get
            {
                return _disk;
            }
            set
            {
                Set(DiskPropertyName, ref _disk, value.TrimEnd('\\'));
            }
        }


        /// <summary>
        /// The <see cref="IsNull" /> property's name.
        /// </summary>
        public const string IsNullPropertyName = "IsNull";

        private bool _isNull = false;

        /// <summary>
        /// Sets and gets the IsNull property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsNull
        {
            get
            {
                return _isNull;
            }
            set
            {
                Set(IsNullPropertyName, ref _isNull, value);
            }
        }


        private RelayCommand _sourceCommand;

        /// <summary>
        /// Gets the SourceCommand.
        /// </summary>
        public RelayCommand SourceCommand
        {
            get
            {
                return _sourceCommand
                    ?? (_sourceCommand = new RelayCommand(ExecuteSourceCommand));
            }
        }

        private void ExecuteSourceCommand()
        {
            var file = Open.ChooseFolder();
            if (!string.IsNullOrWhiteSpace(file))
            {
                Source = file;
            }
        }


        private RelayCommand _diskCommand;

        /// <summary>
        /// Gets the DiskCommand.
        /// </summary>
        public RelayCommand DiskCommand
        {
            get
            {
                return _diskCommand
                    ?? (_diskCommand = new RelayCommand(ExecuteDiskCommand));
            }
        }

        private void ExecuteDiskCommand()
        {
            var file = Open.ChooseFolder();
            if (!string.IsNullOrWhiteSpace(file))
            {
                Disk = file;
            }
        }

        private RelayCommand _startCommand;

        /// <summary>
        /// Gets the StartCommand.
        /// </summary>
        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand
                    ?? (_startCommand = new RelayCommand(ExecuteStartCommand));
            }
        }

        private void ExecuteStartCommand()
        {
            if (!Directory.Exists(Source))
            {
                _showMessage("源文件夹不存在！");
            }
            Task.Factory.StartNew(()=>
            {
                if (!string.IsNullOrWhiteSpace(Disk) && !Directory.Exists(Disk))
                {
                    _createFolder(Disk);
                }
                _deleteFile(Source);
                _showMessage("执行完成！");
            });
        }

        private bool _deleteFile(string folder)
        {
            var isDelete = true;
            var theFolder = new DirectoryInfo(folder);
            var dirInfo = theFolder.GetDirectories();
            //遍历文件夹
            foreach (var nextFolder in dirInfo)
            {
                if (!_deleteFile(nextFolder.FullName) && isDelete == true)
                {
                    isDelete = false;
                }
            }
            var fileInfo = theFolder.GetFiles();
            foreach (var item in fileInfo)
            {
                if (Regex.IsMatch(item.Name, Pattern))
                {
                    _moveFile(item);
                    isDelete = false;
                } else if (string.IsNullOrWhiteSpace(Disk))
                {
                    item.Delete();
                }

            }
            if (string.IsNullOrWhiteSpace(Disk) && IsNull && isDelete)
            {
                theFolder.Delete(true);
            }
            return isDelete;
        }

        private void _moveFile(FileInfo file)
        {
            if (string.IsNullOrWhiteSpace(Disk))
            {
                return;
            }
            var diskFile = Disk + "\\" + file.DirectoryName.Replace(Source, "");
            _createFolder(diskFile);
            file.CopyTo( Disk + "\\" + diskFile + "\\" + file.Name, true);
        }

        private void _createFolder(string foldder)
        {
            if (string.IsNullOrWhiteSpace(foldder))
            {
                return;
            }
            var args = foldder.TrimEnd('\\').Split('\\');
            var file = new StringBuilder();
            file.Append(args[0]);
            foreach (var item in args)
            {
                file.Append('\\');
                file.Append(item);
                var dir = file.ToString();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
             }
        }
        



        private void _showMessage(string message)
        {
            Message = message;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(3000);
                Message = string.Empty;
            });
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}