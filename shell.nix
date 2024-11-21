{
  pkgs ?
    let
      npins = import ./npins;
    in
    import npins.nixpkgs { },
}:
pkgs.mkShellNoCC {
  packages = with pkgs; [
    nixfmt-rfc-style
    npins
  ];
}
