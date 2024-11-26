{
  pkgs ?
    let
      npins = import ../npins;
    in
    import npins.nixpkgs { },
  compressor ? "none",
}:
let
  lib = pkgs.lib;

  nonRootShadowSetup =
    {
      user,
      uid,
      gid ? uid,
    }:
    with pkgs;
    [
      (writeTextDir "etc/shadow" ''
        root:!x:::::::
        ${user}:!:::::::
      '')
      (writeTextDir "etc/passwd" ''
        root:x:0:0::/root:${runtimeShell}
        ${user}:x:${toString uid}:${toString gid}::/home/${user}:
      '')
      (writeTextDir "etc/group" ''
        root:x:0:
        ${user}:x:${toString gid}:
      '')
      (writeTextDir "etc/gshadow" ''
        root:x::
        ${user}:x::
      '')
    ];

  runner = pkgs.callPackage ./. { };
in
pkgs.dockerTools.buildLayeredImage {
  name = "amaurot-processor";

  inherit compressor;

  config = {
    Entrypoint = [
      (lib.getExe runner.dotnet-runtime)
      "${runner}/lib/amaurot-processor/Amaurot.Processor.dll"
    ];

    Env = [
      "PATH=${pkgs.git}/bin:${pkgs.openssh}/bin"
      "TOFU_PATH=${lib.getExe pkgs.opentofu}"
    ];
  };

  contents =
    with pkgs;
    [ cacert ]
    ++ nonRootShadowSetup {
      uid = 1000;
      user = "amaurot";
    };
  maxLayers = 105;
  tag = "latest";
}
