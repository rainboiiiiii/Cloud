using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheCloud.Commands;
using TheCloud.config;
using TheCloud.Logging;

namespace TheCloud.Utilities
{
    public static class GitManager
    {
        public static string RepoPath = @"C:\Users\user\Desktop\CloudLive";
        public static string RuntimePath = @"C:\Users\user\CloudRuntime";

        private static JSONStructure config => AdminCommands.GetConfig();

        // Store the last launched process so we can kill it on update
        private static Process _runningBotProcess;

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

            var reset = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "reset --hard origin/master",
                WorkingDirectory = RepoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            try
            {
                using var fetchProcess = Process.Start(fetch);
                await fetchProcess.StandardOutput.ReadToEndAsync();
                await fetchProcess.StandardError.ReadToEndAsync();
                fetchProcess.WaitForExit();

                using var resetProcess = Process.Start(reset);
                await resetProcess.StandardOutput.ReadToEndAsync();
                await resetProcess.StandardError.ReadToEndAsync();
                resetProcess.WaitForExit();

                return resetProcess.ExitCode == 0;
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Git sync failed: {ex.Message}");
                return false;
            }
        }

        // 🔨 Build project and copy to temp
        public static async Task<(bool Success, string RuntimeDllPath)> BuildProjectAsync()
        {
            await BotLogger.LogEventAsync("🔧 GitManager: Starting dotnet build...");

            var build = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build -c Release",
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

            // ✅ Copy built .dll to CloudRun
            try
            {
                string sourceDll = Path.Combine(RepoPath, "bin", "Release", "net9.0-windows7.0", "TheCloud.dll");
                string targetDll = Path.Combine(RuntimePath, "bin", "Release", "net9.0-windows7.0", "TheCloud.dll");

                Directory.CreateDirectory(RuntimePath);
                File.Copy(sourceDll, targetDll, overwrite: true);

                await BotLogger.LogEventAsync($"📦 GitManager: Copied .dll to runtime folder: {targetDll}");
                return (true, targetDll);
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Failed to copy .dll to runtime: {ex.Message}");
                return (false, null);
            }
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
                    Arguments = $"/c start dotnet \"{exePath}\"",
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(exePath),
                    WindowStyle = ProcessWindowStyle.Normal
                });

                await BotLogger.LogEventAsync($"✅ GitManager: Bot relaunched successfully. Version: {commitHash}");

                var client = AdminCommands.GetClient();
                var channel = await client.GetChannelAsync(AdminCommands.GetConfig().AnnouncementChannelID);
                await channel.SendMessageAsync($"✅ Cloud restarted successfully.\nVersion: `{commitHash}`");

                return true;
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Failed to relaunch bot: {ex.Message}");
                return false;
            }
        }

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
    }
}