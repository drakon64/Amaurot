{ pkgs }:
let
  fs = pkgs.lib.fileset;
  sourceFiles = fs.intersection (fs.gitTracked ../.) (
    fs.unions [
      (fs.difference ./. (fs.unions [ (fs.fileFilter (file: file.hasExt "nix") ./.) ]))
      ../Amaurot.Common/.
    ]
  );
in
pkgs.buildDotnetModule {
  pname = "amaurot-processor";
  version = "0.0.1";

  src = fs.toSource {
    fileset = sourceFiles;

    root = ../.;
  };

  projectFile = "Amaurot.Processor/Amaurot.Processor.csproj";
  nugetDeps = ./deps.nix;

  dotnet-sdk = pkgs.dotnetCorePackages.sdk_9_0;
  dotnet-runtime = null;

  executables = [ ];

  selfContainedBuild = true;
}
