# PixelProxy
Application to intercept the incoming and outgoing packets from Pixel Worlds.

# Setup
Modify the hosts file to redirect the dns resolve for the pixel worlds server domain to localhost.
- If on Windows, open `C:\Windows\System32\drivers\etc\hosts`, if on Linux (or another Unix-based OS) open `/etc/hosts`
- Add the following line `127.0.0.1 prod.gamev81.portalworldsgame.com`
- Start the proxy.
- Launch the game.

Domain may change in the future for the server.

# Todo
- Implement a proper way for modifying incoming/outgoing packets.
- Ability to have multiple clients connect to the proxy.
- <del>Fix my stupid solution for handling the exception on handling the packet.</del>
