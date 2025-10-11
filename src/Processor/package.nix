{
  pkgs,
  lib,
  buildDotnetModule,
  dotnetCorePackages,
  stdenv,
  dockerTools,
  opentofu,
  git,
  openssh,
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

  passthru.docker =
    {
      enableGit ? true,
      enableSsh ? true,
    }:
    dockerTools.buildLayeredImage {
      name = "amaurot-processor";
      tag = "latest";

      config = {
        Cmd = [ (lib.getExe finalAttrs.finalPackage) ];

        Env = [
          (
            "PATH=${builtins.dirOf (lib.getExe opentofu)}"
            + lib.optionalString enableGit ":${builtins.dirOf (lib.getExe git)}"
            + lib.optionalString (enableGit && enableSsh) ":${builtins.dirOf (lib.getExe openssh)}"
          )
        ];
      };

      contents = with dockerTools; [ caCertificates ] ++ lib.optional (enableGit && enableSsh) fakeNss;
    };
})
