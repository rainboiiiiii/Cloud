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
        private const string RepoPath = "C:\\Users\\user\\CloudLive";
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

        public static async Task<bool> PullLatestAsync()
        {
            await BotLogger.LogEventAsync("🔄 GitManager: Starting git pull...");

            // ✅ Step 1: Pre-pull diagnostics
            var statusInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "status --short",
                WorkingDirectory = RepoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (var statusProcess = Process.Start(statusInfo))
            {
                string statusOutput = await statusProcess.StandardOutput.ReadToEndAsync();
                string statusError = await statusProcess.StandardError.ReadToEndAsync();
                await statusProcess.WaitForExitAsync();

                await BotLogger.LogEventAsync($"📋 GitManager: git status output:\n{statusOutput}");
                if (!string.IsNullOrWhiteSpace(statusError))
                    await BotLogger.LogEventAsync($"⚠️ GitManager: git status error:\n{statusError}");
            }

            // ✅ Step 2: Git pull with timeout
            var pullInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "pull --verbose --no-edit --no-rebase",
                WorkingDirectory = RepoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var pullProcess = Process.Start(pullInfo);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)); // ⏱ 60-second timeout

            try
            {
                await pullProcess.WaitForExitAsync(cts.Token);

                string pullOutput = await pullProcess.StandardOutput.ReadToEndAsync();
                string pullError = await pullProcess.StandardError.ReadToEndAsync();

                await BotLogger.LogEventAsync($"🔄 GitManager: git pull output:\n{pullOutput}");
                if (!string.IsNullOrWhiteSpace(pullError))
                    await BotLogger.LogEventAsync($"⚠️ GitManager: git pull error:\n{pullError}");

                await BotLogger.LogEventAsync($"🔚 GitManager: git pull exit code: {pullProcess.ExitCode}");

                return pullProcess.ExitCode == 0;
            }
            catch (OperationCanceledException)
            {
                await BotLogger.LogEventAsync("⏱ GitManager: git pull timed out after 60 seconds.");
                return false;
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ GitManager: git pull failed: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> BuildProjectAsync()
        {
            await BotLogger.LogEventAsync("🔧 GitManager: Starting dotnet build...");

            await Task.Delay(10000); // Wait 10 seconds for file system to settle

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

            return process.ExitCode == 0;
        }

        public static async Task<bool> RelaunchBotAsync(string commitHash)
        {
            await BotLogger.LogEventAsync("🚀 GitManager: Attempting to relaunch bot...");

            string exePath = Path.Combine(RepoPath, "bin", "Release", "net9.0", "TheCloud.dll");

            if (!File.Exists(exePath))
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Executable not found at {exePath}");
                return false;
            }

            try
            {
                // ✅ Log the path being launched
                await BotLogger.LogEventAsync($"🚀 Relaunching from: {exePath}");
                Console.WriteLine($"🚀 Relaunching from: {exePath}");

                // ✅ Launch in a new console window
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start dotnet \"{exePath}\"",
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(exePath),
                    WindowStyle = ProcessWindowStyle.Normal
                });

                await BotLogger.LogEventAsync($"✅ GitManager: Bot relaunched successfully. Version: {commitHash}");

                // ✅ Auto-ping announcement channel
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