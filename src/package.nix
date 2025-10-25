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
  pname = "amaurot";
  version = builtins.readFile ../version;

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

  projectFile = "Amaurot.csproj";
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = null;

  executables = [ "Amaurot" ];

  selfContainedBuild = true;

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "Amaurot";
    maintainers = with lib.maintainers; [ drakon64 ];
  };

  passthru.docker =
    {
      withGit ? true,
      withSsh ? true,
    }:
    dockerTools.buildLayeredImage {
      name = "amaurot";
      tag = "latest";

      config = {
        Entrypoint = [ (lib.getExe finalAttrs.finalPackage) ];

        Env =
          let
            path =
              let
                binPath = lib.makeBinPath ([ gitMinimal ] ++ lib.optional withSsh openssh);
              in
              if withGit then "PATH=${binPath}" else null;
          in
          [
            "OPENTOFU=${lib.getExe opentofu}"
            path
          ];
      };

      contents = with dockerTools; [ caCertificates ] ++ lib.optional (withGit && withSsh) fakeNss;
    };
})
