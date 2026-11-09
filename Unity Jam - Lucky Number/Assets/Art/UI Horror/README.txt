Horror UI Kit
=============

218 pixel art UI pieces in one dark, rusted, blood-stained horror style.
Made to pair with Horror UI SFX: the same dread for the eyes and the ears.

NEW IN v2.0 (155 new pieces, 218 in total)
  input/keyboard/   A-Z, 0-9, arrows, Space, Enter, Esc, Shift, Tab, Ctrl
                    (normal and pressed)
  input/gamepad/    A B X Y, LB RB LT RT, d-pad (5 states), both sticks,
                    Start and Select
  input/mouse/      left, right, middle and scroll
  speech/           talk bubbles (left and right tail), think, shout, small
  banners/          ribbon, small ribbon, title plate
  controls/         dropdown (closed and open), list rows, text field,
                    page dots
  panels/           window title bar
  loading/          8-frame spinner and a loading bar
  markers/          quest available (!), quest turn-in (?), NEW badge,
                    notification
  fonts/            pixel_font.png + pixel_font.fnt (BMFont: Godot, Unity,
                    GameMaker, Phaser), also at 4x

COLOR THEMES (new in v2.0)
  Every piece also comes in 2 more color themes, redrawn color by color
  (same pixels, same sizes, same file names), so you can switch the whole
  look of your game by swapping one folder:
    ghost     Ghost
    toxic     Toxic
  themes/<theme>/x1/ and x4/   all 218 pieces
  themes/<theme>/fonts/         the pixel font in that theme
  themes/<theme>/rpgmaker/      the RPG Maker windowskin in that theme
  Health, damage and danger reds are kept red in every theme on purpose.
  RPG Maker: each theme's Window.png keeps the standard \C[n] message colors
  and HP / MP / TP gauge colors, and recolors only the system color and the
  gauge background.

FOLDERS
  x1/   every piece at its native pixel size (use this in pixel art games)
  x4/   the same pieces scaled 4x with nearest-neighbour

  panels/     rusted iron panels in 4 sizes, a dark panel, a 9-slice panel,
              dialogue boxes and a tooltip, with cracks and blood drips
  buttons/    ember, toxic, ghost and blood x normal / hover / pressed,
              disabled, icon buttons, close, arrows, plus and minus
  controls/   checkbox, radio, toggle, slider track and knob, scrollbar
  bars/       bar frame and 4 fills (blood, ghost, toxic, soul)
  inventory/  slots (normal / selected / disabled), tabs, portrait frame
  icons/      hearts, skull, eye, blood drop, stars, soul coin, cursor,
              selection arrow, divider, digits

9-SLICE
  The clean panel_9slice.png stretches with a 6 px border on every side
  (x1). The detailed panels keep their cracks and drips, so use them at
  their size or scale them by whole numbers.

USING THEM
  Import with nearest-neighbour filtering (Unity: Filter Mode Point,
  Compression None; Godot: Texture Filter Nearest) and scale by whole
  numbers so the pixels stay crisp.

RPG MAKER MZ / MV (new in v1.2)
  rpgmaker/img/system/Window.png is a windowskin made from this kit.
  Steps in README_RPGMAKER.txt.

LICENSE
  Free for commercial use, royalty free, no attribution required. Reselling or
  redistributing the files as an asset pack is not permitted. Full terms in
  LICENSE.txt

ENJOYING IT?
  A quick rating on the page helps a lot and keeps the free updates coming:
  https://willosuna.itch.io/horror-ui-kit
