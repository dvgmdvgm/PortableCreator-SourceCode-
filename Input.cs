using System;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Resources;
using System.Reflection;
using Microsoft.Win32;

namespace test {
  static class Program {
    [STAThread]
    static void Main(string[] args) {
      try {
          Application.EnableVisualStyles();
          Application.SetCompatibleTextRenderingDefault(false);
          Application.Run(new Form1(args));
      } finally {
          InstanceChecker.ReleaseMemory();
      }
    }
  }

  public partial class Form1 : Form {
    private readonly NativeMethods nativeMethods;
    private readonly PortableSetupEngine portableSetupEngine;

    public Form1(string[] args) {
      nativeMethods = NativeMethods.GetInstance;
      portableSetupEngine = PortableSetupEngine.GetInstance;
      InitializeComponent(args);
    }
  }

  class NativeMethods{
    #region Static
    // Получить Instance класса
    static volatile NativeMethods thisClass;
    static object SyncObject = new object();
    public static NativeMethods GetInstance {
      get {
        if (thisClass == null)
          lock (SyncObject) {
            if (thisClass == null)
              thisClass = new NativeMethods();
          }
        return thisClass;
      }
    }
    #endregion
    // ==================== Kernel32.dll ====================
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool CreateSymbolicLink(
        string lpSymlinkFileName,
        string lpTargetFileName,
        SymbolicLink dwFlags
    );

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern uint GetFinalPathNameByHandle(
        IntPtr hFile,
        [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath,
        uint cchFilePath,
        uint dwFlags
    );

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);
    
    // ==================== User.dll ====================
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    // ==================== Shell32.dll ====================
    [DllImport("Shell32.dll", SetLastError = true)]
    public static extern int SHChangeNotify(
        int eventId,
        int flags,
        IntPtr item1,
        IntPtr item2
    );

    // ==================== Advapi32.dll (для реестра) ====================
    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int RegDeleteKeyEx(
        IntPtr hKey,
        string lpSubKey,
        uint samDesired,
        uint Reserved
    );

    // ==================== Перечисления и структуры ====================
    public enum SymbolicLink
    {
        File = 0,
        Directory = 1
    }

    // ==================== Константы для CreateFile ====================
    public const uint GENERIC_READ = 0x80000000;
    public const uint FILE_SHARE_READ = 0x00000001;
    public const uint OPEN_EXISTING = 3;
    public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

    // ==================== Константы для SHChangeNotify ====================
    public const int SHCNE_ASSOCCHANGED = 0x08000000;
    public const int SHCNF_IDLIST = 0x0000;
    public const int SHCNE_ALLEVENTS = 0x7FFFFFFF;
    public const int SHCNE_UPDATEITEM = 0x00002000;
    public const int SHCNE_RENAMEITEM = 0x00000001;
    public const int SHCNE_CREATE = 0x00000002;
    public const int SHCNE_DELETE = 0x00000004;
    public const int SHCNF_PATHW = 0x0005;
    public const int SHCNF_FLUSHNOWAIT = 0x2000;

    // ==================== Константы для реестра ====================
    public const int ERROR_SUCCESS = 0;
    public static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
    public static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
    public static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
    public static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
    public static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(unchecked((int)0x80000005));

    // ==================== Константы для WM_CLOSE ====================
    public const uint WM_CLOSE = 0x0010;
  }

  // Хранение полей и переменных
  class Variables {
    #region Static
    // Получить Instance класса
    static volatile Variables thisClass;
    static object SyncObject = new object();
    public static Variables GetInstance {
      get {
        if (thisClass == null)
          lock (SyncObject) {
            if (thisClass == null)
              thisClass = new Variables();
          }
        return thisClass;
      }
    }
    #endregion

    public string launcherName = System.AppDomain.CurrentDomain.FriendlyName;
    public string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
    public string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public string localLowPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("Local", "LocalLow");
    public string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public string musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
    public string videosPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
    public string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public string picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    public string taskbarPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Microsoft\Internet Explorer\Quick Launch\User Pinned";
    public string logFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "created_symlinks.txt");
    public string executablePath = System.AppDomain.CurrentDomain.BaseDirectory + @"\EXEPATH";
    public string skippedLogFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "non_created_symlinks.txt");
    public string ar = "";
    public string backupState = "empty";

    public bool forceRestart = false;
    public bool isUpdateMode = false;
    public bool networkBlocked = false;
    public bool clearLogs = false;
    public bool showClosePopup = false;
    public bool repairingInProcess = false;

    public List<string> allValuesFormCurrentLauncher = new List<string>();  // Все созданные линки от текущей программы
    public List<string> allValuesFormOthersLaunchers = new List<string>();  // Все созданные линки от других программ
    public List<string> allLauncherNames = new List<string>();
    public List<string> valuesForRecheck = new List<string>();  // Все линки которые конфликтовали и не были удалены, но нужно перепроверить
    public List<string> driversPath = new List<string>();
    public List<string> list = new List<string>();
    public List<string> servicesForDelete = new List<string>();
    public List<string> logPaths = new List<string>();
    public List<Process> cumulativeProcesses = new List<Process>();

    public dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
    public Form1 form1;
  }

  // Основной класс запуска портативки
  partial class Form1 {
    private IContainer components = null;

    private void InitializeComponent(string[] args) {
      ShowInTaskbar = false;
      Variables.GetInstance.form1 = this;
      components = new Container();
      AutoScaleMode = AutoScaleMode.Font;
      Text = "Form1";
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      Opacity = 0;

      Load += async (s, a) => {
        if (args.Length > 0) {
          List<string> tokens = args.ToList();
          tokens.ForEach(async x => {
            if (x.ToLower() == (Variables.GetInstance.appPath + @"data\" + Variables.GetInstance.launcherName + ".zip").ToLower()) {
              Variables.GetInstance.repairingInProcess = true;
              PopupManager.GetInstance.CreatePopup(exePath: Variables.GetInstance.launcherName, isBackup: false);
              await PortableRepairManager.GetInstance.StartRestoreProcess();
            } else if (x.ToLower() == "--um") {
              Variables.GetInstance.isUpdateMode = true;
            } else if (x.ToLower() == "--showargs") {
              MessageBox.Show(string.Join(System.Environment.NewLine, tokens));
            } else {
              if (!x.StartsWith("\"") && !x.EndsWith("\"")) {
                x = "\"" + x + "\"";
              }
              Variables.GetInstance.ar = Variables.GetInstance.ar + x + " ";
            }
          });
        }
        if (!InstanceChecker.TakeMemory()) {
            // Если уже есть запущенный экземпляр - запускаем программу с аргументами и завершаемся
            using (Process process = Process.Start(Variables.GetInstance.executablePath, Variables.GetInstance.ar)) { }
            Application.Exit();
            return;
        }

        while (Variables.GetInstance.repairingInProcess) {
          await Task.Delay(200);
        }

        if (File.Exists(Variables.GetInstance.appPath + @"\--um.txt")) {
          Variables.GetInstance.isUpdateMode = true;
        }

        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\PortableCreator\Running")) {
          if (key.GetValue(Variables.GetInstance.appPath) == null) {
            key.SetValue(Variables.GetInstance.appPath, "");
          } 
          using (RegistryKey keyPrev = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Symlinks\" + Variables.GetInstance.launcherName, true)) {
            if (keyPrev != null) {
              if (File.Exists(Variables.GetInstance.logFilePath)) {
                MessageBox.Show(
                    "В реестре, остался раздел от портативной программы. Возможно программа была закрыта принудительно без очистки реестра и рабочих каталогов." +
                    "\r\nПроверь наличие файлов \"created_symlinks\" и \"non_created_symlinks\" в папке Data твоего портативного приложения и напиши на форум win10tweaker.ru" +
                    "\r\nМожешь игнорировать это сообщение, если после следующего запуска программы всё будет работать как положено.");
                Variables.GetInstance.forceRestart = true;
                await CleanupEngine.GetInstance.RemoveFilesFolders(Variables.GetInstance.appPath, Variables.GetInstance.userPath, Variables.GetInstance.logFilePath);
                Application.Exit();
                return;
              }
            }
          }
        }

        await SetupPortable();

        await ProcessManager.GetInstance.RunProcessAsync(Variables.GetInstance.executablePath, Variables.GetInstance.ar);

        await Cleanup();

        Application.Exit();
        return;
      };

      FormClosing += async (s, a) => {
        await Task.Delay(1);
        if (Variables.GetInstance.forceRestart) {
        }
      };
    }

    async Task SetupPortable() {
      await PortableSetupEngine.GetInstance.Import(Variables.GetInstance.appPath);
      await PortableSetupEngine.GetInstance.CreateCustomSymLinks(Variables.GetInstance.appPath, Variables.GetInstance.userPath, Variables.GetInstance.list);
      PortableSetupEngine.GetInstance.GeneralSymlinksCreator();

      new List<string> { Variables.GetInstance.appPath + @"Data\Закрыть.txt", Variables.GetInstance.appPath + @"Data\Реестр\Удалить.txt",
                         Variables.GetInstance.appPath + @"Data\Кастом.txt", Variables.GetInstance.appPath + @"Data\Проводник.txt",
                         Variables.GetInstance.appPath + @"Data\Службы.txt" }
          .ForEach(x => {
            if (File.Exists(x)) {
              if (new FileInfo(x).Length == 0) {
                File.Delete(x);
              } else {
                if (x.EndsWith(@"Data\Службы.txt")) {
                  List<string> services = new List<string>(File.ReadAllLines(Variables.GetInstance.appPath + @"Data\Службы.txt"));
                  int num = 1;
                  services.ForEach(y => {
                    string serviceName = Variables.GetInstance.launcherName.Replace(".exe", " " + num);
                    ProcessManager.GetInstance.PS("New-Service -Name \"" + serviceName + "\" -StartupType Manual -BinaryPathName \"" + y.Replace("\"", "`\"") + "\"");
                    ProcessManager.GetInstance.PS("Start-Service -Name \"" + serviceName + "\"");
                    Variables.GetInstance.servicesForDelete.Add(serviceName);
                    num++;
                  });
                }
              }
            }
          });

      new List<string> { Variables.GetInstance.appPath + @"Data\Пользователи\Общие", Variables.GetInstance.appPath + @"Data\Пользователи",
                         Variables.GetInstance.appPath + @"Data\Реестр", Variables.GetInstance.appPath + @"Data\AppData" }
          .ForEach(x => {
            if (Directory.Exists(x)) {
              if (FileUtils.GetInstance.IsDirectoryEmpty(x)) {
                Directory.Delete(x, true);
              }
            }
          });

      TaskBarWorker.GetInstance.Runner(Variables.GetInstance.shell, Variables.GetInstance.appPath, Variables.GetInstance.taskbarPath);
      FirewallWorker.GetInstance.GetExesForFirewall(Variables.GetInstance.executablePath, Variables.GetInstance.networkBlocked, false);

      if (File.Exists(Variables.GetInstance.logFilePath)) {
        Variables.GetInstance.logPaths = File.ReadAllLines(Variables.GetInstance.logFilePath).ToList();
        try {
          using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\PortableCreator\Symlinks\" + Variables.GetInstance.launcherName)) {
            if (key != null) {
              foreach (var logPath in Variables.GetInstance.logPaths) {
                key.SetValue(logPath.Trim(), "", RegistryValueKind.String);
              }
            }
          }
        } catch (Exception ex) {
          MessageBox.Show("Ошибка при записи в реестр: " + ex.Message);
        }
      }

      if (PortableRepairManager.GetInstance.needBackup) {
        Variables.GetInstance.backupState = "empty";
        PopupManager.GetInstance.CreatePopup(exePath: Variables.GetInstance.launcherName, isBackup: true);
        MessageBox.Show(
            "\"PortableCreator\" обнаружил, что у Тебя нет резервной копии портативки для аварийного восстановления! Следующие шаги помогут Тебе создать бэкап.\r\n" +
                "1. Сейчас внизу экрана есть уведомление и Твоя программа запустится.\r\n2. Если Твоя программа запустилась и работает исправно, настрой её, как Тебе нужно. (НЕ ЗАКРЫВАЙ)\r\n" +
                "3. После первоначальной настройки нажми на уведомление внизу справа.\r\nКак только Ты нажмёшь на него \"PortableCreator\" создаст копию данных для восстановления.",
            "  ПРОЧТИ ЭТО ВНИМАТЕЛЬНО!");
      } else {
        PopupManager.GetInstance.CreatePopup(exePath: Variables.GetInstance.launcherName, isBackup: false);
      }
    }

    async Task Cleanup() {
      if (Variables.GetInstance.backupState == "inProcess") {
          await PortableRepairManager.GetInstance.StartBackupProcessAsync();
          while (Variables.GetInstance.backupState == "inProcess") {
              await Task.Delay(100);
        }
        await ProcessManager.GetInstance.RunProcessAsync(Variables.GetInstance.executablePath, Variables.GetInstance.ar);
      }

      FirewallWorker.GetInstance.GetExesForFirewall(Variables.GetInstance.executablePath, false, true);

      using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Symlinks\" + Variables.GetInstance.launcherName, true)) {
        if (key != null) {
          foreach (var logPath in Variables.GetInstance.logPaths) {
            try {
              if (key.GetValue(logPath) != null) { 
              }
            } catch (Exception ex) {
              MessageBox.Show("Ошибка при удалении из реестра: " + ex.Message);
            }
          }
        }
      }

      TaskBarWorker.GetInstance.Closer(Variables.GetInstance.shell, Variables.GetInstance.appPath, Variables.GetInstance.taskbarPath);

      if (File.Exists(Variables.GetInstance.appPath + @"\--um.txt")) {
        Variables.GetInstance.isUpdateMode = true;
      }

      if (!Variables.GetInstance.isUpdateMode) {
        await CleanupEngine.GetInstance.KillProcess(Variables.GetInstance.appPath);
        await CleanupEngine.GetInstance.Export(Variables.GetInstance.appPath);
        await CleanupEngine.GetInstance.RemoveSymLinks(Variables.GetInstance.list);
        await CleanupEngine.GetInstance.RemoveFilesFolders(Variables.GetInstance.appPath, Variables.GetInstance.userPath, Variables.GetInstance.logFilePath);
        RegistryHelper.GetInstance.RemoveCustomRegistryKeys(Variables.GetInstance.appPath);

        Variables.GetInstance.servicesForDelete.ForEach(x => {
          ProcessManager.GetInstance.PS("Stop-Service -Name \"" + x + "\"");
          ProcessManager.GetInstance.PS("sc.exe Delete \"" + x + "\"");
        });

        Variables.GetInstance.driversPath.AsParallel().ForAll(dir => {
          string command = "Get-ChildItem \"" + dir + "\" -Recurse -Filter \"*.inf\" | ForEach-Object { PNPUtil.exe /delete-driver $_.FullName /uninstall }";
          ProcessManager.GetInstance.PS(command);
        });

        if (Variables.GetInstance.clearLogs) {
          try {
            Directory.Delete(Variables.GetInstance.userPath + @"\AppData\Local\Microsoft\CLR_v4.0", true);
            Directory.Delete(Variables.GetInstance.userPath + @"\AppData\Local\Microsoft\CLR_v4.0_32", true);
          } catch {
          }
        }

      NativeMethods.SHChangeNotify(NativeMethods.SHCNE_ASSOCCHANGED,NativeMethods.SHCNF_IDLIST,IntPtr.Zero,IntPtr.Zero);
        await CleanupEngine.GetInstance.RegistryRemoveLaunchedUnits();
      }

      if (Variables.GetInstance.showClosePopup) {
        await Task.Delay(3000);
      }
    }
  }

  // Все методы для реализации портативного режима
  class PortableSetupEngine {
    #region Static
    // Получить Instance класса (реализация паттерна Singleton)
    static volatile PortableSetupEngine thisClass;
    static object SyncObject = new object();
    public static PortableSetupEngine GetInstance {
        get {
            if (thisClass == null)
                lock (SyncObject) {
                    if (thisClass == null)
                        thisClass = new PortableSetupEngine();
                }
            return thisClass;
        }
    }
    #endregion
    
    public async Task Import(string appPath) {
      try {
        string registryPath = Path.Combine(appPath, @"Data\Реестр");
        if (!Directory.Exists(registryPath))
          return;
        var regFiles = Directory.EnumerateFiles(registryPath, "*.reg", SearchOption.TopDirectoryOnly);
        string blockedRegistryKey = "";
        await Task.Run(() => {
          Parallel.ForEach(regFiles, regFile => {
            try {
              // Чтение и модификация файла
              string originalContent = File.ReadAllText(regFile, Encoding.UTF8);
              string modifiedContent = new Regex(@"HKEY_USERS\\.*?\\SOFTWARE\\").Replace(originalContent, @"HKEY_CURRENT_USER\SOFTWARE\");
              // Запись временного файла
              string tempFile = Path.GetTempFileName();
              File.WriteAllText(tempFile, modifiedContent, Encoding.UTF8);
              // Импорт в реестр
              ProcessManager.GetInstance.RunProccess("regedit.exe", ("/s \"" + tempFile + "\""));
              var lines = File.ReadAllLines(tempFile);
              foreach (var line in lines)  // Блокировка удаления ключа для других Лаунчеров.
              {
                if (line.StartsWith("[") && line.EndsWith("]")) {
                  blockedRegistryKey = line.Trim('[', ']');
                  try {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\PortableCreator\BlockedKeys")) {
                      if (key != null) {
                        key.SetValue(blockedRegistryKey.Trim(), Variables.GetInstance.launcherName + ";" + key.GetValue(blockedRegistryKey.Trim()),
                                     RegistryValueKind.String);
                      }
                    }
                  } catch (Exception ex) {
                    MessageBox.Show("Ошибка при записи в реестр: " + ex.Message);
                  }
                  break;
                }
              }
              // Удаление временного файла
              File.Delete(tempFile);
            } catch (Exception ex) {
              Debug.WriteLine("Ошибка при обработке файла" + regFile + ": " + ex.Message);
            }
          });
        });
      } catch (Exception ex) {
        Debug.WriteLine("Ошибка в Import: " + ex.Message);
      }
    }

    public async Task CreateCustomSymLinks(string appPath, string userPath, List<string> list) {
      try {
        List<string> pathsForCreate = new List<string>(File.ReadAllLines(appPath + @"\Data\Кастом.txt"));
        pathsForCreate.AsParallel().ForAll(path => {
          if (!path.StartsWith("#")) {
            string pathEdited = path.ToLower()
                                    .Replace("%appdata%", userPath + "\\AppData")
                                    .Replace("%user%", userPath)
                                    .Replace("%documents%", Variables.GetInstance.documentsPath)
                                    .Replace("%videos%", Variables.GetInstance.videosPath)
                                    .Replace("%desktop%", Variables.GetInstance.desktopPath)
                                    .Replace("%pictures%", Variables.GetInstance.picturesPath)
                                    .Replace("%downloads%", userPath + "\\downloads")
                                    .Replace("%portable%", appPath)
                                    .Replace("%temp%", Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine));
            if (pathEdited.StartsWith("\"")) {
              pathEdited = pathEdited.Replace("\"", "");
            }
            pathEdited = pathEdited.Replace("\\\\", "\\");
            string to = pathEdited.Substring(pathEdited.IndexOf('>') + 2);
            string from = pathEdited.Substring(0, pathEdited.IndexOf(">") - 1);
            list.Add(to);
            if (Directory.Exists(from)) {
              FileUtils.GetInstance.Create(from, to, "Folder");
            }
            if (File.Exists(from)) {
              FileUtils.GetInstance.Create(from, to, "File");
            }
            
          }
        });
      } catch {
      }
      await Task.Delay(1);
    }
    
    public void GeneralSymlinksCreator() {
      string[] dirs;

      if (Directory.Exists(Variables.GetInstance.appPath + @"App")) {
        if (FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"App")) {
          Directory.Delete(Variables.GetInstance.appPath + @"App", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\AppData\Local")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\AppData\Local")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\AppData\Local", Variables.GetInstance.localPath, Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\AppData\Local", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\AppData\Roaming")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\AppData\Roaming")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\AppData\Roaming", Variables.GetInstance.roamingPath, Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\AppData\Roaming", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\AppData\LocalLow")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\AppData\LocalLow")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\AppData\LocalLow", Variables.GetInstance.localLowPath, Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\AppData\LocalLow", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Пользователи\Музыка")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Пользователи\Музыка")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\Пользователи\Музыка", Variables.GetInstance.musicPath, Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Пользователи\Музыка", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Пользователи\Изображения")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Пользователи\Изображения")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\Пользователи\Изображения", Variables.GetInstance.picturesPath,
                           Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Пользователи\Изображения", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Пользователи\Загрузки")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Пользователи\Загрузки")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\Пользователи\Загрузки", Variables.GetInstance.userPath + "\\downloads",
                           Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Пользователи\Загрузки", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Пользователи\Документы")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Пользователи\Документы")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\Пользователи\Документы", Variables.GetInstance.documentsPath,
                           Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Пользователи\Документы", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Пользователи\Общие\Документы")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Пользователи\Общие\Документы")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\Пользователи\Общие\Документы", "C:\\users\\public\\documents",
                           Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Пользователи\Общие\Документы", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Пользователи\Видео")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Пользователи\Видео")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\Пользователи\Видео", Variables.GetInstance.videosPath, Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Пользователи\Видео", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Пользователи\Рабочий стол")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Пользователи\Рабочий стол")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\Пользователи\Рабочий стол", Variables.GetInstance.desktopPath,
                           Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Пользователи\Рабочий стол", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\ProgramData")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\ProgramData")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\ProgramData", @"C:\ProgramData", Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\ProgramData", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Program Files\Common Files")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Program Files\Common Files")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\Program Files\Common Files", @"C:\Program Files\Common Files",
                           Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Program Files\Common Files", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Program Files (x86)\Common Files")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Program Files (x86)\Common Files")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\Program Files (x86)\Common Files", @"C:\Program Files (x86)\Common Files",
                           Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Program Files (x86)\Common Files", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Program Files")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Program Files")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\Program Files", @"C:\Program Files", Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Program Files", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Program Files (x86)")) {
        if (!FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Program Files (x86)")) {
          FileUtils.GetInstance.ProcessDirectory(Variables.GetInstance.appPath + @"Data\Program Files (x86)", @"C:\Program Files (x86)", Variables.GetInstance.logFilePath);
        } else {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Program Files (x86)", true);
        }
      }

      if (Directory.Exists(Variables.GetInstance.appPath + @"Data\Драйвер")) {
        if (FileUtils.GetInstance.IsDirectoryEmpty(Variables.GetInstance.appPath + @"Data\Драйвер")) {
          Directory.Delete(Variables.GetInstance.appPath + @"Data\Драйвер", true);
        } else {
          dirs = Directory.GetDirectories(Variables.GetInstance.appPath + @"Data\Драйвер", "*", SearchOption.TopDirectoryOnly);
          List<string> driverDirs = (Directory.GetDirectories(Variables.GetInstance.appPath + @"Data\Драйвер", "*", SearchOption.TopDirectoryOnly)).ToList();
          driverDirs.AsParallel().ForAll(dir => {
            Variables.GetInstance.driversPath.Add(dir);
            string command = "Get-ChildItem \"" + dir + "\" -Recurse -Filter \"*.inf\" | ForEach-Object { PNPUtil.exe /add-driver $_.FullName /install }";
            ProcessManager.GetInstance.PS(command);
          });
        }
      }
    }   
  }

  // Все методы для очистки хвостов
  class CleanupEngine {
    #region Static
    // Получить Instance класса (реализация паттерна Singleton)
    static volatile CleanupEngine thisClass;
    static object SyncObject = new object();
    public static CleanupEngine GetInstance {
        get {
            if (thisClass == null)
                lock (SyncObject) {
                    if (thisClass == null)
                        thisClass = new CleanupEngine();
                }
            return thisClass;
        }
    }
    #endregion

    public async Task KillProcess(string appPath) {
      try {
        string[] processesForClose = File.ReadAllLines(appPath + @"\Data\Закрыть.txt");
        string procName = "";
        foreach (string name in processesForClose) {
          if (name.EndsWith(".exe")) {
            procName = name.Replace(".exe", "");
          }
          foreach (var processKiller in Process.GetProcessesByName(procName)) {
            processKiller.Kill();
            processKiller.WaitForExit();
          }
        }
      } catch {
      }
      await Task.Delay(1);
    }

    public async Task Export(string appPath) {
      if (Directory.Exists(appPath + @"\Data\Реестр")) {
        try {
          var regFiles = Directory.GetFiles(appPath + @"\Data\Реестр", "*.reg", SearchOption.TopDirectoryOnly);
          var tasks = new List<Task>();
          foreach (var regFile in regFiles) {
            tasks.Add(Task.Run(() => {
              try {
                // Читаем оригинальный файл
                string originalContent = File.ReadAllText(regFile);
                // Заменяем пути в реестре для импорта
                string modifiedContent = new Regex(@"HKEY_USERS\\.*?\\SOFTWARE\\").Replace(originalContent, @"HKEY_CURRENT_USER\SOFTWARE\");
                File.WriteAllText(regFile, modifiedContent, Encoding.UTF8);
                // Получаем путь в реестре из файла
                string[] lines = File.ReadAllLines(regFile);
                if (lines.Length < 3)
                  return;
                string registryPath = lines[2].Replace("[", "").Replace("]", "").Trim();
                // Экспортируем текущие значения
                ProcessManager.GetInstance.RunProccess("reg.exe", ("export \"" + registryPath + "\" \"" + regFile + "\" /y"));
                // Удаляем ключ из реестра если он не заблокирован другими Лаунчерами и не происходит процесс резервного копирования
                if (Variables.GetInstance.backupState != "created") {
                  using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\BlockedKeys", true)) {
                    if (key != null) {
                      string[] valueNamesCur = key.GetValueNames();
                      foreach (string valueName in valueNamesCur) {
                        if (valueName == registryPath) {
                          string valueData = key.GetValue(valueName).ToString();
                          key.SetValue(valueName, valueData.Replace(Variables.GetInstance.launcherName + ";", ""), RegistryValueKind.String);
                          valueData = key.GetValue(valueName).ToString();
                          bool hasOtherEntries = !string.IsNullOrEmpty(valueData) && valueData.Contains(".exe;");
                          if (!hasOtherEntries) {
                            ProcessManager.GetInstance.RunProccess("reg.exe", ("delete \"" + registryPath + "\" /f"));
                            valueData = key.GetValue(valueName).ToString();
                            if (valueData.Length < 3) {
                              key.DeleteValue(valueName, true);
                            }
                          }
                        }
                      }
                    }
                  }
                }
              } catch (Exception ex) {
                // Логирование ошибки при необходимости
                Debug.WriteLine("Error processing " + regFile + ": " + ex.Message);
              }
            }));
          }
          await Task.WhenAll(tasks);
        } catch (Exception ex) {
          Debug.WriteLine("Export error: " + ex.Message);
        }
      }
    }

    public async Task RemoveSymLinks(List<string> dirsForDelete) {
      try {
        dirsForDelete.AsParallel().ForAll(dir => {
          if (FileUtils.GetInstance.IsSymbolic(dir)) {
            if (Directory.Exists(dir)) {
              Directory.Delete(dir, true);
            }
            if (Directory.Exists(dir + ".portable_backup")) {
              Directory.Move(dir + ".portable_backup", dir);
            }
            if (File.Exists(dir)) {
              File.Delete(dir);
            }
            if (File.Exists(dir + ".portable_backup")) {
              File.Move(dir + ".portable_backup", dir);
            }
          } else {
            // MessageBox.Show("This " + dir + " is not Symbol");
          }
        });
      } catch {
      }
      await Task.Delay(1);
    }

    public async Task RemoveFilesFolders(string appPath, string userPath, string logFilePath) {
      if (File.Exists(logFilePath)) {
        try {
          List<string> logPaths = File.ReadAllLines(logFilePath)
                                      .Select(line => line.Trim())                 // Убираем лишние пробелы
                                      .Where(line => !string.IsNullOrEmpty(line))  // Исключаем пустые строки
                                      .ToList();
          if (logPaths.Count == 0) {
            return;
          }
          if (Variables.GetInstance.forceRestart) {
            await CleanupEngine.GetInstance.RemoveSymLinks(logPaths);
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Symlinks\" + Variables.GetInstance.launcherName, true)) {
              if (key != null) {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\PortableCreator\Symlinks\" + Variables.GetInstance.launcherName, false);
              }
            }
            // Application.Restart();
            // Environment.Exit(0);
          }
          using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Symlinks", true)) {
            if (key != null) {
              string[] subKeyNames = key.GetSubKeyNames();
              RegistryKey currentLauncher;
              RegistryKey otherLauncher;
              // Выводим имена подключей
              foreach (string subKeyName in subKeyNames) {
                Variables.GetInstance.allLauncherNames.Add(subKeyName);  // Составляем список всех активных лаунчеров
                if (subKeyName == Variables.GetInstance.launcherName) {
                  currentLauncher = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Symlinks\" + subKeyName);
                  if (currentLauncher != null) {
                    string[] valueNamesCur = currentLauncher.GetValueNames();
                    foreach (string valueName in valueNamesCur) {
                      Variables.GetInstance.allValuesFormCurrentLauncher.Add(valueName);  // Получаем все созданные симлинки от активного лаунчера
                    }
                  }
                } else {
                  otherLauncher = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Symlinks\" + subKeyName);
                  if (otherLauncher != null) {
                    string[] valueNamesOth = otherLauncher.GetValueNames();
                    foreach (string valueName in valueNamesOth) {
                      Variables.GetInstance.allValuesFormOthersLaunchers.Add(valueName);  // Получаем все созданные симлинки от других лаунчеров
                    }
                  }
                }
              }
            }
          }
          Variables.GetInstance.allValuesFormCurrentLauncher =
              (await Task.WhenAll(Variables.GetInstance.allValuesFormCurrentLauncher.Select(async currentValue => {
                bool shouldRemove = Variables.GetInstance.allValuesFormOthersLaunchers.Any(otherValue => otherValue.Contains(currentValue) &&
                                                                                                         currentValue.Length < otherValue.Length);
                if (shouldRemove) {
                  Variables.GetInstance.valuesForRecheck.Add(currentValue);
                } else {
                  using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Symlinks\" + Variables.GetInstance.launcherName, true)) {
                    await FileUtils.GetInstance.ProcessSinglePathAsync(currentValue);
                    key.DeleteValue(currentValue, true);
                    // MessageBox.Show(currentValue + " Удалён на строке 855");
                  }
                }
                return new { Value = currentValue, Keep = !shouldRemove };
              })))
                  .Where(x => x.Keep)
                  .Select(x => x.Value)
                  .ToList();
          foreach (var recheck in Variables.GetInstance.valuesForRecheck) {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Symlinks\RecheckPortableValues", true)) {
              if (key == null) {
                using (RegistryKey newKey = Registry.CurrentUser.CreateSubKey(@"Software\PortableCreator\Symlinks\RecheckPortableValues")) {
                  if (newKey != null) {
                    newKey.SetValue(recheck, "", RegistryValueKind.String);
                  }
                }
              } else {
                key.SetValue(recheck, "", RegistryValueKind.String);
              }
            }
          }
          try {
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\PortableCreator\Symlinks\" + Variables.GetInstance.launcherName, false);
          } catch {
          }
          using (RegistryKey recheckRegistryKey = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Symlinks\RecheckPortableValues", true)) {
            if (recheckRegistryKey != null) {
              string[] valueNamesRecheck = recheckRegistryKey.GetValueNames();
              foreach (string valueName in valueNamesRecheck) {
                bool hasLongerVariant = Variables.GetInstance.allValuesFormOthersLaunchers.Any(otherValue => otherValue.StartsWith(valueName + "\\") &&
                                                                                                             otherValue.Length > valueName.Length);
                if (hasLongerVariant) {
                } else {
                  await FileUtils.GetInstance.ProcessSinglePathAsync(valueName);
                  recheckRegistryKey.DeleteValue(valueName, true);
                  // MessageBox.Show(valueName + " Удалён на строке 895");
                }
              }
            }
          }
          try {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Symlinks\RecheckPortableValues")) {
              if (key != null) {
                if (IsEmptyKey(key)) {
                  Registry.CurrentUser.DeleteSubKey(@"Software\PortableCreator\Symlinks\RecheckPortableValues");
                }
              }
            }
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Symlinks")) {
              if (key != null) {
                if (IsEmptyKey(key)) {
                  Registry.CurrentUser.DeleteSubKey(@"Software\PortableCreator\Symlinks");
                }
              }
            }
          } catch {
          }
          File.Delete(logFilePath);
          File.Delete(Variables.GetInstance.skippedLogFilePath);
        } catch {
        }
        // Чтение списка путей из основного файла
        if (File.Exists(appPath + @"\Data\Проводник.txt")) {
          try {
            List<string> pathsForRemove = File.ReadAllLines(appPath + @"\Data\Проводник.txt").ToList();
            foreach (var path in pathsForRemove) {
              string trimmedPath = path.Trim();
              // Проверяем, заканчивается ли строка на " > null"
              if (trimmedPath.EndsWith("> null")) {
                // Убираем " > null" и получаем путь к директории
                string directoryPath = trimmedPath.Replace(" > null", "").Trim();
                string pathEdited = FileUtils.GetInstance.PreparePath(path, userPath, appPath);
                if (!Variables.GetInstance.valuesForRecheck.Contains(pathEdited)) {
                  // Проверяем, существует ли директория и является ли она пустой
                  if (Directory.Exists(pathEdited) && FileUtils.GetInstance.IsDirectoryEmpty(pathEdited)) {
                    await ProcessPathAsync(pathEdited, userPath, appPath);
                  } else {
                    Console.WriteLine("Directory is not empty or does not exist: " + pathEdited);
                  }
                }
              } else {
                // Удаляем папку в любом случае
                await ProcessPathAsync(trimmedPath, userPath, appPath);
              }
            }
          } catch (Exception ex) {
            Console.WriteLine("Error processing paths: " + ex.Message);
          }
        }
      }
    }

    private bool IsEmptyKey(RegistryKey key) {
      if (key.GetValueNames().Length > 0) {
        return false;  // Ключ содержит значения
      }
      if (key.GetSubKeyNames().Length > 0) {
        return false;  // Ключ содержит подключи
      }
      return true;  // Ключ пустой
    }

    private async Task ProcessPathAsync(string path, string userPath, string appPath) {
      try {
        // Пропускаем комментарии
        if (path.StartsWith("#"))
          return;
        // Подготовка пути
        string pathEdited = FileUtils.GetInstance.PreparePath(path, userPath, appPath);
        // Проверка на маски (*, ?)
        if (FileUtils.GetInstance.ContainsMask(pathEdited)) {
          await FileUtils.GetInstance.ProcessPathWithMaskAsync(pathEdited);
        } else {
          await FileUtils.GetInstance.ProcessSinglePathAsync(pathEdited);
        }
      } catch {
      }
    }

    public async Task RegistryRemoveLaunchedUnits() {
      using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Running", writable: true)) {
        if (key != null) {
          try {
            key.DeleteValue(Variables.GetInstance.appPath, throwOnMissingValue: false);
          } catch {
          }
        }
      }
      using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\PortableCreator\Running", writable: false)) {
        if (key != null && key.ValueCount == 0 && key.SubKeyCount == 0) {
          key.Close();
          using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey(@"Software", writable: true)) {
            if (softwareKey != null) {
              try {
                softwareKey.DeleteSubKey("PortableCreator\\Running");
              } catch {
              }
            }
          }
        }
      }
      await Task.Delay(10);
    }

  }

  // Класс для Удаления записей реестра по файлу Удалить.txt
  class RegistryHelper {
    #region Static
    // Получить Instance класса (реализация паттерна Singleton)
    static volatile RegistryHelper thisClass;
    static object SyncObject = new object();
    public static RegistryHelper GetInstance {
        get {
            if (thisClass == null)
                lock (SyncObject) {
                    if (thisClass == null)
                        thisClass = new RegistryHelper();
                }
            return thisClass;
        }
    }
    #endregion

    private const int ERROR_SUCCESS = NativeMethods.ERROR_SUCCESS;

    private static readonly IntPtr HKEY_LOCAL_MACHINE = NativeMethods.HKEY_LOCAL_MACHINE;
    private static readonly IntPtr HKEY_CURRENT_USER = NativeMethods.HKEY_CURRENT_USER;
    private static readonly IntPtr HKEY_CLASSES_ROOT = NativeMethods.HKEY_CLASSES_ROOT;
    private static readonly IntPtr HKEY_USERS = NativeMethods.HKEY_USERS;
    private static readonly IntPtr HKEY_CURRENT_CONFIG = NativeMethods.HKEY_CURRENT_CONFIG;

    // Удаляет пользовательские ключи реестра, указанные в файле "Удалить.txt"
    public void RemoveCustomRegistryKeys(string appPath) {
        try {
            string[] allKeys = File.ReadAllLines(Path.Combine(appPath, "Data", "Реестр", "Удалить.txt"));

            foreach (var entry in allKeys) {
                var parts = entry.Split(new[] { '>' }, 2, StringSplitOptions.RemoveEmptyEntries);
                IntPtr rootKey;
                string keyPath;

                if (parts.Length == 1) {
                    keyPath = NormalizeRegistryPath(parts[0].Trim(), out rootKey);
                    if (rootKey != IntPtr.Zero) {
                        DeleteRegistryKeyRecursive(rootKey, keyPath);
                    } else {
                        Console.WriteLine("Unsupported root key in path: " + entry);
                    }
                } else if (parts.Length == 2) {
                    keyPath = NormalizeRegistryPath(parts[0].Trim(), out rootKey);
                    string valueName = parts[1].Trim();
                    if (rootKey != IntPtr.Zero) {
                        DeleteRegistryValue(rootKey, keyPath, valueName);
                    } else {
                        Console.WriteLine("Unsupported root key in path: " + entry);
                    }
                } else {
                    Console.WriteLine("Invalid format in entry: " + entry);
                }
            }
        } catch (Exception ex) {
            Console.WriteLine("Error removing registry keys: " + ex.Message);
        }
    }

    // Удаляет указанное значение из реестра
    private void DeleteRegistryValue(IntPtr rootKey, string keyPath, string valueName) {
        try {
            using (RegistryKey key = GetRegistryRoot(rootKey).OpenSubKey(keyPath, true)) {
                if (key != null) {
                    key.DeleteValue(valueName, false);
                    Console.WriteLine("Deleted value: " + valueName + " from key: " + keyPath);
                } else {
                    Console.WriteLine("Key not found: " + keyPath);
                }
            }
        } catch (Exception ex) {
            Console.WriteLine("Exception deleting value " + valueName + " from key " + keyPath + ": " + ex.Message);
        }
    }

    // Рекурсивно удаляет ключ реестра и все его подключи
    private void DeleteRegistryKeyRecursive(IntPtr rootKey, string keyPath) {
        try {
            using (RegistryKey key = GetRegistryRoot(rootKey).OpenSubKey(keyPath, true)) {
                if (key != null) {
                    foreach (string subKey in key.GetSubKeyNames()) {
                        DeleteRegistryKeyRecursive(rootKey, Path.Combine(keyPath, subKey));
                    }
                    key.Close();

                    // WinAPI для удаления ключа
                    int result = NativeMethods.RegDeleteKeyEx(rootKey, keyPath, 0, 0);
                    if (result != ERROR_SUCCESS) {
                        Console.WriteLine("Failed to delete key: " + keyPath + ", Error Code: " + result);
                    } else {
                        Console.WriteLine("Deleted key: " + keyPath);
                    }
                } else {
                    Console.WriteLine("Key not found: " + keyPath);
                }
            }
        } catch (Exception ex) {
            Console.WriteLine("Exception deleting key " + keyPath + ": " + ex.Message);
        }
    }

    // Нормализует путь к реестру, определяя корневой ключ
    private string NormalizeRegistryPath(string keyPath, out IntPtr rootKey) {
        if (keyPath.StartsWith("HKLM\\") || keyPath.StartsWith("HKEY_LOCAL_MACHINE\\")) {
            rootKey = HKEY_LOCAL_MACHINE;
            return keyPath.Replace("HKLM\\", "").Replace("HKEY_LOCAL_MACHINE\\", "");
        }
        if (keyPath.StartsWith("HKCU\\") || keyPath.StartsWith("HKEY_CURRENT_USER\\")) {
            rootKey = HKEY_CURRENT_USER;
            return keyPath.Replace("HKCU\\", "").Replace("HKEY_CURRENT_USER\\", "");
        }
        if (keyPath.StartsWith("HKCR\\") || keyPath.StartsWith("HKEY_CLASSES_ROOT\\")) {
            rootKey = HKEY_CLASSES_ROOT;
            return keyPath.Replace("HKCR\\", "").Replace("HKEY_CLASSES_ROOT\\", "");
        }
        if (keyPath.StartsWith("HKU\\") || keyPath.StartsWith("HKEY_USERS\\")) {
            rootKey = HKEY_USERS;
            return keyPath.Replace("HKU\\", "").Replace("HKEY_USERS\\", "");
        }
        if (keyPath.StartsWith("HKCC\\") || keyPath.StartsWith("HKEY_CURRENT_CONFIG\\")) {
            rootKey = HKEY_CURRENT_CONFIG;
            return keyPath.Replace("HKCC\\", "").Replace("HKEY_CURRENT_CONFIG\\", "");
        }

        rootKey = IntPtr.Zero;  // Неподдерживаемый корневой ключ
        return keyPath;
    }

    // Возвращает объект RegistryKey для указанного корневого ключа
    private RegistryKey GetRegistryRoot(IntPtr rootKey) {
        if (rootKey == HKEY_LOCAL_MACHINE)
            return Registry.LocalMachine;
        if (rootKey == HKEY_CURRENT_USER)
            return Registry.CurrentUser;
        if (rootKey == HKEY_CLASSES_ROOT)
            return Registry.ClassesRoot;
        if (rootKey == HKEY_USERS)
            return Registry.Users;
        if (rootKey == HKEY_CURRENT_CONFIG)
            return Registry.CurrentConfig;
        return null;
    }
  }

  // Класс для закрепления портативки на панели задач
  class TaskBarWorker {
    #region Static
    // Получить Instance класса (реализация паттерна Singleton)
    static volatile TaskBarWorker thisClass;
    static object SyncObject = new object();
    public static TaskBarWorker GetInstance {
        get {
            if (thisClass == null)
                lock (SyncObject) {
                    if (thisClass == null)
                        thisClass = new TaskBarWorker();
                }
            return thisClass;
        }
    }
    #endregion

    public static List<string> exes = Directory.GetFiles(Variables.GetInstance.appPath, "*.exe", SearchOption.TopDirectoryOnly).ToList();
    public static List<string> apps = new List<string>();
    public static List<string> exeArgs = new List<string>();
    public static List<string> lnks = Directory.GetFiles(Variables.GetInstance.taskbarPath, "*.lnk", SearchOption.AllDirectories).ToList();

    // Обновляет ярлыки на панели задач, заменяя их целевые пути на соответствующие приложения
    public void Runner(dynamic shell, string appPath, string taskbarPath) {
        exes.ForEach(x => {
            apps.Add(FileUtils.GetInstance.GetAppExecutablePath(x, "executablePath"));
            exeArgs.Add(FileUtils.GetInstance.GetAppExecutablePath(x, "executableArgs").Replace(x, ""));
        });

        if (lnks.Count > 0) {
            lnks.ForEach(lnk => {
                dynamic shortcut = shell.CreateShortcut(lnk);
                for (int i = 0; i < exes.Count; i++) {
                    if (exes[i] == shortcut.TargetPath) {
                        shortcut.TargetPath = apps[i].StartsWith("C:\\Program Files") ? apps[i].Replace("C:", appPath + "Data") : apps[i];
                        shortcut.Arguments = exeArgs[i];
                        shortcut.Save();
                        NativeMethods.SHChangeNotify(NativeMethods.SHCNE_ASSOCCHANGED, NativeMethods.SHCNF_PATHW | NativeMethods.SHCNF_FLUSHNOWAIT, Marshal.StringToHGlobalUni(taskbarPath), IntPtr.Zero);
                    }
                }
            });
        }
    }

    // Восстанавливает ярлыки на панели задач, возвращая их целевые пути к исходным значениям
    public void Closer(dynamic shell, string appPath, string taskbarPath) {
        lnks = Directory.GetFiles(taskbarPath, "*.lnk", SearchOption.AllDirectories).ToList();
        if (lnks.Count > 0) {
            lnks.ForEach(lnk => {
                dynamic shortcut = shell.CreateShortcut(lnk);
                for (int i = 0; i < apps.Count; i++) {
                    string targetPath = apps[i].StartsWith("C:\\Program Files") ? apps[i].Replace("C:", appPath + "Data") : apps[i];
                    if (targetPath == shortcut.TargetPath) {
                        shortcut.TargetPath = exes[i];
                        shortcut.Arguments = "";
                        shortcut.Save();
                        NativeMethods.SHChangeNotify(NativeMethods.SHCNE_ASSOCCHANGED, NativeMethods.SHCNF_PATHW | NativeMethods.SHCNF_FLUSHNOWAIT, Marshal.StringToHGlobalUni(taskbarPath), IntPtr.Zero);
                    }
                }
            });
        }
    }
  }

  // Класс для блокировки интернета и отключения запросов на сеть
  class FirewallWorker {
    #region Static
    // Получить Instance класса (реализация паттерна Singleton)
    static volatile FirewallWorker thisClass;
    static object SyncObject = new object();
    public static FirewallWorker GetInstance {
        get {
            if (thisClass == null)
                lock (SyncObject) {
                    if (thisClass == null)
                        thisClass = new FirewallWorker();
                }
            return thisClass;
        }
    }
    #endregion

    // Находит все .exe файлы в указанной директории и применяет правила брандмауэра
    public void GetExesForFirewall(string executablePath, bool networkBlocked, bool isExit) {
        string folderPath = Path.GetDirectoryName(FileUtils.GetInstance.GetRealPath(executablePath));

        if (Directory.Exists(folderPath)) {
            try {
                // Перебираем все .exe файлы в папке и её подпапках
                foreach (string exeFile in Directory.EnumerateFiles(folderPath, "*.exe", SearchOption.AllDirectories)) {
                    ProcessManager.GetInstance.RunProccess("cmd.exe", "/c netsh advfirewall firewall delete rule name=all program=\"" + exeFile + "\"");
                    if (!isExit) {
                        ProcessManager.GetInstance.RunProccess("cmd.exe", "/c netsh advfirewall firewall add rule name=\"" + exeFile + " Allow_LAN\" dir=out program=\"" +
                                                                              exeFile + "\" action=allow remoteip=LocalSubnet");
                        if (networkBlocked)
                            ProcessManager.GetInstance.RunProccess("cmd.exe", "/c netsh advfirewall firewall add rule name=\"" + exeFile +
                                                                                  " Block_Internet\" dir=out program=\"" + exeFile + "\" action=block");
                    }
                }
            } catch (UnauthorizedAccessException) {
                Console.WriteLine("Access to some files or folders is denied.");
            } catch (Exception ex) {
                Console.WriteLine("Error: " + ex.Message);
            }
        } else {
            Console.WriteLine("Folder does not exist: " + folderPath);
        }
    }
  }

  // Вспомогательный класс для работы с файлами/папками
  class FileUtils {
    #region Static
    // Получить Instance класса (реализация паттерна Singleton)
    static volatile FileUtils thisClass;
    static object SyncObject = new object();
    public static FileUtils GetInstance {
        get {
            if (thisClass == null)
                lock (SyncObject) {
                    if (thisClass == null)
                        thisClass = new FileUtils();
                }
            return thisClass;
        }
    }
    #endregion

    private const uint GENERIC_READ = NativeMethods.GENERIC_READ;
    private const uint FILE_SHARE_READ = NativeMethods.FILE_SHARE_READ;
    private const uint OPEN_EXISTING = NativeMethods.OPEN_EXISTING;
    private const uint FILE_FLAG_BACKUP_SEMANTICS = NativeMethods.FILE_FLAG_BACKUP_SEMANTICS;

    // Возвращает путь к исполняемому файлу приложения на основе данных из ресурсов
    public string GetAppExecutablePath(string targetExePath, string resourceKey) {
        try {
            Assembly assembly = Assembly.LoadFile(targetExePath);
            using (Stream resourceStream = assembly.GetManifestResourceStream("StringResource.resx")) {
                using (ResXResourceReader resxReader = new ResXResourceReader(resourceStream)) {
                    foreach (System.Collections.DictionaryEntry entry in resxReader) {
                        if (entry.Key.ToString().Substring(0, entry.Key.ToString().Length - 1) == resourceKey) {
                            if (resourceKey == "executablePath") {
                                string value = entry.Value.ToString();
                                if (!value.StartsWith("C:\\Program Files")) {
                                    value = Variables.GetInstance.appPath + value;
                                }
                                if (!File.Exists(value)) {
                                    List<string> checkForExe =
                                        Directory.GetFiles(Directory.GetParent(Path.GetDirectoryName(value)).FullName, Path.GetFileName(value), SearchOption.AllDirectories)
                                            .ToList();
                                    if (checkForExe.Count > 0) {
                                        value = checkForExe[0];
                                    }
                                }
                                return value;
                            } else {
                                return entry.Value.ToString();
                            }
                        }
                    }
                }
            }
        } catch {
        }
        return targetExePath;
    }

    // Возвращает реальный путь к файлу или директории, удаляя префиксы и используя нативные методы
    public string GetRealPath(string path) {
        IntPtr handle = IntPtr.Zero;
        try {
            // Открываем файл или директорию
            handle = NativeMethods.CreateFile(
                path,
                NativeMethods.GENERIC_READ,
                NativeMethods.FILE_SHARE_READ,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                NativeMethods.FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero
            );
            if (handle == IntPtr.Zero || handle.ToInt32() == -1) {
                throw new IOException("Failed to open file: " + path);
            }

            // Получаем реальный путь
            StringBuilder realPath = new StringBuilder(1024);
            uint result = NativeMethods.GetFinalPathNameByHandle(
                handle,
                realPath,
                (uint)realPath.Capacity,
                0
            );
            if (result == 0) {
                throw new IOException("Failed to get real path for: " + path);
            }

            // Убираем префикс "\\?\" из пути
            string finalPath = realPath.ToString();
            if (finalPath.StartsWith(@"\\?\")) {
                finalPath = finalPath.Substring(4);
            }

            return finalPath;
        } finally {
            if (handle != IntPtr.Zero) {
                NativeMethods.CloseHandle(handle);
            }
        }
    }

    // Проверяет, есть ли что-то в директории
    public bool IsDirectoryEmpty(string directoryPath) {
      try {
        // Проверяем, есть ли файлы или поддиректории
        return !Directory.EnumerateFileSystemEntries(directoryPath).Any();
      } catch {
        // Если возникла ошибка (например, нет доступа), считаем директорию непустой
        return false;
      }
    }

    public async Task ProcessPathWithMaskAsync(string path) {
      // Обработка путей с масками
      string directory = Path.GetDirectoryName(path);
      string mask = Path.GetFileName(path);
      try {
        // Удаление файлов
        foreach (var file in Directory.GetFiles(directory, mask)) {
          await Task.Run(() => File.Delete(file));
        }
        // Удаление папок
        foreach (var folder in Directory.GetDirectories(directory, mask)) {
          await Task.Run(() => Directory.Delete(folder, true));
        }
      } catch {
      }
    }

    public async Task ProcessSinglePathAsync(string path) {
      // Обработка одиночного пути
      try {
        if (Directory.Exists(path)) {
          await Task.Run(() => Directory.Delete(path, true));
        } else if (File.Exists(path)) {
          await Task.Run(() => File.Delete(path));
        }
      } catch {
      }
    }

    public void WriteAsStream(string path, string data, bool append, Encoding encoding) {
      if (append) {
        using (StreamWriter sw = new StreamWriter(path, true)) {
          sw.WriteLine(data, encoding);
        }
      } else {
        using (StreamWriter sw = new StreamWriter(path)) {
          sw.Write(data, encoding);
        }
      }
    }

    public void ProcessDirectory(string sourceDir, string targetDir, string logFilePath) {
      // Список для хранения путей, которые нужно проверить повторно
      List<string> skippedPaths = new List<string>();
      // Первый этап: Создание ссылок
      CreateSymLinks(sourceDir, targetDir, logFilePath);
      // Второй этап: Логирование пропущенных ссылок
      CheckAndLogSkippedPaths(sourceDir, targetDir, Variables.GetInstance.skippedLogFilePath);
      try {
        // Чтение всех строк из обоих файлов
        var linesFromFile1 = File.ReadAllLines(Variables.GetInstance.skippedLogFilePath);
        var linesFromFile2 = File.ReadAllLines(logFilePath);
        // Используем LINQ для фильтрации строк
        var filteredLines = linesFromFile1.Where(line => !linesFromFile2.Any(pattern => line.Contains(pattern))).ToList();
        // Записываем отфильтрованные строки обратно в первый файл
        WriteAsStream(Variables.GetInstance.skippedLogFilePath, String.Join(Environment.NewLine, filteredLines), false, Encoding.UTF8);
      } catch {
      }
    }

    private void CreateSymLinks(string sourceDir, string targetDir, string logFilePath) {
      // Получаем все файлы и поддиректории в текущей директории
      string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly);
      string[] dirs = Directory.GetDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly);
      // Проверяем файлы
      foreach (string file in files) {
        string relativePath = GetRelativePath(sourceDir, file);
        string targetFile = Path.Combine(targetDir, relativePath);
        if (!File.Exists(targetFile)) {
          string symLinkDestination = Path.Combine(targetDir, relativePath);
          Create(file, symLinkDestination, "File");
          LogSymLinkPath(symLinkDestination, logFilePath);
        }
      }
      // Проверяем папки
      foreach (string dir in dirs) {
        string relativePath = GetRelativePath(sourceDir, dir);
        string targetSubDir = Path.Combine(targetDir, relativePath);
        if (!Directory.Exists(targetSubDir)) {
          string symLinkDestination = Path.Combine(targetDir, relativePath);
          Create(dir, symLinkDestination, "Folder");
          LogSymLinkPath(symLinkDestination, logFilePath);
        }
        // Рекурсивно проверяем содержимое папки
        string nextTargetDir = Path.Combine(targetDir, relativePath);
        CreateSymLinks(dir, nextTargetDir, logFilePath);
      }
    }

    public void Create(string source, string destination, string type) {
      if (type == "File") {
        if (!File.Exists(destination)) {
          NativeMethods.CreateSymbolicLink(destination, source, NativeMethods.SymbolicLink.File);
        } else {
          if (!IsSymbolic(destination)) {
            File.Move(destination, destination + ".portable_backup");
            // MessageBox.Show("Файл уже существует по пути: " + destination + "\r Символическая ссылка не будет создана во избежание потери данных");
          }
        }
      } else {
        if (!Directory.Exists(destination)) {
          NativeMethods.CreateSymbolicLink(destination, source, NativeMethods.SymbolicLink.Directory);
        } else {
          if (!IsSymbolic(destination)) {
            Directory.Move(destination, destination + ".portable_backup");
            // MessageBox.Show("Папка уже существует по пути: " + destination + "\r Символическая ссылка не будет создана во избежание потери данных");
          }
        }
      }
    }

    public bool IsSymbolic(string path) {
      FileInfo pathInfo = new FileInfo(path);
      return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }

    private void CheckAndLogSkippedPaths(string sourceDir, string targetDir, string skippedLogFilePath) {
      // Получаем все файлы и поддиректории в текущей директории
      string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly);
      string[] dirs = Directory.GetDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly);
      // Проверяем файлы
      foreach (string file in files) {
        string relativePath = GetRelativePath(sourceDir, file);
        string targetFile = Path.Combine(targetDir, relativePath);
        if (File.Exists(targetFile)) {
          // Если файл уже существует, добавляем его в список пропущенных
          LogSkippedPath(targetFile, skippedLogFilePath);
        }
      }
      // Проверяем папки
      foreach (string dir in dirs) {
        string relativePath = GetRelativePath(sourceDir, dir);
        string targetSubDir = Path.Combine(targetDir, relativePath);
        if (Directory.Exists(targetSubDir)) {
          // Если папка уже существует, добавляем её в список пропущенных
          LogSkippedPath(targetSubDir, skippedLogFilePath);
        }
        // Рекурсивно проверяем содержимое папки
        string nextTargetDir = Path.Combine(targetDir, relativePath);
        CheckAndLogSkippedPaths(dir, nextTargetDir, skippedLogFilePath);
      }
    }

    // Метод для получения относительного пути
    private string GetRelativePath(string rootDir, string fullPath) {
      return fullPath.Substring(rootDir.Length).TrimStart(Path.DirectorySeparatorChar);
    }

    // Метод для логирования пропущенных ссылок
    private void LogSkippedPath(string skippedPath, string skippedLogFilePath) {
      WriteAsStream(skippedLogFilePath, String.Join(Environment.NewLine, skippedPath), true, Encoding.UTF8);
    }

    // Метод для логирования Успешно созданных ссылок
    private void LogSymLinkPath(string symLinkDestination, string logFilePath) {
      // Записываем путь созданной ссылки в текстовый файл
      WriteAsStream(logFilePath, symLinkDestination, true, Encoding.UTF8);
    }

    public string PreparePath(string path, string userPath, string appPath) {
      // Замена переменных и очистка пути
      path = path.ToLower();
      if (path.Contains("%appdata%\\") && !path.Contains("%appdata%\\roaming\\") && !path.Contains("%appdata%\\local\\") &&
          !path.Contains("%appdata%\\locallow\\")) {
        path = path.Replace("%appdata%\\", "%appdata%\\roaming\\");
      }
      path = path.Replace("%appdata%", userPath + "\\AppData")
                 .Replace("%localappdata%", userPath + "\\AppData\\Local")
                 .Replace("%localappdatalow%", userPath + "\\AppData\\LocalLow")
                 .Replace("%programdata%", "C:\\ProgramData")
                 .Replace("%allusersappdata%", "C:\\ProgramData")
                 .Replace("%commonprogramfiles%", "C:\\Program Files\\Common Files")
                 .Replace("%commonprogramfiles(x86)%", "C:\\Program Files (x86)\\Common Files")
                 .Replace("%public%", "C:\\Users\\Public")
                 .Replace("%userprofile%", userPath)
                 .Replace("%user%", userPath)
                 .Replace("%documents%", Variables.GetInstance.documentsPath)
                 .Replace("%videos%", Variables.GetInstance.videosPath)
                 .Replace("%desktop%", Variables.GetInstance.desktopPath)
                 .Replace("%pictures%", Variables.GetInstance.picturesPath)
                 .Replace("%downloads%", userPath + "\\downloads")
                 .Replace("%portable%", appPath)
                 .Replace("%temp%", Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine))
                 .Replace("\"", "")       // Удаление кавычек
                 .Replace("\\\\", "\\");  // Замена двойных слэшей
      return path;
    }

    public bool ContainsMask(string path) {
      // Проверка наличия маски (*, ?)
      string mask = Path.GetFileName(path);
      return mask.Contains("*") || mask.Contains("?");
    }


  }

  // Класс для удобного запуска команд и процессов
  class ProcessManager {
    #region Static
    // Получить Instance класса (реализация паттерна Singleton)
    static volatile ProcessManager thisClass;
    static object SyncObject = new object();
    public static ProcessManager GetInstance {
        get {
            if (thisClass == null)
                lock (SyncObject) {
                    if (thisClass == null)
                        thisClass = new ProcessManager();
                }
            return thisClass;
        }
    }
    #endregion

    // Запускает процесс и дожидается его завершения синхронно
    public async void RunProccess(string exePath, string arguments) {
        if (exePath.ToLower().Contains("update")) {
            await RunProccessAndWaitAsync(exePath, arguments);
            return;
        }
        var processInfo = new ProcessStartInfo {
            FileName = exePath,
            Arguments = arguments,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        using (Process process = Process.Start(processInfo)) {
            if (process != null) {
                process.WaitForExit();
            }
        }
    }

    // Запускает процесс асинхронно и ожидает его завершения
    public async Task RunProcessAsync(string exePath, string arguments) {
        if (exePath.ToLower().Contains("update")) {
            await RunProccessAndWaitAsync(exePath, arguments);
            return;
        }
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = exePath,
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false
            },
            EnableRaisingEvents = true
        };

        var tcs = new TaskCompletionSource<bool>();
        process.Exited += (sender, args) => tcs.SetResult(true);

        process.Start();
        Variables.GetInstance.cumulativeProcesses.Add(process);
        await tcs.Task;  // Не блокирует UI-поток
    }

    // Запускает процесс, отслеживает дочерние процессы и ожидает их завершения
    public async Task RunProccessAndWaitAsync(string exePath, string arguments) {
        // 1. Запоминаем все текущие процессы ДО запуска
        var processesBefore = GetProcessesSnapshot();

        // 2. Запускаем процесс
        using (Process process = Process.Start(exePath, arguments)) {
            if (process != null) {
                await process.WaitForExitAsync();  // Асинхронное ожидание

                // 3. Делаем несколько проверок с интервалами
                for (int i = 0; i < 5; i++)  // 5 попыток с интервалом
                {
                    // 4. Получаем разницу между новыми и старыми процессами
                    var newProcesses = GetNewProcesses(processesBefore);

                    if (newProcesses.Count > 0) {
                        Console.WriteLine("Найдены потенциальные дочерние процессы:");
                        foreach (var p in newProcesses) {
                            // Console.WriteLine($"- {p.ProcessName} (PID: {p.Id})");
                        }

                        // 5. Ждем завершения всех новых процессов
                        await WaitForProcessesExitAsync(newProcesses);

                        return;
                    }

                    await Task.Delay(500);  // Асинхронная задержка
                }

                Console.WriteLine("Не удалось обнаружить дочерние процессы");
            }
        }
    }

    // Ожидает завершения всех переданных процессов асинхронно
    private async Task WaitForProcessesExitAsync(List<Process> processes) {
        var tasks = processes
                        .Select(p => {
                            try {
                                return p.WaitForExitAsync();
                            } catch {
                                return Task.CompletedTask;
                            }
                        })
                        .ToList();

        await Task.WhenAll(tasks);

        // Освобождаем ресурсы
        foreach (Process p in processes) {
            try {
                p.Dispose();
            } catch {
            }
        }
    }

    // Создает снимок всех текущих процессов для последующего сравнения
    private Dictionary<int, Process> GetProcessesSnapshot() {
        return Process.GetProcesses().ToDictionary(p => p.Id, p => p);
    }

    // Находит новые процессы, которых не было в исходном снимке
    private List<Process> GetNewProcesses(Dictionary<int, Process> beforeSnapshot) {
        Variables.GetInstance.cumulativeProcesses = new List<Process>();

        foreach (Process p in Process.GetProcesses()) {
            try {
                if (!beforeSnapshot.ContainsKey(p.Id) && p.Id != Process.GetCurrentProcess().Id) {
                    Variables.GetInstance.cumulativeProcesses.Add(p);
                } else {
                    p.Dispose();
                }
            } catch { /* Игнорируем ошибки */
            }
        }

        return Variables.GetInstance.cumulativeProcesses;
    }

    // Закрывает все процессы из указанного списка
    public async Task CloseProcessesFromList(List<Process> processes) {
      await Task.Run(() => {
        foreach (var proc in processes)
        {
            if (proc == null || proc.HasExited) {
                Console.WriteLine("Процесс уже завершен или равен null.");
                continue; // Переходим к следующему процессу
            }
            try {
                IntPtr mainWindowHandle = proc.MainWindowHandle;
                if (mainWindowHandle == IntPtr.Zero) {
                    Console.WriteLine("У процесса " + proc.ProcessName + " нет главного окна. Завершение невозможно.");
                    continue; // Переходим к следующему процессу
                }
                bool result = NativeMethods.PostMessage(mainWindowHandle, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                if (result) {
                    Console.WriteLine("Сообщение WM_CLOSE успешно отправлено процессу " + proc.ProcessName + ".");
                    if (!proc.HasExited) {
                        Console.WriteLine("Ожидание завершения процесса " + proc.ProcessName + "...");
                        proc.WaitForExit(5000); // Ожидаем до 5 секунд
                        if (!proc.HasExited) {
                            Console.WriteLine("Процесс " + proc.ProcessName + " не завершился после WM_CLOSE. Принудительное завершение...");
                            proc.Kill();
                            proc.WaitForExit(); // Убедимся, что процесс завершился
                        }
                    }
                }
                else {
                    Console.WriteLine("Не удалось отправить сообщение WM_CLOSE процессу " + proc.ProcessName + ". Ошибка: " + Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Ошибка при попытке закрыть процесс " + proc.ProcessName + ": " + ex.Message);
            }
          }
      });
    }

    // Нативный запуск PowerShell через .dll
    public void PS(string command) {
      using (var ps = System.Management.Automation.PowerShell.Create()) {
        ps.AddScript(command);
        ps.Invoke();
      }
    }
  }

  // Асинхронное ожидание завершения процесса (метод расширения)
  public static class ProcessExtensions {    
    public static Task WaitForExitAsync(this Process process) {
      if (process.HasExited)
        return Task.CompletedTask;

      var tcs = new TaskCompletionSource<bool>();
      process.EnableRaisingEvents = true;
      process.Exited += (sender, args) => tcs.TrySetResult(true);
      return tcs.Task;
    }
  }

  // Класс для создания уведомлений
  class PopupManager {
    #region Static
    // Получить Instance класса (реализация паттерна Singleton)
    static volatile PopupManager thisClass;
    static object SyncObject = new object();
    public static PopupManager GetInstance {
        get {
            if (thisClass == null)
                lock (SyncObject) {
                    if (thisClass == null)
                        thisClass = new PopupManager();
                }
            return thisClass;
        }
    }
    #endregion

    public Form1 form1;

    // Создает всплывающее уведомление с настраиваемым текстом и поведением
    public async void CreatePopup(string exePath, bool isBackup) {
        form1 = Variables.GetInstance.form1;
        form1.TopMost = true;
        form1.BackColor = Color.FromArgb(24, 32, 47);
        form1.FormBorderStyle = FormBorderStyle.None;

        // Создаем PictureBox для отображения иконки
        PictureBox pb = new PictureBox {
            Size = new Size(64, 64),
            SizeMode = PictureBoxSizeMode.StretchImage,
            BackColor = Color.FromArgb(24, 32, 47),
            Location = new Point(10, 10)
        };

        try {
            Icon icon = Icon.ExtractAssociatedIcon(exePath);
            if (icon != null) {
                pb.Image = icon.ToBitmap();
            }
        } catch {
            Console.WriteLine("Can't extract icon for CreatePopup");
        }

        form1.Controls.Add(pb);

        // Создаем Label для отображения текста уведомления
        Label label = new Label {
            AutoSize = true, // Автоматическая настройка размера текста
            ForeColor = Color.White,
            Font = new Font("Microsoft Sans Serif", 10, FontStyle.Regular),
            MaximumSize = new Size(300, 0), // Ограничение ширины текста
            Location = new Point(pb.Right + 10, 10)
        };

        form1.Controls.Add(label);

        // Подписываемся на событие изменения текста для корректировки размеров формы
        label.TextChanged += async (s, a) => {
            AdjustFormSize(form1, label, pb);
            pb.Location = new Point(10, (form1.ClientSize.Height - pb.Height) / 2);
            PositionWindowBottomRight();
            await FadeInAsync(form1);
        };

        if (isBackup) {
            List<Control> clickableControls = new List<Control> { label, pb, form1 };
            clickableControls.ForEach(x => x.Click += async (sender, e) => {
                if (Variables.GetInstance.backupState == "empty") {
                    Variables.GetInstance.backupState = "inProcess";
                    label.Text = "Создаю бэкап, подожди...\r\n\r\nКогда бэкап будет готов, программа перезапустится\r\nИ Ты получишь новое уведомление.";
                    await ProcessManager.GetInstance.CloseProcessesFromList(Variables.GetInstance.cumulativeProcesses);
                } else if (Variables.GetInstance.backupState == "created") {
                    await FadeOutAsync(form1);
                    form1.Opacity = 0;
                    form1.Visible = false;
                }
            });

            if (Variables.GetInstance.backupState == "empty") {
                label.Text = "Настройка резервной копии...\r\n\r\nНажми на уведомление, когда будешь уверен,\r\nчто программа настроена, как надо.";
            }

            while (Variables.GetInstance.backupState != "created") {
                await Task.Delay(100);
            }

            if (Variables.GetInstance.backupState == "created") {
                label.Text =
                    "В папке \"Data\" появился-ZIP архив.\r\n\r\n" +
                    "Чтобы восстановить резервную копию:\r\n" +
                    "Убедись, что портативка закрыта и просто перетащи\r\n" +
                    "этот ZIP-архив на Лаунчер.\r\n\r\n" +
                    "Нажми на уведомление, чтобы скрыть его.";
            }
        } else if (Variables.GetInstance.repairingInProcess) {
            label.Text = "Восстанавливаю настройки...\r\n\r\nПодожди немного. Программа автоматически запустится.";
            while (Variables.GetInstance.repairingInProcess) {
              await Task.Delay(100);
            }
            label.Text = "Готово.\r\n\r\nМожешь пользоваться программой.";
            List<Control> clickableControls = new List<Control> { label, pb, form1 };
            clickableControls.ForEach(x => x.Click += async (sender, e) => {
                await FadeOutAsync(form1);
                form1.Opacity = 0;
                form1.Visible = false;
            });
            await Task.Delay(5000);
            await FadeOutAsync(form1);
            form1.Opacity = 0;
            form1.Visible = false;
        } else if (Variables.GetInstance.showClosePopup) {
            label.Text = Path.GetFileNameWithoutExtension(Application.ExecutablePath).Replace("Portable", "") + " closed";
        }
        
    }

    // Корректирует размеры формы в зависимости от содержимого текста и PictureBox
    private void AdjustFormSize(Form form, Label label, PictureBox pictureBox) {
        Size textSize = TextRenderer.MeasureText(label.Text, label.Font, new Size(300, 0), TextFormatFlags.WordBreak);

        int formWidth = pictureBox.Right + 10 + Math.Min(textSize.Width, 300) + 10;
        int formHeight = Math.Max(pictureBox.Bottom, textSize.Height + 20);

        form.ClientSize = new Size(formWidth, formHeight);

        pictureBox.Location = new Point(10, 10);
        label.Location = new Point(pictureBox.Right + 10, 10);
    }

    // Позиционирует форму в правом нижнем углу экрана
    void PositionWindowBottomRight() {
        Rectangle screen = Screen.PrimaryScreen.WorkingArea;
        form1.StartPosition = FormStartPosition.Manual;
        form1.Location = new Point(
            screen.Right - form1.Width - 8,
            screen.Bottom - form1.Height - 8
        );
    }

    // Плавно скрывает форму, изменяя её прозрачность
    private async Task FadeOutAsync(Form form) {
        while (form.Opacity > 0) {
            form.Opacity -= 0.1;
            await Task.Delay(50);
        }
        form.Opacity = 0;
    }

    // Плавно показывает форму, изменяя её прозрачность
    private async Task FadeInAsync(Form form) {
        form.Opacity = 0;
        while (form.Opacity < 1) {
            form.Opacity += 0.1;
            await Task.Delay(50);
        }
        form.Opacity = 1;
    }
  }

  // Класс для работы с Бэкапом портативок (Сброс до определённого состояния)
  class PortableRepairManager {
    #region Static
    // Получить Instance класса
    static volatile PortableRepairManager thisClass;
    static object SyncObject = new object();
    public static PortableRepairManager GetInstance {
        get {
            if (thisClass == null)
                lock (SyncObject) {
                    if (thisClass == null)
                        thisClass = new PortableRepairManager();
                }
            return thisClass;
        }
    }
    #endregion

    public bool needBackup = !File.Exists(Variables.GetInstance.appPath + @"\Data\" + Variables.GetInstance.launcherName + ".zip");

    public async Task StartBackupProcessAsync() {
        string sourceFolder = Variables.GetInstance.appPath + @"\Data";
        string zipFilePath = Variables.GetInstance.appPath + @"\Data\" + Variables.GetInstance.launcherName + ".zip";
        string excludeFile = Path.GetFileName(Variables.GetInstance.executablePath);
        string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        // Флаг для отслеживания успешности операции
        bool operationSuccessful = false;

        try {
            // Создаем временную папку
            Directory.CreateDirectory(tempFolder);

            // Экспортируем данные в фоновом потоке
            await Task.Run(() => CleanupEngine.GetInstance.Export(Variables.GetInstance.appPath).Wait());

            // Копируем файлы с исключениями асинхронно
            await CopyFolderWithExclusionsAsync(sourceFolder, tempFolder, excludeFile);

            // Создаем архив асинхронно
            await CreateZipWithCompressionAsync(tempFolder, zipFilePath);

            // Если все успешно, помечаем операцию как завершенную
            operationSuccessful = true;

        } catch (Exception ex) {
            // Логирование ошибок
            Console.WriteLine("Ошибка при создании бэкапа: " + ex.Message);
            throw; // Пробрасываем исключение дальше, если необходимо

        } finally {
            // Удаляем временную папку после завершения всех операций
            if (!operationSuccessful && Directory.Exists(tempFolder)) {
                // Если операция не завершилась успешно, удаляем папку синхронно
                DeleteDirectorySync(tempFolder);
            }
        }

        // Если операция завершилась успешно, удаляем папку асинхронно
        if (operationSuccessful && Directory.Exists(tempFolder)) {
            await DeleteDirectoryAsync(tempFolder);
        }

        // Устанавливаем состояние бэкапа и показываем уведомление
        Variables.GetInstance.backupState = "created";
        PopupManager.GetInstance.CreatePopup(
            exePath: Variables.GetInstance.launcherName,
            isBackup: true
        );
    }

    private async Task CopyFolderWithExclusionsAsync(string sourceFolder, string destinationFolder, string excludeFile) {
        if (File.Exists(Path.Combine(sourceFolder, excludeFile))) {
            return;
        }

        Directory.CreateDirectory(destinationFolder);

        // Используем массивы вместо коллекций для производительности
        string[] files = Directory.GetFiles(sourceFolder);
        string[] subFolders = Directory.GetDirectories(sourceFolder);

        foreach (string filePath in files) {
            string fileName = Path.GetFileName(filePath);
            if (!fileName.EndsWith("created_symlinks.txt") && !fileName.EndsWith("non_created_symlinks.txt")) {
                string destFilePath = Path.Combine(destinationFolder, fileName);
                await Task.Run(() => File.Copy(filePath, destFilePath, overwrite: true));
            }
        }

        foreach (string subFolder in subFolders) {
            string folderName = Path.GetFileName(subFolder);
            string destSubFolder = Path.Combine(destinationFolder, folderName);
            await CopyFolderWithExclusionsAsync(subFolder, destSubFolder, excludeFile);
        }
    }

    private async Task CreateZipWithCompressionAsync(string sourceFolder, string zipFilePath) {
        await Task.Run(() => {
            ZipFile.CreateFromDirectory(
                sourceFolder,
                zipFilePath,
                CompressionLevel.Fastest, // Быстрое сжатие
                includeBaseDirectory: false
            );
        });
    }

    private void DeleteDirectorySync(string path) {
        try {
            if (Directory.Exists(path)) {
                // Удаляем все файлы
                foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories)) {
                    File.SetAttributes(file, FileAttributes.Normal); // Сбрасываем атрибуты
                    File.Delete(file);
                }

                // Удаляем все подпапки
                foreach (string dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories)) {
                    Directory.Delete(dir, true);
                }

                // Удаляем корневую папку
                Directory.Delete(path, true);
            }
        } catch (Exception ex) {
            Console.WriteLine("Ошибка при удалении: " + ex.Message);
        }
    }

    private async Task DeleteDirectoryAsync(string path) {
        try {
            // Удаляем все файлы
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            foreach (string file in files) {
                await Task.Run(() => {
                    File.SetAttributes(file, FileAttributes.Normal); // Сбрасываем атрибуты
                    File.Delete(file);
                });
            }

            // Удаляем все папки
            string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
            Array.Reverse(dirs); // Удаляем снизу вверх
            foreach (string dir in dirs) {
                await Task.Run(() => Directory.Delete(dir));
            }

            // Удаляем корневую папку
            await Task.Run(() => Directory.Delete(path));
        } catch (Exception ex) {
            Console.WriteLine("Ошибка при удалении: " + ex.Message);
        }
    }

    private void StartDeleteProcess(string rootFolder, string excludeFile) {
        try {
            DeleteFilesAndFoldersWithExclusions(rootFolder, excludeFile);
            Console.WriteLine("Удаление завершено!");
        } catch (Exception ex) {
            Console.WriteLine("Ошибка при удалении: " + ex.Message);
        }
    }

    private void DeleteFilesAndFoldersWithExclusions(string folderPath, string excludeFile) {
        // Проверяем, содержит ли текущая папка файл excludeFile
        if (File.Exists(Path.Combine(folderPath, excludeFile))) {
            // Console.WriteLine($"Папка пропущена, так как содержит файл {excludeFile}: {folderPath}");
            return;  // Пропускаем эту папку
        }

        // Удаляем все файлы в текущей папке
        foreach (string filePath in Directory.GetFiles(folderPath)) {
            try {
                if (!filePath.ToLower().EndsWith("\\data\\" + Variables.GetInstance.launcherName.ToLower() + ".zip"))
                    File.Delete(filePath);
                // Console.WriteLine($"Файл удалён: {filePath}");
            } catch (Exception ex) {
                Console.WriteLine("Не удалось удалить файл " + filePath + ":" + ex.Message);
            }
        }

        // Рекурсивно удаляем подпапки
        foreach (string subFolder in Directory.GetDirectories(folderPath)) {
            DeleteFilesAndFoldersWithExclusions(subFolder, excludeFile);

            // После обработки подпапки удаляем её, если она пуста
            try {
                if (Directory.Exists(subFolder) && Directory.GetFiles(subFolder).Length == 0 && Directory.GetDirectories(subFolder).Length == 0) {
                    Directory.Delete(subFolder);
                    Console.WriteLine("Папка удалена: " + subFolder);
                }
            } catch (Exception ex) {
                Console.WriteLine("Не удалось удалить папку " + subFolder + ":" + ex.Message);
            }
        }
    }

    public async Task StartRestoreProcess() {
        string zipFilePath = Variables.GetInstance.appPath + @"\Data\" + Variables.GetInstance.launcherName + ".zip";
        string extractPath = Variables.GetInstance.appPath + @"\Data";

        StartDeleteProcess(extractPath, Path.GetFileName(Variables.GetInstance.executablePath));

        try
        {
            if (!File.Exists(zipFilePath)) {
                MessageBox.Show("Архив не найден: " + zipFilePath);
                return;
            }

            await Task.Run(() => {
                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipFilePath)) {
                        foreach (ZipArchiveEntry entry in archive.Entries) {
                            string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                            if (destinationPath.Length >= 260) {
                                destinationPath = @"\\?\" + destinationPath;
                            }
                            
                            string directory = Path.GetDirectoryName(destinationPath);
                            if (!Directory.Exists(directory)) {
                                Directory.CreateDirectory(directory);
                            }
                            if (!string.IsNullOrEmpty(entry.Name)) {
                                try {
                                    entry.ExtractToFile(destinationPath, overwrite: true);
                                }
                                catch
                                {
                                    Console.WriteLine("Отказано в доступе: " + destinationPath);
                                }
                            }
                        }
                    }
                    Variables.GetInstance.repairingInProcess = false;
                    //MessageBox.Show("Распакован");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при распаковке архива: " + ex.Message);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Критическая ошибка: " + ex.Message);
        }
    }
  }

  class InstanceChecker {
    static readonly Mutex mutex = new Mutex(false, Variables.GetInstance.launcherName.Replace(".exe", ""));
    static bool taken;
    public static bool TakeMemory() {
        return taken = mutex.WaitOne(0, true);
    }
    public static void ReleaseMemory() 
    {
        if (taken)
        try { mutex.ReleaseMutex(); } catch { }
    }
  }
}
