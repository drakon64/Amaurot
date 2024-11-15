{
  pkgs ?
    let
      npins = import ../npins;
    in
    import npins.nixpkgs { },
  compressor ? "none",
}:
let
  receiver = pkgs.callPackage ./. { };
in
pkgs.dockerTools.buildLayeredImage {
  name = "amaurot-receiver";

  inherit compressor;

  config.Entrypoint = [
    (pkgs.lib.getExe receiver.dotnet-runtime)
    "${receiver}/lib/receiver/Receiver.dll"
  ];

  contents = with pkgs; [ cacert ];

  tag = "latest";
}
