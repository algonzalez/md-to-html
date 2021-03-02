using System.IO;
using System.Linq;
using static System.Console;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build
{
    class Program
    {
        const string OUTPUT_DIR = "./dist";
        const string PROJECT_DIR = "./src";
        static readonly string PROJECT = $"{PROJECT_DIR}/md2html.csproj";

        static void Main(string[] args)
        {
            // save colors to restore at the end
            var colors = new { fgColor = ForegroundColor, bgColor = BackgroundColor};
            void restoreColors() {
                ForegroundColor = colors.fgColor;
                BackgroundColor = colors.bgColor;
            }

            bool needsHelp = args.Any(arg => string.Compare(arg, "--help", ignoreCase: true) == 0
                || string.Compare(arg, "-h", ignoreCase: true) == 0
                || arg == "-?");

            bool isSingleFile = args.Any(arg => string.Compare(arg, "--single-file", ignoreCase: true) == 0);
            if (isSingleFile)
                args = args.Where(arg => string.Compare(arg, "--single-file", ignoreCase: true) != 0).ToArray();

            string prodOptions = isSingleFile
                ? "-p:PublishSingleFile=true"
                : "";

            Target("clean", "Delete the {project}/bin and {project}/obj directories.",
                () => DeleteDirs($"{PROJECT_DIR}/bin", $"{PROJECT_DIR}/obj"));

            Target("clean-all", "Calls clean and then also deletes the /dist directory\n",
                DependsOn("clean"),
                () => DeleteDirs(OUTPUT_DIR));

            Target("default", "Release that should run on any platform where .NET 5 runtime is installed.",
                DependsOn("clean"),
                () => {
                    var runtime = "any";
                    DeleteDirs($"{OUTPUT_DIR}/{runtime}");
                    Run("dotnet", $"publish -c Release -o {OUTPUT_DIR}/{runtime} {PROJECT}");
                });

            Target("linux", "Self contained release targetting Linux.",
                DependsOn("clean"),
                () => {
                    var runtime = "linux-x64";
                    DeleteDirs($"{OUTPUT_DIR}/{runtime}");
                    Run("dotnet", $"publish -c Release -o {OUTPUT_DIR}/{runtime} -r {runtime} {prodOptions} {PROJECT}");
                });

            Target("macos", $"Self contained release targetting Apple macOS.",
                DependsOn("clean"),
                () => {
                    var runtime = "osx-x64";
                    DeleteDirs($"{OUTPUT_DIR}/{runtime}");
                    Run("dotnet", $"publish -c Release -o {OUTPUT_DIR}/{runtime} -r {runtime} {prodOptions} {PROJECT}");
                });

            Target("win10", "Self contained release targetting Microsoft Windows 10.",
                DependsOn("clean"),
                () => {
                    var runtime = "win10-x64";
                    DeleteDirs($"{OUTPUT_DIR}/{runtime}");
                    Run("dotnet", $"publish -c Release -o {OUTPUT_DIR}/{runtime} -r {runtime} {prodOptions} {PROJECT}");
                });

            Target("all", $"builds all the targets: default, linux, macos & win10",
                DependsOn("clean-all", "default", "linux", "macos", "win10"));

            if (needsHelp) {
                RunTargetsWithoutExiting(args);
                // display additional usage details and features
                WriteLine();
                WriteLine("NOTES:");
                WriteLine("  - default, linux, macos & win10 targets build with the Release configuration");
                WriteLine("  - linux, macos & win10 targets build as --self-contained");
                WriteLine("  - the --single-file option is available for linux, macos & win10 targets");
                restoreColors();
                System.Environment.Exit(0);
            }

            RunTargetsWithoutExiting(args);
            restoreColors();
        }

        static void DeleteDirs(params string[] dirs)
        {
            foreach (var dir in dirs) {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
        }
    }
}
