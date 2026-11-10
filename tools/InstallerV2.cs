using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("Hi-Fi RUSH - Bộ Cài Đặt Việt Hóa")]
[assembly: AssemblyDescription("Trình cài đặt Việt hóa đầy đủ (Chữ + Phông + Overdub) cho Hi-Fi RUSH")]
[assembly: AssemblyCompany("Việt Hóa Cộng Đồng")]
[assembly: AssemblyProduct("Hi-Fi RUSH Việt Hóa")]
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]

namespace HiFiRushVietHoaInstaller
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static int Main(string[] args)
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                try { SetProcessDPIAware(); } catch { }
            }

            // Hỗ trợ chạy dòng lệnh tự động (--silent, --uninstall, --dir "...")
            bool silent = false;
            bool uninstall = false;
            string customDir = null;

            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i].ToLowerInvariant();
                if (a == "--silent" || a == "/s" || a == "-s") silent = true;
                else if (a == "--uninstall" || a == "/u" || a == "-u") uninstall = true;
                else if ((a == "--dir" || a == "-d") && i + 1 < args.Length) customDir = args[++i];
            }

            if (silent)
            {
                string targetDir = !string.IsNullOrEmpty(customDir) ? InstallerLogic.NormalizeGameDir(customDir) : InstallerLogic.DetectSteamGameDir();
                if (string.IsNullOrEmpty(targetDir) || !InstallerLogic.IsValidGameDir(targetDir))
                {
                    Console.Error.WriteLine("Khong tim thay thu muc game Hi-Fi RUSH hop le.");
                    return 1;
                }
                try
                {
                    if (uninstall)
                        InstallerLogic.Uninstall(targetDir, null);
                    else
                        InstallerLogic.Install(targetDir, null);
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Loi: " + ex.Message);
                    return 2;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(customDir));
            return 0;
        }
    }

    public static class InstallerLogic
    {
        public const string RESOURCE_NAME = "VietHoaPayload.zip";

        /// <summary>
        /// Tự động dò tìm thư mục cài đặt Hi-Fi RUSH của Steam trước tiên (qua Registry + libraryfolders.vdf + các ổ đĩa).
        /// </summary>
        public static string DetectSteamGameDir()
        {
            List<string> candidates = new List<string>();

            // 1. Đọc đường dẫn Steam từ Registry
            string steamPath = GetSteamPathFromRegistry();
            if (!string.IsNullOrEmpty(steamPath))
            {
                steamPath = steamPath.Replace('/', '\\').TrimEnd('\\');
                candidates.Add(Path.Combine(steamPath, @"steamapps\common\Hi-Fi RUSH"));

                // 2. Đọc file steamapps\libraryfolders.vdf để tìm tất cả ổ chứa thư viện Steam
                string vdfPath = Path.Combine(steamPath, @"steamapps\libraryfolders.vdf");
                if (File.Exists(vdfPath))
                {
                    try
                    {
                        string vdfContent = File.ReadAllText(vdfPath);
                        MatchCollection matches = Regex.Matches(vdfContent, "\"path\"\\s+\"([^\"]+)\"", RegexOptions.IgnoreCase);
                        foreach (Match m in matches)
                        {
                            string libPath = m.Groups[1].Value.Replace("\\\\", "\\").Replace('/', '\\').TrimEnd('\\');
                            if (!string.IsNullOrEmpty(libPath))
                            {
                                candidates.Add(Path.Combine(libPath, @"steamapps\common\Hi-Fi RUSH"));
                            }
                        }
                    }
                    catch { }
                }
            }

            // 3. Quét các thư mục SteamLibrary phổ biến trên mọi ổ đĩa cố định (C:, D:, E:, F:, G:...)
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable) continue;
                    string root = drive.RootDirectory.FullName;
                    candidates.Add(Path.Combine(root, @"SteamLibrary\steamapps\common\Hi-Fi RUSH"));
                    candidates.Add(Path.Combine(root, @"Program Files (x86)\Steam\steamapps\common\Hi-Fi RUSH"));
                    candidates.Add(Path.Combine(root, @"Program Files\Steam\steamapps\common\Hi-Fi RUSH"));
                    candidates.Add(Path.Combine(root, @"Steam\steamapps\common\Hi-Fi RUSH"));
                    candidates.Add(Path.Combine(root, @"Games\Steam\steamapps\common\Hi-Fi RUSH"));
                    candidates.Add(Path.Combine(root, @"Games\Hi-Fi RUSH"));
                    candidates.Add(Path.Combine(root, @"Hi-Fi RUSH"));
                    candidates.Add(Path.Combine(root, @"XboxGames\Hi-Fi RUSH\Content"));
                }
            }
            catch { }

            foreach (string cand in candidates)
            {
                string norm = NormalizeGameDir(cand);
                if (IsValidGameDir(norm))
                {
                    return norm;
                }
            }

            return "";
        }

        private static string GetSteamPathFromRegistry()
        {
            string[] regPaths = new string[]
            {
                @"HKEY_CURRENT_USER\Software\Valve\Steam",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam"
            };
            string[] valueNames = new string[] { "SteamPath", "InstallPath" };

            foreach (string rp in regPaths)
            {
                foreach (string vn in valueNames)
                {
                    try
                    {
                        object val = Registry.GetValue(rp, vn, null);
                        string s = val as string;
                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            return s;
                        }
                    }
                    catch { }
                }
            }
            return null;
        }

        /// <summary>
        /// Nếu người dùng chọn nhầm thư mục con (như Hibiki, Binaries, Win64, Content, Paks), tự động quy về thư mục gốc Hi-Fi RUSH.
        /// </summary>
        public static string NormalizeGameDir(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";
            try
            {
                string p = Path.GetFullPath(path.Trim().Trim('"'));
                if (IsValidGameDir(p)) return p;

                // Thử đi ngược lên tối đa 4 cấp cha nếu người dùng chọn thư mục con bên trong
                DirectoryInfo cur = new DirectoryInfo(p);
                for (int i = 0; i < 4 && cur != null; i++)
                {
                    if (IsValidGameDir(cur.FullName)) return cur.FullName;
                    cur = cur.Parent;
                }
                return p;
            }
            catch
            {
                return path.Trim();
            }
        }

        public static bool IsValidGameDir(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            try
            {
                string exePath = Path.Combine(path, @"Hibiki\Binaries\Win64\Hi-Fi-RUSH.exe");
                string paksDir = Path.Combine(path, @"Hibiki\Content\Paks");
                string utoc0 = Path.Combine(paksDir, "Hibiki-WindowsNoEditor.utoc");
                return File.Exists(exePath) && File.Exists(utoc0);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsModInstalled(string gameDir)
        {
            if (!IsValidGameDir(gameDir)) return false;
            string pak = Path.Combine(gameDir, @"Hibiki\Binaries\Win64\mods\vietnamese.hifirush.v2\assets\VietHoaV2_P.pak");
            string overdub = Path.Combine(gameDir, @"Hibiki\Binaries\Win64\overdub.dll");
            return File.Exists(pak) && File.Exists(overdub);
        }

        public static bool IsGameRunning()
        {
            try
            {
                Process[] procs = Process.GetProcessesByName("Hi-Fi-RUSH");
                return procs != null && procs.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        // State is stored beside the game, never in a developer directory.
        const string BackupName = ".viet-hoa-v2-backup.zip";
        const string ModId = "vietnamese.hifirush.v2";

        static string Digest(byte[] bytes)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        static string DigestFile(string path)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            using (var stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        static string SafePath(string root, string relative)
        {
            string prefix = Path.GetFullPath(root).TrimEnd('\\') + "\\";
            string full = Path.GetFullPath(Path.Combine(prefix, relative.Replace('/', '\\')));
            if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Đường dẫn dữ liệu không hợp lệ.");
            // Avoid writing through links/junctions into an unrelated directory.
            for (string p = full; p != null && p.Length >= prefix.Length - 1; p = Path.GetDirectoryName(p))
                if ((File.Exists(p) || Directory.Exists(p)) && (File.GetAttributes(p) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("Không cài qua liên kết thư mục: " + p);
            return full;
        }

        static void CheckTarget(string gameDir)
        {
            if (!IsValidGameDir(gameDir)) throw new IOException("Thư mục không phải game Hi-Fi RUSH.");
            if (IsGameRunning()) throw new IOException("Hãy thoát Hi-Fi RUSH trước khi cài hoặc gỡ.");
        }

        static Dictionary<string, string[]> ReadState(string backup)
        {
            var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            using (var z = ZipFile.OpenRead(backup))
            using (var reader = new StreamReader(z.GetEntry("__state.tsv").Open()))
                while (!reader.EndOfStream)
                {
                    string[] row = reader.ReadLine().Split('\t');
                    if (row.Length != 3) throw new IOException("Hồ sơ phục hồi không hợp lệ.");
                    result.Add(row[2], row);
                }
            return result;
        }

        static void VerifyInstalled(string gameDir, Dictionary<string, string[]> state)
        {
            foreach (var item in state)
            {
                string target = SafePath(gameDir, item.Key);
                string actual = File.Exists(target) ? DigestFile(target) : "-";
                if (actual != item.Value[0])
                    throw new IOException("File đã đổi sau khi cài; giữ nguyên để tránh mất dữ liệu: " + item.Key);
            }
        }

        static void Restore(string gameDir, string backup, IEnumerable<string> paths)
        {
            using (var z = ZipFile.OpenRead(backup))
                foreach (string relative in paths)
                {
                    string target = SafePath(gameDir, relative);
                    var entry = z.GetEntry(relative);
                    if (entry == null) { if (File.Exists(target)) File.Delete(target); }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(target));
                        entry.ExtractToFile(target, true);
                    }
                }
        }

        static byte[] MergeConfig(string path, byte[] fallback)
        {
            if (!File.Exists(path)) return fallback;
            var lines = new List<string>(File.ReadAllLines(path));
            var output = new List<string>();
            string section = "";
            bool added = false;
            foreach (string line in lines)
            {
                string t = line.Trim();
                if (t.StartsWith("[") && t.EndsWith("]"))
                {
                    section = t.ToLowerInvariant();
                    output.Add(line);
                    if (section == "[mod_order]") { output.Add(ModId); added = true; }
                    continue;
                }
                if ((section == "[mod_order]" || section == "[disabled_mods]") &&
                    (t == ModId || t == "vietnamese.hifirush" || t == "vietnamese.hifirush.v2pilot")) continue;
                // Preserve other mod settings and the user's loader configuration.
                output.Add(line);
            }
            if (!added) { output.Add("[mod_order]"); output.Add(ModId); }
            return System.Text.Encoding.UTF8.GetBytes(string.Join("\r\n", output) + "\r\n");
        }

        public static void Install(string gameDir, Action<int, string> onProgress)
        {
            CheckTarget(gameDir);
            string win = SafePath(gameDir, "Hibiki/Binaries/Win64");
            string backup = SafePath(gameDir, BackupName);
            string pending = backup + ".pending";
            if (File.Exists(pending)) throw new IOException("Có bản sao lưu cài đặt chưa hoàn tất: " + pending);
            if (File.Exists(backup))
            {
                var installed = ReadState(backup);
                VerifyInstalled(gameDir, installed);
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCE_NAME))
                using (var z = new ZipArchive(stream, ZipArchiveMode.Read))
                    foreach (var entry in z.Entries)
                    {
                        if (entry.FullName == "overdub-config.ini") continue;
                        string relative = "Hibiki/Binaries/Win64/" + entry.FullName;
                        using (var data = new MemoryStream())
                        using (var input = entry.Open())
                        {
                            input.CopyTo(data);
                            if (!installed.ContainsKey(relative) || installed[relative][0] != Digest(data.ToArray()))
                                throw new IOException("Bản cài hiện tại khác bộ cài này. Hãy gỡ bằng bộ cài cũ trước khi cập nhật.");
                        }
                    }
                if (onProgress != null) onProgress(100, "Bản v2 đã cài đầy đủ; không cần cài lại.");
                return;
            }
            var payload = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCE_NAME))
            using (var z = new ZipArchive(stream, ZipArchiveMode.Read))
                foreach (var entry in z.Entries)
                {
                    using (var data = new MemoryStream())
                    using (var input = entry.Open())
                    {
                        input.CopyTo(data);
                        payload.Add("Hibiki/Binaries/Win64/" + entry.FullName, data.ToArray());
                    }
                }
            string config = "Hibiki/Binaries/Win64/overdub-config.ini";
            payload[config] = MergeConfig(SafePath(gameDir, config), payload[config]);
            // Remove only known competing localization files, retaining their original bytes.
            foreach (string old in new[] { "vietnamese.hifirush", "vietnamese.hifirush.v2pilot" })
            {
                string folder = SafePath(gameDir, "Hibiki/Binaries/Win64/mods/" + old);
                if (Directory.Exists(folder))
                    foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
                        payload.Add(file.Substring(Path.GetFullPath(gameDir).TrimEnd('\\').Length + 1).Replace('\\', '/'), null);
            }
            foreach (string ext in new[] { ".pak", ".sig", ".utoc", ".ucas" })
            {
                string name = "Hibiki/Content/Paks/Hibiki-WindowsNoEditor_9999_P" + ext;
                if (File.Exists(SafePath(gameDir, name))) payload.Add(name, null);
            }
            var changes = new List<string>();
            // Create a complete backup before the first game file is changed.
            try
            {
                using (var z = ZipFile.Open(pending, ZipArchiveMode.Create))
                {
                    var state = new System.Text.StringBuilder();
                    foreach (var item in payload)
                    {
                        string target = SafePath(gameDir, item.Key);
                        bool exists = File.Exists(target);
                        if (Directory.Exists(target)) throw new IOException("Đường dẫn bị thư mục khác chiếm: " + item.Key);
                        if (exists) z.CreateEntryFromFile(target, item.Key, CompressionLevel.Optimal);
                        state.Append(item.Value == null ? "-" : Digest(item.Value)).Append('\t').Append(exists ? "1" : "0").Append('\t').Append(item.Key).Append('\n');
                    }
                    using (var writer = new StreamWriter(z.CreateEntry("__state.tsv").Open())) writer.Write(state.ToString());
                }
            }
            catch { if (File.Exists(pending)) File.Delete(pending); throw; }
            try
            {
                foreach (var item in payload)
                {
                    string target = SafePath(gameDir, item.Key);
                    changes.Add(item.Key);
                    if (item.Value == null) File.Delete(target);
                    else { Directory.CreateDirectory(Path.GetDirectoryName(target)); File.WriteAllBytes(target, item.Value); }
                    if (onProgress != null) onProgress(changes.Count * 95 / payload.Count, "Đang cài bản Việt hóa v2...");
                }
                VerifyInstalled(gameDir, ReadState(pending));
                File.Move(pending, backup);
            }
            catch (Exception ex)
            {
                try { Restore(gameDir, pending, changes); File.Delete(pending); }
                catch (Exception recovery) { throw new IOException("Cài lỗi; giữ bản sao lưu để phục hồi: " + pending + ". " + recovery.Message, ex); }
                throw;
            }
            if (onProgress != null) onProgress(100, "Đã cài v2. Chọn ngôn ngữ chữ English trong game.");
        }

        public static void Uninstall(string gameDir, Action<int, string> onProgress)
        {
            CheckTarget(gameDir);
            string backup = SafePath(gameDir, BackupName);
            if (!File.Exists(backup)) throw new IOException("Không có hồ sơ cài v2; không tự xóa file của bản khác.");
            var state = ReadState(backup);
            VerifyInstalled(gameDir, state);
            Restore(gameDir, backup, state.Keys);
            File.Delete(backup);
            if (onProgress != null) onProgress(100, "Đã khôi phục các file về trạng thái trước khi cài v2.");
        }

    }

    public class MainForm : Form
    {
        private TextBox txtGamePath;
        private Button btnBrowse;
        private Label lblStatus;
        private ProgressBar progressBar;
        private Label lblProgressText;
        private Button btnInstall;
        private Button btnUninstall;
        private bool isBusy = false;

        public MainForm(string initialDir)
        {
            InitializeComponent();
            string detected = !string.IsNullOrEmpty(initialDir)
                ? InstallerLogic.NormalizeGameDir(initialDir)
                : InstallerLogic.DetectSteamGameDir();

            txtGamePath.Text = detected;
            UpdatePathValidation();
        }

        private void InitializeComponent()
        {
            this.Text = "Hi-Fi RUSH — Bộ Cài Đặt Việt Hóa (v2.0)";
            this.ClientSize = new Size(660, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(22, 22, 28);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            // Header Banner
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 92,
                BackColor = Color.FromArgb(245, 190, 15)
            };
            headerPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    headerPanel.ClientRectangle,
                    Color.FromArgb(255, 200, 15),
                    Color.FromArgb(235, 95, 20),
                    15f))
                {
                    e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
                }
                // Đường viền đen đậm phong cách comic của Hi-Fi RUSH
                using (Pen p = new Pen(Color.FromArgb(15, 15, 20), 4))
                {
                    e.Graphics.DrawLine(p, 0, headerPanel.Height - 2, headerPanel.Width, headerPanel.Height - 2);
                }
            };

            Label lblTitle = new Label
            {
                Text = "HI-FI RUSH — VIỆT HÓA TOÀN DIỆN",
                Font = new Font("Segoe UI Black", 17f, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 18, 22),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(22, 16)
            };

            Label lblSubtitle = new Label
            {
                Text = "Bản dịch hiệu đính 8.509 dòng + Font tiếng Việt Google Sans Flex + Overdub Loader",
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 35, 42),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(24, 54)
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            this.Controls.Add(headerPanel);

            // Đường dẫn thư mục game
            Label lblSectionPath = new Label
            {
                Text = "THƯ MỤC CÀI ĐẶT GAME (TỰ ĐỘNG NHẬN DIỆN STEAM):",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 205, 45),
                AutoSize = true,
                Location = new Point(24, 114)
            };
            this.Controls.Add(lblSectionPath);

            txtGamePath = new TextBox
            {
                Location = new Point(24, 140),
                Size = new Size(476, 30),
                Font = new Font("Segoe UI", 10.5f),
                BackColor = Color.FromArgb(36, 36, 46),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtGamePath.TextChanged += (s, e) => UpdatePathValidation();
            this.Controls.Add(txtGamePath);

            btnBrowse = new Button
            {
                Text = "Chọn thư mục...",
                Location = new Point(510, 138),
                Size = new Size(126, 31),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 52, 66),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 110);
            btnBrowse.Click += BtnBrowse_Click;
            this.Controls.Add(btnBrowse);

            lblStatus = new Label
            {
                Location = new Point(24, 178),
                Size = new Size(612, 24),
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 230, 130),
                Text = "Đang kiểm tra đường dẫn..."
            };
            this.Controls.Add(lblStatus);

            // Khung thông tin hướng dẫn nhanh
            Panel infoBox = new Panel
            {
                Location = new Point(24, 210),
                Size = new Size(612, 82),
                BackColor = Color.FromArgb(30, 30, 38),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblInfo = new Label
            {
                Location = new Point(12, 10),
                Size = new Size(588, 62),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(205, 205, 215),
                Text = "• Bộ cài tự động tích hợp toàn bộ dữ liệu Việt hóa, phông chữ tiếng Việt và Overdub mod loader.\r\n" +
                       "• Sau khi cài đặt, bạn chỉ cần mở game bình thường qua Steam (không cần thêm Launch Options).\r\n" +
                       "• Trong mục Options của game, giữ ngôn ngữ văn bản là English để hiển thị Tiếng Việt."
            };
            infoBox.Controls.Add(lblInfo);
            this.Controls.Add(infoBox);

            // Thanh tiến trình
            lblProgressText = new Label
            {
                Location = new Point(24, 304),
                Size = new Size(612, 20),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(190, 190, 205),
                Text = "Sẵn sàng cài đặt."
            };
            this.Controls.Add(lblProgressText);

            progressBar = new ProgressBar
            {
                Location = new Point(24, 327),
                Size = new Size(612, 18),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };
            this.Controls.Add(progressBar);

            // Nút Cài Đặt & Gỡ Cài Đặt
            btnInstall = new Button
            {
                Text = "CÀI ĐẶT VIỆT HÓA",
                Location = new Point(24, 360),
                Size = new Size(430, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 185, 15),
                ForeColor = Color.FromArgb(18, 18, 22),
                Font = new Font("Segoe UI Black", 12f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += BtnInstall_Click;
            this.Controls.Add(btnInstall);

            btnUninstall = new Button
            {
                Text = "Gỡ Việt hóa",
                Location = new Point(468, 360),
                Size = new Size(168, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(48, 48, 60),
                ForeColor = Color.FromArgb(230, 110, 110),
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUninstall.FlatAppearance.BorderColor = Color.FromArgb(110, 65, 65);
            btnUninstall.Click += BtnUninstall_Click;
            this.Controls.Add(btnUninstall);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            if (isBusy) return;
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Chọn thư mục cài đặt game Hi-Fi RUSH (ví dụ: ...\\steamapps\\common\\Hi-Fi RUSH)";
                if (Directory.Exists(txtGamePath.Text))
                {
                    fbd.SelectedPath = txtGamePath.Text;
                }
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    txtGamePath.Text = InstallerLogic.NormalizeGameDir(fbd.SelectedPath);
                }
            }
        }

        private void UpdatePathValidation()
        {
            string norm = InstallerLogic.NormalizeGameDir(txtGamePath.Text);
            bool valid = InstallerLogic.IsValidGameDir(norm);

            if (valid)
            {
                bool installed = InstallerLogic.IsModInstalled(norm);
                if (installed)
                {
                    lblStatus.ForeColor = Color.FromArgb(90, 230, 135);
                    lblStatus.Text = "✓ Đã tìm thấy game Hi-Fi RUSH (Đang có sẵn bản Việt hóa — bấm Cài đặt để cập nhật).";
                    btnInstall.Text = "CẬP NHẬT / CÀI LẠI VIỆT HÓA";
                    btnUninstall.Enabled = !isBusy;
                }
                else
                {
                    lblStatus.ForeColor = Color.FromArgb(90, 230, 135);
                    lblStatus.Text = "✓ Đã tìm thấy thư mục game Hi-Fi RUSH hợp lệ.";
                    btnInstall.Text = "CÀI ĐẶT VIỆT HÓA";
                    btnUninstall.Enabled = !isBusy;
                }
                btnInstall.Enabled = !isBusy;
                btnInstall.BackColor = Color.FromArgb(245, 185, 15);
            }
            else
            {
                lblStatus.ForeColor = Color.FromArgb(255, 120, 100);
                lblStatus.Text = "⚠ Không tìm thấy Hi-Fi-RUSH.exe ở đường dẫn này. Hãy bấm \"Chọn thư mục...\" tới nơi cài game.";
                btnInstall.Enabled = false;
                btnInstall.BackColor = Color.FromArgb(90, 90, 100);
                btnUninstall.Enabled = false;
            }
        }

        private void SetBusy(bool busy)
        {
            isBusy = busy;
            txtGamePath.Enabled = !busy;
            btnBrowse.Enabled = !busy;
            UpdatePathValidation();
        }

        private async void BtnInstall_Click(object sender, EventArgs e)
        {
            string gameDir = InstallerLogic.NormalizeGameDir(txtGamePath.Text);
            if (!InstallerLogic.IsValidGameDir(gameDir)) return;

            SetBusy(true);
            progressBar.Value = 0;

            try
            {
                await Task.Run(() =>
                {
                    InstallerLogic.Install(gameDir, (pct, msg) =>
                    {
                        this.BeginInvoke((Action)(() =>
                        {
                            progressBar.Value = Math.Max(0, Math.Min(100, pct));
                            lblProgressText.Text = msg;
                        }));
                    });
                });

                SetBusy(false);
                MessageBox.Show(
                    this,
                    "Cài đặt bản Việt hóa Hi-Fi RUSH thành công!\n\n" +
                    "Thư mục game: " + gameDir + "\n\n" +
                    "Bạn có thể mở game trực tiếp từ Steam để trải nghiệm ngay.",
                    "Hoàn tất cài đặt",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SetBusy(false);
                lblProgressText.Text = "Lỗi: " + ex.Message;
                MessageBox.Show(
                    this,
                    "Không thể hoàn tất cài đặt:\n" + ex.Message,
                    "Lỗi cài đặt",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void BtnUninstall_Click(object sender, EventArgs e)
        {
            string gameDir = InstallerLogic.NormalizeGameDir(txtGamePath.Text);
            if (!InstallerLogic.IsValidGameDir(gameDir)) return;

            DialogResult dr = MessageBox.Show(
                this,
                "Bạn có chắc chắn muốn gỡ bản Việt hóa và phục hồi các file về trạng thái trước khi cài v2 không?",
                "Xác nhận gỡ cài đặt",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            SetBusy(true);
            progressBar.Value = 0;

            try
            {
                await Task.Run(() =>
                {
                    InstallerLogic.Uninstall(gameDir, (pct, msg) =>
                    {
                        this.BeginInvoke((Action)(() =>
                        {
                            progressBar.Value = Math.Max(0, Math.Min(100, pct));
                            lblProgressText.Text = msg;
                        }));
                    });
                });

                SetBusy(false);
                MessageBox.Show(
                    this,
                    "Đã gỡ bản Việt hóa thành công! Các file đã được phục hồi về trạng thái trước khi cài v2.",
                    "Hoàn tất gỡ cài đặt",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SetBusy(false);
                lblProgressText.Text = "Lỗi: " + ex.Message;
                MessageBox.Show(
                    this,
                    "Không thể gỡ cài đặt:\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
