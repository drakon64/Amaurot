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
      (fs.difference ./. (fs.unions [ (fs.fileFilter (file: file.hasExt "nix") ./.) ]))
      ../Amaurot.Lib/.
    ]
  );

  dotnetCorePackages = pkgs.dotnetCorePackages;
in
pkgs.buildDotnetModule {
  pname = "amaurot-receiver";
  version = "0.0.1";

  src = fs.toSource {
    fileset = sourceFiles;

    root = ../.;
  };

  projectFile = "Amaurot.Receiver/Amaurot.Receiver.csproj";
  nugetDeps = ./deps.nix;

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = dotnetCorePackages.aspnetcore_9_0;

  executables = [ ];
}
