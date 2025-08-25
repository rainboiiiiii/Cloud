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

        public static async Task<bool> ForceSyncRepoAsync()
        {
            await BotLogger.LogEventAsync("🔄 GitManager: Starting force sync...");

            var fetchInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "fetch --all",
                WorkingDirectory = RepoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            var resetInfo = new ProcessStartInfo
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
                using var fetchProcess = Process.Start(fetchInfo);
                string fetchOutput = await fetchProcess.StandardOutput.ReadToEndAsync();
                string fetchError = await fetchProcess.StandardError.ReadToEndAsync();
                await fetchProcess.WaitForExitAsync();

                await BotLogger.LogEventAsync($"📥 Git fetch output:\n{fetchOutput}");
                if (!string.IsNullOrWhiteSpace(fetchError))
                    await BotLogger.LogEventAsync($"⚠️ Git fetch error:\n{fetchError}");

                using var resetProcess = Process.Start(resetInfo);
                string resetOutput = await resetProcess.StandardOutput.ReadToEndAsync();
                string resetError = await resetProcess.StandardError.ReadToEndAsync();
                await resetProcess.WaitForExitAsync();

                await BotLogger.LogEventAsync($"🔁 Git reset output:\n{resetOutput}");
                if (!string.IsNullOrWhiteSpace(resetError))
                    await BotLogger.LogEventAsync($"⚠️ Git reset error:\n{resetError}");

                return resetProcess.ExitCode == 0;
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Force sync failed: {ex.Message}");
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