{
  pkgs ? import (import ./lon.nix).nixpkgs { },
}:
pkgs.mkShellNoCC {
  packages = with pkgs; [
    graphviz
    lon
    nixfmt-rfc-style
    opentofu
  ];

  passthru = {
    lon = pkgs.mkShellNoCC {
      packages = with pkgs; [
        git
        lon
      ];
    };

    opentofu = pkgs.mkShellNoCC {
      packages = with pkgs; [ opentofu ];
    };
  };
}
