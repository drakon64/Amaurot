{
  pkgs,
  lib,
  buildDotnetModule,
  dotnetCorePackages,
  ...
}:
let
  fs = lib.fileset;
in
buildDotnetModule {
  pname = "amaurot-receiver";
  version = builtins.readFile ../../version;

  src = fs.toSource {
    root = ./.;

    fileset = fs.difference (./.) (
      fs.unions [
        ./appsettings.Development.json
        (lib.fileset.maybeMissing ./bin)
        (lib.fileset.maybeMissing ./config)
        (lib.fileset.maybeMissing ./obj)

        (lib.fileset.maybeMissing ./deps.json)
        ./package.nix
        ./docker.nix
      ]
    );
  };

  projectFile = "Amaurot.Receiver.csproj";
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = dotnetCorePackages.aspnetcore_9_0;

  executables = [ "Amaurot.Receiver" ];

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "Amaurot.Receiver";
    maintainers = with lib.maintainers; [ drakon64 ];
  };
}
