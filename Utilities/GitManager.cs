using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TheCloud.Commands;
using TheCloud.config;
using TheCloud.Logging;

namespace TheCloud.Utilities
{
    public static class GitManager
    {
        private const string RepoPath = @"C:\Users\user\Desktop\CloudLive";
        private static JSONStructure config => AdminCommands.GetConfig();

        public static async Task<string> GetLatestCommitHashAsync()
        {
            var info = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse HEAD",
                WorkingDirectory = RepoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(info);
            string hash = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(error))
                await BotLogger.LogEventAsync($"⚠️ GitManager: rev-parse error:\n{error}");

            await BotLogger.LogEventAsync($"🔍 GitManager: Current commit hash: {hash.Trim()}");

            return hash.Trim();
        }

        public static async Task<bool> ForceSyncRepoAsync()
        {
            await BotLogger.LogEventAsync("🔄 GitManager: Starting force sync...");

            var gitFolder = Path.Combine(RepoPath, ".git");
            if (!Directory.Exists(gitFolder))
            {
                await BotLogger.LogEventAsync($"❌ GitManager: .git folder not found at `{gitFolder}`. RepoPath may be incorrect.");
                return false;
            }

            async Task<(int exitCode, string stdout, string stderr)> RunGitCommand(string args)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"-c core.askpass=echo {args}", // prevents credential prompt hangs
                    WorkingDirectory = RepoPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,  // avoids lock if git wants input
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                string stdout = await process.StandardOutput.ReadToEndAsync();
                string stderr = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                return (process.ExitCode, stdout, stderr);
            }

            try
            {
                // 1. Fetch
                await BotLogger.LogEventAsync("🔧 Running: git fetch --all");
                var fetch = await RunGitCommand("fetch --all");

                if (!string.IsNullOrWhiteSpace(fetch.stdout))
                    await BotLogger.LogEventAsync($"📥 Git fetch output:\n{fetch.stdout}");
                if (!string.IsNullOrWhiteSpace(fetch.stderr))
                    await BotLogger.LogEventAsync($"⚠️ Git fetch error:\n{fetch.stderr}");

                if (fetch.exitCode != 0)
                {
                    await BotLogger.LogEventAsync("❌ Git fetch failed.");
                    return false;
                }

                // 2. Reset to origin/master
                await BotLogger.LogEventAsync("🔧 Running: git reset --hard origin/master");
                var reset = await RunGitCommand("reset --hard origin/master");

                if (!string.IsNullOrWhiteSpace(reset.stdout))
                    await BotLogger.LogEventAsync($"🔁 Git reset output:\n{reset.stdout}");
                if (!string.IsNullOrWhiteSpace(reset.stderr))
                    await BotLogger.LogEventAsync($"⚠️ Git reset error:\n{reset.stderr}");

                if (reset.exitCode != 0)
                {
                    await BotLogger.LogEventAsync("❌ Git reset failed.");
                    return false;
                }

                await BotLogger.LogEventAsync("✅ GitManager: Force sync completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Force sync crashed: {ex.Message}");
                return false;
            }
        }

        public static async Task<(bool Success, string TempDllPath)> BuildProjectAsync()
        {
            await BotLogger.LogEventAsync("🔧 GitManager: Starting dotnet build...");

            await Task.Delay(10000); // Wait for file system to settle

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

            // ✅ Create unique temp folder
            string tempFolder = Path.Combine(@"C:\Users\user\CloudTemp", $"Build_{DateTime.UtcNow:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(tempFolder);

            string sourceDll = Path.Combine(RepoPath, "bin", "Release", "net9.0", "TheCloud.dll");
            string tempDll = Path.Combine(tempFolder, "TheCloud.dll");

            try
            {
                File.Copy(sourceDll, tempDll, overwrite: true);
                await BotLogger.LogEventAsync($"📦 GitManager: Copied .dll to temp folder: {tempDll}");
                return (true, tempDll);
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Failed to copy .dll to temp folder: {ex.Message}");
                return (false, null);
            }
        }

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
                await BotLogger.LogEventAsync($"🚀 Relaunching from: {exePath}");
                Console.WriteLine($"🚀 Relaunching from: {exePath}");

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
                var channel = await client.GetChannelAsync(config.AnnouncementChannelID);
                await channel.SendMessageAsync($"✅ Cloud restarted successfully.\nVersion: `{commitHash}`");

                return true;
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Failed to relaunch bot: {ex.Message}");
                return false;
            }
        }
    }
}