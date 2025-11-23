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

  withGit ? false,
  withSsh ? false,
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

  postFixup = ''
    wrapProgram $out/bin/Amaurot.Processor --prefix PATH : ${
      lib.makeBinPath (
        [ opentofu ] ++ lib.optional withGit gitMinimal ++ lib.optional (withGit && withSsh) openssh
      )
    }
  '';

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "Amaurot.Processor";
    maintainers = with lib.maintainers; [ drakon64 ];
  };

  passthru.docker = dockerTools.buildLayeredImage {
    name = "amaurot-processor";
    tag = "latest";

    config.Entrypoint = [ (lib.getExe finalAttrs.finalPackage) ];
    contents = with dockerTools; [ caCertificates ] ++ lib.optional (withGit && withSsh) fakeNss;
  };
})
