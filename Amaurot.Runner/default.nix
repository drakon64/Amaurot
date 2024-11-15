{
  pkgs ?
    let
      npins = import ../npins;
    in
    import npins.nixpkgs { },
}:
let
  fs = pkgs.lib.fileset;
  sourceFiles = fs.intersection (fs.gitTracked ../.) (
    fs.unions [
      (fs.difference ../Amaurot.Runner/. (fs.unions [ (fs.fileFilter (file: file.hasExt "nix") ../Amaurot.Runner/.) ]))
      ../Amaurot.Lib/.
    ]
  );

  dotnetCorePackages = pkgs.dotnetCorePackages;
in
pkgs.buildDotnetModule {
  pname = "amaurot-runner";
  version = "0.0.1";

  src = fs.toSource {
    fileset = sourceFiles;

    root = ../.;
  };

  projectFile = "Amaurot.Runner/Amaurot.Runner.csproj";
  nugetDeps = ./deps.nix;

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = dotnetCorePackages.runtime_9_0;

  executables = [ ];
}
