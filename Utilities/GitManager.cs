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
        private const string RepoPath = @"C:\Users\user\Desktop\CloudLive";
        private static JSONStructure config => AdminCommands.GetConfig();

        // Store the last launched process so we can kill it on update
        private static Process _runningBotProcess;

        // 🔍 Get current commit hash
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

        // 🔄 Force sync repo
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
                    RedirectStandardInput = true,
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
                if (fetch.exitCode != 0) return false;

                // 2. Reset to origin/master
                await BotLogger.LogEventAsync("🔧 Running: git reset --hard origin/master");
                var reset = await RunGitCommand("reset --hard origin/master");
                if (!string.IsNullOrWhiteSpace(reset.stdout))
                    await BotLogger.LogEventAsync($"🔁 Git reset output:\n{reset.stdout}");
                if (!string.IsNullOrWhiteSpace(reset.stderr))
                    await BotLogger.LogEventAsync($"⚠️ Git reset error:\n{reset.stderr}");
                if (reset.exitCode != 0) return false;

                await BotLogger.LogEventAsync("✅ GitManager: Force sync completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ GitManager: Force sync crashed: {ex.Message}");
                return false;
            }
        }

        // 🔨 Build project and copy to temp
        public static async Task<(bool Success, string TempDllPath)> BuildProjectAsync()
        {
            await BotLogger.LogEventAsync("🔧 GitManager: Starting dotnet build...");

            // ✅ Create unique temp folder for build output
            string tempFolder = Path.Combine(@"C:\Users\user\CloudTemp", $"Build_{DateTime.UtcNow:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(tempFolder);

            var build = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build -c Release -o \"{tempFolder}\"",
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

            // ✅ Confirm the .dll exists
            string tempDll = Path.Combine(tempFolder, "TheCloud.dll");
            if (!File.Exists(tempDll))
            {
                await BotLogger.LogEventAsync($"❌ GitManager: .dll not found in temp folder: {tempDll}");
                return (false, null);
            }

            await BotLogger.LogEventAsync($"📦 GitManager: Built .dll at: {tempDll}");
            return (true, tempDll);
        }

        // 🚀 Relaunch bot safely
        public static async Task<bool> RelaunchBotAsync(string commitHash, string dllPath)
        {
            await BotLogger.LogEventAsync("🚀 GitManager: Attempting to relaunch bot...");

            try
            {
                // 🔥 Kill old process if still running
                if (_runningBotProcess != null && !_runningBotProcess.HasExited)
                {
                    try
                    {
                        await BotLogger.LogEventAsync("🛑 GitManager: Stopping old bot process...");
                        _runningBotProcess.Kill(true);
                        await _runningBotProcess.WaitForExitAsync();
                        await BotLogger.LogEventAsync("🛑 GitManager: Old bot process terminated.");
                    }
                    catch (Exception ex)
                    {
                        await BotLogger.LogEventAsync($"⚠️ GitManager: Failed to kill old process: {ex.Message}");
                    }
                }

                if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
                {
                    await BotLogger.LogEventAsync($"❌ GitManager: Provided DLL path invalid: {dllPath}");
                    return false;
                }

                await BotLogger.LogEventAsync($"🚀 Relaunching from: {dllPath}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{dllPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(dllPath)
                };

                _runningBotProcess = new Process { StartInfo = startInfo };
                _runningBotProcess.OutputDataReceived += async (s, e) => { if (e.Data != null) await BotLogger.LogEventAsync($"[BOT] {e.Data}"); };
                _runningBotProcess.ErrorDataReceived += async (s, e) => { if (e.Data != null) await BotLogger.LogEventAsync($"[BOT ERR] {e.Data}"); };

                _runningBotProcess.Start();
                _runningBotProcess.BeginOutputReadLine();
                _runningBotProcess.BeginErrorReadLine();

                await BotLogger.LogEventAsync($"✅ GitManager: Bot relaunched successfully. Version: {commitHash}");

                // Discord announcement
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