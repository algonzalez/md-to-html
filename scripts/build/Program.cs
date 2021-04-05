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
        const string BUILD_SCRIPT_DIR = "./scripts/build";
        const string OUTPUT_DIR = "./dist";
        const string PROJECT_DIR = "./src";
        const string DEFAULT_RUNTIME = "any";
        static readonly string PROJECT = $"{PROJECT_DIR}/md2html.csproj";

        static bool _isSingleFile = false;
        static bool _zipFiles = false;

        static void Main(string[] args)
        {
            // save colors to restore at the end
            var (fgColor, bgColor) = (ForegroundColor, BackgroundColor);

            _isSingleFile = CheckOptionAndRemove(ref args, "--single-file");

            _zipFiles = CheckOptionAndRemove(ref args, "--zip");

            Target("check", "Checks packages and lists those that have a newer version,\n             have been deprecated or have known vulnerabilities.",
                () => {
                    string[] checkPackageOptions = new[] {
                        "--outdated", "--deprecated", "--vulnerable --include-transitive"
                    };

                    void RunChecks(string prjDir)
                    {
                        foreach (var option in checkPackageOptions) {
                            WriteLine($"\n>>> Checking for {option.Split(' ')[0][2..]} packages");
                            Run("dotnet", $"list {prjDir} package {option}");
                        }
                    };

                    WriteLine(">>> ==============================");
                    WriteLine(">>> Checking build script packages");
                    WriteLine(">>> ==============================");
                    RunChecks(BUILD_SCRIPT_DIR);

                    WriteLine();
                    WriteLine(">>> =========================");
                    WriteLine(">>> Checking project packages");
                    WriteLine(">>> =========================");
                    RunChecks(PROJECT_DIR);
                }
            );

            Target("clean", "Delete the {project}/bin and {project}/obj directories.",
                () => DeleteDirs($"{PROJECT_DIR}/bin", $"{PROJECT_DIR}/obj"));

            Target("clean-all", "Calls clean and then also deletes the /dist directory",
                DependsOn("clean"),
                () => DeleteDirs(OUTPUT_DIR));

            Target("default", "Release that should run on any platform where .NET 5 runtime is installed.",
                DependsOn("clean"),
                () => Publish("any"));

            Target("format", "Calls dotnet-format to enforce format rules in the .editorconfig file.",
                () => {
                    try {
                        Run("dotnet", $"format {PROJECT}");
                    }
                    catch (SimpleExec.NonZeroExitCodeException) {
                        WriteLine("Target requires the dotnet-format tool.");
                        WriteLine("Install it by running: dotnet tool install --global dotnet-format");
                    }
                });

            Target("prompt", "Interactive mode that will ask for available options.", () => {
                var runtime = Prompt.Select("Select target runtime"
                    , new[] { "any", "linux-x64", "macos-x64", "win10-x64" }
                    , defaultValue: DEFAULT_RUNTIME);
                _isSingleFile = (runtime != DEFAULT_RUNTIME)
                    && Prompt.Confirm("Set the PublishSingleFile property to true?", defaultValue: false);
                _zipFiles = Prompt.Confirm("Move files to a zip file after the build?", false);
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

            if (args.HasAny(new[] { "-h", "--help", "-?", "-l", "--list-targets" }, ignoreCase: true)) {
                RunTargetsWithoutExiting(args);
                // display additional usage details and features
                WriteLine();
                WriteLine("NOTES:");
                WriteLine("  - default, linux, macos & win10 targets use the 'Release' configuration");
                WriteLine("  - linux, macos & win10 targets build as --self-contained");
                WriteLine("  - use --single-file option to set the PublishSingleFile property to true;\n    available for linux, macos & win10 targets");
                WriteLine("  - use --zip option to generate a compressed .zip file of the release");
                RestoreColors();
                System.Environment.Exit(0);
            }

            try {
                RunTargetsWithoutExiting(args);
            }
            finally {
                RestoreColors();
            }

            static bool CheckOptionAndRemove(ref string[] args, string optionName)
            {
                bool found = args.Has(optionName, ignoreCase: true);
                if (found) {
                    args = args.Remove(optionName, ignoreCase: true);
                }
                return found;
            }

            void RestoreColors()
            {
                ForegroundColor = fgColor;
                BackgroundColor = bgColor;
            }
        }

        static void DeleteDirs(params string[] dirs)
        {
            foreach (var dir in dirs) {
                if (Directory.Exists(dir)) {
                    Directory.Delete(dir, recursive: true);
                }
            }
        }

        static void Publish(string runtime)
        {
            string outputDir = $"{OUTPUT_DIR}/{runtime}";

            DeleteDirs(outputDir);

            if (runtime == "any") {
                Run("dotnet", $"publish -c Release -o {outputDir} {PROJECT}");
            }
            else {
                Run("dotnet", $"publish -c Release -o {outputDir} -r {runtime} -p:PublishSingleFile={_isSingleFile} {PROJECT}");
            }

            if (_zipFiles) {
                string zipfilePath = Path.Combine(OUTPUT_DIR, Path.GetFileNameWithoutExtension(PROJECT))
                    + $"-{runtime}.zip";

                if (File.Exists(zipfilePath)) {
                    File.Delete(zipfilePath);
                }

                System.IO.Compression.ZipFile.CreateFromDirectory(outputDir, zipfilePath
                    , System.IO.Compression.CompressionLevel.Optimal
                    , includeBaseDirectory: false);

                WriteLine($"  {Path.GetFileNameWithoutExtension(PROJECT)} -> {zipfilePath}");
                DeleteDirs(outputDir);
            }
        }
    }

    public static class ExtensionMethods
    {
        public static bool Has(this string[] array, string value, bool ignoreCase = false)
            => array.Any(e => string.Compare(e, value, ignoreCase: ignoreCase) == 0);

        public static bool HasAny(this string[] array, string[] values, bool ignoreCase = false)
            => array.Any(e => values.Has(e, ignoreCase));

        public static string[] Remove(this string[] array, string value, bool ignoreCase = false)
            => array.Where(e => string.Compare(e, value, ignoreCase: ignoreCase) != 0).ToArray();
    }
}
