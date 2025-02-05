{
  pkgs,
  enableGit ? true,
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
    Entrypoint = [ "${processor}/lib/amaurot-processor/Amaurot.Processor" ];

    Env =
      with pkgs;
      [ "TOFU_PATH=${lib.getExe opentofu}" ]
      ++ pkgs.lib.optional enableGit "PATH=${git}/bin:${openssh}/bin";
  };

  contents = [ pkgs.cacert ] ++ pkgs.lib.optional enableGit shadow;

  tag = "latest";
}
