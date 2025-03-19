let
  pkgs = (import ./. { }).nixpkgs;
in
pkgs.mkShellNoCC {
  packages = with pkgs; [
    nixfmt-rfc-style
    npins
  ];
}
