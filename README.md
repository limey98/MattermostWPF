# MattermostWPF
A fully native Mattermost client built in WPF.

##Why fully native?
Currently, the only Windows Standalone Mattermost client is one built with Electron. It's basically just a wrapper around the web client. This has several deficiencies, most notably the lack of offline support. 

#Status
This program is still very much in development. I've probably only spend the equivalent of a couple of days on it so far.

The list of current features:
* Authentication
* Listing and switching channels
* Retrieving the last 30 posts
* Creating new posts
* Real time messaging (a websocket connection to the server to instantly be notified of new posts)

Current working on:
* Local storage
  * Server info (including token) to automatically log on
  * Caching posts, channels and users to give some basic offline functionality