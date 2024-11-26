{
  pkgs ?
    let
      npins = import ../npins;
    in
    import npins.nixpkgs { },
}:
let
  # A user must be defined else OpenSSH won't work
  shadow = with pkgs; [
    (writeTextDir "etc/shadow" ''
      root:!x:::::::
    '')
    (writeTextDir "etc/passwd" ''
      root:x:0:0::/root:${runtimeShell}
    '')
    (writeTextDir "etc/group" ''
      root:x:0:
    '')
    (writeTextDir "etc/gshadow" ''
      root:x::
    '')
  ];

  processor = pkgs.callPackage ./. { };
in
pkgs.dockerTools.buildLayeredImage {
  name = "amaurot-processor";

  config = {
    Entrypoint = [
      (pkgs.lib.getExe processor.dotnet-runtime)
      "${processor}/lib/amaurot-processor/Amaurot.Processor.dll"
    ];

    Env = with pkgs; [
      "PATH=${git}/bin:${openssh}/bin"
      "TOFU_PATH=${lib.getExe opentofu}"
    ];
  };

  contents = [
    pkgs.cacert
    shadow
  ];

  maxLayers = 105;
  tag = "latest";
}
