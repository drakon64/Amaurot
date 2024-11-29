{
  pkgs ?
    let
      npins = import ../npins;
    in
    import npins.nixpkgs { },
}:
let
  receiver = pkgs.callPackage ./. { };
in
pkgs.dockerTools.buildLayeredImage {
  name = "amaurot-receiver";

  config.Entrypoint = [ "${receiver}/lib/amaurot-receiver/Amaurot.Receiver" ];

  contents = with pkgs; [ cacert ];

  tag = "latest";
}
