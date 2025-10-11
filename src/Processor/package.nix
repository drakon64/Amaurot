{
  pkgs,
  lib,
  buildDotnetModule,
  dotnetCorePackages,
  stdenv,
  dockerTools,
  ...
}:
let
  fs = lib.fileset;
in
buildDotnetModule (finalAttrs: {
  pname = "amaurot-processor";
  version = builtins.readFile ../../version;

  src = fs.toSource {
    root = ./.;

    fileset = fs.difference (./.) (
      fs.unions [
        (lib.fileset.maybeMissing ./bin)
        (lib.fileset.maybeMissing ./config)
        (lib.fileset.maybeMissing ./obj)

        (lib.fileset.maybeMissing ./deps.json)
        ./package.nix
      ]
    );
  };

  projectFile = "Amaurot.Processor.csproj";
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_9_0;

  executables = [ "Amaurot.Processor" ];

  # Native AOT
  dotnet-runtime = null;
  selfContainedBuild = true;
  nativeBuildInputs = [ stdenv.cc ];

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "Amaurot.Processor";
    maintainers = with lib.maintainers; [ drakon64 ];
  };

  passthru.docker = dockerTools.buildLayeredImage {
    name = "amaurot-processor";
    tag = "latest";

    config.Cmd = [ (lib.getExe finalAttrs.finalPackage) ];

    contents = [ dockerTools.caCertificates ];
  };
})
