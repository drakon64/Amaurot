{
  pkgs ?
    let
      npins = import ../npins;
    in
    import npins.nixpkgs { },
}:
pkgs.buildDotnetModule {
  pname = "amaurot-processor";
  version = "0.0.1";

  src = ./.;

  projectFile = "Amaurot.Processor.csproj";
  nugetDeps = ./deps.nix;

  dotnet-sdk = pkgs.dotnetCorePackages.sdk_9_0;
  dotnet-runtime = pkgs.dotnetCorePackages.runtime_9_0;

  executables = [ ];
}
