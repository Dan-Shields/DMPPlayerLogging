#### Reports user sessions in a DMP server (KSP multiplayer mod) back to a MySQL server for nefarious statistical purposes ####
Current Version: 0.1

### Environment setup ###

* [MySQL Connector/Net](https://dev.mysql.com/downloads/connector/net/6.9.html) is required.
* Clone this repo and [DarkMultiPlayer](https://github.com/godarklight/DarkMultiPlayer) into the same folder.
* Import Common and Server from DMP (or it might do it for you, I don't know how visual studio works to be honest).
* Build and follow the end user configuration instructions.

### End User Configuration ###
* Create a database and create a user to access this (or just use root if you're edgy like that).
* Download the .dll from the [Downloads section](https://bitbucket.org/Dan_Shields/dmpplayerlogging/downloads/?tab=downloads) and place in your 'DMP/plugins' folder
* Run the server once, ignore the errors while it generates the settings file.
* Close the server and edit the settings file to your heart's content.
