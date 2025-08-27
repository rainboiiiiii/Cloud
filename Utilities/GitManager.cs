using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheCloud.Commands;
using TheCloud.config;
using TheCloud.Logging;
using TheCloud.Logging.BotLogger;

namespace TheCloud.Utilities
{
    public static class GitManager
    {
        public static string RepoPath = @"C:\Users\user\CloudLive";
        public static string RuntimeBasePath = @"C:\Users\user"; // base path for dynamic folders

        private static JSONStructure config => AdminCommands.GetConfig();

        // 🔄 Force sync repo
        public static async Task<bool> ForceSyncRepoAsync()
        {
            await BotLogger.LogEventAsync("📥 GitManager: Starting git fetch + reset...");

            var fetch = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "fetch --all",
                WorkingDirectory = RepoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            string defaultBranch = "master";

            var reset = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"reset --hard origin/{defaultBranch}",
                WorkingDirectory = RepoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            try
            {
                using var fetchProcess = Process.Start(fetch);
                string fetchOut = await fetchProcess.StandardOutput.ReadToEndAsync();
                string fetchErr = await fetchProcess.StandardError.ReadToEndAsync();
                fetchProcess.WaitForExit();

                using var resetProcess = Process.Start(reset);
                string resetOut = await resetProcess.StandardOutput.ReadToEndAsync();
                string resetErr = await resetProcess.StandardError.ReadToEndAsync();
                resetProcess.WaitForExit();

                await BotLogger.LogEventAsync($"📤 GitManager: fetch exit code = {fetchProcess.ExitCode}");
                await BotLogger.LogEventAsync($"📤 GitManager: reset exit code = {resetProcess.ExitCode}");
                await BotLogger.LogEventAsync($"📤 GitManager: reset stdout:\n{resetOut}");
                await BotLogger.LogEventAsync($"📤 GitManager: reset stderr:\n{resetErr}");

                return resetProcess.ExitCode == 0;
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Git sync failed: {ex.Message}");
                return false;
            }
        }

        
           // 🔨 Build project and publish to a new runtime folder
            public static async Task<(bool Success, string RuntimeDllPath)> BuildProjectAsync()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string newRuntimePath = Path.Combine(RuntimeBasePath, $"CloudRun_{timestamp}");

            // ✅ Create the folder first
            Directory.CreateDirectory(newRuntimePath);
            await BotLogger.LogEventAsync($"🔧 GitManager: Publishing to new runtime folder: {newRuntimePath}");

            // ✅ Copy config.enc after folder exists
            string configPath = Path.Combine(RepoPath, "config.enc");
            string targetPath = Path.Combine(newRuntimePath, "config.enc");

            if (File.Exists(configPath))
            {
                File.Copy(configPath, targetPath, overwrite: true);
                await BotLogger.LogEventAsync($"📦 GitManager: Copied config.enc to runtime folder: {targetPath}");
            }
            else
            {
                await BotLogger.LogEventAsync($"⚠️ GitManager: config.enc not found at {configPath}");
            }

            var build = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"publish -c Release -o \"{newRuntimePath}\"",
                WorkingDirectory = RepoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(build);
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            await BotLogger.LogEventAsync($"🔧 GitManager: build output:\n{output}");
            if (!string.IsNullOrWhiteSpace(error))
                await BotLogger.LogEventAsync($"⚠️ GitManager: build error:\n{error}");

            if (process.ExitCode != 0)
                return (false, null);

            string dllPath = Path.Combine(newRuntimePath, "TheCloud.dll");

            if (!File.Exists(dllPath))
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Published .dll not found at {dllPath}");
                return (false, null);
            }

            await BotLogger.LogEventAsync($"📦 GitManager: Published .dll ready at: {dllPath}");
            return (true, dllPath);
        }

        // 🚀 Relaunch bot safely
        public static async Task<bool> RelaunchBotAsync(string commitHash, string exePath)
        {
            await BotLogger.LogEventAsync("🚀 GitManager: Attempting to relaunch bot...");

            if (!File.Exists(exePath))
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Executable not found at {exePath}");
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"Cloud Bot\" cmd /k dotnet \"{exePath}\"",
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(exePath),
                    WindowStyle = ProcessWindowStyle.Normal
                });

                await BotLogger.LogEventAsync($"✅ GitManager: Bot relaunched successfully. Version: {commitHash}");

                var client = AdminCommands.GetClient();
                var config = AdminCommands.GetConfig();

                if (client == null || config == null)
                {
                    await BotLogger.LogEventAsync("❌ GitManager: Cannot announce restart—client or config is null.");
                    return true;
                }

                try
                {
                    var channel = await client.GetChannelAsync(config.AnnouncementChannelID);
                    await channel.SendMessageAsync($"✅ Cloud restarted successfully.\nVersion: `{commitHash}`");
                }
                catch (Exception ex)
                {
                    await BotLogger.LogEventAsync($"⚠️ GitManager: Failed to send restart message: {ex.Message}");
                }

                CleanupOldRuntimes(RuntimeBasePath, keepLatest: 3);
                return true;
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Failed to relaunch bot: {ex.Message}");
                return false;
            }
        }

        // 🔍 Get latest commit hash
        public static async Task<string> GetLatestCommitHashAsync()
        {
            var hashCmd = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse HEAD",
                WorkingDirectory = RepoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var process = Process.Start(hashCmd);
            string hash = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            return hash.Trim();
        }

        // 🧹 Cleanup old runtime folders
        public static void CleanupOldRuntimes(string basePath, int keepLatest = 3)
        {
            try
            {
                var folders = Directory.GetDirectories(basePath, "CloudRun_*")
                    .OrderByDescending(f => f)
                    .Skip(keepLatest);

                foreach (var folder in folders)
                {
                    try
                    {
                        Directory.Delete(folder, true);
                        BotLogger.LogEventAsync($"🧹 GitManager: Deleted old runtime folder: {folder}");
                    }
                    catch (Exception ex)
                    {
                        BotLogger.LogEventAsync($"⚠️ GitManager: Failed to delete old folder {folder}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                BotLogger.LogEventAsync($"⚠️ GitManager: Cleanup failed: {ex.Message}");
            }
        }
    }
}