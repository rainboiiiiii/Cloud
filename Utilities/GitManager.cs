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
        public static async Task<(bool Success, string? TempFolder)> BuildProjectAsync()
        {
            try
            {
                // Run the build
                var buildProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "build --configuration Release",
                        WorkingDirectory = RepoPath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                buildProcess.Start();
                string output = await buildProcess.StandardOutput.ReadToEndAsync();
                string error = await buildProcess.StandardError.ReadToEndAsync();
                buildProcess.WaitForExit();

                if (buildProcess.ExitCode != 0)
                {
                    await BotLogger.LogEventAsync($"❌ Build failed:\n{error}");
                    return (false, null);
                }

                // Create a unique temp folder for this build
                string tempRoot = @"C:\Users\user\CloudTemp";
                string tempFolder = Path.Combine(tempRoot, $"Build_{DateTime.UtcNow:yyyyMMdd_HHmmss}");
                Directory.CreateDirectory(tempFolder);

                // Copy build output
                string releaseDir = Path.Combine(RepoPath, "bin", "Release");
                foreach (var dir in Directory.GetDirectories(releaseDir, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(releaseDir, dir);
                    Directory.CreateDirectory(Path.Combine(tempFolder, relativePath));
                }

                foreach (var file in Directory.GetFiles(releaseDir, "*.*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(releaseDir, file);
                    string destFile = Path.Combine(tempFolder, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                    File.Copy(file, destFile, true);
                }

                // ✅ Cleanup old builds (keep last 3)
                if (Directory.Exists(tempRoot))
                {
                    var oldBuilds = new DirectoryInfo(tempRoot)
                        .GetDirectories("Build_*")
                        .OrderByDescending(d => d.CreationTimeUtc)
                        .Skip(3); // keep newest 3

                    foreach (var dir in oldBuilds)
                    {
                        try
                        {
                            dir.Delete(true);
                            await BotLogger.LogEventAsync($"🗑️ Deleted old build folder: {dir.FullName}");
                        }
                        catch (Exception ex)
                        {
                            await BotLogger.LogEventAsync($"⚠️ Failed to delete old build {dir.FullName}: {ex.Message}");
                        }
                    }
                }

                await BotLogger.LogEventAsync($"✅ Build succeeded. Output copied to: {tempFolder}");
                return (true, tempFolder);
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ BuildProjectAsync failed: {ex.Message}");
                return (false, null);
            }
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