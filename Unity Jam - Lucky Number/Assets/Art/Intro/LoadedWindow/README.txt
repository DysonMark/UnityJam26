Loaded - rainy window sprite set
================================

Scale: native pixel art drawn for a 640x360 reference resolution.
Window 160x120 px plus a 172x5 px sill; every frame/layer canvas is 172x125 px.

Files
- window_animated_sheet_12f.png  Everything combined, 12 frames in one row (172x125 per frame). Simplest drop-in.
- window_static.png              One combined frame, if you don't want animation.
- layers/window_glass.png        Glass, glint and static droplets.
- layers/window_rain_sheet_12f.png  Rain only, 12 frames (172x125 each), transparent.
- layers/window_frame.png        Frame, mullions and sill (glass area is transparent).
- layers/window_glow.png         Soft cold light spill, 252x205; place it so the window sits 40 px in from its top-left.
- window_preview_4x.gif / .png   Preview only, not for the game.

Unity import (all PNGs)
- Texture Type: Sprite (2D and UI), Filter Mode: Point (no filter), Compression: None
- Pixels Per Unit: match your other sprites
- Sheets: Sprite Mode Multiple > Sprite Editor > Slice > Grid By Cell Size 172 x 125
- Animation: 12 fps (0.083 s per frame), Loop Time on. The loop is seamless.

Layered setup (if you use the layers)
- Same position for glass, rain and frame. Order in Layer: glow 0, glass 1, rain 2, frame 3.
- glow can use an additive or alpha material; drop its alpha if it is too strong on your wall.
