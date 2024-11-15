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
    fs.difference ../Amaurot.Firewall/. (
      fs.unions [ (fs.fileFilter (file: file.hasExt "nix") ../Amaurot.Firewall/.) ]
    )
  );

  dotnetCorePackages = pkgs.dotnetCorePackages;
in
pkgs.buildDotnetModule {
  pname = "amaurot-firewall";
  version = "0.0.1";

  src = fs.toSource {
    fileset = sourceFiles;

    root = ../Amaurot.Firewall/.;
  };

  projectFile = "Amaurot.Firewall.csproj";
  nugetDeps = ./deps.nix;

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = dotnetCorePackages.aspnetcore_9_0;

  executables = [ ];
}
