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
        public static string RepoPath = @"C:\Users\user\CloudLive";
        public static string RuntimePath = @"C:\Users\user\CloudRun";

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

        // 🔨 Build project and copy to runtime
        public static async Task<(bool Success, string RuntimeDllPath)> BuildProjectAsync()
        {
            await BotLogger.LogEventAsync("🔧 GitManager: Starting dotnet build...");

            var build = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"publish -c Release -o \"{RuntimePath}\"",
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

            try
            {
                string sourceDll = Path.Combine(RepoPath, "bin", "Release", "net9.0-windows7.0", "TheCloud.dll");
                string targetDll = Path.Combine(RuntimePath, "TheCloud.dll");

                Directory.CreateDirectory(RuntimePath);

                // ✅ Ensure bot is not running from targetDll
                if (File.Exists(targetDll))
                {
                    try
                    {
                        File.Delete(targetDll);
                        await BotLogger.LogEventAsync("🧹 GitManager: Deleted old runtime .dll before copy.");
                    }
                    catch (Exception ex)
                    {
                        await BotLogger.LogEventAsync($"❌ GitManager: Failed to delete locked .dll: {ex.Message}");
                        return (false, null);
                    }
                }

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

                // ✅ Safe announcement block
                var client = AdminCommands.GetClient();
                var config = AdminCommands.GetConfig();

                if (client == null || config == null)
                {
                    await BotLogger.LogEventAsync("❌ GitManager: Cannot announce restart—client or config is null.");
                    return true; // Relaunch succeeded, just no announcement
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
    }
}