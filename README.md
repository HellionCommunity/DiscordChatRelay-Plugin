# DiscordChatRelay-Plugin
Relays Configurable In-game Chat from a Hellion Server running HES to a Discord server and Sends discord chat back. 

Plugin For https://github.com/HellionCommunity/HellionExtendedServer

    Version: 1.0.2 - Dev
    Name: HesDiscordChatBot

Config.xml

    DiscordToken - Your discord bot token goes here
    MainChannelID - The channel id (right click channel, copy id in discord(if you have discord developer mode enabled)) you want to 
    send chat to, and from (its a single channel in the initial release)

    BotClientID - currently has no purpose

    BotOwnerID - currently has no purpose

    Logging Options - to print to console


    PlayerSpawningMessage - The message people see on discord when a player spawns

    PlayerRespawningMessage - The message people see on discord when a player respawns

    PlayerLeavingMessage - when a player leaves

    MessageSentToDiscord - The message thats sent from the game server to discord.

    MessageSentToGameServer - the message thats sent from discord to the game server.

    NewPlayerSpawningMessage - A brand new player has connected to the server
    
    DebugMode - enables debug mode, prints more information to the console.
  
The messages all support template replacers, the available replacers are;
  
All:
     (%PlayerName%) - The name of the player - all but Discord to Server
     (%CurrentDateTime%) - the current date and time
     
Server to Discord:
     (%ChatMessage%) - the chat message

Discord to Server:
     (%DiscordChannelName%) - the name of the incoming channel
     (%DiscordUserName%)    - the username of the discord user
     (%DiscordChatMessage%) - The message from discord to the game

  
If the token and channelID are correct, and your bot has been authorized in your server with permissions granted, your chat from the 
specified channel will be relayed inside the game chat box.  And any global in-game chat will be relayed to the specified channel in discord.


But there isn't any customization except the discord channel to get and receive chat from! 

Features that will be added:

    Customize the text color.
    Using the same commands in discord and in-game if desired
    Able to create custom text based commands.
   
The listed features are only some of the planned features for this plugin.  There is no timetable on when these features will be added.
We still have work to do on HES before we polish our plugins ;)


