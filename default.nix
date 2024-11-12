let
  pkgs =
    let
      npins = import ./nix/npins;
    in
    import npins.nixpkgs { };
in
pkgs.callPackage ./nix { }
