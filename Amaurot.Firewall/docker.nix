{
  pkgs ?
    let
      npins = import ../npins;
    in
    import npins.nixpkgs { },
  compressor ? "none",
}:
let
  firewall = pkgs.callPackage ./. { };
in
pkgs.dockerTools.buildLayeredImage {
  name = "amaurot-firewall";

  inherit compressor;

  config.Entrypoint = [
    (pkgs.lib.getExe firewall.dotnet-runtime)
    "${firewall}/lib/amaurot-firewall/Amaurot.Firewall.dll"
  ];

  contents = with pkgs; [ cacert ];

  tag = "latest";
}
