{
  pkgs,
  lib,
  buildDotnetModule,
  dotnetCorePackages,
  stdenv,
  dockerTools,
  gitMinimal,
  openssh,
  opentofu,
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

        ./Dockerfile
      ]
    );
  };

  projectFile = "Amaurot.Processor.csproj";
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_10_0;
  dotnet-runtime = null;

  executables = [ "Amaurot.Processor" ];

  selfContainedBuild = true;

  nativeBuildInputs = [ stdenv.cc ];

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "Amaurot.Processor";
    maintainers = with lib.maintainers; [ drakon64 ];
  };

  passthru.docker =
    {
      withGit ? false,
      withSsh ? false,
    }:
    dockerTools.buildLayeredImage {
      name = "amaurot-processor";
      tag = "latest";

      config = {
        Entrypoint = [ (lib.getExe finalAttrs.finalPackage) ];

        Env = [
          "OPENTOFU=${lib.getExe opentofu}"
        ]
        ++ lib.optional withGit (
          "PATH=${lib.makeBinPath ([ gitMinimal ] ++ lib.optional withSsh openssh)}"
        );
      };

      contents = with dockerTools; [ caCertificates ] ++ lib.optional (withGit && withSsh) fakeNss;
    };
})
