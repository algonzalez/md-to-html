using System.IO;
using System.Linq;
using Sharprompt;
using static System.Console;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build
{
    class Program
    {
        const string OUTPUT_DIR = "./dist";
        const string PROJECT_DIR = "./src";
        const string DEFAULT_RUNTIME = "any";
        static readonly string PROJECT = $"{PROJECT_DIR}/md2html.csproj";

        static bool _isSingleFile = false;

        static void Main(string[] args)
        {
            // save colors to restore at the end
            var colors = new { fgColor = ForegroundColor, bgColor = BackgroundColor};
            void restoreColors() {
                ForegroundColor = colors.fgColor;
                BackgroundColor = colors.bgColor;
            }

            _isSingleFile = args.HasAnyMatching("--single-file");
            if (_isSingleFile) {
                args = args.RemoveAllMatching("--single-file");
            }

            Target("clean", "Delete the {project}/bin and {project}/obj directories.",
                () => DeleteDirs($"{PROJECT_DIR}/bin", $"{PROJECT_DIR}/obj"));

            Target("clean-all", "Calls clean and then also deletes the /dist directory\n",
                DependsOn("clean"),
                () => DeleteDirs(OUTPUT_DIR));

            Target("default", "Release that should run on any platform where .NET 5 runtime is installed.",
                DependsOn("clean"),
                () => Publish("any"));

            Target("prompt", "Interactive mode that will ask for available options.", () => {
                var runtime = Prompt.Select("Select target runtime"
                    , new [] {"any", "linux-x64", "macos-x64", "win10-x64"}
                    , defaultValue: DEFAULT_RUNTIME);
                _isSingleFile = (runtime != DEFAULT_RUNTIME)
                    && Prompt.Confirm("set the PublishSingleFile property to true?", defaultValue: false);
                if (Prompt.Confirm("Proceed with above values?", defaultValue: true)) {
                    Publish(runtime);
                }
            });

            Target("linux", "Self contained release targeting Linux.",
                DependsOn("clean"),
                () => Publish("linux-x64"));

            Target("macos", $"Self contained release targeting Apple macOS.",
                DependsOn("clean"),
                () => Publish("osx-x64"));

            Target("win10", "Self contained release targeting Microsoft Windows 10.",
                DependsOn("clean"),
                () => Publish("win10-x64"));

            Target("all", $"builds all the targets: default, linux, macos & win10",
                DependsOn("clean-all", "default", "linux", "macos", "win10"));

            if (args.HasAnyMatching("-h", "--help", "-?")) {
                RunTargetsWithoutExiting(args);
                // display additional usage details and features
                WriteLine();
                WriteLine("NOTES:");
                WriteLine("  - default, linux, macos & win10 targets use the 'Release' configuration");
                WriteLine("  - linux, macos & win10 targets build as --self-contained");
                WriteLine("  - use --single-file option to set the PublishSingleFile property to true;\n    available for linux, macos & win10 targets");
                restoreColors();
                System.Environment.Exit(0);
            }

            try {
                RunTargetsWithoutExiting(args);
            }
            finally {
                restoreColors();
            }
        }

        static void DeleteDirs(params string[] dirs)
        {
            foreach (var dir in dirs) {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
        }

        static void Publish(string runtime)
        {
            string outputDir = $"{OUTPUT_DIR}/{runtime}";
            DeleteDirs(outputDir);
            if (runtime == "any")
                Run("dotnet", $"publish -c Release -o {outputDir} {PROJECT}");
            else {
                Run("dotnet", $"publish -c Release -o {outputDir} -r {runtime} -p:PublishSingleFile={_isSingleFile} {PROJECT}");
            }
        }
    }

    public static class ExtensionMethods
    {
        public static bool HasAnyMatching(this string[] args, string value)
            => args.Any(arg => string.Compare(arg, value, ignoreCase: true) == 0);

        public static bool HasAnyMatching(this string[] args, params string[] values)
        {
            foreach (var value in values)
            {
                if (args.HasAnyMatching(value))
                    return true;
            }
            return false;
        }

        public static string[] RemoveAllMatching(this string[] args, string value)
            => args.Where(arg => string.Compare(arg, value, ignoreCase: true) != 0).ToArray();
    }
}
