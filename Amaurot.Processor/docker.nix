{
  pkgs,
  enableGit ? true,
}:
let
  processor = pkgs.callPackage ./. { };
in
pkgs.dockerTools.buildLayeredImage {
  name = "amaurot-processor";

  config = {
    Entrypoint = [ "${processor}/lib/amaurot-processor/Amaurot.Processor" ];

    Env =
      with pkgs;
      [ "TOFU_PATH=${lib.getExe opentofu}" ]
      ++ lib.optional enableGit "PATH=${git}/bin:${openssh}/bin";
  };

  contents = with pkgs; [ dockerTools.caCertificates ] ++ lib.optional enableGit dockerTools.fakeNss;

  tag = "latest";
}
