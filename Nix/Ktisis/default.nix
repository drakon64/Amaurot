{
  pkgs ?
    let
      npins = import ../npins;
    in
    import npins.nixpkgs { },
}:
let
  fs = pkgs.lib.fileset;
  sourceFiles = fs.intersection (fs.gitTracked ../../.) (
    fs.unions [
      ../../Ktisis/.
      ../../Elpis/.
    ]
  );

  dotnetCorePackages = pkgs.dotnetCorePackages;
in
pkgs.buildDotnetModule {
  pname = "ktisis";
  version = "0.0.1";

  src = fs.toSource {
    fileset = sourceFiles;

    root = ../../.;
  };

  projectFile = "Ktisis/Ktisis.csproj";
  nugetDeps = ./deps.nix;

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = dotnetCorePackages.runtime_9_0;

  executables = [ ];
}
