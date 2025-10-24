{
  pkgs,
  lib,
  buildDotnetModule,
  dotnetCorePackages,
  stdenv,
  dockerTools,
  opentofu,
  gitMinimal,
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
        ./Dockerfile

        (lib.fileset.maybeMissing ./deps.json)
        ./package.nix
      ]
    );
  };

  projectFile = "Amaurot.Processor.csproj";
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_10_0;
  dotnet-runtime = null;

  executables = [ "Amaurot.Processor" ];

  selfContainedBuild = true;

  # Native AOT
  nativeBuildInputs = [ stdenv.cc ];

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "Amaurot.Processor";
    maintainers = with lib.maintainers; [ drakon64 ];
  };

  passthru.docker =
    {
      withGit ? true,
      withSsh ? true,
    }:
    dockerTools.buildLayeredImage {
      name = "amaurot-processor";
      tag = "latest";

      config = {
        Entrypoint = [ (lib.getExe finalAttrs.finalPackage) ];

        Env =
          let
            path =
              let
                git = builtins.dirOf (lib.getExe gitMinimal);
                ssh = builtins.dirOf (lib.getExe openssh);
              in
              if withGit then git + lib.optionalString withSsh ":${ssh}" else null;
          in
          [
            "OPENTOFU=${lib.getExe opentofu}"
            path
          ];
      };

      contents = with dockerTools; [ caCertificates ] ++ lib.optional (withGit && withSsh) fakeNss;
    };
})
